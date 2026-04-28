# REST contract — `CategoryController` (existing) — behavioural deltas

**Path prefix**: `/category`
**Authentication**: NAuth Bearer (unchanged)

This document describes only what *changes* in the existing `Lofn.API.Controllers.CategoryController`. Endpoints not listed below behave exactly as today.

## Mode-dependent gate (applied to every action)

A mode check runs at the start of each write action. Pseudo-code:

```csharp
if (_tenantResolver.Marketplace)
    return Forbid();   // returns 403
```

This produces `403 Forbidden` on the following actions when the tenant is in marketplace mode:

| Endpoint | Status in non-marketplace tenant | Status in marketplace tenant |
|---|---|---|
| `POST /category/{storeSlug}/insert` | unchanged | **`403 Forbidden`** |
| `POST /category/{storeSlug}/update` | unchanged | **`403 Forbidden`** |
| `DELETE /category/{storeSlug}/delete/{categoryId}` | unchanged | **`403 Forbidden`** |

The body of the 403 response is empty (consistent with `Forbid()` default).

## Read paths

The current `CategoryController` doesn't expose a list endpoint — listing happens via GraphQL. There is therefore nothing to change at REST for reads.

## Backward compatibility

Tenants with `Marketplace = false` (or with the key absent) see no change at all. The existing test `CategoryControllerTests.Insert_WithAuth_ShouldNotReturn401` continues to pass; a new test asserts `403` when run against a marketplace tenant.
