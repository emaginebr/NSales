# Feature Specification: Category Subcategories Support

**Feature Branch**: `002-category-subcategories`
**Created**: 2026-04-29
**Status**: Draft
**Input**: User description: "Será necessário alterar as categorias para aceitar subcategorias: criar um campo ParentId relacionado com a propria categoria; o slug deve ser gerado com o caminho completo; deve existir um endpoint ou pelo graphql (de preferência) uma forma de listar toda a arvore de categorias, com categorias, subcategorias e sub das subcategorias."

## Clarifications

### Session 2026-04-29

- Q: Categorias com produtos podem receber subcategorias (modo misto) ou apenas categorias-folha aceitam produtos? → A: Permitir modo misto — uma categoria pode ter produtos E subcategorias simultaneamente, sem validação extra.
- Q: Qual é a profundidade máxima da árvore de categorias? → A: 5 níveis (raiz conta como nível 1).
- Q: Como os irmãos devem ser ordenados dentro de cada nível na árvore? → A: Alfabética por nome (case-insensitive, com normalização de acentos), sem campo de ordenação manual.
- Q: Buscar produtos por categoria deve ser transitiva (incluir descendentes) ou direta (só produtos com `categoryId` exato)? → A: Direta — sem propagação para descendentes. Out of scope para esta feature; busca transitiva, se necessária, será tratada em spec separada.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Cataloging products under nested categories (Priority: P1)

A store administrator (or marketplace administrator, depending on tenant mode) needs to organize a growing catalog by grouping products under broad categories AND nesting more specific groups beneath them. For example, a clothing store wants "Vestuário" as a top-level category, with "Camisetas" and "Calças" beneath it, and "Camisetas Vintage" further nested under "Camisetas". Without subcategories, every group sits at the same level and the catalog becomes unbrowsable as soon as it grows past a few dozen items.

**Why this priority**: This is the foundational capability of the feature — every other story depends on a category being able to declare a parent. Without this, the system has no way to express hierarchy, and no other behaviour (slug paths, tree listing, reorganization) is meaningful.

**Independent Test**: Can be fully tested by creating one root category, then creating a second category that references the first as its parent, and confirming both records persist and report their relationship correctly. Delivers the core value of expressing "this category lives under that one".

**Acceptance Scenarios**:

1. **Given** an admin with permission to manage categories in the active tenant mode, **When** they create a new category and supply the identifier of an existing category as its parent, **Then** the new category is persisted and its parent reference is retained.
2. **Given** an admin with permission to manage categories, **When** they create a new category without supplying a parent, **Then** the category is persisted as a root (top-level) category.
3. **Given** an existing category that has no parent, **When** an admin updates that category to reference a different existing category as parent, **Then** the relationship is updated and the category becomes a child of the chosen parent.
4. **Given** an existing parent-child relationship between two categories, **When** an admin tries to set the parent's parent to its own descendant, **Then** the system rejects the change with a clear error explaining cycles are not allowed.
5. **Given** an admin operating in marketplace mode, **When** they nest a global category under another global category, **Then** the operation succeeds; **When** they try to nest a global category under a store-scoped category (or vice versa), **Then** the system rejects the change because the two scopes cannot mix.

---

### User Story 2 - Browsing the full category tree (Priority: P1)

Anyone listing categories — admins editing the catalog, shoppers browsing the storefront, or downstream tools rendering navigation menus — needs to retrieve the entire hierarchy at once, with each category nested inside its parent so the consumer can render a tree without making one request per level. The current flat list cannot express "this list of items belongs under that category".

**Why this priority**: Tied with US1 because hierarchy that cannot be read is useless. Browsing is also the most-used surface (every storefront page that shows category navigation reads it), so any latency or shape regression here is highly visible.

**Independent Test**: Can be fully tested by seeding a known three-level hierarchy (root → child → grandchild), invoking the listing surface, and confirming the response contains exactly that nested structure with each node carrying its children inline. Delivers immediate value to any UI rendering category navigation.

