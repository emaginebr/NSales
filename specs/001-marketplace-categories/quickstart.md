# Quickstart — Marketplace category mode per tenant

**Feature**: `001-marketplace-categories`
**Audience**: developer wanting to verify the feature end-to-end on a local Docker Compose environment.
**Pre-req**: feature branch checked out and built, Docker Desktop running, PostgreSQL 17 reachable as configured by `docker-compose.yml`.

This is the manual smoke-test recipe. Automated coverage lives in `Lofn.Tests` (unit) and `Lofn.ApiTests` (integration).

## 1 — Bring the stack up

```bash
docker compose up -d
```

API at `http://localhost:5000`, Postgres at `5432` (per `.env`).

## 2 — Apply the EF migration on the tenant DB

```bash
dotnet ef database update \
    --project Lofn.Infra \
    --startup-project Lofn.API \
    --connection "Host=localhost;Port=5432;Database=lofn_db;Username=lofn_user;Password=pikpro6"
```

Verify the new index:

```sql
SELECT indexname FROM pg_indexes
WHERE tablename = 'lofn_categories' AND indexname = 'ix_lofn_categories_slug_global';
-- expect: 1 row
```

> If the index creation fails because two existing rows have `store_id IS NULL` and the same `slug`, deduplicate first (`UPDATE lofn_categories SET slug = slug || '-' || category_id WHERE ...`) or drop the unique constraint and re-add as non-unique. The data layout in this codebase shouldn't have that case in practice.

## 3 — Toggle `Marketplace` for a tenant

Edit the development config:

```jsonc
// Lofn.API/appsettings.Development.json
"Tenants": {
  "emagine": {
    "ConnectionString": "...",
    "JwtSecret": "...",
    "BucketName": "lofn",
    "Marketplace": true   // <-- add this line
  }
}
```

Or, using environment variable (Docker), set `Tenants__emagine__Marketplace=true` and `docker compose up -d --force-recreate api`.

## 4 — Login as a platform admin

The user provided to the tests (`rodrigo@emagine.com.br`) already has `IsAdmin = true` per the seed data. Login:

```bash
curl -X POST https://emagine.com.br/auth-api/user/loginWithEmail \
  -H "X-Tenant-Id: emagine" \
  -H "X-Device-Fingerprint: dev" \
  -H "User-Agent: quickstart/1.0" \
  -H "Content-Type: application/json" \
  -d '{"email":"rodrigo@emagine.com.br","password":"<your-password>"}'
```

Save the returned `token` as `$TOKEN`.

## 5 — Create a global category

```bash
curl -X POST http://localhost:5000/category-global/insert \
  -H "Authorization: Bearer $TOKEN" \
  -H "X-Tenant-Id: emagine" \
  -H "X-Device-Fingerprint: dev" \
  -H "User-Agent: quickstart/1.0" \
  -H "Content-Type: application/json" \
  -d '{"name":"Periféricos"}'
```

Expected: `200 OK` with `{ "categoryId": ..., "slug": "perifericos", "name": "Periféricos", "storeId": null, "isGlobal": true, ... }`.

## 6 — Verify the legacy store-scoped surface is locked

```bash
curl -i -X POST http://localhost:5000/category/loja-de-informatica/insert \
  -H "Authorization: Bearer $TOKEN" \
  -H "X-Tenant-Id: emagine" \
  -H "X-Device-Fingerprint: dev" \
  -H "User-Agent: quickstart/1.0" \
  -H "Content-Type: application/json" \
  -d '{"name":"Won'\''t go through"}'
```

Expected: `403 Forbidden`.

## 7 — Create a product picking the global category

```bash
GLOBAL_CATEGORY_ID=$(curl -s http://localhost:5000/category-global/list \
  -H "Authorization: Bearer $TOKEN" -H "X-Tenant-Id: emagine" \
  -H "X-Device-Fingerprint: dev" -H "User-Agent: quickstart/1.0" | jq '.[0].categoryId')

curl -X POST http://localhost:5000/product/loja-de-informatica/insert \
  -H "Authorization: Bearer $TOKEN" -H "X-Tenant-Id: emagine" \
  -H "X-Device-Fingerprint: dev" -H "User-Agent: quickstart/1.0" \
  -H "Content-Type: application/json" \
  -d "{\"categoryId\": $GLOBAL_CATEGORY_ID, \"name\": \"MX Master 3\", \"description\": \"Mouse\", \"price\": 599.90, \"discount\": 0, \"frequency\": 0, \"limit\": 0, \"status\": 1, \"productType\": 1, \"featured\": false}"
```

Expected: `200 OK`, the product is persisted with the global `categoryId`.

## 8 — Try to assign a non-global category — expect rejection

```bash
# ProductId of any category that exists with non-null store_id
LEGACY_CATEGORY_ID=$(psql -U lofn_user -d lofn_db -tAc \
  "SELECT category_id FROM lofn_categories WHERE store_id IS NOT NULL LIMIT 1")

curl -i -X POST http://localhost:5000/product/loja-de-informatica/insert \
  -H "Authorization: Bearer $TOKEN" -H "X-Tenant-Id: emagine" \
  -H "X-Device-Fingerprint: dev" -H "User-Agent: quickstart/1.0" \
  -H "Content-Type: application/json" \
  -d "{\"categoryId\": $LEGACY_CATEGORY_ID, \"name\": \"Bad\", \"description\": \"\", \"price\": 1, \"discount\": 0, \"frequency\": 0, \"limit\": 0, \"status\": 1, \"productType\": 1, \"featured\": false}"
```

Expected: `400 Bad Request` with `{ "success": false, "errors": ["CategoryId must reference a tenant-global category in marketplace mode"] }`.

## 9 — Verify the public GraphQL exposes the new field

```bash
curl -X POST http://localhost:5000/graphql \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: emagine" \
  -H "X-Device-Fingerprint: dev" -H "User-Agent: quickstart/1.0" \
  -d '{"query":"{ categories(skip:0, take:5) { items { categoryId name slug isGlobal } } }"}'
```

Expected: every item has `"isGlobal": true` in marketplace mode.

## 10 — Roll the flag back

Set `Tenants__emagine__Marketplace=false` (or remove the key), restart the API, repeat steps 6 and 7 — both succeed exactly as today's behaviour. Steps 5 and the global-list endpoint now return `403 Forbidden` because the gate fails on the tenant condition.

## Cleanup

```bash
docker compose down
```

The test suite will recreate any data needed.
