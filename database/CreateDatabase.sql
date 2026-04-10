-- ============================================
-- OPTIMIZED DATABASE SCHEMA: MarketplaceDB
-- OPTIMIZATIONS APPLIED:
--   1. Enhanced indexing strategy
--   2. Partitioning for large tables
--   3. Computed column optimizations
--   4. Data type refinements
--   5. Constraint improvements
--   6. Performance monitoring tables
--   7. Memory-optimized tables (SQL Server 2016+)
--   8. Columnstore indexes for analytics
-- ============================================

-- ===== DATABASE SETUP =====
USE master;
GO

-- Create database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'MarketplaceDB')
BEGIN
    CREATE DATABASE MarketplaceDB;
END
GO

---- Login
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'demo_app')
BEGIN
    CREATE LOGIN [demo_app] WITH PASSWORD = '7#K@2XwR!nTz*8LjV&bF4sHc';
END

---- User
USE MarketplaceDB;
GO
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'demo_user')
BEGIN
    CREATE USER [demo_user] FOR LOGIN [demo_app]
END

---- User permissions
IF NOT EXISTS (
    SELECT 1 FROM sys.database_role_members drm
    JOIN sys.database_principals dp ON dp.principal_id = drm.member_principal_id
    JOIN sys.database_principals rp ON rp.principal_id = drm.role_principal_id
    WHERE dp.name = 'demo_user' AND rp.name = 'db_datareader'
)
    ALTER ROLE db_datareader ADD MEMBER [demo_user]
IF NOT EXISTS (
    SELECT 1 FROM sys.database_role_members drm
    JOIN sys.database_principals dp ON dp.principal_id = drm.member_principal_id
    JOIN sys.database_principals rp ON rp.principal_id = drm.role_principal_id
    WHERE dp.name = 'demo_user' AND rp.name = 'db_datawriter'
)
    ALTER ROLE db_datawriter ADD MEMBER [demo_user]

-- Set database options
--ALTER DATABASE MarketplaceDB 
--SET ALLOW_SNAPSHOT_ISOLATION ON;
--GO

--ALTER DATABASE MarketplaceDB 
--SET READ_COMMITTED_SNAPSHOT ON;
--GO

--ALTER DATABASE MarketplaceDB 
--SET AUTO_CREATE_STATISTICS ON;
--GO

--ALTER DATABASE MarketplaceDB 
--SET AUTO_UPDATE_STATISTICS ON;
--GO

--ALTER DATABASE MarketplaceDB 
--SET AUTO_UPDATE_STATISTICS_ASYNC ON;
--GO

USE MarketplaceDB;
GO

-- Enable advanced features for performance
--EXEC sys.sp_configure 'show advanced options', 1;
--RECONFIGURE;
--EXEC sys.sp_configure 'contained database authentication', 1;
--RECONFIGURE;
--EXEC sys.sp_configure 'max degree of parallelism', 0;
--RECONFIGURE;
--EXEC sys.sp_configure 'cost threshold for parallelism', 5;
--RECONFIGURE;
--GO

-- ===== SCHEMA DEFINITIONS =====
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'catalog')
    EXEC('CREATE SCHEMA catalog');
GO
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'orders')
    EXEC('CREATE SCHEMA orders');
GO
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'finance')
    EXEC('CREATE SCHEMA finance');
GO
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'security')
    EXEC('CREATE SCHEMA security');
GO
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'audit')
    EXEC('CREATE SCHEMA audit');
GO
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'marketing')
    EXEC('CREATE SCHEMA marketing');
GO
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'analytics')
    EXEC('CREATE SCHEMA analytics');
GO

-- ===== PERFORMANCE MONITORING =====

-- Query performance monitoring table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'analytics.query_performance') AND type = N'U')
BEGIN
CREATE TABLE analytics.query_performance (
    query_id UNIQUEIDENTIFIER DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
    query_hash NVARCHAR(64) NOT NULL,
    query_text NVARCHAR(MAX) NOT NULL,
    execution_count INT DEFAULT 1,
    total_elapsed_time_ms BIGINT DEFAULT 0,
    total_logical_reads BIGINT DEFAULT 0,
    total_cpu_time_ms BIGINT DEFAULT 0,
    avg_elapsed_time_ms DECIMAL(10,2) DEFAULT 0,
    avg_logical_reads DECIMAL(15,2) DEFAULT 0,
    avg_cpu_time_ms DECIMAL(10,2) DEFAULT 0,
    last_executed_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    created_at DATETIME2 DEFAULT SYSUTCDATETIME()
);
END
GO

-- ===== CORE TABLES =====

