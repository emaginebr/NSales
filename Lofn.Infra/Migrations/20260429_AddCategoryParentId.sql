-- Migration: 20260429_AddCategoryParentId
-- Feature: 002-category-subcategories
-- Apply per tenant DB:
--   psql "$ConnectionString" -f 20260429_AddCategoryParentId.sql
--
-- Adds the self-referencing parent column to lofn_categories so categories form
-- a tree, widens slug to fit the full ancestor path, and enforces sibling-name
-- uniqueness through a composite expression index that treats NULL parent_id
-- and NULL store_id as sentinels (so two stores can each have a root with the
-- same name, but a single store cannot).
--
-- Rollout note:
-- Pre-existing rows have parent_id = NULL ⇒ treated as roots.
-- Their slugs are unchanged (single-segment paths still satisfy the new format).
-- If two pre-existing rows in the same store share a name (case-insensitive),
-- the unique index in step 4 will FAIL — operator must dedupe first.

-- 1. Add parent_id column (NULL ⇒ root, preserving pre-rollout categories as roots).
ALTER TABLE lofn_categories
    ADD COLUMN IF NOT EXISTS parent_id BIGINT NULL;

-- 2. Self-FK with ON DELETE RESTRICT — DB enforces hard fallback for FR-010.
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'fk_lofn_category_parent'
    ) THEN
        ALTER TABLE lofn_categories
            ADD CONSTRAINT fk_lofn_category_parent
            FOREIGN KEY (parent_id)
            REFERENCES lofn_categories (category_id)
            ON DELETE RESTRICT;
    END IF;
END $$;

-- 3. Widen slug to fit full ancestor path (5 levels × ~100 chars + 4 slashes).
ALTER TABLE lofn_categories
    ALTER COLUMN slug TYPE VARCHAR(512);

-- 4. Sibling-name uniqueness — see specs/002-category-subcategories/research.md §R5.
CREATE UNIQUE INDEX IF NOT EXISTS ix_lofn_categories_sibling_name_unique
    ON lofn_categories ((COALESCE(parent_id, 0)), (COALESCE(store_id, 0)), lower(name));

-- 5. Helper index for cascade descendant walks (parent_id is queried frequently).
CREATE INDEX IF NOT EXISTS ix_lofn_categories_parent_id
    ON lofn_categories (parent_id) WHERE parent_id IS NOT NULL;
