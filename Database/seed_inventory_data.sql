-- =====================================================
-- IslandLink Sales Distribution Network (ISDN)
-- Sample Inventory Data - For Testing Inventory Management
-- =====================================================

USE isdn_distribution_db;

-- =====================================================
-- STEP 1: Insert Sample Products (if not exist)
-- =====================================================

-- Insert products with various categories
INSERT INTO products (product_name, description, sku, unit_price, category, is_active, product_image_url, created_at)
VALUES
-- Food Items
('Rice - 5kg', 'Premium basmati rice 5kg pack', 'RICE-5KG-001', 1200.00, 'Food', 1, '/images/products/rice-5kg.jpg', NOW()),
('Sugar - 1kg', 'White refined sugar 1kg pack', 'SUGAR-1KG-001', 180.00, 'Food', 1, '/images/products/sugar-1kg.jpg', NOW()),
('Flour - 1kg', 'All-purpose wheat flour 1kg', 'FLOUR-1KG-001', 150.00, 'Food', 1, '/images/products/flour-1kg.jpg', NOW()),
('Cooking Oil - 1L', 'Sunflower cooking oil 1L bottle', 'OIL-1L-001', 550.00, 'Food', 1, '/images/products/oil-1l.jpg', NOW()),
('Tea - 200g', 'Premium black tea 200g pack', 'TEA-200G-001', 420.00, 'Beverages', 1, '/images/products/tea-200g.jpg', NOW()),

-- Beverages
('Coffee - 100g', 'Instant coffee powder 100g jar', 'COFFEE-100G-001', 650.00, 'Beverages', 1, '/images/products/coffee-100g.jpg', NOW()),
('Milk Powder - 400g', 'Full cream milk powder 400g', 'MILK-400G-001', 780.00, 'Beverages', 1, '/images/products/milk-400g.jpg', NOW()),
('Soft Drink - 2L', 'Carbonated soft drink 2L bottle', 'SODA-2L-001', 280.00, 'Beverages', 1, '/images/products/soda-2l.jpg', NOW()),

-- Household Items
('Laundry Detergent - 1kg', 'Washing powder 1kg pack', 'DETERGENT-1KG-001', 450.00, 'Household', 1, '/images/products/detergent-1kg.jpg', NOW()),
('Dish Soap - 500ml', 'Liquid dish washing soap 500ml', 'SOAP-500ML-001', 220.00, 'Household', 1, '/images/products/soap-500ml.jpg', NOW()),
('Toilet Paper - 4 Roll', 'Soft toilet tissue 4 roll pack', 'TISSUE-4R-001', 380.00, 'Household', 1, '/images/products/tissue-4r.jpg', NOW()),

-- Snacks
('Biscuits - 200g', 'Cream biscuits 200g pack', 'BISCUIT-200G-001', 180.00, 'Snacks', 1, '/images/products/biscuit-200g.jpg', NOW()),
('Potato Chips - 100g', 'Crispy potato chips 100g', 'CHIPS-100G-001', 150.00, 'Snacks', 1, '/images/products/chips-100g.jpg', NOW()),
('Chocolate Bar - 50g', 'Milk chocolate bar 50g', 'CHOCO-50G-001', 120.00, 'Snacks', 1, '/images/products/choco-50g.jpg', NOW()),

-- Personal Care
('Toothpaste - 100g', 'Fluoride toothpaste 100g tube', 'PASTE-100G-001', 280.00, 'Personal Care', 1, '/images/products/paste-100g.jpg', NOW()),
('Shampoo - 400ml', 'Hair shampoo 400ml bottle', 'SHAMPOO-400ML-001', 520.00, 'Personal Care', 1, '/images/products/shampoo-400ml.jpg', NOW()),
('Soap Bar - 100g', 'Beauty soap bar 100g', 'SOAPBAR-100G-001', 180.00, 'Personal Care', 1, '/images/products/soapbar-100g.jpg', NOW())

ON DUPLICATE KEY UPDATE product_name = product_name;

-- =====================================================
-- STEP 2: Get Product IDs for Inventory Creation
-- =====================================================