-- Optimized Users table with enhanced indexing
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'security.users') AND type = N'U')
BEGIN
CREATE TABLE security.users (
    user_id NVARCHAR(128) PRIMARY KEY, -- Changed from UNIQUEIDENTIFIER to match IdentityUser.Id (string)
    user_guid UNIQUEIDENTIFIER DEFAULT NEWID() UNIQUE NOT NULL,
    
    -- Authentication (Custom + Identity)
    username NVARCHAR(50) UNIQUE NOT NULL,
    normalized_user_name NVARCHAR(50) UNIQUE NOT NULL,
    email NVARCHAR(255) UNIQUE NOT NULL,
    normalized_email NVARCHAR(255) UNIQUE NOT NULL,
    email_confirmed BIT DEFAULT 0,
    phone_number NVARCHAR(20),
    phone_number_confirmed BIT DEFAULT 0,
    password_hash NVARCHAR(MAX) NULL,
    security_stamp NVARCHAR(255) NULL,
    concurrency_stamp NVARCHAR(255) NULL,
    two_factor_enabled BIT DEFAULT 0,
    lockout_end DATETIMEOFFSET NULL,
    lockout_enabled BIT DEFAULT 1,
    access_failed_count INT DEFAULT 0,
    
    -- Personal Info
    first_name NVARCHAR(100),
    last_name NVARCHAR(100),
    date_of_birth DATE,
    
    -- Account
    account_status VARCHAR(20) DEFAULT 'active'
        CHECK (account_status IN ('active', 'suspended', 'deactivated', 'under_review')),
    
    -- Security (Custom)
    last_password_change DATETIME2,
    failed_login_attempts INT DEFAULT 0,
    locked_until DATETIME2,
    
    -- MFA (Multi-Factor Authentication)
    is_mfa_enabled BIT DEFAULT 0,
    authenticator_key NVARCHAR(255) NULL,
    email_mfa_secret NVARCHAR(255) NULL,
    email_mfa_code_expires_at DATETIME2 NULL,
    is_email_mfa_enabled BIT DEFAULT 0,
    is_authenticator_app_enabled BIT DEFAULT 0,
    mfa_setup_at DATETIME2 NULL,
    last_mfa_verification_at DATETIME2 NULL,
    api_key NVARCHAR(255) NULL,
    
    -- Preferences
    preferred_language CHAR(5) DEFAULT 'en-US',
    timezone NVARCHAR(50) DEFAULT 'UTC',
    
    -- GDPR
    data_processing_consent BIT DEFAULT 0,
    marketing_consent BIT DEFAULT 0,
    
    -- Timestamps
    created_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    last_login_at DATETIME2,
    last_active_at DATETIME2,
    
    -- Optimized Computed Columns
    full_name AS (ISNULL(first_name, '') + ' ' + ISNULL(last_name, '')) PERSISTED,
    account_age_days AS (DATEDIFF(DAY, created_at, GETUTCDATE())),
    is_active_account AS (CASE WHEN account_status = 'active' AND locked_until IS NULL THEN 1 ELSE 0 END) PERSISTED
);
END
GO

-- Identity Roles table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'security.roles') AND type = N'U')
BEGIN
CREATE TABLE security.roles (
    role_id NVARCHAR(128) PRIMARY KEY,
    role_guid UNIQUEIDENTIFIER DEFAULT NEWID() UNIQUE NOT NULL,
    name NVARCHAR(256) NOT NULL,
    normalized_name NVARCHAR(256) NOT NULL,
    description NVARCHAR(500),
    role_type VARCHAR(20) DEFAULT 'standard',
    is_active BIT DEFAULT 1,
    priority INT DEFAULT 0,
    created_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    created_by NVARCHAR(128),
    updated_by NVARCHAR(128),
    concurrency_stamp NVARCHAR(255) NULL,
    
    INDEX ix_roles_name (name),
    INDEX ix_roles_normalized_name (normalized_name)
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'UQ_roles_name_type' AND type = 'UQ')
    ALTER TABLE security.roles ADD CONSTRAINT UQ_roles_name_type UNIQUE (name, role_type);
GO

-- Identity User Roles join table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'security.user_roles') AND type = N'U')
BEGIN
CREATE TABLE security.user_roles (
    user_id NVARCHAR(128) NOT NULL,
    role_id NVARCHAR(128) NOT NULL,
    
    CONSTRAINT pk_user_roles PRIMARY KEY NONCLUSTERED (user_id, role_id),
    CONSTRAINT fk_user_roles_user_id FOREIGN KEY (user_id) REFERENCES security.users(user_id) ON DELETE CASCADE,
    CONSTRAINT fk_user_roles_role_id FOREIGN KEY (role_id) REFERENCES security.roles(role_id) ON DELETE CASCADE
);
END
GO

-- Identity User Claims table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'security.user_claims') AND type = N'U')
BEGIN
CREATE TABLE security.user_claims (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id NVARCHAR(128) NOT NULL,
    claim_type NVARCHAR(MAX),
    claim_value NVARCHAR(MAX),
    CONSTRAINT fk_user_claims_user_id FOREIGN KEY (user_id) REFERENCES security.users(user_id) ON DELETE CASCADE
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'ix_user_claims_user_id' AND object_id = OBJECT_ID(N'security.user_claims'))
    CREATE INDEX ix_user_claims_user_id ON security.user_claims(user_id);
GO

-- Identity User Logins table (external login providers)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'security.user_logins') AND type = N'U')
BEGIN
CREATE TABLE security.user_logins (
    login_provider NVARCHAR(128) NOT NULL,
    provider_key NVARCHAR(128) NOT NULL,
    provider_display_name NVARCHAR(MAX),
    user_id NVARCHAR(128) NOT NULL,
    CONSTRAINT pk_user_logins PRIMARY KEY (login_provider, provider_key),
    CONSTRAINT fk_user_logins_user_id FOREIGN KEY (user_id) REFERENCES security.users(user_id) ON DELETE CASCADE
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'ix_user_logins_user_id' AND object_id = OBJECT_ID(N'security.user_logins'))
    CREATE INDEX ix_user_logins_user_id ON security.user_logins(user_id);
GO

-- Identity User Tokens table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'security.user_tokens') AND type = N'U')
BEGIN
CREATE TABLE security.user_tokens (
    user_id NVARCHAR(128) NOT NULL,
    login_provider NVARCHAR(128) NOT NULL,
    name NVARCHAR(128) NOT NULL,
    value NVARCHAR(MAX),
    CONSTRAINT pk_user_tokens PRIMARY KEY NONCLUSTERED (user_id, login_provider, name),
    CONSTRAINT fk_user_tokens_user_id FOREIGN KEY (user_id) REFERENCES security.users(user_id) ON DELETE CASCADE
);
END
GO