**Acceptance Scenarios**:

1. **Given** a tenant with categories arranged in three nested levels, **When** a caller requests the category tree, **Then** the response returns root categories at the top and each non-root category appears as a child of its parent, recursively, to the deepest existing level.
2. **Given** a tenant whose categories are all roots (no nesting yet), **When** a caller requests the category tree, **Then** the response returns the full flat list of roots, each with an empty children collection.
3. **Given** a tenant in marketplace mode, **When** a caller requests the category tree, **Then** only global categories are returned; **Given** a tenant not in marketplace mode, **Then** only store-scoped categories for the queried store are returned. The mode-mutex established for category management also governs the tree response.
4. **Given** a category tree of moderate size (up to a few hundred categories across all levels), **When** a caller requests the tree, **Then** the response is delivered as a single payload without requiring follow-up requests per node.

---

### User Story 3 - Slug reflects the full ancestor path (Priority: P2)

When a category is referenced in URLs or shareable links, the slug should encode where the category sits in the hierarchy, so two categories named the same under different parents do not collide and so the URL itself communicates context. For example, a category named "Vintage" under "Camisetas" should have a slug like `camisetas/vintage`, and a category also named "Vintage" under "Calças" should have `calcas/vintage` — both unique, both meaningful.

**Why this priority**: Important for URL stability and human readability, but the system would still be functional with simple per-name slugs. It comes after US1/US2 because it is a refinement of how the hierarchy is exposed, not the hierarchy itself.

**Independent Test**: Can be fully tested by creating a parent and a child, observing the child's slug includes the parent segment, then renaming the parent and observing the child's slug updates accordingly. Delivers the value of stable, hierarchical URLs.

**Acceptance Scenarios**:

1. **Given** a category created at the root level with the name "Camisetas", **When** the system generates its slug, **Then** the slug is `camisetas`.
2. **Given** a child category named "Vintage" whose parent is the "Camisetas" root, **When** the system generates its slug, **Then** the slug is `camisetas/vintage`.
3. **Given** a grandchild category named "Anos 80" whose parent is "Vintage" (which is itself a child of "Camisetas"), **When** the system generates its slug, **Then** the slug is `camisetas/vintage/anos-80`.
4. **Given** an existing parent category with descendants, **When** an admin renames the parent, **Then** the parent's slug updates AND every descendant's slug updates so that the new parent name appears in their paths.
5. **Given** an existing category, **When** an admin moves it under a different parent, **Then** the moved category's slug AND every descendant's slug update to reflect the new ancestor chain.
6. **Given** two sibling categories, **When** an admin tries to create or rename them so they would share the same name under the same parent, **Then** the system rejects the change because the resulting slug would not be unique among siblings.

---

### Edge Cases

