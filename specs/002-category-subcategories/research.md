# Phase 0 — Research & Decisions

**Feature**: 002-category-subcategories
**Spec**: [spec.md](./spec.md)
**Plan**: [plan.md](./plan.md)

This document resolves the open questions surfaced in `plan.md`'s Technical Context and locks in patterns for the implementation phase. Each entry uses the `Decision / Rationale / Alternatives considered` format.

---

## R1. Cycle detection algorithm

**Decision**: Application-layer ancestor walk implemented inside `CategoryService`. When inserting/updating a category with a non-null `ParentId`, walk upward from the prospective parent following `Parent.Parent.Parent…` until either (a) we hit a root (`ParentId == null`) → no cycle, or (b) we hit the category being edited (`category.CategoryId`) → reject with cycle error. The walk is bounded by FR-004 (max depth 5), so worst case is 5 row-fetches per save.

**Rationale**:
- Postgres recursive CTE is more elegant but adds a SQL dialect dependency in the application layer; the existing repository methods are all single-table queries with EF Core LINQ.
- The 5-level depth bound makes the linear walk O(5) — recursive CTE is overkill at this scale.
- The walk also doubles as the depth check (count hops), so we do not duplicate the traversal.

**Alternatives considered**:
- *Postgres recursive CTE* — rejected: adds raw SQL inside repository, breaks the pure-EF-LINQ pattern used everywhere else, harder to unit-test with Moq.
- *Materialized path stored in column* — rejected: nice for tree queries but every move triggers updates of every descendant's path even when only a parent changes; we already need that cascade for slugs, so duplicating it for path doesn't pay off.
- *Closure table* — rejected: industrial-strength tree pattern, but at 500 nodes max it's vast over-engineering and adds a second table to keep in sync.

---

## R2. Tree assembly strategy for the GraphQL field

**Decision**: Eager-load the tenant's full category set into memory (one `SELECT * FROM lofn_categories WHERE …` filtered by scope), then build the tree client-side in the resolver: dictionary by `ParentId`, attach children, sort each level by `Name` (with culture-invariant case-insensitive comparison after applying the same accent normalization as `IStringClient.GenerateSlugAsync`). Return the list of roots; HotChocolate's projection will materialize the recursive `children` field via `CategoryTypeExtension.GetChildren([Parent] Category c, CategoriesById dict)`.

**Rationale**:
- At ≤500 categories per tenant (SC-002), one query + an in-memory build is faster and simpler than N+1 child queries via field resolvers hitting the DB.
- The in-memory tree is built once per request (scoped to the HTTP request via DataLoader) and reused by every depth level the client asks for.
- Sorting alphabetically in C# avoids a Postgres collation-vs-Locale headache (Portuguese accents specifically) and matches FR-013's case-insensitive + accent-normalized rule.

**Alternatives considered**:
- *Field resolver per level (HotChocolate `[UseProjection]` over self-FK)* — rejected: each level becomes a separate DB round-trip, breaking SC-006 ("single round-trip").
- *Postgres recursive CTE returning the full tree shape via raw SQL* — rejected: complicates testing, returns flat rows that still need in-memory assembly.
- *DataLoader keyed by `ParentId`* — kept as an option for future scaling beyond 500 nodes, but unnecessary at current target. We document the path forward in this section so a future dev can swap in a DataLoader without restructuring.

---

## R3. Slug full-path generation and column width

**Decision**: Slug is derived as `parent.Slug + "/" + IStringClient.GenerateSlugAsync(name)`. For roots it equals `IStringClient.GenerateSlugAsync(name)` (single segment, no slash) — preserving FR-014. The `slug` column is widened from `varchar(120)` to `varchar(512)` to accommodate the worst-case path (5 segments × 100 chars each — `Name` itself is `varchar(120)` so a slugified segment fits within ~120 chars per level). Existing rows are unaffected.

**Rationale**:
- 120 chars is too tight: 5 × 24 + 4 = 124 was the optimistic estimate, but `Name` allows 120 chars, so a single overlong segment can already break the 120 column. Widening to 512 gives headroom even for pathological names.
- Reuse the existing `IStringClient.GenerateSlugAsync` per segment so the established lowercase + accent-strip + non-alphanumeric → hyphen behaviour is preserved verbatim.
- Putting the `/` separator means clients reading the slug get URL-routable paths directly (web frontend already supports nested routing).

**Alternatives considered**:
- *Keep slug at 120 chars and reject names that overflow* — rejected: adds a runtime validation that's invisible to the admin until they save a deep child; better to provision generously and let the DB handle the constraint.
- *Store only the last segment and compute the full path on read* — rejected: defeats the point of having the slug be the public identifier; consumers (the storefront URL `/category/{slug}`) need the full string available with no extra computation.
- *Use `text` column* — rejected: `varchar(512)` is explicit, indexed cheaply, and signals an intentional cap.

