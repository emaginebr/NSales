-- Migration: 20260428_AddGlobalCategoryUniqueIndex
-- Feature: 001-marketplace-categories (FR-015, R6, R9)
-- Apply per tenant DB:
--   psql "$ConnectionString" -f 20260428_AddGlobalCategoryUniqueIndex.sql
--
-- Creates a partial unique index enforcing tenant-wide uniqueness of
-- `slug` for tenant-global categories (rows with store_id IS NULL).
-- Per-store uniqueness (rows with store_id IS NOT NULL) is enforced by
-- `CategoryService.GenerateSlugAsync` at the service layer, unchanged.
--
-- Pre-existing rows are NOT migrated. If two existing rows have
-- store_id IS NULL and the same slug, this DDL will FAIL — operator must
-- deduplicate first or fall back to a non-unique index.

CREATE UNIQUE INDEX IF NOT EXISTS ix_lofn_categories_slug_global
    ON lofn_categories (slug)
    WHERE store_id IS NULL;
