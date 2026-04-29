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

CREATE TABLE lofn_categories (
    category_id BIGSERIAL NOT NULL,
    slug VARCHAR(512) NOT NULL,
    name VARCHAR(120) NOT NULL,
    store_id BIGINT,
    parent_id BIGINT,
    CONSTRAINT lofn_categories_pkey PRIMARY KEY (category_id),
    CONSTRAINT fk_lofn_category_store FOREIGN KEY (store_id) REFERENCES lofn_stores (store_id),
    CONSTRAINT fk_lofn_category_parent FOREIGN KEY (parent_id) REFERENCES lofn_categories (category_id) ON DELETE RESTRICT
);

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