---

## R4. Cascade slug recompute atomicity

**Decision**: Wrap rename and move operations in a single EF Core transaction (`await using var tx = await context.Database.BeginTransactionAsync(...)`). Algorithm: (1) recompute the moved/renamed node's slug, (2) breadth-first walk descendants, recomputing each slug from its (now updated) parent's slug, (3) on any uniqueness violation (sibling name OR tenant slug index), throw — transaction rolls back, leaving the tree untouched. Save once at the end.

**Rationale**:
- SC-004 requires atomicity: "every descendant's slug is updated or none is".
- With ≤500 rows max in the worst case, a single DB transaction is well within reasonable transaction-size limits.
- Pre-flight uniqueness checking before any UPDATE is wasteful — let the DB enforce uniqueness via the existing partial unique index (feature 001) plus a new sibling index, and rely on rollback to undo half-applied changes.

**Alternatives considered**:
- *Two-phase: dry-run all new slugs in memory first, fail fast, then write* — rejected: requires duplicating uniqueness logic in code that the DB already enforces.
- *Per-row transaction* — rejected: violates atomicity (SC-004), cannot be undone if a later child fails.
- *Background job for cascade* — rejected: introduces an async window where parent shows new slug and children show old, breaking SC-004's "no half-finished states".

---

## R5. Sibling-name uniqueness enforcement

**Decision**: New unique partial index on `(parent_id, lower(name))` per scope:
- For store-scoped: `WHERE store_id IS NOT NULL` (per-store)
- For global: `WHERE store_id IS NULL` (per-tenant)

But "per scope" actually collapses to per-tenant DB anyway because `store_id IS NULL` rows are global within the tenant, and `store_id IS NOT NULL` rows are partitioned by `store_id`. The simplest expression is a single composite index on `(store_id, parent_id, lower(name))` enforced as `UNIQUE`. Postgres treats `NULL` values as distinct in a regular unique index, so the index correctly allows two roots in different stores or one root global plus one root in a store with the same name.

**Rationale**:
- FR-009 says siblings can't share names. A composite index nails this at the DB layer.
- Including `lower(name)` matches the case-insensitive sibling rule from FR-013 (used for ordering) and avoids relying solely on slug-collision rejection (which is a derived rule, not the underlying invariant).
- `NULL`-as-distinct is fine here because two stores with `store_id = 1` and `store_id = 2` are independent worlds.

**Edge case handled**: When `parent_id IS NULL` (root level), the index still works because Postgres's standard unique-index semantics treat NULL as not-equal-to-NULL; we want roots to be unique by (store_id, name) with NULL parent_id, but two stores can each have a root named "Vestuário". The composite index `UNIQUE(store_id, parent_id, lower(name))` actually permits multiple roots with the same name within the same store because NULL parent_ids do not collide. We MUST use `COALESCE(parent_id, 0)` in the index expression — Postgres supports indexes on expressions: `UNIQUE INDEX ON lofn_categories ((COALESCE(parent_id, 0)), (COALESCE(store_id, 0)), lower(name))`. This forces NULL parent_id to behave as a sentinel root, and NULL store_id to behave as the global scope.

**Alternatives considered**:
- *Service-layer-only check via repository* — rejected: race-condition prone under concurrent admin edits (two admins simultaneously creating two siblings with the same name would both pass the read-then-write check).
- *Trigger* — rejected: adds Postgres-specific procedural code; hard to test, hard to migrate.

---

## R6. Tree query mutex with marketplace mode

**Decision**: Add two GraphQL fields:
- `PublicQuery.GetCategoryTree(storeSlug: String)` — public schema, anonymous, mode-conditional. When `Marketplace=true`: ignore `storeSlug`, return global tree. When `Marketplace=false` AND `storeSlug` is provided: return that store's tree. When `Marketplace=false` AND `storeSlug` is null: return empty (or error — see below).
- `AdminQuery.GetMyCategoryTree` — admin schema, authenticated, infers scope from session. When `Marketplace=true`: returns global tree (admin must be marketplace admin to see all globals; otherwise sees only what the admin schema permits today). When `Marketplace=false`: returns the union of trees for all stores the user owns (matching existing `GetMyCategories` behaviour).

For the public-schema "no storeSlug in non-marketplace mode" case, we return an **empty tree** (consistent with how `GetCategories` currently behaves — it returns categories that have at least one active product, not store-scoped — but with subcategories we want the consumer to scope by store explicitly). This is the only deviation from the strict mutex and is documented as such.

**Rationale**:
- Mirrors existing `GetCategories` resolver branching on `tenantResolver.Marketplace`.
- Preserves the feature-001 mutex: in marketplace mode there are no store-scoped categories to traverse anyway, so the storeSlug arg becomes meaningless.
- Returning empty (rather than 4xx) for the no-arg non-marketplace case keeps the API contract simple — the storefront simply fetches an empty list and renders nothing, no error path needed.

