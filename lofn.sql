-- Lofn Database Creation Script
-- PostgreSQL
-- Generated from EF Core Code First model

CREATE TABLE lofn_stores (
    store_id BIGSERIAL NOT NULL,
    slug VARCHAR(120) NOT NULL,
    name VARCHAR(120) NOT NULL,
    owner_id BIGINT NOT NULL,
    logo VARCHAR(150),
    status INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT lofn_stores_pkey PRIMARY KEY (store_id)
);

CREATE UNIQUE INDEX ix_lofn_stores_slug ON lofn_stores (slug);

CREATE TABLE lofn_store_users (
    store_user_id BIGSERIAL NOT NULL,
    store_id BIGINT NOT NULL,
    user_id BIGINT NOT NULL,
    CONSTRAINT lofn_store_users_pkey PRIMARY KEY (store_user_id),
    CONSTRAINT fk_lofn_store_user_store FOREIGN KEY (store_id) REFERENCES lofn_stores (store_id) ON DELETE CASCADE
);

-- Product Type classifier (003-product-type-filters).
-- Tenant-scoped. Admin-only management. Hard delete cascades into filters/groups/options.
-- Linked from lofn_categories.product_type_id (0..1, ON DELETE SET NULL).
CREATE TABLE lofn_product_types (
    product_type_id BIGSERIAL NOT NULL,
    name VARCHAR(120) NOT NULL,
    description VARCHAR(500),
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now(),
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now(),
    CONSTRAINT lofn_product_types_pkey PRIMARY KEY (product_type_id)
);

CREATE UNIQUE INDEX ix_lofn_product_types_name_unique
    ON lofn_product_types (lower(name));

CREATE TABLE lofn_product_type_filters (
    filter_id BIGSERIAL NOT NULL,
    product_type_id BIGINT NOT NULL,
    label VARCHAR(120) NOT NULL,
    data_type VARCHAR(20) NOT NULL,
    is_required BOOLEAN NOT NULL DEFAULT FALSE,
    display_order INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now(),
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now(),
    CONSTRAINT lofn_product_type_filters_pkey PRIMARY KEY (filter_id),
    CONSTRAINT fk_lofn_product_type_filter_type FOREIGN KEY (product_type_id) REFERENCES lofn_product_types (product_type_id) ON DELETE CASCADE,
    CONSTRAINT chk_lofn_product_type_filter_data_type CHECK (data_type IN ('text','integer','decimal','boolean','enum'))
);

CREATE UNIQUE INDEX ix_lofn_product_type_filter_label_unique
    ON lofn_product_type_filters (product_type_id, lower(label));

