# Feature Specification: Marketplace category mode per tenant

**Feature Branch**: `001-marketplace-categories`
**Created**: 2026-04-28
**Status**: Draft
**Input**: User description: "- O produto deverá ter uma categoria para a loja específica ou um geral que funcionará para todas as lojas / - Isso deverá ser uma opção definida por tenant, chamada Marketplace / - Se o Tenant for Marketplace = True, as categorias serão fixas (cadastradas antecipadamente por um usuário com IsAdmin = True) / - Se Marketplace = False, funcionará exatamente como funciona hoje, o admin da loja pode cadastrar suas categorias"

## Clarifications

### Session 2026-04-28

- Q: Numa tenant com `Marketplace = true` e produtos pré-existentes apontando para categorias store-scoped legadas, o que o sistema deve permitir? → A: Ao virar `true`, todos os produtos com categoria legada têm o vínculo zerado (`null`) automaticamente. Categorias legadas viram dados "soltos".
- Q: Qual é a superfície de API para o platform admin operar o catálogo global numa tenant Marketplace? → A: Novos endpoints dedicados, sem `storeSlug` na rota, exclusivos para gerenciar categorias globais; autorização exige `IsAdmin=true` e `tenant.Marketplace=true`.
- Q: Em que camada a flag `Marketplace` é persistida e alterada? → A: Em `appsettings.json`, no mesmo bloco `Tenants:{slug}` que já abriga `ConnectionString`, `JwtSecret` e `BucketName`. Alteração requer deploy/restart e o audit é feito via histórico do controle de versão do arquivo de configuração.
- Q: Qual é a regra de unicidade do `Slug` de uma categoria dentro de uma tenant? → A: Unicidade por tenant — nenhum slug de categoria pode se repetir dentro da mesma tenant, independente do escopo (global ou store-scoped).
- Q: O que dispara a aplicação automática do zeramento de `CategoryId` quando a flag `Marketplace` passa de `false` para `true`? → A: Migração é responsabilidade do admin. O sistema não executa nenhum `UPDATE` automático ao flip da flag; se houver necessidade de zerar `CategoryId` ou qualquer outro ajuste, é feito fora do sistema, manualmente pelo administrador. (Refina Q1: o estado-alvo segue sendo `CategoryId = null` para produtos com categorias legadas, mas a transformação não é executada pela aplicação.)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Tenant administrator curates a fixed catalog of categories for a marketplace (Priority: P1)

A platform administrator (a user with system-wide `IsAdmin = true`) configures a tenant as a marketplace and curates the canonical list of categories that every store within that tenant will use. From that point on, sellers can only attach products to the categories the platform administrator has defined — keeping the marketplace's catalog consistent across stores.

**Why this priority**: This is the core value proposition. Without an administrator being able to define the global catalog and lock store admins out of category management, the whole marketplace mode has no behaviour to enforce. It is the smallest end-to-end slice that proves the feature works.

**Independent Test**: Configure a fresh tenant with `Marketplace = true`, sign in as a platform administrator, create three categories, list them — they appear as the tenant's global catalog. Sign in as a store administrator of any store inside that tenant, attempt to create a category, the action is rejected; attempt to list categories, the same three created by the platform administrator appear.

**Acceptance Scenarios**:

1. **Given** a tenant configured with `Marketplace = true` and no categories yet, **When** a platform administrator creates a category named "Eletrônicos", **Then** the category is persisted as a tenant-global category and becomes visible to every store in the tenant.
2. **Given** a tenant configured with `Marketplace = true` and three global categories, **When** a store administrator (without `IsAdmin = true`) attempts to create a new category, **Then** the request is rejected with an authorization error and no category is created.
3. **Given** a tenant configured with `Marketplace = true` and three global categories, **When** any store administrator lists categories available for products in their store, **Then** the response contains the three tenant-global categories (and no others).
4. **Given** a tenant configured with `Marketplace = true`, **When** a platform administrator updates or deletes a global category, **Then** the change is immediately visible to every store in the tenant, and any product previously associated with a deleted category is left without a category until reassigned.

---

### User Story 2 - Store administrator selects a global category when registering a product (Priority: P1)

In a marketplace tenant, when a store administrator creates or edits a product, they choose its category from the platform's global catalog rather than creating a new ad-hoc category. This keeps every product on the marketplace consistently classified.

**Why this priority**: Categories on their own provide no value — they exist to classify products. Without product registration honouring the tenant's mode, the curated catalog from User Story 1 is unused.

**Independent Test**: In a marketplace tenant with at least one global category, a store administrator creates a product and assigns it that category — the product is saved and, when listed, shows that category. The same store administrator attempts to assign a category that does not belong to the global catalog of their tenant — the operation fails.

**Acceptance Scenarios**:

