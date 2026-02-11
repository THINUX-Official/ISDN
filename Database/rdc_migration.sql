-- =====================================================
-- IslandLink Sales Distribution Network (ISDN)
-- RDC-Based Data Partitioning - Database Migration
-- =====================================================

-- This script adds rdc_id columns to tables for Regional Distribution Centre (RDC) data partitioning
-- RDC IDs: 1=North, 2=South, 3=East, 4=West, 5=Central
-- NULL or 0 = Head Office (access to all RDCs)

USE isdn_distribution_db;

-- =====================================================
-- STEP 1: Add rdc_id columns to tables
-- =====================================================

-- Add rdc_id to users table
ALTER TABLE users 
ADD COLUMN rdc_id INT NULL COMMENT 'Regional Distribution Centre ID (NULL = Head Office)';

-- Add rdc_id to orders table
ALTER TABLE orders 
ADD COLUMN rdc_id INT NULL COMMENT 'Regional Distribution Centre ID';

-- Add rdc_id to inventory table
ALTER TABLE inventory 
ADD COLUMN rdc_id INT NULL COMMENT 'Regional Distribution Centre ID';

-- Add rdc_id to deliveries table
ALTER TABLE deliveries 
ADD COLUMN rdc_id INT NULL COMMENT 'Regional Distribution Centre ID';

-- Add rdc_id to payments table
ALTER TABLE payments 
ADD COLUMN rdc_id INT NULL COMMENT 'Regional Distribution Centre ID';

-- =====================================================
-- STEP 2: Create indexes for RDC filtering performance
-- =====================================================

CREATE INDEX idx_users_rdc_id ON users(rdc_id);
CREATE INDEX idx_orders_rdc_id ON orders(rdc_id);
CREATE INDEX idx_inventory_rdc_id ON inventory(rdc_id);
CREATE INDEX idx_deliveries_rdc_id ON deliveries(rdc_id);
CREATE INDEX idx_payments_rdc_id ON payments(rdc_id);

-- =====================================================
-- STEP 3: Assign RDC to existing users (CUSTOMIZE AS NEEDED)
-- =====================================================

-- Keep ADMIN and HEAD_OFFICE users as Head Office (rdc_id = NULL)
UPDATE users u
JOIN roles r ON u.role_id = r.role_id
SET u.rdc_id = NULL
WHERE r.role_name IN ('ADMIN', 'HEAD_OFFICE');

-- Example: Assign specific users to RDCs based on email pattern
-- UPDATE users SET rdc_id = 1 WHERE email LIKE '%north%';
-- UPDATE users SET rdc_id = 2 WHERE email LIKE '%south%';
-- UPDATE users SET rdc_id = 3 WHERE email LIKE '%east%';
-- UPDATE users SET rdc_id = 4 WHERE email LIKE '%west%';
-- UPDATE users SET rdc_id = 5 WHERE email LIKE '%central%';

-- Or assign by user ID
-- UPDATE users SET rdc_id = 1 WHERE user_id IN (10, 11, 12); -- North RDC
-- UPDATE users SET rdc_id = 2 WHERE user_id IN (13, 14, 15); -- South RDC

-- =====================================================
-- STEP 4: Propagate RDC to related data
-- =====================================================

-- Set orders.rdc_id based on the user who created the order
UPDATE orders o
JOIN users u ON o.user_id = u.user_id
SET o.rdc_id = u.rdc_id
WHERE u.rdc_id IS NOT NULL;

-- Set deliveries.rdc_id based on the order
UPDATE deliveries d
JOIN orders o ON d.order_id = o.order_id
SET d.rdc_id = o.rdc_id
WHERE o.rdc_id IS NOT NULL;

-- Set payments.rdc_id based on the order
UPDATE payments p
JOIN orders o ON p.order_id = o.order_id
SET p.rdc_id = o.rdc_id
WHERE o.rdc_id IS NOT NULL;