CREATE TABLE lofn_product_type_filter_allowed_values (
    allowed_value_id BIGSERIAL NOT NULL,
    filter_id BIGINT NOT NULL,
    value VARCHAR(120) NOT NULL,
    display_order INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT lofn_product_type_filter_allowed_values_pkey PRIMARY KEY (allowed_value_id),
    CONSTRAINT fk_lofn_product_type_filter_allowed_value_filter FOREIGN KEY (filter_id) REFERENCES lofn_product_type_filters (filter_id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX ix_lofn_product_type_filter_allowed_value_unique
    ON lofn_product_type_filter_allowed_values (filter_id, value);

CREATE TABLE lofn_product_type_customization_groups (
    group_id BIGSERIAL NOT NULL,
    product_type_id BIGINT NOT NULL,
    label VARCHAR(120) NOT NULL,
    selection_mode VARCHAR(10) NOT NULL,
    is_required BOOLEAN NOT NULL DEFAULT FALSE,
    display_order INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now(),
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now(),
    CONSTRAINT lofn_product_type_customization_groups_pkey PRIMARY KEY (group_id),
    CONSTRAINT fk_lofn_product_type_customization_group_type FOREIGN KEY (product_type_id) REFERENCES lofn_product_types (product_type_id) ON DELETE CASCADE,
    CONSTRAINT chk_lofn_product_type_customization_group_selection_mode CHECK (selection_mode IN ('single','multi'))
);

CREATE UNIQUE INDEX ix_lofn_product_type_customization_group_label_unique
    ON lofn_product_type_customization_groups (product_type_id, lower(label));

CREATE TABLE lofn_product_type_customization_options (
    option_id BIGSERIAL NOT NULL,
    group_id BIGINT NOT NULL,
    label VARCHAR(120) NOT NULL,
    price_delta_cents BIGINT NOT NULL DEFAULT 0,
    is_default BOOLEAN NOT NULL DEFAULT FALSE,
    display_order INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now(),
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now(),
    CONSTRAINT lofn_product_type_customization_options_pkey PRIMARY KEY (option_id),
    CONSTRAINT fk_lofn_product_type_customization_option_group FOREIGN KEY (group_id) REFERENCES lofn_product_type_customization_groups (group_id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX ix_lofn_product_type_customization_option_label_unique
    ON lofn_product_type_customization_options (group_id, lower(label));

CREATE TABLE lofn_categories (
    category_id BIGSERIAL NOT NULL,
    slug VARCHAR(512) NOT NULL,
    name VARCHAR(120) NOT NULL,
    store_id BIGINT,
    parent_id BIGINT,
    product_type_id BIGINT,
    CONSTRAINT lofn_categories_pkey PRIMARY KEY (category_id),
    CONSTRAINT fk_lofn_category_store FOREIGN KEY (store_id) REFERENCES lofn_stores (store_id),
    CONSTRAINT fk_lofn_category_parent FOREIGN KEY (parent_id) REFERENCES lofn_categories (category_id) ON DELETE RESTRICT,
    CONSTRAINT fk_lofn_categories_product_type FOREIGN KEY (product_type_id) REFERENCES lofn_product_types (product_type_id) ON DELETE SET NULL
);

CREATE INDEX ix_lofn_categories_product_type_id
    ON lofn_categories (product_type_id) WHERE product_type_id IS NOT NULL;

-- Tenant-global category slug uniqueness (FR-015 / 001-marketplace-categories).
-- Enforces that no two tenant-global categories (store_id IS NULL) share the same slug.
-- Per-store slug uniqueness is enforced at the service layer (CategoryService.GenerateSlugAsync).
CREATE UNIQUE INDEX ix_lofn_categories_slug_global
    ON lofn_categories (slug)
    WHERE store_id IS NULL;

-- Sibling-name uniqueness (002-category-subcategories / FR-009).
-- Composite expression index that treats NULL parent_id and NULL store_id as
-- sentinel zeros so that two stores can each have a root with the same name,
-- but a single store cannot have two siblings sharing a name (case-insensitive).
CREATE UNIQUE INDEX ix_lofn_categories_sibling_name_unique
    ON lofn_categories ((COALESCE(parent_id, 0)), (COALESCE(store_id, 0)), lower(name));

-- Helper index for cascade descendant walks (parent_id is queried frequently).
CREATE INDEX ix_lofn_categories_parent_id
    ON lofn_categories (parent_id) WHERE parent_id IS NOT NULL;

CREATE TABLE lofn_products (
    product_id BIGSERIAL NOT NULL,
    user_id BIGINT NOT NULL,
    slug VARCHAR(120) NOT NULL,
    name VARCHAR(120) NOT NULL,
    price DOUBLE PRECISION NOT NULL,
    discount DOUBLE PRECISION NOT NULL DEFAULT 0,
    frequency INTEGER NOT NULL,
    "limit" INTEGER NOT NULL,
    status INTEGER NOT NULL,
    product_type INTEGER NOT NULL DEFAULT 1,
    description TEXT,
    image VARCHAR(150),
    store_id BIGINT,
    category_id BIGINT,
    featured BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    CONSTRAINT lofn_products_pkey PRIMARY KEY (product_id),
    CONSTRAINT fk_lofn_product_store FOREIGN KEY (store_id) REFERENCES lofn_stores (store_id),
    CONSTRAINT fk_lofn_product_category FOREIGN KEY (category_id) REFERENCES lofn_categories (category_id)
);

CREATE TABLE lofn_product_images (
    image_id BIGSERIAL NOT NULL,
    product_id BIGINT NOT NULL,
    image VARCHAR(150),
    sort_order INTEGER NOT NULL DEFAULT 0,
    CONSTRAINT lofn_product_images_pkey PRIMARY KEY (image_id),
    CONSTRAINT fk_lofn_product_image_product FOREIGN KEY (product_id) REFERENCES lofn_products (product_id) ON DELETE CASCADE
);

-- Product filter values (003-product-type-filters).
-- Polymorphic value column interpreted by lofn_product_type_filters.data_type.
-- Cascade delete from product OR from filter cleans up orphaned rows.
CREATE TABLE lofn_product_filter_values (
    product_filter_value_id BIGSERIAL NOT NULL,
    product_id BIGINT NOT NULL,
    filter_id BIGINT NOT NULL,
    value TEXT NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now(),
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT now(),
    CONSTRAINT lofn_product_filter_values_pkey PRIMARY KEY (product_filter_value_id),
    CONSTRAINT fk_lofn_product_filter_value_product FOREIGN KEY (product_id) REFERENCES lofn_products (product_id) ON DELETE CASCADE,
    CONSTRAINT fk_lofn_product_filter_value_filter FOREIGN KEY (filter_id) REFERENCES lofn_product_type_filters (filter_id) ON DELETE CASCADE
);

CREATE UNIQUE INDEX ix_lofn_product_filter_value_product_filter_unique
    ON lofn_product_filter_values (product_id, filter_id);

CREATE INDEX ix_lofn_product_filter_value_filter_value
    ON lofn_product_filter_values (filter_id, value);
