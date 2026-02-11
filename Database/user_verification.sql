-- ============================================================
-- ISDN User Verification SQL Script
-- Purpose: Verify all 27 users are seeded correctly
-- ============================================================

-- 1. CHECK RDCs (Should have 5)
SELECT '=== RDC LIST ===' AS section;
SELECT rdc_id, rdc_name, rdc_code, region, is_active 
FROM rdcs 
ORDER BY rdc_id;

-- 2. CHECK ROLES (Should have 8)
SELECT '=== ROLES LIST ===' AS section;
SELECT role_id, role_name 
FROM roles 
ORDER BY role_id;

-- 3. COUNT USERS BY ROLE
SELECT '=== USER COUNT BY ROLE ===' AS section;
SELECT r.role_name, COUNT(u.user_id) AS user_count
FROM roles r
LEFT JOIN users u ON r.role_id = u.role_id
GROUP BY r.role_name
ORDER BY r.role_name;

-- 4. COUNT USERS BY RDC
SELECT '=== USER COUNT BY RDC ===' AS section;
SELECT 
    CASE 
        WHEN u.rdc_id IS NULL THEN 'Head Office (NULL)'
        ELSE CONCAT(rd.rdc_name, ' (', u.rdc_id, ')')
    END AS rdc_assignment,
    COUNT(u.user_id) AS user_count
FROM users u
LEFT JOIN rdcs rd ON u.rdc_id = rd.rdc_id
GROUP BY u.rdc_id, rd.rdc_name
ORDER BY u.rdc_id;

-- 5. LIST ALL USERS WITH DETAILS
SELECT '=== ALL USERS ===' AS section;
SELECT 
    u.user_id,
    u.full_name,
    u.email,
    r.role_name,
    CASE 
        WHEN u.rdc_id IS NULL THEN 'NULL (Head Office)'
        ELSE CONCAT(rd.rdc_name, ' (', u.rdc_id, ')')
    END AS rdc_assignment,
    u.is_active,
    u.created_at
FROM users u
JOIN roles r ON u.role_id = r.role_id
LEFT JOIN rdcs rd ON u.rdc_id = rd.rdc_id
ORDER BY u.rdc_id, r.role_name, u.email;

-- 6. VERIFY SPECIFIC TEST ACCOUNTS
SELECT '=== VERIFY KEY ACCOUNTS ===' AS section;
SELECT 
    u.email,
    r.role_name,
    u.rdc_id,
    u.is_active,
    CASE 
        WHEN u.password_hash LIKE '$2a$%' THEN '? BCrypt Hashed'
        ELSE '? NOT BCrypt'
    END AS password_status
FROM users u
JOIN roles r ON u.role_id = r.role_id
WHERE u.email IN (
    'admin@isdn.lk',
    'headoffice@isdn.lk',
    'north-rdc@isdn.lk',
    'south-driver@isdn.lk',
    'east-logistics@isdn.lk',
    'west-finance@isdn.lk',
    'central-sales@isdn.lk'
)
ORDER BY u.email;

-- 7. CHECK FOR MISSING RDC USERS (Should show 5 users per RDC)
SELECT '=== USERS PER RDC BREAKDOWN ===' AS section;
SELECT 
    rd.rdc_name,
    r.role_name,
    COUNT(u.user_id) AS user_count
FROM rdcs rd
CROSS JOIN roles r
LEFT JOIN users u ON u.rdc_id = rd.rdc_id AND u.role_id = r.role_id
WHERE r.role_name IN ('RDC_STAFF', 'LOGISTICS', 'DRIVER', 'FINANCE', 'SALES_REP')
GROUP BY rd.rdc_name, r.role_name
ORDER BY rd.rdc_name, r.role_name;

-- 8. TOTAL USER COUNT (Should be 27+)
SELECT '=== TOTAL COUNT ===' AS section;
SELECT 
    COUNT(*) AS total_users,
    SUM(CASE WHEN is_active = 1 THEN 1 ELSE 0 END) AS active_users,
    SUM(CASE WHEN rdc_id IS NULL THEN 1 ELSE 0 END) AS head_office_users,
    SUM(CASE WHEN rdc_id IS NOT NULL THEN 1 ELSE 0 END) AS rdc_assigned_users
FROM users;

-- 9. PASSWORD HASH VERIFICATION (All should be BCrypt)
SELECT '=== PASSWORD HASH CHECK ===' AS section;
SELECT 
    COUNT(*) AS total_users,
    SUM(CASE WHEN password_hash LIKE '$2a$%' OR password_hash LIKE '$2b$%' THEN 1 ELSE 0 END) AS bcrypt_hashed,
    SUM(CASE WHEN password_hash NOT LIKE '$2a$%' AND password_hash NOT LIKE '$2b$%' THEN 1 ELSE 0 END) AS not_bcrypt
FROM users;

-- 10. RECENT LOGIN ACTIVITY
SELECT '=== RECENT LOGIN ACTIVITY ===' AS section;
SELECT 
    u.email,
    r.role_name,
    u.last_login,
    CASE 
        WHEN u.last_login IS NULL THEN 'Never logged in'
        WHEN u.last_login > DATE_SUB(NOW(), INTERVAL 1 DAY) THEN 'Recently active'
        ELSE 'Inactive'
    END AS login_status
FROM users u
JOIN roles r ON u.role_id = r.role_id
ORDER BY u.last_login DESC
LIMIT 10;

-- ============================================================
-- Expected Results Summary:
-- ============================================================
-- RDCs:          5 (North, South, East, West, Central)
-- Roles:         8 (ADMIN, HEAD_OFFICE, RDC_STAFF, LOGISTICS, DRIVER, FINANCE, SALES_REP, CUSTOMER)
-- Total Users:   27+ (2 Head Office + 25 RDC Staff + additional test users)
-- Active Users:  All should be active (is_active = 1)
-- Password Hash: All should use BCrypt ($2a$ or $2b$ prefix)
-- ============================================================

-- ============================================================
-- Quick Test: Login with these accounts
-- ============================================================
-- Admin:         admin@isdn.lk / Admin@123
-- Head Office:   headoffice@isdn.lk / HeadOffice@123
-- North RDC:     north-rdc@isdn.lk / NorthRDC@123
-- South Driver:  south-driver@isdn.lk / SouthDriver@123
-- ============================================================