1. **Given** a tenant with `Marketplace = true` and a global category "Periféricos", **When** a store administrator creates a product and assigns it to "Periféricos", **Then** the product is persisted with that category.
2. **Given** a tenant with `Marketplace = true`, **When** a store administrator attempts to create a product with a category that does not exist in the tenant's global catalog, **Then** the operation is rejected with a validation error explaining that only global categories are allowed.
3. **Given** a tenant with `Marketplace = true` and a product associated with a global category, **When** the platform administrator renames that global category, **Then** every product associated with it reflects the new name without further action by store administrators.

---

### User Story 3 - Non-marketplace tenants keep today's per-store category management (Priority: P1)

A tenant configured with `Marketplace = false` (the default for any tenant that does not explicitly opt in) keeps the current behaviour: each store administrator owns their store's category list, can create, update and delete categories scoped to that store, and assigns those categories when registering products.

**Why this priority**: Backward compatibility for every tenant currently using the system. Breaking the existing flow when shipping the new mode is unacceptable, so this story is also P1 — it must ship together with the marketplace mode.

**Independent Test**: A tenant with `Marketplace = false` (or with the flag absent) behaves identically to today: a store administrator creates a category in their store, lists categories — only their store's categories appear; categories from another store within the same tenant are not visible.

**Acceptance Scenarios**:

1. **Given** a tenant with `Marketplace = false`, **When** a store administrator creates a category in their store, **Then** the category is persisted scoped to that store and is invisible to other stores in the same tenant.
2. **Given** a tenant with `Marketplace = false` and two stores A and B with their own categories, **When** an administrator of store A lists categories, **Then** only the categories of store A are returned.
3. **Given** a tenant with `Marketplace = false` (or with the flag never configured), **When** any operation involving categories is performed, **Then** the system behaves exactly as it does today, with no change to authorization or visibility.

---

### Edge Cases

