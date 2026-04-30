-- Migration: 20260430_AddProductTypes
-- Feature: 003-product-type-filters
-- Apply per tenant DB:
--   psql "$ConnectionString" -f 20260430_AddProductTypes.sql
--
-- Adds the Product Type classifier system: 6 new tables (lofn_product_types,
-- lofn_product_type_filters, lofn_product_type_filter_allowed_values,
-- lofn_product_type_customization_groups, lofn_product_type_customization_options,
-- lofn_product_filter_values) and a 0..1 FK from lofn_categories to lofn_product_types.
--
-- Idempotent: safe to re-run on the same tenant DB.
--
-- Naming note: the new entity "ProductType" is distinct from the legacy
-- lofn_products.product_type INT column (Physical/InfoProduct enum). They
-- coexist; this migration does not touch the legacy column.
--
-- Rollout: additive. Pre-existing categories get product_type_id = NULL
-- and continue functioning as before (no applied type, no filter schema,
-- no customizations). Vendor flow for non-typed categories is unchanged.

-- =========================================================================
-- 1. lofn_product_types — root classifier per tenant
-- =========================================================================
CREATE TABLE IF NOT EXISTS lofn_product_types (
    product_type_id BIGSERIAL PRIMARY KEY,
    name            VARCHAR(120) NOT NULL,
    description     VARCHAR(500) NULL,
    created_at      TIMESTAMP NOT NULL DEFAULT now(),
    updated_at      TIMESTAMP NOT NULL DEFAULT now()
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_lofn_product_types_name_unique
    ON lofn_product_types (lower(name));

-- =========================================================================
-- 2. lofn_product_type_filters — schema rows under a Type
-- =========================================================================
CREATE TABLE IF NOT EXISTS lofn_product_type_filters (
    filter_id        BIGSERIAL PRIMARY KEY,
    product_type_id  BIGINT NOT NULL,
    label            VARCHAR(120) NOT NULL,
    data_type        VARCHAR(20)  NOT NULL,
    is_required      BOOLEAN NOT NULL DEFAULT false,
    display_order    INT NOT NULL DEFAULT 0,
    created_at       TIMESTAMP NOT NULL DEFAULT now(),
    updated_at       TIMESTAMP NOT NULL DEFAULT now(),
    CONSTRAINT chk_lofn_product_type_filter_data_type
        CHECK (data_type IN ('text','integer','decimal','boolean','enum'))
);

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_lofn_product_type_filter_type') THEN
        ALTER TABLE lofn_product_type_filters
            ADD CONSTRAINT fk_lofn_product_type_filter_type
            FOREIGN KEY (product_type_id)
            REFERENCES lofn_product_types (product_type_id)
            ON DELETE CASCADE;
    END IF;
END $$;

CREATE UNIQUE INDEX IF NOT EXISTS ix_lofn_product_type_filter_label_unique
    ON lofn_product_type_filters (product_type_id, lower(label));

-- =========================================================================
-- 3. lofn_product_type_filter_allowed_values — enum values for filters
-- =========================================================================
CREATE TABLE IF NOT EXISTS lofn_product_type_filter_allowed_values (
    allowed_value_id BIGSERIAL PRIMARY KEY,
    filter_id        BIGINT NOT NULL,
    value            VARCHAR(120) NOT NULL,
    display_order    INT NOT NULL DEFAULT 0
);

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_lofn_product_type_filter_allowed_value_filter') THEN
        ALTER TABLE lofn_product_type_filter_allowed_values
            ADD CONSTRAINT fk_lofn_product_type_filter_allowed_value_filter
            FOREIGN KEY (filter_id)
            REFERENCES lofn_product_type_filters (filter_id)
            ON DELETE CASCADE;
    END IF;
END $$;

CREATE UNIQUE INDEX IF NOT EXISTS ix_lofn_product_type_filter_allowed_value_unique
    ON lofn_product_type_filter_allowed_values (filter_id, value);

-- =========================================================================
-- 4. lofn_product_type_customization_groups
-- =========================================================================
CREATE TABLE IF NOT EXISTS lofn_product_type_customization_groups (
    group_id         BIGSERIAL PRIMARY KEY,
    product_type_id  BIGINT NOT NULL,
    label            VARCHAR(120) NOT NULL,
    selection_mode   VARCHAR(10)  NOT NULL,
    is_required      BOOLEAN NOT NULL DEFAULT false,
    display_order    INT NOT NULL DEFAULT 0,
    created_at       TIMESTAMP NOT NULL DEFAULT now(),
    updated_at       TIMESTAMP NOT NULL DEFAULT now(),
    CONSTRAINT chk_lofn_product_type_customization_group_selection_mode
        CHECK (selection_mode IN ('single','multi'))
);

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_lofn_product_type_customization_group_type') THEN
        ALTER TABLE lofn_product_type_customization_groups
            ADD CONSTRAINT fk_lofn_product_type_customization_group_type
            FOREIGN KEY (product_type_id)
            REFERENCES lofn_product_types (product_type_id)
            ON DELETE CASCADE;
    END IF;
END $$;

CREATE UNIQUE INDEX IF NOT EXISTS ix_lofn_product_type_customization_group_label_unique
    ON lofn_product_type_customization_groups (product_type_id, lower(label));

-- =========================================================================
-- 5. lofn_product_type_customization_options
-- =========================================================================
CREATE TABLE IF NOT EXISTS lofn_product_type_customization_options (
    option_id          BIGSERIAL PRIMARY KEY,
    group_id           BIGINT NOT NULL,
    label              VARCHAR(120) NOT NULL,
    price_delta_cents  BIGINT NOT NULL DEFAULT 0,
    is_default         BOOLEAN NOT NULL DEFAULT false,
    display_order      INT NOT NULL DEFAULT 0,
    created_at         TIMESTAMP NOT NULL DEFAULT now(),
    updated_at         TIMESTAMP NOT NULL DEFAULT now()
);

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_lofn_product_type_customization_option_group') THEN
        ALTER TABLE lofn_product_type_customization_options
            ADD CONSTRAINT fk_lofn_product_type_customization_option_group
            FOREIGN KEY (group_id)
            REFERENCES lofn_product_type_customization_groups (group_id)
            ON DELETE CASCADE;
    END IF;
END $$;

CREATE UNIQUE INDEX IF NOT EXISTS ix_lofn_product_type_customization_option_label_unique
    ON lofn_product_type_customization_options (group_id, lower(label));

-- =========================================================================
-- 6. lofn_product_filter_values — instances per product
-- =========================================================================
CREATE TABLE IF NOT EXISTS lofn_product_filter_values (
    product_filter_value_id BIGSERIAL PRIMARY KEY,
    product_id              BIGINT NOT NULL,
    filter_id               BIGINT NOT NULL,
    value                   TEXT NOT NULL,
    created_at              TIMESTAMP NOT NULL DEFAULT now(),
    updated_at              TIMESTAMP NOT NULL DEFAULT now()
);

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_lofn_product_filter_value_product') THEN
        ALTER TABLE lofn_product_filter_values
            ADD CONSTRAINT fk_lofn_product_filter_value_product
            FOREIGN KEY (product_id)
            REFERENCES lofn_products (product_id)
            ON DELETE CASCADE;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_lofn_product_filter_value_filter') THEN
        ALTER TABLE lofn_product_filter_values
            ADD CONSTRAINT fk_lofn_product_filter_value_filter
            FOREIGN KEY (filter_id)
            REFERENCES lofn_product_type_filters (filter_id)
            ON DELETE CASCADE;
    END IF;
END $$;

CREATE UNIQUE INDEX IF NOT EXISTS ix_lofn_product_filter_value_product_filter_unique
    ON lofn_product_filter_values (product_id, filter_id);

CREATE INDEX IF NOT EXISTS ix_lofn_product_filter_value_filter_value
    ON lofn_product_filter_values (filter_id, value);

-- =========================================================================
-- 7. lofn_categories.product_type_id — 0..1 FK link
-- =========================================================================
ALTER TABLE lofn_categories
    ADD COLUMN IF NOT EXISTS product_type_id BIGINT NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'fk_lofn_categories_product_type') THEN
        ALTER TABLE lofn_categories
            ADD CONSTRAINT fk_lofn_categories_product_type
            FOREIGN KEY (product_type_id)
            REFERENCES lofn_product_types (product_type_id)
            ON DELETE SET NULL;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS ix_lofn_categories_product_type_id
    ON lofn_categories (product_type_id) WHERE product_type_id IS NOT NULL;