-- Set inventory.rdc_id based on location or assign manually
-- Example: Set based on location name
-- UPDATE inventory SET rdc_id = 1 WHERE location LIKE '%North%';
-- UPDATE inventory SET rdc_id = 2 WHERE location LIKE '%South%';
-- Or manually assign all to a default RDC
-- UPDATE inventory SET rdc_id = 1 WHERE rdc_id IS NULL;

-- =====================================================
-- STEP 5: Verification Queries
-- =====================================================

-- Check users by RDC
SELECT 
    COALESCE(rdc_id, 0) AS rdc_id,
    CASE 
        WHEN rdc_id IS NULL THEN 'Head Office'
        WHEN rdc_id = 1 THEN 'North'
        WHEN rdc_id = 2 THEN 'South'
        WHEN rdc_id = 3 THEN 'East'
        WHEN rdc_id = 4 THEN 'West'
        WHEN rdc_id = 5 THEN 'Central'
        ELSE 'Unknown'
    END AS rdc_name,
    COUNT(*) AS user_count
FROM users
GROUP BY rdc_id
ORDER BY rdc_id;

-- Check orders by RDC
SELECT 
    COALESCE(rdc_id, 0) AS rdc_id,
    COUNT(*) AS order_count,
    SUM(total_amount) AS total_revenue
FROM orders
GROUP BY rdc_id
ORDER BY rdc_id;

-- Check data distribution
SELECT 
    'orders' AS table_name,
    COUNT(*) AS total_records,
    SUM(CASE WHEN rdc_id IS NULL THEN 1 ELSE 0 END) AS head_office,
    SUM(CASE WHEN rdc_id = 1 THEN 1 ELSE 0 END) AS north,
    SUM(CASE WHEN rdc_id = 2 THEN 1 ELSE 0 END) AS south,
    SUM(CASE WHEN rdc_id = 3 THEN 1 ELSE 0 END) AS east,
    SUM(CASE WHEN rdc_id = 4 THEN 1 ELSE 0 END) AS west,
    SUM(CASE WHEN rdc_id = 5 THEN 1 ELSE 0 END) AS central
FROM orders
UNION ALL
SELECT 
    'deliveries',
    COUNT(*),
    SUM(CASE WHEN rdc_id IS NULL THEN 1 ELSE 0 END),
    SUM(CASE WHEN rdc_id = 1 THEN 1 ELSE 0 END),
    SUM(CASE WHEN rdc_id = 2 THEN 1 ELSE 0 END),
    SUM(CASE WHEN rdc_id = 3 THEN 1 ELSE 0 END),
    SUM(CASE WHEN rdc_id = 4 THEN 1 ELSE 0 END),
    SUM(CASE WHEN rdc_id = 5 THEN 1 ELSE 0 END)
FROM deliveries
UNION ALL
SELECT 
    'payments',
    COUNT(*),
    SUM(CASE WHEN rdc_id IS NULL THEN 1 ELSE 0 END),
    SUM(CASE WHEN rdc_id = 1 THEN 1 ELSE 0 END),
    SUM(CASE WHEN rdc_id = 2 THEN 1 ELSE 0 END),
    SUM(CASE WHEN rdc_id = 3 THEN 1 ELSE 0 END),
    SUM(CASE WHEN rdc_id = 4 THEN 1 ELSE 0 END),
    SUM(CASE WHEN rdc_id = 5 THEN 1 ELSE 0 END)
FROM payments
UNION ALL
SELECT 
    'inventory',
    COUNT(*),
    SUM(CASE WHEN rdc_id IS NULL THEN 1 ELSE 0 END),
    SUM(CASE WHEN rdc_id = 1 THEN 1 ELSE 0 END),
    SUM(CASE WHEN rdc_id = 2 THEN 1 ELSE 0 END),
    SUM(CASE WHEN rdc_id = 3 THEN 1 ELSE 0 END),
    SUM(CASE WHEN rdc_id = 4 THEN 1 ELSE 0 END),
    SUM(CASE WHEN rdc_id = 5 THEN 1 ELSE 0 END)