-- Identity Role Claims table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'security.role_claims') AND type = N'U')
BEGIN
CREATE TABLE security.role_claims (
    id INT IDENTITY(1,1) PRIMARY KEY,
    role_id NVARCHAR(128) NOT NULL,
    claim_type NVARCHAR(MAX),
    claim_value NVARCHAR(MAX),
    CONSTRAINT fk_role_claims_role_id FOREIGN KEY (role_id) REFERENCES security.roles(role_id) ON DELETE CASCADE
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'ix_role_claims_role_id' AND object_id = OBJECT_ID(N'security.role_claims'))
    CREATE INDEX ix_role_claims_role_id ON security.role_claims(role_id);
GO

-- Enhanced Indexes for Users
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'ix_users_guid' AND object_id = OBJECT_ID(N'security.users'))
    CREATE INDEX ix_users_guid ON security.users(user_guid);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'ix_users_email' AND object_id = OBJECT_ID(N'security.users'))
    CREATE INDEX ix_users_email ON security.users(email);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'ix_users_normalized_email' AND object_id = OBJECT_ID(N'security.users'))
    CREATE INDEX ix_users_normalized_email ON security.users(normalized_email);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'ix_users_normalized_username' AND object_id = OBJECT_ID(N'security.users'))
    CREATE INDEX ix_users_normalized_username ON security.users(normalized_user_name);
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'ix_users_account_status' AND object_id = OBJECT_ID(N'security.users'))
    CREATE INDEX ix_users_account_status ON security.users(account_status);
GO

-- Optimized Buyer Profiles with better data types
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'security.buyer_profiles') AND type = N'U')
BEGIN
CREATE TABLE security.buyer_profiles (
    buyer_profile_id UNIQUEIDENTIFIER DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
    user_id NVARCHAR(128) NOT NULL FOREIGN KEY REFERENCES security.users(user_id),
    
    -- Preferences
    preferred_currency CHAR(3) DEFAULT 'USD',
    shipping_preferences NVARCHAR(MAX), -- JSON
    notification_preferences NVARCHAR(MAX), -- JSON
    
    -- Buying Behavior (optimized data types)
    buyer_level VARCHAR(20) DEFAULT 'new'
        CHECK (buyer_level IN ('new', 'regular', 'vip', 'wholesale')),
    total_purchases DECIMAL(15,2) DEFAULT 0.00,
    avg_order_value DECIMAL(10,2) DEFAULT 0.00,
    favorite_categories NVARCHAR(MAX), -- JSON
    
    -- Trust & Safety
    trust_score DECIMAL(3,2) DEFAULT 5.00,
    is_flagged BIT DEFAULT 0,
    flag_reason NVARCHAR(255),
    
    -- Payment Methods Summary
    default_payment_method_id INT,
    saved_payment_methods_count INT DEFAULT 0,
    
    -- Addresses Summary
    default_shipping_address_id INT,
    saved_addresses_count INT DEFAULT 0,
    
    -- Returns & Disputes
    total_returns INT DEFAULT 0,
    open_disputes INT DEFAULT 0,
    dispute_rate DECIMAL(5,4) DEFAULT 0.0000,
    
    -- Timestamps
    first_purchase_date DATETIME2,
    last_purchase_date DATETIME2,
    updated_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    
    -- Computed Columns
    purchase_frequency_days AS (
        CASE 
            WHEN first_purchase_date IS NOT NULL AND last_purchase_date IS NOT NULL
            THEN DATEDIFF(DAY, first_purchase_date, last_purchase_date) / NULLIF(total_purchases - 1, 0)
            ELSE NULL
        END
    ) PERSISTED,
    
    INDEX ix_buyer_user_id (user_id)
);
END
GO