SET @rice_id = (SELECT product_id FROM products WHERE sku = 'RICE-5KG-001' LIMIT 1);
SET @sugar_id = (SELECT product_id FROM products WHERE sku = 'SUGAR-1KG-001' LIMIT 1);
SET @flour_id = (SELECT product_id FROM products WHERE sku = 'FLOUR-1KG-001' LIMIT 1);
SET @oil_id = (SELECT product_id FROM products WHERE sku = 'OIL-1L-001' LIMIT 1);
SET @tea_id = (SELECT product_id FROM products WHERE sku = 'TEA-200G-001' LIMIT 1);
SET @coffee_id = (SELECT product_id FROM products WHERE sku = 'COFFEE-100G-001' LIMIT 1);
SET @milk_id = (SELECT product_id FROM products WHERE sku = 'MILK-400G-001' LIMIT 1);
SET @soda_id = (SELECT product_id FROM products WHERE sku = 'SODA-2L-001' LIMIT 1);
SET @detergent_id = (SELECT product_id FROM products WHERE sku = 'DETERGENT-1KG-001' LIMIT 1);
SET @dishsoap_id = (SELECT product_id FROM products WHERE sku = 'SOAP-500ML-001' LIMIT 1);
SET @tissue_id = (SELECT product_id FROM products WHERE sku = 'TISSUE-4R-001' LIMIT 1);
SET @biscuit_id = (SELECT product_id FROM products WHERE sku = 'BISCUIT-200G-001' LIMIT 1);
SET @chips_id = (SELECT product_id FROM products WHERE sku = 'CHIPS-100G-001' LIMIT 1);
SET @choco_id = (SELECT product_id FROM products WHERE sku = 'CHOCO-50G-001' LIMIT 1);
SET @paste_id = (SELECT product_id FROM products WHERE sku = 'PASTE-100G-001' LIMIT 1);
SET @shampoo_id = (SELECT product_id FROM products WHERE sku = 'SHAMPOO-400ML-001' LIMIT 1);
SET @soapbar_id = (SELECT product_id FROM products WHERE sku = 'SOAPBAR-100G-001' LIMIT 1);

-- =====================================================
-- STEP 3: Insert Sample Inventory for North RDC (rdc_id = 1)
-- =====================================================

-- Main Warehouse Stock
INSERT INTO inventory (product_id, rdc_id, location, quantity_available, quantity_reserved, reorder_level, last_updated)
VALUES
(@rice_id, 1, 'Main Warehouse', 1100, 10, 100, NOW()),
(@sugar_id, 1, 'Main Warehouse', 1000, 0, 100, NOW()),
(@flour_id, 1, 'Main Warehouse', 1000, 0, 100, NOW()),
(@oil_id, 1, 'Main Warehouse', 1000, 0, 100, NOW()),
(@tea_id, 1, 'Main Warehouse', 1000, 0, 100, NOW()),
(@coffee_id, 1, 'Main Warehouse', 800, 20, 100, NOW()),
(@milk_id, 1, 'Main Warehouse', 600, 50, 100, NOW()),
(@soda_id, 1, 'Main Warehouse', 500, 0, 150, NOW()),
(@detergent_id, 1, 'Main Warehouse', 400, 10, 80, NOW()),
(@dishsoap_id, 1, 'Main Warehouse', 350, 0, 80, NOW()),
(@tissue_id, 1, 'Main Warehouse', 450, 30, 100, NOW()),
(@biscuit_id, 1, 'Main Warehouse', 750, 0, 150, NOW()),
(@chips_id, 1, 'Main Warehouse', 650, 20, 150, NOW()),
(@choco_id, 1, 'Main Warehouse', 900, 50, 200, NOW()),
(@paste_id, 1, 'Main Warehouse', 550, 0, 100, NOW()),
(@shampoo_id, 1, 'Main Warehouse', 480, 20, 100, NOW()),
(@soapbar_id, 1, 'Main Warehouse', 820, 0, 150, NOW());

-- Low Stock Items (to test low stock alerts)
INSERT INTO inventory (product_id, rdc_id, location, quantity_available, quantity_reserved, reorder_level, last_updated)
VALUES
(@rice_id, 1, 'Storage Room A', 85, 5, 100, NOW()),
(@milk_id, 1, 'Cold Storage', 60, 10, 100, NOW());

-- Critical Stock Items (to test critical alerts)
INSERT INTO inventory (product_id, rdc_id, location, quantity_available, quantity_reserved, reorder_level, last_updated)
VALUES
(@tea_id, 1, 'Storage Room B', 25, 5, 100, NOW()),
(@coffee_id, 1, 'Storage Room B', 15, 0, 100, NOW());

-- =====================================================
-- STEP 4: Insert Sample Quarantine/Return Stock
-- =====================================================

-- Damaged items (for testing Damage filter)
INSERT INTO inventory (product_id, rdc_id, location, quantity_available, quantity_reserved, reorder_level, last_updated)
VALUES
(@biscuit_id, 1, 'RETURNS-HOLD-DAMAGE', 50, 0, 0, NOW()),
(@chips_id, 1, 'RETURNS-HOLD-DAMAGE', 30, 0, 0, NOW());

-- Return items (for testing Return filter and Quarantine section)
INSERT INTO inventory (product_id, rdc_id, location, quantity_available, quantity_reserved, reorder_level, last_updated)
VALUES
(@oil_id, 1, 'RETURNS-HOLD', 20, 0, 0, NOW()),
(@detergent_id, 1, 'RETURNS-HOLD', 15, 0, 0, NOW()),
(@shampoo_id, 1, 'RETURNS-HOLD', 25, 0, 0, NOW());

-- =====================================================
-- STEP 5: Insert Sample Inventory for South RDC (rdc_id = 2)
-- =====================================================