FROM inventory;

-- =====================================================
-- STEP 6: Create STANDARDIZED users for each RDC
-- =====================================================
-- IMPORTANT: All 5 RDCs have the SAME roles for consistency
-- Each RDC has: RDC_STAFF, LOGISTICS, DRIVER, FINANCE, SALES_REP
-- =====================================================

-- Get role IDs first
SET @admin_role := (SELECT role_id FROM roles WHERE role_name = 'ADMIN' LIMIT 1);
SET @head_office_role := (SELECT role_id FROM roles WHERE role_name = 'HEAD_OFFICE' LIMIT 1);
SET @rdc_staff_role := (SELECT role_id FROM roles WHERE role_name = 'RDC_STAFF' LIMIT 1);
SET @logistics_role := (SELECT role_id FROM roles WHERE role_name = 'LOGISTICS' LIMIT 1);
SET @driver_role := (SELECT role_id FROM roles WHERE role_name = 'DRIVER' LIMIT 1);
SET @finance_role := (SELECT role_id FROM roles WHERE role_name = 'FINANCE' LIMIT 1);
SET @sales_rep_role := (SELECT role_id FROM roles WHERE role_name = 'SALES_REP' LIMIT 1);

-- Head Office Users (rdc_id = NULL)
INSERT INTO users (full_name, email, password_hash, role_id, rdc_id, is_active, created_at) VALUES
('Head Office Manager', 'headoffice@isdn.lk', '$2a$11$YourHashedPasswordHere', @head_office_role, NULL, 1, NOW()),
('Head Office Admin', 'admin-ho@isdn.lk', '$2a$11$YourHashedPasswordHere', @admin_role, NULL, 1, NOW());

-- =====================================================
-- NORTH RDC (rdc_id = 1) - Complete Standardized Staff
-- =====================================================
INSERT INTO users (full_name, email, password_hash, role_id, rdc_id, is_active, created_at) VALUES
('North RDC Staff', 'north-rdc@isdn.lk', '$2a$11$YourHashedPasswordHere', @rdc_staff_role, 1, 1, NOW()),
('North Logistics', 'north-logistics@isdn.lk', '$2a$11$YourHashedPasswordHere', @logistics_role, 1, 1, NOW()),
('North Driver', 'north-driver@isdn.lk', '$2a$11$YourHashedPasswordHere', @driver_role, 1, 1, NOW()),
('North Finance', 'north-finance@isdn.lk', '$2a$11$YourHashedPasswordHere', @finance_role, 1, 1, NOW()),
('North Sales Rep', 'north-sales@isdn.lk', '$2a$11$YourHashedPasswordHere', @sales_rep_role, 1, 1, NOW());

-- =====================================================
-- SOUTH RDC (rdc_id = 2) - Complete Standardized Staff
-- =====================================================
INSERT INTO users (full_name, email, password_hash, role_id, rdc_id, is_active, created_at) VALUES
('South RDC Staff', 'south-rdc@isdn.lk', '$2a$11$YourHashedPasswordHere', @rdc_staff_role, 2, 1, NOW()),
('South Logistics', 'south-logistics@isdn.lk', '$2a$11$YourHashedPasswordHere', @logistics_role, 2, 1, NOW()),
('South Driver', 'south-driver@isdn.lk', '$2a$11$YourHashedPasswordHere', @driver_role, 2, 1, NOW()),
('South Finance', 'south-finance@isdn.lk', '$2a$11$YourHashedPasswordHere', @finance_role, 2, 1, NOW()),
('South Sales Rep', 'south-sales@isdn.lk', '$2a$11$YourHashedPasswordHere', @sales_rep_role, 2, 1, NOW());