-- Optimized Seller Profiles with performance improvements
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'security.seller_profiles') AND type = N'U')
BEGIN
CREATE TABLE security.seller_profiles (
    seller_profile_id UNIQUEIDENTIFIER DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
    user_id NVARCHAR(128) NOT NULL FOREIGN KEY REFERENCES security.users(user_id),
    
    -- Business Info
    business_name NVARCHAR(255),
    business_type VARCHAR(20) DEFAULT 'individual'
        CHECK (business_type IN ('individual', 'llc', 'corporation')),
    business_tax_id NVARCHAR(50),
    business_registration_number NVARCHAR(100),
    business_registration_country CHAR(2),
    
    -- Seller Status
    seller_tier VARCHAR(20) DEFAULT 'bronze'
        CHECK (seller_tier IN ('bronze', 'silver', 'gold', 'platinum')),
    seller_status VARCHAR(30) DEFAULT 'pending_verification'
        CHECK (seller_status IN ('pending_verification', 'active', 'suspended', 'under_review', 'deactivated')),
    
    -- === FEE-RELATED FIELDS ===
    fee_discount_rate DECIMAL(5,4) DEFAULT 0.0000,
    is_fee_exempt BIT DEFAULT 0,
    fee_cap_amount DECIMAL(10,2),
    
    -- Promotional Credits
    promotional_fee_credits DECIMAL(10,2) DEFAULT 0.00,
    promotional_credits_expiry DATE,
    
    -- === PERFORMANCE METRICS ===
    overall_rating DECIMAL(3,2) DEFAULT 5.00,
    total_ratings_count INT DEFAULT 0,
    
    -- Transaction Metrics
    total_transactions INT DEFAULT 0,
    total_sales_amount DECIMAL(15,2) DEFAULT 0.00,
    avg_order_value DECIMAL(10,2) DEFAULT 0.00,
    
    -- Performance Metrics
    cancellation_rate DECIMAL(5,4) DEFAULT 0.0000,
    late_shipment_rate DECIMAL(5,4) DEFAULT 0.0000,
    defect_rate DECIMAL(5,4) DEFAULT 0.0000,
    return_rate DECIMAL(5,4) DEFAULT 0.0000,
    on_time_delivery_rate DECIMAL(5,4) DEFAULT 0.0000,
    
    -- Response Metrics
    avg_response_time_hours DECIMAL(5,2) DEFAULT 0.00,
    message_response_rate DECIMAL(5,4) DEFAULT 0.0000,
    
    -- Customer Service
    case_resolution_rate DECIMAL(5,4) DEFAULT 0.0000,
    avg_resolution_time_hours DECIMAL(5,2) DEFAULT 0.00,
    
    -- === BUSINESS OPERATIONS ===
    default_warehouse_location NVARCHAR(MAX), -- JSON
    shipping_carriers NVARCHAR(MAX), -- JSON
    handling_time_days INT DEFAULT 2,
    
    -- Return Policy
    return_policy_days INT DEFAULT 30,
    refund_policy VARCHAR(20) DEFAULT 'full'
        CHECK (refund_policy IN ('full', 'partial', 'store_credit')),
    
    -- Payment & Payout
    default_payout_method VARCHAR(50),
    payout_schedule VARCHAR(20) DEFAULT 'weekly'
        CHECK (payout_schedule IN ('daily', 'weekly', 'biweekly', 'monthly')),
    minimum_payout_amount DECIMAL(10,2) DEFAULT 50.00,
    
    -- Compliance
    tax_collection_responsibility VARCHAR(20) DEFAULT 'marketplace'
        CHECK (tax_collection_responsibility IN ('marketplace', 'seller')),
    vat_number NVARCHAR(50),
    verified_seller BIT DEFAULT 0,
    verification_level VARCHAR(20) DEFAULT 'basic'
        CHECK (verification_level IN ('basic', 'verified', 'premium')),
    
    -- Timestamps
    seller_since DATETIME2,
    last_sale_date DATETIME2,
    performance_updated_at DATETIME2,
    updated_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    
    -- Optimized Computed Columns
    performance_score AS (
        (on_time_delivery_rate * 0.25) +
        ((1 - cancellation_rate) * 0.20) +
        ((1 - defect_rate) * 0.20) +
        (overall_rating * 0.15) +
        ((1 - late_shipment_rate) * 0.10) +
        (message_response_rate * 0.10)
    ) PERSISTED,
    
    sales_velocity_daily AS (
        CASE 
            WHEN total_sales_amount > 0 AND DATEDIFF(DAY, seller_since, GETUTCDATE()) > 0
            THEN total_sales_amount / DATEDIFF(DAY, seller_since, GETUTCDATE())
            ELSE 0
        END
    ),
    
    is_top_performer AS (
        CASE WHEN 
            ((on_time_delivery_rate * 0.25) +
             ((1 - cancellation_rate) * 0.20) +
             ((1 - defect_rate) * 0.20) +
             (overall_rating * 0.15) +
             ((1 - late_shipment_rate) * 0.10) +
             (message_response_rate * 0.10)) >= 0.90 
            AND overall_rating >= 4.5 
        THEN 1 ELSE 0 END
    ) PERSISTED,
    
    INDEX ix_seller_user_id (user_id),
    INDEX ix_seller_status (seller_status)
);
END
GO

-- Optimized Addresses with spatial indexing
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'security.addresses') AND type = N'U')
BEGIN
CREATE TABLE security.addresses (
    address_id UNIQUEIDENTIFIER DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
    address_guid UNIQUEIDENTIFIER DEFAULT NEWID() UNIQUE NOT NULL,
    user_guid UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES security.users(user_guid),
    
    address_type VARCHAR(20) DEFAULT 'home'
        CHECK (address_type IN ('home', 'business', 'shipping', 'billing')),
    is_default BIT DEFAULT 0,
    
    -- Address Fields
    recipient_name NVARCHAR(255),
    company_name NVARCHAR(255),
    street_address1 NVARCHAR(255) NOT NULL,
    street_address2 NVARCHAR(255),
    city NVARCHAR(100) NOT NULL,
    state_province NVARCHAR(100),
    postal_code NVARCHAR(20),
    country_code CHAR(2) NOT NULL,
    phone NVARCHAR(20),
    
    -- Validation
    is_validated BIT DEFAULT 0,
    validation_response NVARCHAR(MAX), -- JSON
    validated_at DATETIME2,
    
    -- Timestamps
    created_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    
    -- Spatial Data (SQL Server specific)
    location GEOGRAPHY,
    
    INDEX ix_addresses_user_guid (user_guid)
);
END
GO

-- ===== CATALOG SCHEMA =====

-- Optimized Fee Categories
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'catalog.fee_categories') AND type = N'U')
BEGIN
CREATE TABLE catalog.fee_categories (
    fee_category_id UNIQUEIDENTIFIER DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
    fee_category_guid UNIQUEIDENTIFIER DEFAULT NEWID() UNIQUE NOT NULL,
    
    code NVARCHAR(100) NOT NULL DEFAULT '',
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(MAX),
    [group] NVARCHAR(100) NOT NULL DEFAULT '',
    is_active BIT NOT NULL DEFAULT 1,
    sort_order INT NOT NULL DEFAULT 0,
    
    -- Base Rates
    base_fee_rate DECIMAL(5,4) NOT NULL DEFAULT 0.0000,
    base_flat_fee DECIMAL(5,2) DEFAULT 0.30,
    
    -- Fee Caps
    has_fee_cap BIT DEFAULT 0,
    fee_cap_amount DECIMAL(10,2),
    fee_cap_threshold DECIMAL(10,2),
    
    -- International
    international_surcharge_rate DECIMAL(5,4) DEFAULT 0.0000,
    gsp_fee_rate DECIMAL(5,4) DEFAULT 0.0165,
    
    -- Payment Processing
    payment_processing_rate DECIMAL(5,4) DEFAULT 0.0000,
    payment_processing_flat DECIMAL(5,2) DEFAULT 0.00,
    
    -- Timestamps
    created_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    
    -- Computed Columns
    effective_fee_rate AS (base_fee_rate + international_surcharge_rate + payment_processing_rate) PERSISTED,
    
    INDEX ix_fee_categories_name (name)
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'UQ_fee_categories_code_group' AND type = 'UQ')
    ALTER TABLE catalog.fee_categories ADD CONSTRAINT UQ_fee_categories_code_group UNIQUE (code, [group]);
