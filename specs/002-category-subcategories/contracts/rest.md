# REST Contract — Category endpoints (additive changes)

**Feature**: 002-category-subcategories · **Phase 1 contract**

This feature does **not** add new REST endpoints. The existing endpoints are preserved verbatim with one additive change: their request and response payloads gain an optional `parentCategoryId` field. Controllers themselves require no code change — they already forward the DTOs untouched.

---

## Affected endpoints

### Store-scoped surface (`Marketplace=false`)

| Method | Route | Auth |
|--------|-------|------|
| `POST` | `/category/{storeSlug}/insert` | `[Authorize]` |
| `POST` | `/category/{storeSlug}/update` | `[Authorize]` |
| `DELETE` | `/category/{storeSlug}/delete/{categoryId}` | `[Authorize]` |

### Marketplace surface (`Marketplace=true`)

| Method | Route | Auth |
|--------|-------|------|
| `POST` | `/category-global/insert` | `[Authorize][MarketplaceAdmin]` |
| `POST` | `/category-global/update` | `[Authorize][MarketplaceAdmin]` |
| `DELETE` | `/category-global/delete/{categoryId}` | `[Authorize][MarketplaceAdmin]` |
| `GET` | `/category-global/list` | `[Authorize][MarketplaceAdmin]` |

The marketplace mutex from feature 001 is preserved exactly — calling the store surface in marketplace mode returns `403`, and vice versa.

---

## Payload changes

### `CategoryInsertInfo` (REST `POST /category/{storeSlug}/insert`)

```jsonc
{
    "name": "Camisetas",
    "parentCategoryId": 12   // OPTIONAL — null/omitted ⇒ root
}
```

### `CategoryUpdateInfo` (REST `POST /category/{storeSlug}/update`)

```jsonc
{
    "categoryId": 17,
    "name": "Camisetas",
    "parentCategoryId": 12   // OPTIONAL — null/omitted ⇒ detach to root; same id ⇒ no-op
}
```

### `CategoryGlobalInsertInfo` (REST `POST /category-global/insert`)

```jsonc
{
    "name": "Vestuário",
    "parentCategoryId": null    // OPTIONAL — global root by default
}
```

### `CategoryGlobalUpdateInfo` (REST `POST /category-global/update`)

```jsonc
{
    "categoryId": 17,
    "name": "Camisetas",
    "parentCategoryId": 12       // OPTIONAL
}
```

### `CategoryInfo` (response body of insert/update, items in `/category-global/list`)

```jsonc
{
    "categoryId": 17,
    "name": "Camisetas",
    "slug": "vestuario/camisetas",   // full ancestor path
    "storeId": null,
    "isGlobal": true,
    "parentCategoryId": 12,           // NEW — null for roots
    "productCount": 0
}
```

Existing consumers ignore the new field; new consumers can reconstruct the tree client-side from a flat list using `parentCategoryId` (FR-016).

---

## Validation errors

All four mutating endpoints return `400 Bad Request` with the existing FluentValidation error envelope when validation fails. The new error cases are:

| Error | Trigger | Message |
|-------|---------|---------|
| Parent not found | `parentCategoryId` references a non-existent or deleted category | `Parent category {id} not found` |
| Cross-scope nesting | Parent's scope doesn't match the request surface (e.g., global parent supplied to store-scoped endpoint) | `Parent and child must share the same scope` |
| Cycle | Resulting parent chain would include the category itself | `Setting parent {id} would create a cycle` |
| Depth exceeded | Resulting depth > 5 | `Maximum nesting depth (5) would be exceeded` |
| Sibling name collision | Another sibling under the same parent already has the same name (case-insensitive) | `A category named "{name}" already exists under this parent` |
| Has children (delete) | `DELETE` of a category that has at least one direct child | `Category {id} has subcategories; remove them first` |

The pre-existing rules (cannot delete category with products, etc.) continue to apply in addition.

---

## Behavioral notes

### Update without `parentCategoryId`

When the field is omitted (or sent as `null`) on update:
- If the category currently HAS a parent → it is detached to root (the move case).
- If the category currently HAS NO parent → no-op on parent.

To distinguish "leave alone" from "detach to root" using JSON-null only, callers MUST send `parentCategoryId: null` to detach (explicit) and OMIT the property entirely if they want to leave the parent unchanged. (System.Text.Json with default settings does treat null and missing as different when binding to nullable types — verify behaviour during implementation; if it conflates them, introduce an explicit "patch" semantics or a separate `detachFromParent: true` flag.) **Decision deferred to implementation: confirm via a test that omit ≠ null on the wire.** If the runtime collapses them, ship a `parentCategoryIdSpecified` shadow field or switch to a JSON Patch document.

### Slug recomputation

On every successful insert and update, the response carries the new full-path slug. Clients that cache by slug must invalidate after these calls.

### Delete behaviour

- Delete with subcategories → 400 (new) — `Category {id} has subcategories; remove them first`.
- Delete with products → 400 (existing).
- Otherwise → 204 No Content (existing).

---

## Status code reference

| Surface | Insert | Update | Delete | List |
|---------|--------|--------|--------|------|
| Store-scoped (non-marketplace) | 200 / 400 / 401 / 403 (in marketplace mode) / 404 | 200 / 400 / 401 / 403 / 404 | 204 / 400 / 401 / 403 / 404 | (no list endpoint) |
| Global (marketplace) | 200 / 400 / 401 / 403 (non-marketplace or non-admin) | 200 / 400 / 401 / 403 / 404 | 204 / 400 / 401 / 403 / 404 | 200 / 401 / 403 |

Anonymous calls to either surface continue to return 401 (existing behaviour).
