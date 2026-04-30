-- Migration: 20260430_IncreaseStoreLogoColumnLength
-- Apply per tenant DB:
--   psql "$ConnectionString" -f 20260430_IncreaseStoreLogoColumnLength.sql
--
-- Aumenta lofn_stores.logo de varchar(150) para varchar(1000) para acomodar
-- a URL completa da logomarca (anteriormente armazenava só o filename).
-- O campo continua nullable.
--
-- Idempotente: safe to re-run on the same tenant DB.

ALTER TABLE lofn_stores
    ALTER COLUMN logo TYPE VARCHAR(1000);