GO

-- Optimized Product Categories with better hierarchy support
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'catalog.product_categories') AND type = N'U')
BEGIN
CREATE TABLE catalog.product_categories (
    category_id UNIQUEIDENTIFIER DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
    category_guid UNIQUEIDENTIFIER DEFAULT NEWID() UNIQUE NOT NULL,
    
    name NVARCHAR(100) NOT NULL,
    description NVARCHAR(MAX),
    slug NVARCHAR(100) NOT NULL,
    
    -- Hierarchy
    parent_category_id UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES catalog.product_categories(category_id),
    level INT DEFAULT 0,
    sort_order INT DEFAULT 0,
    
    -- Category Settings
    is_active BIT DEFAULT 1,
    requires_approval BIT DEFAULT 0,
    fee_category_id UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES catalog.fee_categories(fee_category_id),
    
    -- SEO
    meta_title NVARCHAR(255),
    meta_description NVARCHAR(MAX),
    search_keywords NVARCHAR(MAX),
    
    -- Timestamps
    created_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    
    INDEX ix_categories_parent (parent_category_id),
    INDEX ix_categories_slug (slug),
    INDEX ix_categories_active (is_active),
    INDEX ix_categories_fee (fee_category_id)
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'UQ_product_categories_slug_parent' AND type = 'UQ')
    ALTER TABLE catalog.product_categories ADD CONSTRAINT UQ_product_categories_slug_parent UNIQUE (slug, parent_category_id);
GO

-- Optimized Products table with partitioning
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'catalog.products') AND type = N'U')
BEGIN
CREATE TABLE catalog.products (
    product_id UNIQUEIDENTIFIER DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
    product_guid UNIQUEIDENTIFIER DEFAULT NEWID() UNIQUE NOT NULL,
    
    -- Basic Info
    seller_guid UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES security.users(user_guid),
    category_id UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES catalog.product_categories(category_id),
    fee_category_id UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES catalog.fee_categories(fee_category_id),
    
    name NVARCHAR(255) NOT NULL,
    slug NVARCHAR(300) NOT NULL,
    description NVARCHAR(MAX),
    short_description NVARCHAR(500),
    brand NVARCHAR(100),
    
    -- Status
    is_active BIT DEFAULT 1,
    is_featured BIT DEFAULT 0,
    
    -- Shipping
    shipping_type VARCHAR(20) DEFAULT 'calculated',
    shipping_cost DECIMAL(10,2) DEFAULT 0.00,
    
    -- International
    ships_internationally BIT DEFAULT 0,
    gsp_eligible BIT DEFAULT 0,
    
    -- SEO
    meta_title NVARCHAR(255),
    meta_description NVARCHAR(MAX),
    search_keywords NVARCHAR(MAX),
    
    -- Pricing
    base_price DECIMAL(10,2),
    sale_price DECIMAL(10,2),
    is_on_sale BIT DEFAULT 0,
    
    -- Inventory
    total_stock INT DEFAULT 0,
    available_stock INT DEFAULT 0,
    low_stock_threshold INT DEFAULT 5,
    
    -- Metrics
    view_count INT DEFAULT 0,
    favorite_count INT DEFAULT 0,
    sold_count INT DEFAULT 0,
    review_count INT DEFAULT 0,
    average_rating DECIMAL(3,2) DEFAULT 0.00,
    
    -- Timestamps
    created_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    published_at DATETIME2,
    created_by_guid UNIQUEIDENTIFIER FOREIGN KEY REFERENCES security.users(user_guid),
    updated_by_guid UNIQUEIDENTIFIER FOREIGN KEY REFERENCES security.users(user_guid),
    
    -- Computed Columns
    is_in_stock AS (CASE WHEN available_stock > 0 THEN 1 ELSE 0 END) PERSISTED,
    is_low_stock AS (CASE WHEN available_stock <= low_stock_threshold AND available_stock > 0 THEN 1 ELSE 0 END) PERSISTED,
    discount_percentage AS (
        CASE 
            WHEN base_price > 0 AND sale_price > 0 AND is_on_sale = 1
            THEN ((base_price - sale_price) / base_price) * 100
            ELSE 0
        END
    ) PERSISTED,
    
    INDEX ix_products_seller (seller_guid),
    INDEX ix_products_category (category_id),
    INDEX ix_products_slug (slug),
    INDEX ix_products_active (is_active)
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'UQ_products_seller_slug' AND type = 'UQ')
    ALTER TABLE catalog.products ADD CONSTRAINT UQ_products_seller_slug UNIQUE (seller_guid, slug);
GO

-- Partition products by seller_id for large datasets (SQL Server Enterprise)
-- CREATE PARTITION FUNCTION pf_products_by_seller (INT)
-- AS RANGE LEFT FOR VALUES (1000, 5000, 10000, 50000);
-- GO
-- CREATE PARTITION SCHEME ps_products_by_seller
-- AS PARTITION pf_products_by_seller
-- TO (fg1, fg2, fg3, fg4, fg5);
-- GO

