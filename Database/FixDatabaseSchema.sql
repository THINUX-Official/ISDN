-- Fix Database Schema Mismatch
-- 1. Add missing StockQuantity column to products table
ALTER TABLE products
ADD COLUMN stock_quantity INT NOT NULL DEFAULT 0;
-- 2. Populate stock_quantity from inventory table (if inventory exists)
UPDATE products p
    JOIN inventory i ON p.product_id = i.product_id
SET p.stock_quantity = i.quantity_available;
-- 3. Add modified_date column
ALTER TABLE products
ADD COLUMN modified_date DATETIME NULL;
-- 4. Check results
SELECT product_id,
    product_name,
    stock_quantity,
    product_image_url
FROM products
LIMIT 5;