**Alternatives considered**:
- *Single field that auto-detects scope* — rejected: surprising to the caller; explicit `storeSlug` arg is more honest.
- *Separate `GetGlobalTree` and `GetStoreTree(storeSlug)` fields* — rejected: doubles API surface for no benefit; the resolver already has to branch internally.

---

## R7. ParentId on the existing CategoryInfo flat DTO

**Decision**: Add `ParentCategoryId` (nullable `long?`) to `CategoryInfo`. All existing flat-list endpoints (`/category-global/list`, REST list, GraphQL `GetCategories`) start surfacing it. Existing consumers ignore unknown JSON fields (Newtonsoft and System.Text.Json default behaviour) so this is non-breaking.

**Rationale**:
- FR-016 requires the flat list to expose `parentCategoryId` so consumers can rebuild the tree client-side if they prefer.
- Adding a nullable field to a JSON response is a backward-compatible change — pre-rollout consumers see the same fields they always saw, plus an extra one they ignore.
- No DTO version-ing required.

**Alternatives considered**:
- *New separate `CategoryWithParentInfo`* — rejected: doubles DTOs for no API benefit; `null` already conveys "no parent".

---

## R8. Tests-as-contract approach

**Decision**: Three test surfaces, mirroring feature 001:

1. **Unit (Lofn.Tests)** — service-level: `CategoryServiceTests` covers cycle rejection, depth-5 cap, scope mismatch, sibling-name collision, slug cascade on rename, slug cascade on move, root-detach, mixed-mode (FR-018), tree assembly shape, alphabetical sort.

2. **Validator unit (Lofn.Tests/Validators)** — FluentValidation: each of the four DTO validators gets parent-related cases (parent must exist, scope match, depth, cycle).

3. **Integration (Lofn.ApiTests)** — XOR mutex pattern from feature 001 reused: `CategoryMutualExclusionTests` adds a parent-aware case (insert subcategory on each surface, exactly one succeeds). `CategoryTreeGraphQLTests` is new and asserts: (a) tree shape with seeded 3-level hierarchy, (b) alphabetical order at each level, (c) mutex (one schema returns populated, the other returns empty), (d) public unauthenticated tree access, (e) admin authenticated tree access.

**Rationale**:
- Feature 001 already settled the test architecture (XOR mutex, config-agnostic).
- Validator unit tests catch the bulk of reject paths cheaply.
- Service unit tests catch atomicity (mock repo + verify save order).
- API integration tests catch the wiring (controller → service → repo → DB → GraphQL → client).

**Alternatives considered**:
- *Skip API integration tests for this feature* — rejected: GraphQL tree assembly + alphabetical sort + mutex are emergent behaviours that the unit tests cannot fully cover.

---

## R9. lofn.sql update

**Decision**: Append the new DDL to the existing `lofn.sql` (the bootstrap script for fresh DB provisioning) AND ship a separate idempotent migration file `Lofn.Infra/Migrations/20260429_AddCategoryParentId.sql` for existing tenants. Both files contain the same logical DDL: ALTER TABLE add column, ADD FK, CREATE INDEX, ALTER COLUMN slug TYPE varchar(512). The migration file has additional safety: `IF NOT EXISTS`, `DROP INDEX IF EXISTS` guards.

**Rationale**:
- Mirrors the feature-001 pattern (`20260428_AddGlobalCategoryUniqueIndex.sql` as the migration, lofn.sql as the source-of-truth bootstrap).
- New tenants provisioned from `lofn.sql` get the schema right the first time.
- Existing tenants run the migration script once, idempotently.

**Alternatives considered**:
- *EF Core `Add-Migration` to autogenerate* — rejected: project does not use EF migrations as the source of truth; raw SQL is the convention.

---

## Open items deferred to implementation/operational stage

- **Concurrency strategy on overlapping moves**: Not addressed in this research because the worst-case impact (two admins fighting over the same subtree) is operationally low — admins coordinate informally. If this becomes a real problem, add an `xmin` row-version check on Category and surface a 409 Conflict to the second writer. Added to the "future work" follow-ups, not part of this feature.
- **Audit trail for moves**: Not addressed. If desired, lift wholesale from the existing audit pattern (if any) — out of scope here.
- **Slug-collision edge case** when widening 120→512 could in theory expose latent dupes already in DB. Mitigation: pre-flight a `SELECT slug, COUNT(*) FROM lofn_categories GROUP BY slug HAVING COUNT(*) > 1` in `lofn.sql`'s rollout note. Documented in `quickstart.md`.
- **Behavior beyond 500 categories**: spec says "up to 500" — beyond that, we don't guarantee SC-002 latency. Operationally we'd add pagination to the tree query or fall back to lazy loading via DataLoader. Documented as a known limitation in `quickstart.md`.