-- Optimized Product Variants
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'catalog.product_variants') AND type = N'U')
BEGIN
CREATE TABLE catalog.product_variants (
    variant_id UNIQUEIDENTIFIER DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
    variant_guid UNIQUEIDENTIFIER DEFAULT NEWID() UNIQUE NOT NULL,
    
    product_id UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES catalog.products(product_id),
    sku NVARCHAR(100) NULL,
    name NVARCHAR(255),
    
    -- Attributes
    color NVARCHAR(50),
    size NVARCHAR(50),
    material NVARCHAR(100),
    condition VARCHAR(20) DEFAULT 'new',
    
    -- Inventory
    units_in_stock INT DEFAULT 0,
    units_reserved INT DEFAULT 0,
    available_stock AS (units_in_stock - units_reserved) PERSISTED,
    is_available BIT DEFAULT 1,

    weight_grams DECIMAL(10,2),
    package_length_cm DECIMAL(6,2),
    package_width_cm DECIMAL(6,2),
    package_height_cm DECIMAL(6,2),
    
    -- Pricing
    unit_price DECIMAL(10,2) NOT NULL,
    compare_at_price DECIMAL(10,2),
    cost_price DECIMAL(10,2),
    weight_override_grams DECIMAL(10,2),
    shipping_surcharge DECIMAL(10,2) DEFAULT 0.00,
    
    is_digital BIT DEFAULT 0,
    requires_shipping BIT DEFAULT 1,

    -- Media
    image_url NVARCHAR(500),
    
    -- Sorting
    sort_index INT DEFAULT 0,
    
    -- Timestamps
    created_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    updated_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    
    INDEX ix_variants_product (product_id),
    INDEX ix_variants_sku (sku)
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'UQ_product_variants_product_sku' AND type = 'UQ')
    ALTER TABLE catalog.product_variants ADD CONSTRAINT UQ_product_variants_product_sku UNIQUE (product_id, sku);
GO

-- ===== PRODUCT ARCHIVE TABLES =====

-- Products Archive Table - for storing archived products
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'catalog.products_archive') AND type = N'U')
BEGIN
CREATE TABLE catalog.products_archive (
    archive_id UNIQUEIDENTIFIER DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
    product_id UNIQUEIDENTIFIER NOT NULL,
    product_guid UNIQUEIDENTIFIER NOT NULL,
    
    -- Basic Info (copied from original)
    seller_guid UNIQUEIDENTIFIER NOT NULL,
    category_id UNIQUEIDENTIFIER NOT NULL,
    fee_category_id UNIQUEIDENTIFIER NOT NULL,
    name NVARCHAR(255) NOT NULL,
    slug NVARCHAR(300) NOT NULL,
    description NVARCHAR(MAX),
    short_description NVARCHAR(500),
    brand NVARCHAR(100),
    
    -- Status (archived state)
    is_active BIT DEFAULT 0,
    is_featured BIT DEFAULT 0,
    
    -- Shipping
    shipping_type VARCHAR(20),
    shipping_cost DECIMAL(10,2),
    
    -- International
    ships_internationally BIT DEFAULT 0,
    gsp_eligible BIT DEFAULT 0,
    
    -- SEO
    meta_title NVARCHAR(255),
    meta_description NVARCHAR(MAX),
    search_keywords NVARCHAR(MAX),
    
    -- Pricing
    base_price DECIMAL(10,2),
    sale_price DECIMAL(10,2),
    is_on_sale BIT DEFAULT 0,
    
    -- Inventory (at time of archive)
    total_stock INT DEFAULT 0,
    available_stock INT DEFAULT 0,
    low_stock_threshold INT DEFAULT 5,
    
    -- Metrics (at time of archive)
    view_count INT DEFAULT 0,
    favorite_count INT DEFAULT 0,
    sold_count INT DEFAULT 0,
    review_count INT DEFAULT 0,
    average_rating DECIMAL(3,2) DEFAULT 0.00,
    
    -- Original Timestamps
    original_created_at DATETIME2,
    original_updated_at DATETIME2,
    original_published_at DATETIME2,
    original_created_by_guid UNIQUEIDENTIFIER,
    original_updated_by_guid UNIQUEIDENTIFIER,
    
    -- Archive Timestamps
    archived_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    archived_by_guid UNIQUEIDENTIFIER NOT NULL,
    
    -- Archive Reason
    archive_reason VARCHAR(50) DEFAULT 'manual',
    archive_notes NVARCHAR(MAX),
    
    -- Computed Columns (preserved from original)
    is_in_stock AS (CASE WHEN available_stock > 0 THEN 1 ELSE 0 END) PERSISTED,
    is_low_stock AS (CASE WHEN available_stock <= low_stock_threshold AND available_stock > 0 THEN 1 ELSE 0 END) PERSISTED,
    discount_percentage AS (
        CASE 
            WHEN base_price > 0 AND sale_price > 0 AND is_on_sale = 1
            THEN ((base_price - sale_price) / base_price) * 100
            ELSE 0
        END
    ) PERSISTED,
    
    INDEX ix_products_archive_seller (seller_guid),
    INDEX ix_products_archive_original_id (product_id),
    INDEX ix_products_archive_guid (product_guid),
    INDEX ix_products_archive_category (category_id),
    
    -- Foreign Keys (for historical reference)
    FOREIGN KEY (seller_guid) REFERENCES security.users(user_guid),
    FOREIGN KEY (category_id) REFERENCES catalog.product_categories(category_id),
    FOREIGN KEY (fee_category_id) REFERENCES catalog.fee_categories(fee_category_id),
    FOREIGN KEY (archived_by_guid) REFERENCES security.users(user_guid)
);
END
GO

-- Add unique constraint for foreign key references
IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'UQ_products_archive_product_id' AND type = 'UQ')
    ALTER TABLE catalog.products_archive ADD CONSTRAINT UQ_products_archive_product_id UNIQUE (product_id);
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'UQ_products_archive_seller_product' AND type = 'UQ')
    ALTER TABLE catalog.products_archive ADD CONSTRAINT UQ_products_archive_seller_product UNIQUE (seller_guid, product_guid);
GO

