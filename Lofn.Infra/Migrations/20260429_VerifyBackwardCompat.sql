-- Verification: 20260429_VerifyBackwardCompat
-- Feature: 002-category-subcategories  (T061)
-- Purpose: assert the rollout invariants on a tenant that already had categories
--          before the migration ran. Run AFTER 20260429_AddCategoryParentId.sql.
--
-- Per FR-014 and SC-005, applying the migration on a tenant that already has
-- categories MUST:
--    (a) leave every existing row's parent_id NULL (i.e. all rows become roots);
--    (b) leave every existing slug unchanged (single-segment, no '/' separator
--        because nothing has been re-saved through the new code path yet);
--    (c) keep the storefront-resolvable URLs intact so old links still work.
--
-- Usage:
--   psql "$ConnectionString" -f Lofn.Infra/Migrations/20260429_VerifyBackwardCompat.sql
--
-- The script raises NOTICEs on success and WARNINGs on failure. It does NOT
-- modify data. Pipe to grep '^WARNING' to detect violations in CI.
--
-- The "pre-existing" set is parameterised via :cutoff_id — pass the largest
-- category_id snapshotted before the migration was applied, e.g.
--     psql "$ConnectionString" -v cutoff_id=12345 -f 20260429_VerifyBackwardCompat.sql
-- If :cutoff_id is unset, the script falls back to "all rows with NULL
-- parent_id and a single-segment slug are pre-existing", which is correct
-- for any tenant that has not yet exercised the new code path.

\if :{?cutoff_id}
\else
\set cutoff_id 9223372036854775807   -- BIGINT max ⇒ every row is "pre-existing"
\endif

DO $$
DECLARE
    cutoff_id BIGINT := :cutoff_id;
    pre_count BIGINT;
    parent_violations BIGINT;
    slug_violations BIGINT;
    duplicate_slug_violations BIGINT;
    no_root_stores BIGINT;
BEGIN
    SELECT COUNT(*) INTO pre_count
        FROM lofn_categories
        WHERE category_id <= cutoff_id;

    RAISE NOTICE 'T061: cutoff_id=%  pre-existing rows=%', cutoff_id, pre_count;

    -- (a) every pre-existing row has parent_id NULL.
    SELECT COUNT(*) INTO parent_violations
        FROM lofn_categories
        WHERE category_id <= cutoff_id
            AND parent_id IS NOT NULL;

    IF parent_violations = 0 THEN
        RAISE NOTICE 'T061 (a) PASS: every pre-existing category has parent_id = NULL';
    ELSE
        RAISE WARNING 'T061 (a) FAIL: % pre-existing row(s) have a non-NULL parent_id (FR-014 violated)',
            parent_violations;
    END IF;

    -- (b) every pre-existing row's slug is single-segment (no '/' in it).
    SELECT COUNT(*) INTO slug_violations
        FROM lofn_categories
        WHERE category_id <= cutoff_id
            AND slug LIKE '%/%';

    IF slug_violations = 0 THEN
        RAISE NOTICE 'T061 (b) PASS: every pre-existing slug is single-segment';
    ELSE
        RAISE WARNING 'T061 (b) FAIL: % pre-existing row(s) have a slug containing "/" — pre-rollout slugs were rewritten (SC-005 violated)',
            slug_violations;
    END IF;

    -- (b extra) no slug appears more than once within the same store scope —
    -- a rewrite that produced a duplicate would silently break storefront URLs.
    SELECT COUNT(*) INTO duplicate_slug_violations
        FROM (
            SELECT COALESCE(store_id, 0) AS store_bucket, slug
            FROM lofn_categories
            WHERE category_id <= cutoff_id
            GROUP BY COALESCE(store_id, 0), slug
            HAVING COUNT(*) > 1
        ) AS dupes;

    IF duplicate_slug_violations = 0 THEN
        RAISE NOTICE 'T061 (b-2) PASS: no duplicate slug within any store scope';
    ELSE
        RAISE WARNING 'T061 (b-2) FAIL: % store(s) have duplicate slugs in their pre-existing rows',
            duplicate_slug_violations;
    END IF;

    -- (c) every store that had categories before still has at least one root
    --     category visible — the storefront navigation can still render.
    --     For marketplace tenants (store_id IS NULL rows), the equivalent check
    --     is "at least one global root exists".
    SELECT COUNT(*) INTO no_root_stores
        FROM (
            SELECT COALESCE(store_id, 0) AS store_bucket
            FROM lofn_categories
            WHERE category_id <= cutoff_id
            GROUP BY COALESCE(store_id, 0)
        ) AS pre_buckets
        WHERE NOT EXISTS (
            SELECT 1 FROM lofn_categories c
                WHERE COALESCE(c.store_id, 0) = pre_buckets.store_bucket
                    AND c.parent_id IS NULL
        );

    IF no_root_stores = 0 THEN
        RAISE NOTICE 'T061 (c) PASS: every scope with pre-existing rows still has at least one root';
    ELSE
        RAISE WARNING 'T061 (c) FAIL: % scope(s) with pre-existing rows no longer have a root category — storefront URLs may break',
            no_root_stores;
    END IF;

    -- Final summary.
    IF parent_violations = 0
        AND slug_violations = 0
        AND duplicate_slug_violations = 0
        AND no_root_stores = 0 THEN
        RAISE NOTICE 'T061 SUMMARY: PASS (FR-014 + SC-005 invariants hold)';
    ELSE
        RAISE WARNING 'T061 SUMMARY: FAIL — see WARNINGs above. Pre-rollout categories did not migrate cleanly.';
    END IF;
END $$;

-- Auxiliary detail report — copy/paste to inspect violations directly.
-- Run only if a violation surfaces above; harmless otherwise (no-op on clean tenants).

\echo
\echo '-- Detail report (rows that violate any invariant) --'

SELECT
    c.category_id,
    c.store_id,
    c.parent_id,
    c.slug,
    c.name,
    CASE
        WHEN c.parent_id IS NOT NULL THEN 'parent_id_not_null'
        WHEN c.slug LIKE '%/%' THEN 'slug_has_path'
        ELSE NULL
    END AS violation
FROM lofn_categories c
WHERE c.category_id <= :cutoff_id
    AND (c.parent_id IS NOT NULL OR c.slug LIKE '%/%')
ORDER BY c.store_id NULLS FIRST, c.category_id;
