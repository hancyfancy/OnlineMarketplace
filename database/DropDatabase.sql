-- ============================================
-- DROP ALL DATABASE OBJECTS IN REVERSE CREATION ORDER
-- Mirrors Database.sql — run as sa or Windows Authentication
-- ============================================

USE MarketplaceDB;
GO

-- ===== TABLES =====

-- Audit tables (created last in Database.sql)
DROP TABLE IF EXISTS audit.product_activity_log;
GO

-- Order tables
DROP TABLE IF EXISTS orders.orders;
GO

-- Archive tables
DROP TABLE IF EXISTS catalog.product_variants_archive;
GO
DROP TABLE IF EXISTS catalog.products_archive;
GO

-- Catalog tables
DROP TABLE IF EXISTS catalog.product_variants;
GO
DROP TABLE IF EXISTS catalog.products;
GO
DROP TABLE IF EXISTS catalog.product_categories;
GO
DROP TABLE IF EXISTS catalog.fee_categories;
GO

-- Security profile tables
DROP TABLE IF EXISTS security.addresses;
GO
DROP TABLE IF EXISTS security.seller_profiles;
GO
DROP TABLE IF EXISTS security.buyer_profiles;
GO

-- Identity tables (role/user associations before base tables)
DROP TABLE IF EXISTS security.role_claims;
GO
DROP TABLE IF EXISTS security.user_tokens;
GO
DROP TABLE IF EXISTS security.user_logins;
GO
DROP TABLE IF EXISTS security.user_claims;
GO
DROP TABLE IF EXISTS security.user_roles;
GO
DROP TABLE IF EXISTS security.roles;
GO
DROP TABLE IF EXISTS security.users;
GO

-- Performance monitoring (created first in Database.sql)
DROP TABLE IF EXISTS analytics.query_performance;
GO

-- ===== SCHEMAS =====

DROP SCHEMA IF EXISTS analytics;
GO
DROP SCHEMA IF EXISTS marketing;
GO
DROP SCHEMA IF EXISTS audit;
GO
DROP SCHEMA IF EXISTS finance;
GO
DROP SCHEMA IF EXISTS orders;
GO
DROP SCHEMA IF EXISTS security;
GO
DROP SCHEMA IF EXISTS catalog;
GO

-- ===== DATABASE USER =====

IF EXISTS (SELECT * FROM sys.database_principals WHERE name = 'demo_user')
    DROP USER [demo_user];
GO

-- ===== DATABASE =====

USE master;
GO

--DROP DATABASE IF EXISTS MarketplaceDB;
--GO

-- ===== SERVER LOGIN =====

IF EXISTS (SELECT * FROM sys.server_principals WHERE name = 'demo_app')
    DROP LOGIN [demo_app];
GO