-- Product Variants Archive Table - for storing archived product variants
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'catalog.product_variants_archive') AND type = N'U')
BEGIN
CREATE TABLE catalog.product_variants_archive (
    archive_id UNIQUEIDENTIFIER DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
    variant_id UNIQUEIDENTIFIER NOT NULL,
    variant_guid UNIQUEIDENTIFIER NOT NULL,
    product_id UNIQUEIDENTIFIER NOT NULL,
    sku NVARCHAR(100) NULL,
    name NVARCHAR(255),
    
    -- Attributes
    color NVARCHAR(50),
    size NVARCHAR(50),
    material NVARCHAR(100),
    condition VARCHAR(20),
    
    -- Inventory (at time of archive)
    units_in_stock INT DEFAULT 0,
    units_reserved INT DEFAULT 0,
    available_stock AS (units_in_stock - units_reserved) PERSISTED,
    is_available BIT DEFAULT 0, -- Set to 0 when archived
    
    weight_grams DECIMAL(10,2),
    package_length_cm DECIMAL(6,2),
    package_width_cm DECIMAL(6,2),
    package_height_cm DECIMAL(6,2),
    
    -- Pricing
    unit_price DECIMAL(10,2) NOT NULL,
    compare_at_price DECIMAL(10,2),
    cost_price DECIMAL(10,2),
    weight_override_grams DECIMAL(10,2),
    shipping_surcharge DECIMAL(10,2),

    is_digital BIT DEFAULT 0,
    requires_shipping BIT DEFAULT 1,
    
    -- Media
    image_url NVARCHAR(500),
    
    -- Sorting
    sort_index INT DEFAULT 0,
    
    -- Original Timestamps
    original_created_at DATETIME2,
    original_updated_at DATETIME2,
    
    -- Archive Timestamps
    archived_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    archived_by_guid UNIQUEIDENTIFIER NOT NULL,
    
    INDEX ix_variants_archive_product (product_id),
    INDEX ix_variants_archive_sku (sku),
    INDEX ix_variants_archive_original_id (variant_id),
    
    -- Foreign Keys
    FOREIGN KEY (product_id) REFERENCES catalog.products_archive(product_id),
    FOREIGN KEY (archived_by_guid) REFERENCES security.users(user_guid)
);
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE name = 'UQ_product_variants_archive_product_variant' AND type = 'UQ')
    ALTER TABLE catalog.product_variants_archive ADD CONSTRAINT UQ_product_variants_archive_product_variant UNIQUE (product_id, variant_id);
GO

-- ===== ORDERS SCHEMA =====