INSERT INTO inventory (product_id, rdc_id, location, quantity_available, quantity_reserved, reorder_level, last_updated)
VALUES
(@rice_id, 2, 'Main Warehouse', 950, 50, 100, NOW()),
(@sugar_id, 2, 'Main Warehouse', 1200, 0, 100, NOW()),
(@flour_id, 2, 'Main Warehouse', 850, 20, 100, NOW()),
(@oil_id, 2, 'Main Warehouse', 1100, 30, 100, NOW()),
(@tea_id, 2, 'Main Warehouse', 890, 10, 100, NOW()),
(@coffee_id, 2, 'Main Warehouse', 720, 0, 100, NOW()),
(@milk_id, 2, 'Main Warehouse', 540, 40, 100, NOW()),
(@soda_id, 2, 'Main Warehouse', 680, 20, 150, NOW());

-- =====================================================
-- STEP 6: Insert Sample Inventory for East RDC (rdc_id = 3)
-- =====================================================

INSERT INTO inventory (product_id, rdc_id, location, quantity_available, quantity_reserved, reorder_level, last_updated)
VALUES
(@detergent_id, 3, 'Main Warehouse', 520, 30, 80, NOW()),
(@dishsoap_id, 3, 'Main Warehouse', 450, 0, 80, NOW()),
(@tissue_id, 3, 'Main Warehouse', 580, 20, 100, NOW()),
(@biscuit_id, 3, 'Main Warehouse', 920, 40, 150, NOW()),
(@chips_id, 3, 'Main Warehouse', 780, 0, 150, NOW()),
(@choco_id, 3, 'Main Warehouse', 1100, 100, 200, NOW());

-- =====================================================
-- STEP 7: Insert Sample Inventory for West RDC (rdc_id = 4)
-- =====================================================

INSERT INTO inventory (product_id, rdc_id, location, quantity_available, quantity_reserved, reorder_level, last_updated)
VALUES
(@paste_id, 4, 'Main Warehouse', 620, 20, 100, NOW()),
(@shampoo_id, 4, 'Main Warehouse', 510, 10, 100, NOW()),
(@soapbar_id, 4, 'Main Warehouse', 940, 0, 150, NOW()),
(@rice_id, 4, 'Main Warehouse', 1050, 20, 100, NOW()),
(@sugar_id, 4, 'Main Warehouse', 980, 0, 100, NOW());

-- =====================================================
-- STEP 8: Insert Sample Inventory for Central RDC (rdc_id = 5)
-- =====================================================

INSERT INTO inventory (product_id, rdc_id, location, quantity_available, quantity_reserved, reorder_level, last_updated)
VALUES
(@flour_id, 5, 'Main Warehouse', 1150, 50, 100, NOW()),
(@oil_id, 5, 'Main Warehouse', 920, 0, 100, NOW()),
(@tea_id, 5, 'Main Warehouse', 1080, 30, 100, NOW()),
(@coffee_id, 5, 'Main Warehouse', 890, 10, 100, NOW()),
(@milk_id, 5, 'Main Warehouse', 710, 40, 100, NOW());

-- =====================================================
-- STEP 9: Verification Queries
-- =====================================================

-- Check total inventory count
SELECT 'Total Inventory Records' AS Info, COUNT(*) AS Count FROM inventory;

-- Check inventory by RDC
SELECT 
    CASE 
        WHEN i.rdc_id = 1 THEN 'North RDC'
        WHEN i.rdc_id = 2 THEN 'South RDC'
        WHEN i.rdc_id = 3 THEN 'East RDC'
        WHEN i.rdc_id = 4 THEN 'West RDC'
        WHEN i.rdc_id = 5 THEN 'Central RDC'
        ELSE 'Head Office'
    END AS RDC_Name,
    COUNT(*) AS Inventory_Count,
    SUM(i.quantity_available) AS Total_Available,
    SUM(i.quantity_reserved) AS Total_Reserved
FROM inventory i
GROUP BY i.rdc_id
ORDER BY i.rdc_id;

-- Check products with inventory
SELECT 
    p.product_name,
    p.sku,
    p.category,
    COUNT(i.inventory_id) AS Locations,
    SUM(i.quantity_available) AS Total_Stock
FROM products p
LEFT JOIN inventory i ON p.product_id = i.product_id
GROUP BY p.product_id
ORDER BY p.category, p.product_name;

-- Check quarantine/return stock
SELECT 
    p.product_name,
    i.location,
    i.quantity_available,
    i.rdc_id
FROM inventory i
JOIN products p ON i.product_id = p.product_id
WHERE i.location LIKE 'RETURNS%'
ORDER BY i.location, p.product_name;

-- Check low stock items (below reorder level)
SELECT 
    p.product_name,
    i.location,
    i.quantity_available,
    i.quantity_reserved,
    i.reorder_level,
    CASE 
        WHEN i.rdc_id = 1 THEN 'North RDC'
        WHEN i.rdc_id = 2 THEN 'South RDC'
        WHEN i.rdc_id = 3 THEN 'East RDC'
        WHEN i.rdc_id = 4 THEN 'West RDC'
        WHEN i.rdc_id = 5 THEN 'Central RDC'
        ELSE 'Head Office'
    END AS RDC_Name
FROM inventory i
JOIN products p ON i.product_id = p.product_id
WHERE i.quantity_available < i.reorder_level 
  AND i.location NOT LIKE 'RETURNS%'
ORDER BY i.quantity_available;

SELECT '=== Inventory Data Loaded Successfully ===' AS Status;