- A tenant has its `Marketplace` flag flipped from `false` to `true` after stores have already created their own store-scoped categories with products attached. The application performs no automatic rewrite of data; pre-existing category records and product associations remain in the database exactly as they were. The desired end state — products that referenced legacy store-scoped categories no longer pointing at them (`CategoryId = null`) — is achieved by an administrator running a migration outside the application (for example, a SQL script executed against the tenant database) at a time of their choice. From that point on no new store-scoped categories can be created and the tenant-global catalog starts empty until the platform administrator populates it; once the admin has nulled the legacy associations, store administrators can reassign products to any global category.
- A tenant has its `Marketplace` flag flipped from `true` to `false`. Tenant-global categories created during marketplace mode are kept and remain visible (read-only) to every store; from this point on, store administrators may also create their own store-scoped categories.
- A platform administrator deletes a global category that has products associated with it. The deletion succeeds, but every product previously associated with that category is left without a category; the system warns the administrator before deletion.
- A user with `IsAdmin = true` who is not associated with any specific store attempts to manage categories in a non-marketplace tenant. The action is allowed (platform admins can manage any category in any tenant) — `IsAdmin` is the highest authority.
- A request without authentication is made to any category management operation. The request is rejected with the existing `401 Unauthorized` behaviour, regardless of marketplace mode.
- A tenant has the `Marketplace` flag set but its value is malformed or unreadable. The system treats it as `false` (safest default — preserves today's behaviour) and logs a warning for the operator.
- An attempt to create or rename a category produces a slug that already exists for any other category in the same tenant (whether global or in another store). The operation is rejected with a validation error naming the conflicting category, and no record is written.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST expose a per-tenant boolean configuration named `Marketplace`, defaulting to `false` for any tenant that does not explicitly opt in.
- **FR-002**: When a tenant has `Marketplace = true`, only users with `IsAdmin = true` MUST be allowed to create, update or delete categories within that tenant; any other user MUST receive an authorization error.
- **FR-003**: When a tenant has `Marketplace = true`, categories MUST be scoped to the tenant (visible and usable across every store within the tenant) rather than to a single store.
- **FR-004**: When a tenant has `Marketplace = false`, store administrators MUST be able to create, update and delete categories scoped to their own store, exactly as today.
- **FR-005**: When a tenant has `Marketplace = false`, the categories visible and usable for product registration in a given store MUST be only that store's own categories.
- **FR-006**: When a product is created or updated in a tenant with `Marketplace = true`, the system MUST accept only categories that exist in that tenant's global catalog.
- **FR-007**: When a product is created or updated in a tenant with `Marketplace = false`, the system MUST accept only categories that belong to the same store as the product.
- **FR-008**: System MUST persist the `Marketplace` flag in the same per-tenant configuration block already used for `ConnectionString`, `JwtSecret` and `BucketName` (i.e. `Tenants:{tenantId}:Marketplace` in the application configuration). The value MUST survive restarts and MUST be consistently applied across all subsequent requests for that tenant once the application has loaded the configuration.
- **FR-009**: System MUST be able to identify, for any category record, whether it is tenant-global or store-scoped, so that listing and authorization rules can be enforced unambiguously.
- **FR-010**: When the `Marketplace` flag of a tenant changes (in either direction), the application MUST NOT perform any automatic rewrite of existing data — no `UPDATE` over categories or products is triggered by the flag flip. Bringing the data into the desired post-flip state (for example, setting `CategoryId = null` on products that referenced legacy store-scoped categories before the tenant became a marketplace) is the responsibility of an administrator and is performed outside the application's request flow.
- **FR-011**: When a global category is deleted in a marketplace tenant, products that referenced it MUST be left without a category (rather than blocking the deletion); the system MUST surface a warning to the administrator before confirming the deletion.
- **FR-012**: Because the `Marketplace` flag lives in version-controlled configuration, the audit trail (who changed it, when, and from which value to which value) MUST be retrievable from the configuration repository's commit history; no additional in-application audit table is required for this flag.
- **FR-013**: System MUST expose a dedicated API surface for managing tenant-global categories — distinct from the existing store-scoped category endpoints — that does not include a store identifier in the path. Authorization on this surface MUST require both `IsAdmin = true` on the caller and `Marketplace = true` on the resolved tenant; a request that does not satisfy both conditions MUST be rejected.
- **FR-014**: When a tenant has `Marketplace = true`, the existing store-scoped category management surface (the per-store create/update/delete endpoints) MUST reject any write operation. Read operations on the store-scoped surface MAY continue to work and MUST return only the tenant-global catalog when invoked in a Marketplace tenant.
- **FR-015**: System MUST enforce slug uniqueness across every category within the same tenant, regardless of scope: no two categories belonging to the same tenant — store-scoped or tenant-global — may share the same `Slug`. Pre-existing data that already violates this rule (legacy categories from different stores within one tenant sharing a slug) MUST be grandfathered (no destructive migration); the rule applies only to inserts and slug-changing updates performed after this feature ships.

### Key Entities *(include if feature involves data)*

- **Tenant**: Represents a customer of the platform. Carries a new `Marketplace` boolean property that determines how categories are scoped within it. All other tenant attributes (connection string, JWT secret, bucket name, default tenant id) are unchanged.
- **Category**: Represents a product classification. Gains a notion of *scope*: either store-scoped (the category belongs to exactly one store) or tenant-global (the category belongs to the tenant and is visible to every store in it). Existing categories created before the feature are store-scoped.
- **Product**: Continues to reference a single category. The set of categories that a product may reference depends on the `Marketplace` flag of the tenant that owns its store.
- **User**: The existing user entity. The `IsAdmin` boolean (already provided by the authentication system) is the only attribute that gates global category management; no new user role is introduced.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A platform administrator can switch a tenant into marketplace mode and publish its first ten global categories within five minutes, end-to-end, without contacting engineering.
- **SC-002**: In a marketplace tenant, every product registered after the feature ships references exactly one category that exists in the tenant's global catalog (target: 100% of newly registered products).
- **SC-003**: After the feature is enabled, zero non-admin users succeed in creating, updating or deleting a category in any marketplace tenant (verified continuously by the audit log).
- **SC-004**: Tenants that do not opt into marketplace mode see no observable change in category management behaviour, measured by the absence of regressions in existing automated coverage and zero new support tickets attributed to the feature in the first thirty days.
- **SC-005**: Listing the categories available to a store loads in under one second at the 95th percentile, regardless of marketplace mode and regardless of how many stores share the tenant.
- **SC-006**: A change to the `Marketplace` value of a tenant takes effect on the first request handled by the application instance after it has been deployed/restarted with the updated configuration; no manual cache invalidation step is required by an operator.

## Assumptions

- The user input names the flag `MarketingPlace`. We assume this is a typo and the intended canonical name is `Marketplace` — used throughout this specification. If the literal name `MarketingPlace` is required, this is a naming-only change and does not affect any of the requirements above.
- A user with `IsAdmin = true` (as already provided by the existing authentication system) is considered the platform-wide administrator and is the only role authorised to manage categories in a marketplace tenant. No new role or permission system is introduced by this feature.
- The default value for `Marketplace` on every existing and newly created tenant is `false`, preserving today's behaviour for anyone who does not opt in.
- Categories created before this feature shipped are treated as store-scoped. The category records themselves are not migrated when a tenant switches to marketplace mode, but products that referenced them have their `CategoryId` zeroed (see FR-010).
- Products that are left without a category as a side effect of a global-category deletion stay valid (the category association is optional, as it already is today). They simply appear without a category until reassigned.
- The tenant resolution mechanism (header `X-Tenant-Id`, `TenantResolver`, multi-tenant configuration) remains the source of truth for which tenant a request belongs to. The `Marketplace` flag is read from the same per-tenant configuration block.
- This feature does not change the public storefront experience for end customers (anonymous shoppers): they keep seeing categories scoped to the store they are browsing. Only the management interface and product creation/update flow change.
- This feature does not introduce a new way to manage tenant configuration. Toggling `Marketplace` for a tenant is a configuration change committed to the version-controlled `appsettings` and applied through the standard deploy pipeline — there is no in-app endpoint or admin UI for toggling it.