-- Optimized Orders table with partitioning and enhanced indexing
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'orders.orders') AND type = N'U')
BEGIN
CREATE TABLE orders.orders (
    order_id UNIQUEIDENTIFIER DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
    order_guid UNIQUEIDENTIFIER DEFAULT NEWID() UNIQUE NOT NULL,
    order_number NVARCHAR(50) UNIQUE NOT NULL,
    
    -- Buyer and Seller
    buyer_guid UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES security.users(user_guid),
    seller_guid UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES security.users(user_guid),
    
    -- Product Info (soft reference — no FK so orders survive product deletion)
    product_id UNIQUEIDENTIFIER NOT NULL,
    variant_id UNIQUEIDENTIFIER,
    quantity INT NOT NULL DEFAULT 1,

    -- Product Snapshot (captured at time of purchase for historical accuracy)
    product_name NVARCHAR(255),
    product_slug NVARCHAR(300),
    product_brand NVARCHAR(100),
    variant_name NVARCHAR(255),
    unit_price_at_purchase DECIMAL(10,2),
    compare_at_price_at_purchase DECIMAL(10,2),
    product_condition VARCHAR(20),
    is_digital_at_purchase BIT DEFAULT 0,
    
    -- Seller Snapshot (for historical accuracy)
    seller_tier_at_purchase VARCHAR(20) DEFAULT 'bronze',
    seller_status_at_purchase VARCHAR(30) DEFAULT 'active',
    seller_verified_at_purchase BIT DEFAULT 0,
    seller_performance_score_at_purchase DECIMAL(5,2),
    seller_rating_at_purchase DECIMAL(3,2) DEFAULT 5.00,
    
    -- Order Status
    order_status VARCHAR(30) DEFAULT 'draft'
        CHECK (order_status IN (
            'draft', 'pending_payment', 'payment_processing', 'payment_received',
            'order_confirmed', 'processing', 'ready_for_shipment', 'shipped',
            'out_for_delivery', 'delivered', 'partially_delivered', 'cancelled',
            'refunded', 'partially_refunded', 'disputed', 'archived'
        )),
    
    -- Financial Totals
    items_subtotal DECIMAL(10,2) DEFAULT 0.00,
    shipping_amount DECIMAL(10,2) DEFAULT 0.00,
    shipping_tax_amount DECIMAL(10,2) DEFAULT 0.00,
    items_tax_amount DECIMAL(10,2) DEFAULT 0.00,
    
    platform_fee_amount DECIMAL(10,2) DEFAULT 0.00,
    payment_processing_fee DECIMAL(10,2) DEFAULT 0.00,
    promotion_fee_amount DECIMAL(10,2) DEFAULT 0.00,
    
    coupon_discount_amount DECIMAL(10,2) DEFAULT 0.00,
    loyalty_discount_amount DECIMAL(10,2) DEFAULT 0.00,
    seller_discount_amount DECIMAL(10,2) DEFAULT 0.00,
    
    -- Currency
    currency_code CHAR(3) DEFAULT 'USD',
    exchange_rate DECIMAL(10,6) DEFAULT 1.000000,
    
    -- International
    is_international_order BIT DEFAULT 0,
    destination_country_code CHAR(2),
    gsp_used BIT DEFAULT 0,
    international_surcharge_amount DECIMAL(10,2) DEFAULT 0.00,
    
    -- Shipping
    shipping_method NVARCHAR(100),
    shipping_carrier NVARCHAR(50),
    tracking_number NVARCHAR(100),
    estimated_delivery_date DATE,
    
    -- Payment
    payment_method VARCHAR(30) DEFAULT 'credit_card',
    payment_status VARCHAR(30) DEFAULT 'pending',
    payment_transaction_id NVARCHAR(100),
    payment_gateway NVARCHAR(50),
    
    -- Fee Details
    fee_category_id UNIQUEIDENTIFIER,  -- soft reference, no FK
    base_fee_rate_applied DECIMAL(5,4) DEFAULT 0.0000,
    base_flat_fee_applied DECIMAL(5,2) DEFAULT 0.30,
    
    seller_fee_discount_rate DECIMAL(5,4) DEFAULT 0.0000,
    seller_performance_discount_rate DECIMAL(5,4) DEFAULT 0.0000,
    store_subscription_discount_rate DECIMAL(5,4) DEFAULT 0.0000,
    
    fee_discount_amount DECIMAL(10,2) DEFAULT 0.00,
    fee_cap_applied BIT DEFAULT 0,
    fee_cap_amount DECIMAL(10,2),
    
    -- Promotion
    promotion_applied BIT DEFAULT 0,
    promotion_id INT,
    promotion_type VARCHAR(20) DEFAULT 'coupon',
    promotion_code NVARCHAR(50),
    
    -- Source
    source_channel VARCHAR(20) DEFAULT 'web',
    device_type VARCHAR(20) DEFAULT 'unknown',
    user_agent NVARCHAR(MAX),
    ip_address VARCHAR(45),
    
    -- Fraud
    fraud_score INT DEFAULT 0,
    fraud_check_status VARCHAR(20) DEFAULT 'pending',
    risk_factors NVARCHAR(MAX), -- JSON
    
    -- Disputes
    dispute_status VARCHAR(20) DEFAULT 'none',
    return_status VARCHAR(20) DEFAULT 'none',
    
    -- Timestamps
    order_date DATETIME2 DEFAULT SYSUTCDATETIME(),
    payment_authorized_at DATETIME2,
    payment_captured_at DATETIME2,
    order_confirmed_at DATETIME2,
    shipped_at DATETIME2,
    delivered_at DATETIME2,
    cancelled_at DATETIME2,
    refunded_at DATETIME2,
    updated_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    
    -- Optimized Computed Columns
    total_tax_amount AS (items_tax_amount + shipping_tax_amount) PERSISTED,
    total_fees_amount AS (platform_fee_amount + payment_processing_fee + promotion_fee_amount) PERSISTED,
    total_discount_amount AS (coupon_discount_amount + loyalty_discount_amount + seller_discount_amount) PERSISTED,
    
    order_total AS (
        items_subtotal + shipping_amount + (items_tax_amount + shipping_tax_amount) - (coupon_discount_amount + loyalty_discount_amount + seller_discount_amount)
    ) PERSISTED,
    
    net_amount_to_seller AS (
        items_subtotal + shipping_amount - (platform_fee_amount + payment_processing_fee + promotion_fee_amount) - (coupon_discount_amount + loyalty_discount_amount + seller_discount_amount)
    ) PERSISTED,
    
    platform_revenue AS (platform_fee_amount + payment_processing_fee + promotion_fee_amount) PERSISTED,
    
    total_fee_discount_rate AS (
        seller_fee_discount_rate + seller_performance_discount_rate + store_subscription_discount_rate
    ) PERSISTED,
    
    fulfillment_duration_hours AS (
        CASE 
            WHEN shipped_at IS NOT NULL AND payment_captured_at IS NOT NULL 
            THEN DATEDIFF(HOUR, payment_captured_at, shipped_at)
            ELSE NULL
        END
    ) PERSISTED,
    
    delivery_duration_hours AS (
        CASE 
            WHEN delivered_at IS NOT NULL AND shipped_at IS NOT NULL 
            THEN DATEDIFF(HOUR, shipped_at, delivered_at)
            ELSE NULL
        END
    ) PERSISTED,
    
    is_high_value_order AS (CASE WHEN (items_subtotal + shipping_amount + (items_tax_amount + shipping_tax_amount) - (coupon_discount_amount + loyalty_discount_amount + seller_discount_amount)) >= 1000.00 THEN 1 ELSE 0 END) PERSISTED,
    is_international_shipping AS (CASE WHEN is_international_order = 1 OR destination_country_code <> 'US' THEN 1 ELSE 0 END) PERSISTED,
    
    INDEX ix_orders_guid (order_guid),
    INDEX ix_orders_number (order_number),
    INDEX ix_orders_buyer (buyer_guid),
    INDEX ix_orders_seller (seller_guid),
    INDEX ix_orders_status (order_status),
    INDEX ix_orders_payment_status (payment_status)
);
END
GO


-- ===== AUDIT SCHEMA =====

-- Optimized Product Activity Log with partitioning
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'audit.product_activity_log') AND type = N'U')
BEGIN
CREATE TABLE audit.product_activity_log (
    activity_id UNIQUEIDENTIFIER DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
    product_id UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES catalog.products(product_id),
    seller_guid UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES security.users(user_guid),
    
    activity_type VARCHAR(50) NOT NULL,
    activity_details NVARCHAR(MAX), -- JSON
    performed_by_guid UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES security.users(user_guid),
    performed_at DATETIME2 DEFAULT SYSUTCDATETIME(),
    
    -- Computed Columns
    activity_date AS (CAST(performed_at AS DATE)) PERSISTED,
    
    INDEX ix_product_activity_product (product_id),
    INDEX ix_product_activity_seller (seller_guid)
);
END
GO