-- =====================================================
-- EAST RDC (rdc_id = 3) - Complete Standardized Staff
-- =====================================================
INSERT INTO users (full_name, email, password_hash, role_id, rdc_id, is_active, created_at) VALUES
('East RDC Staff', 'east-rdc@isdn.lk', '$2a$11$YourHashedPasswordHere', @rdc_staff_role, 3, 1, NOW()),
('East Logistics', 'east-logistics@isdn.lk', '$2a$11$YourHashedPasswordHere', @logistics_role, 3, 1, NOW()),
('East Driver', 'east-driver@isdn.lk', '$2a$11$YourHashedPasswordHere', @driver_role, 3, 1, NOW()),
('East Finance', 'east-finance@isdn.lk', '$2a$11$YourHashedPasswordHere', @finance_role, 3, 1, NOW()),
('East Sales Rep', 'east-sales@isdn.lk', '$2a$11$YourHashedPasswordHere', @sales_rep_role, 3, 1, NOW());

-- =====================================================
-- WEST RDC (rdc_id = 4) - Complete Standardized Staff
-- =====================================================
INSERT INTO users (full_name, email, password_hash, role_id, rdc_id, is_active, created_at) VALUES
('West RDC Staff', 'west-rdc@isdn.lk', '$2a$11$YourHashedPasswordHere', @rdc_staff_role, 4, 1, NOW()),
('West Logistics', 'west-logistics@isdn.lk', '$2a$11$YourHashedPasswordHere', @logistics_role, 4, 1, NOW()),
('West Driver', 'west-driver@isdn.lk', '$2a$11$YourHashedPasswordHere', @driver_role, 4, 1, NOW()),
('West Finance', 'west-finance@isdn.lk', '$2a$11$YourHashedPasswordHere', @finance_role, 4, 1, NOW()),
('West Sales Rep', 'west-sales@isdn.lk', '$2a$11$YourHashedPasswordHere', @sales_rep_role, 4, 1, NOW());

-- =====================================================
-- CENTRAL RDC (rdc_id = 5) - Complete Standardized Staff
-- =====================================================
INSERT INTO users (full_name, email, password_hash, role_id, rdc_id, is_active, created_at) VALUES
('Central RDC Staff', 'central-rdc@isdn.lk', '$2a$11$YourHashedPasswordHere', @rdc_staff_role, 5, 1, NOW()),
('Central Logistics', 'central-logistics@isdn.lk', '$2a$11$YourHashedPasswordHere', @logistics_role, 5, 1, NOW()),
('Central Driver', 'central-driver@isdn.lk', '$2a$11$YourHashedPasswordHere', @driver_role, 5, 1, NOW()),
('Central Finance', 'central-finance@isdn.lk', '$2a$11$YourHashedPasswordHere', @finance_role, 5, 1, NOW()),
('Central Sales Rep', 'central-sales@isdn.lk', '$2a$11$YourHashedPasswordHere', @sales_rep_role, 5, 1, NOW());

-- Note: Replace '$2a$11$YourHashedPasswordHere' with actual BCrypt hashed passwords
-- Use the application's registration feature or hash passwords using BCrypt.Net
-- All RDCs now have identical role structures for consistency and simplicity

-- =====================================================
-- ROLLBACK SCRIPT (if needed)
-- =====================================================

/*
-- Remove indexes
DROP INDEX idx_users_rdc_id ON users;
DROP INDEX idx_orders_rdc_id ON orders;
DROP INDEX idx_inventory_rdc_id ON inventory;
DROP INDEX idx_deliveries_rdc_id ON deliveries;
DROP INDEX idx_payments_rdc_id ON payments;

-- Remove columns
ALTER TABLE users DROP COLUMN rdc_id;
ALTER TABLE orders DROP COLUMN rdc_id;
ALTER TABLE inventory DROP COLUMN rdc_id;
ALTER TABLE deliveries DROP COLUMN rdc_id;
ALTER TABLE payments DROP COLUMN rdc_id;
*/

-- =====================================================
-- END OF MIGRATION SCRIPT
-- =====================================================