- **Cycle attempts**: An admin tries to set a category's parent to one of its own descendants (self, child, grandchild, etc.). The system MUST reject the change.
- **Cross-scope nesting**: An admin tries to nest a global category under a store-scoped category, or a store-scoped category under a category that belongs to a different store, or a global category under a store category. All MUST be rejected because the parent and child must share the same scope (both global, or both belonging to the same store).
- **Parent deletion with children**: An admin attempts to delete a category that still has subcategories. The system MUST reject the deletion until the subcategories are removed or moved elsewhere, mirroring the existing rule that prevents deleting categories that still have products.
- **Parent deletion with products**: A subcategory holding products cannot be deleted — same existing rule applies, irrespective of nesting depth.
- **Slug collision among siblings**: Two siblings with identical names (after normalization) would produce identical slugs; the system MUST reject the second create/rename.
- **Slug collision after a rename ripple**: Renaming a parent recomputes descendant slugs; if the recomputed path would clash with an existing slug elsewhere in the tenant, the rename MUST be rejected before any descendant is mutated, leaving the tree intact.
- **Existing categories at rollout**: Categories already in the system at the moment this feature ships have no parent. They MUST be treated as roots and their slugs MUST remain valid (single-segment path equal to their existing slug).
- **Tree request on empty tenant**: A caller requests the tree on a tenant with zero categories. The response MUST be an empty collection, not an error.
- **Excessive depth**: An admin attempts to nest beyond the maximum allowed depth (see Assumptions). The system MUST reject the operation with a clear message about the depth limit.
- **Orphaned reference**: An admin supplies a parent id that does not exist (or that has been deleted between read and write). The system MUST reject the create/update with a clear "parent not found" error, not silently fall back to root.
- **Empty parent category in product search**: A non-leaf category (one that has subcategories) may legitimately return zero products when filtered directly, even though its descendants hold products. This is expected behavior under the direct-search rule (FR-019); consumers that wish to surface descendant products MUST navigate the tree client-side and issue separate filtered queries per subcategory.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: A category MUST optionally reference exactly one other category as its parent. The reference MAY be empty, in which case the category is a root.
- **FR-002**: When a parent reference is supplied, the parent MUST exist within the same tenant AND share the same scope as the child (both global, or both belonging to the same store). Mixing scopes MUST be rejected.
- **FR-003**: The system MUST prevent cycles. A category MUST NOT be its own ancestor (directly or transitively). Attempting to make a category's parent equal to one of its descendants MUST be rejected.
- **FR-004**: The system MUST enforce a maximum nesting depth of 5 levels (root counts as depth 1). Creating or moving a category whose resulting depth would exceed 5 MUST be rejected.
- **FR-005**: A category's slug MUST be derived from its name AND the names of all its ancestors, joined into a single path. Each segment MUST follow the same normalization rules already used for single-level slugs (lowercase, accents removed, non-alphanumerics replaced with hyphens). Path segments are joined by a forward slash (`/`).
- **FR-006**: When a category is renamed, its own slug MUST recompute AND every descendant's slug MUST recompute to reflect the new ancestor chain.
- **FR-007**: When a category is moved under a different parent (or detached to root), its own slug AND every descendant's slug MUST recompute to reflect the new ancestor chain.
- **FR-008**: Slug uniqueness MUST be enforced across the entire tenant scope (global slugs unique among global; store slugs unique within each store). If a recompute or new entry would produce a duplicate slug, the operation MUST be rejected before any partial mutation is committed.
- **FR-009**: Within a single parent (including the implicit "root" parent), no two children MAY share the same name (after normalization), because that would necessarily collide on slug.
- **FR-010**: A category MUST NOT be deletable while it has at least one direct subcategory. The deletion attempt MUST return a clear error indicating subcategories must be removed first.
- **FR-011**: The system MUST expose a way to retrieve the entire category tree for the active scope (global tree in marketplace mode, store-scoped tree otherwise), where each node carries its direct children inline and each child carries its own children, recursively, to the deepest level. The exposed surface SHOULD be GraphQL.
- **FR-012**: The tree-listing surface MUST honor the same anonymous/authenticated visibility as the existing flat category listing for the same scope (public storefront browsing remains anonymous; admin management surfaces remain authenticated).
- **FR-013**: Each node returned by the tree-listing surface MUST carry, at minimum: identifier, name, slug (full path), parent identifier (or empty for roots), and the inline collection of its children. Children within each node MUST be returned in alphabetical order by name (case-insensitive, with accent normalization), and the same ordering MUST apply to the root collection.
- **FR-014**: Existing categories present in the system at the moment of rollout MUST behave as roots (no parent). Their slugs MUST remain unchanged so existing URLs do not break.
- **FR-015**: Creating, updating, deleting, and listing subcategories MUST honor the existing marketplace-vs-store-scoped mutex: in marketplace mode only the global surface accepts mutations; otherwise only the store-scoped surface accepts them. The new parent-reference field MUST be available on both surfaces.
- **FR-016**: The flat category listing MUST continue to work alongside the new tree-listing surface, so existing consumers do not break. Each item in the flat listing MUST also expose its parent identifier so flat consumers can reconstruct the tree if they want.
- **FR-017**: Validation errors caused by parent rules (cycle, scope mismatch, depth, missing parent, sibling-name collision) MUST be reported with a clear, actionable message identifying which rule was violated.
- **FR-018**: A category MAY hold products directly regardless of whether it also has subcategories (mixed mode). The system MUST NOT reject product assignment to a category because that category has children, NOR reject creating a subcategory because the parent already has products. The pre-existing rule that forbids deleting a category that still holds products continues to apply at any depth.
- **FR-019**: Product search and listing filtered by category MUST return only products whose `categoryId` matches the filter exactly (direct membership). The hierarchy MUST NOT cause descendant categories' products to be included in a parent-category filter. Existing product-search behavior is preserved unchanged. Transitive product browsing across a subtree is explicitly out of scope of this feature.

