# GraphQL Contract — Category Tree

**Feature**: 002-category-subcategories · **Phase 1 contract** (output of `/speckit.plan`)

This document specifies the GraphQL surface added by this feature. Two new fields are introduced — one on the public schema, one on the admin schema — plus an additive change to the existing `Category` object type.

---

## Endpoints (unchanged)

- `POST /graphql` — public schema (anonymous)
- `POST /graphql/admin` — admin schema (Bearer token required)

---

## New: `Category.parentCategoryId` field

Adds an optional field to the existing `Category` type via `CategoryTypeExtension`. Returned wherever a `Category` is currently returned (`categories`, `myCategories`, `storeBySlug.products[].category` if exposed, etc.).

```graphql
extend type Category {
    "Identifier of this category's parent. Null when the category is a root."
    parentCategoryId: Long
}
```

Backwards compatibility: clients that do not request `parentCategoryId` see no change. Clients that request it on a pre-rollout category receive `null`.

---

## New: `Category.children` field (resolved recursively)

Allows traversing children inline from any `Category` node. Resolved by a field resolver that consults the per-request tree dictionary (built once per request — see Research §R2).

```graphql
extend type Category {
    "Direct subcategories of this category, alphabetically ordered by name. Empty when the category is a leaf."
    children: [Category!]!
}
```

The resolver signature in C#:

```csharp
public IList<Category> GetChildren(
    [Parent] Category parent,
    [Service] ICategoryTreeContext treeContext)
{
    return treeContext.GetChildrenOf(parent.CategoryId);
}
```

`ICategoryTreeContext` is a request-scoped service that lazily loads and memoizes the categories of the active scope on first access — preventing the N+1 query that would result from naive per-node DB fetches.

---

## New: `Query.categoryTree` (public schema)

```graphql
type Query {
    """
    Returns the full category tree for the active tenant scope.
    
    Behaviour by tenant mode:
    
    - Marketplace mode (Marketplace=true): returns the global category tree. The
      `storeSlug` argument is ignored.
    - Non-marketplace mode (Marketplace=false): returns the tree for the store
      identified by `storeSlug`. When `storeSlug` is null or no matching store
      exists, returns an empty array.
    
    Each node carries its direct children inline. Children are alphabetically
    ordered by name (case-insensitive, with accent normalization). The root
    collection follows the same ordering.
    """
    categoryTree(storeSlug: String): [Category!]!
}
```

### Resolver behaviour

1. Resolve scope: marketplace → `storeId = null`; otherwise → `storeId = storeBySlug(storeSlug).Id` or empty.
2. Load categories matching that scope into the request-scoped tree dictionary.
3. Return only the roots; HotChocolate then resolves `children` recursively from the same dictionary.

### Authorization

Anonymous (matches existing public flat `categories` query).

---

## New: `Query.myCategoryTree` (admin schema)

```graphql
type Query {
    """
    Returns the category tree visible to the authenticated admin.
    
    Behaviour by tenant mode:
    
    - Marketplace mode: returns the full global tree (same scope as the
      existing `myCategories` field in marketplace mode).
    - Non-marketplace mode: returns the union of trees for every store the
      authenticated user owns or co-manages (matching `myCategories`).
    
    Sorting and shape match the public `categoryTree` field.
    """
    myCategoryTree: [Category!]!
}
```

### Authorization

`@authorize` directive (matches the existing admin schema). The session user is read via `IUserClient.GetUserInSession(HttpContext)` to resolve owned store ids; in marketplace mode the session must additionally pass the existing global-category visibility rules.

---

## Sample queries

### Public, marketplace mode

```graphql
query {
    categoryTree {
        categoryId
        name
        slug
        isGlobal
        children {
            categoryId
            name
            slug
            children {
                categoryId
                name
                slug
            }
        }
    }
}
```

Sample response:

```json
{
    "data": {
        "categoryTree": [
            {
                "categoryId": 12,
                "name": "Vestuário",
                "slug": "vestuario",
                "isGlobal": true,
                "children": [
                    {
                        "categoryId": 18,
                        "name": "Calças",
                        "slug": "vestuario/calcas",
                        "children": []
                    },
                    {
                        "categoryId": 17,
                        "name": "Camisetas",
                        "slug": "vestuario/camisetas",
                        "children": [
                            {
                                "categoryId": 22,
                                "name": "Vintage",
                                "slug": "vestuario/camisetas/vintage"
                            }
                        ]
                    }
                ]
            }
        ]
    }
}
```

Note the alphabetical order at every level: `Calças` appears before `Camisetas` (accent-aware lower-case comparison).

### Public, non-marketplace mode

```graphql
query {
    categoryTree(storeSlug: "circulou") {
        categoryId
        name
        slug
        children { categoryId name slug }
    }
}
```

When the tenant has `Marketplace=false` and the store `circulou` exists with three root categories, the response returns those three roots with their subcategories nested.

### Admin, marketplace mode

```graphql
query {
    myCategoryTree {
        categoryId
        name
        slug
        parentCategoryId
        children {
            categoryId
            name
            slug
            parentCategoryId
        }
    }
}
```

`parentCategoryId` is null for roots and equal to the parent's `categoryId` for nested nodes.

---

## Errors

| Condition | GraphQL error code | Notes |
|-----------|--------------------|-------|
| Anonymous user calls `myCategoryTree` | `AUTH_NOT_AUTHENTICATED` | Returned by HotChocolate `@authorize` |
| `storeSlug` references a non-existent store in non-marketplace mode | none — empty array | Treated as "no tree to show" |
| Tree assembly internally exceeds depth 5 due to corrupted data | none — server returns up to depth 5, dropping deeper nodes silently | This case can only happen if migration safeguards fail; logged via `GraphQLErrorLogger` |

---

## Pagination

Not applicable — the contract returns the entire tree in a single response (per SC-002, SC-006, FR-011). Pagination becomes relevant only if a tenant exceeds 500 categories; in that case it is added in a follow-up feature without breaking the current schema (HotChocolate `[UseOffsetPaging]` could be retrofitted on `categoryTree`).

---

## Schema versioning

This is an additive change — no version bump required. Existing queries continue to compile and execute unchanged.
