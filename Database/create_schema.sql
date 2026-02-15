-- ISDN Database Schema
-- Run this script in MySQL Workbench to create the database and tables
-- Create database if it doesn't exist
CREATE DATABASE IF NOT EXISTS isdn_distribution_db;
USE isdn_distribution_db;
-- Products table
CREATE TABLE IF NOT EXISTS Products (
    ProductId INT AUTO_INCREMENT PRIMARY KEY,
    ProductName VARCHAR(100) NOT NULL,
    Description VARCHAR(500),
    UnitPrice DECIMAL(18, 2) NOT NULL,
    StockQuantity INT NOT NULL,
    Category VARCHAR(50),
    IsActive TINYINT(1) NOT NULL DEFAULT 1,
    CreatedDate DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ModifiedDate DATETIME NULL
);
-- Orders table
CREATE TABLE IF NOT EXISTS Orders (
    OrderId INT AUTO_INCREMENT PRIMARY KEY,
    OrderNumber VARCHAR(50) NOT NULL,
    OrderDate DATETIME NOT NULL,
    TotalAmount DECIMAL(18, 2) NOT NULL,
    UserId INT NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'Pending'
);
-- OrderItems table
CREATE TABLE IF NOT EXISTS OrderItems (
    OrderItemId INT AUTO_INCREMENT PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    Subtotal DECIMAL(18, 2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE RESTRICT
);
-- Insert sample data
INSERT INTO Products (
        ProductName,
        Description,
        Category,
        UnitPrice,
        StockQuantity,
        IsActive
    )
VALUES (
        'Wireless Mouse',
        'Ergonomic wireless mouse with 2.4GHz connectivity and long battery life',
        'Electronics',
        29.99,
        50,
        1
    ),
    (
        'Mechanical Keyboard',
        'RGB backlit mechanical keyboard with blue switches',
        'Electronics',
        89.99,
        30,
        1
    ),
    (
        'USB-C Hub',
        '7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader',
        'Electronics',
        45.99,
        25,
        1
    ),
    (
        'Laptop Stand',
        'Adjustable aluminum laptop stand for better ergonomics',
        'Accessories',
        39.99,
        40,
        1
    ),
    (
        'Webcam HD 1080p',
        'Full HD webcam with built-in microphone and auto-focus',
        'Electronics',
        69.99,
        20,
        1
    ),
    (
        'Desk Organizer',
        'Bamboo desk organizer with multiple compartments',
        'Office Supplies',
        24.99,
        60,
        1
    ),
    (
        'LED Desk Lamp',
        'Adjustable LED desk lamp with touch control and USB charging port',
        'Lighting',
        34.99,
        35,
        1
    ),
    (
        'Noise Cancelling Headphones',
        'Over-ear headphones with active noise cancellation and 30-hour battery',
        'Electronics',
        149.99,
        15,
        1
    ),
    (
        'Portable SSD 1TB',
        'Ultra-fast portable SSD with USB 3.2 Gen 2 interface',
        'Storage',
        119.99,
        22,
        1
    ),
    (
        'Monitor Arm Mount',
        'Adjustable monitor arm mount for screens up to 32 inches',
        'Accessories',
        79.99,
        18,
        1
    ),
    (
        'Wireless Charger',
        'Fast wireless charging pad compatible with Qi-enabled devices',
        'Electronics',
        19.99,
        0,
        0
    ),
    (
        'Cable Management Kit',
        'Complete cable management solution with clips, sleeves, and ties',
        'Accessories',
        14.99,
        75,
        1
    );
SELECT 'Database and tables created successfully!' AS Status;