### Key Entities

- **Category**: Represents a grouping of products. Carries a unique identifier within its tenant, a display name, a slug (now a full ancestor path), an optional reference to another Category as its parent, and the existing scope marker (store-scoped vs global). A Category may have many direct subcategories (its children). The root of a hierarchy is any Category whose parent reference is empty.
- **Category Tree**: A read-only projection assembled from Category records. Each tree node is a Category enriched with the inline list of its direct children, each of which is itself a tree node. The root collection of the tree contains exactly the categories whose parent reference is empty within the queried scope.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An admin can create a three-level hierarchy (root → child → grandchild) and have all three slugs reflect the full path within a single working session, with no manual slug intervention required.
- **SC-002**: A consumer of the tree-listing surface receives the full hierarchy of any tenant with up to 500 categories spread across up to 5 levels in a single response, with no follow-up calls needed to render category navigation.
- **SC-003**: 100% of attempts to create a cycle, exceed the depth limit, mix scopes, or duplicate sibling names are rejected before any data is changed, with an error message naming the violated rule.
- **SC-004**: Renaming a non-leaf category propagates the new slug to 100% of its descendants atomically — either every descendant's slug is updated or none is, with no half-finished states.
- **SC-005**: After this feature ships, no existing URL tied to a pre-rollout category breaks, because pre-rollout categories continue to be treated as roots with their original single-segment slugs.
- **SC-006**: A storefront page that needs to render category navigation can do so with a single round-trip to the tree-listing surface, regardless of hierarchy depth (within the depth limit).

## Assumptions

- **Maximum depth**: The hierarchy is capped at 5 levels (root is level 1). This is generous enough for retail catalogs (Departamento → Categoria → Subcategoria → Sub-subcategoria → Detalhe) while preventing pathological nesting that would degrade tree responses.
- **Delete behaviour**: Deleting a category with subcategories is forbidden — admin must clear or move children first. This mirrors the existing rule that already forbids deleting a category that still holds products, keeps the tree consistent, and avoids surprise data loss.
- **Slug recompute on rename/move**: Renames and moves cascade to descendant slugs because anything else would leave outdated paths in the tree (e.g. "camisetas/vintage" still pointing at a child that has been moved under "calcas"). The cascade is treated as a single atomic operation; any uniqueness violation aborts the entire change.
- **Scope is preserved across the tree**: Every category in a chain shares the same scope marker (global vs a specific store). Mixing scopes within one chain would make tree assembly ambiguous and is therefore disallowed.
- **Tree-listing surface**: Exposed via the existing GraphQL endpoints, since the user explicitly preferred GraphQL. Public schema serves the storefront tree (anonymous, current scope rules); admin schema serves authenticated tenants for management UIs.
- **Existing data is preserved**: Pre-rollout categories migrate as roots with no parent; their slugs do not change. No URL that worked before this feature breaks afterward.
- **Scoping continues to follow the existing marketplace mutex**: Whichever side of the mutex is currently open (global vs store-scoped) is the one that owns the tree. The other side returns the same response shape it returned before — empty or 403 — for parity with the existing behaviour.
- **Tenant isolation**: The tree-listing surface returns categories belonging to the queried tenant only. No cross-tenant data appears under any circumstance.
