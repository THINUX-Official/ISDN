# How to Load Sample Inventory Data

## Option 1: Using MySQL Command Line

1. Open Command Prompt or PowerShell
2. Navigate to the project directory:
   ```
   cd C:\Users\USER\Desktop\thinux\07.03.2026\ISDN
   ```

3. Execute the SQL script:
   ```
   mysql -u root -p isdn_distribution_db < Database\seed_inventory_data.sql
   ```
   
4. Enter your MySQL password when prompted

## Option 2: Using MySQL Workbench

1. Open MySQL Workbench
2. Connect to your database server
3. Open the file: `Database/seed_inventory_data.sql`
4. Execute the entire script (Ctrl+Shift+Enter or click ⚡ Execute button)

## Option 3: Using phpMyAdmin

1. Open phpMyAdmin in your browser
2. Select `isdn_distribution_db` database
3. Go to "SQL" tab
4. Copy and paste the contents of `Database/seed_inventory_data.sql`
5. Click "Go" to execute

## What This Script Does

✅ **Creates 17 Sample Products** across categories:
   - Food (Rice, Sugar, Flour, Oil)
   - Beverages (Tea, Coffee, Milk, Soda)
   - Household (Detergent, Soap, Tissue)
   - Snacks (Biscuits, Chips, Chocolate)
   - Personal Care (Toothpaste, Shampoo, Soap Bar)

✅ **Populates Inventory** for all 5 RDCs:
   - North RDC (rdc_id = 1) - 17+ inventory records
   - South RDC (rdc_id = 2) - 8 inventory records
   - East RDC (rdc_id = 3) - 6 inventory records
   - West RDC (rdc_id = 4) - 5 inventory records
   - Central RDC (rdc_id = 5) - 5 inventory records

✅ **Creates Test Scenarios**:
   - Normal stock with various quantities
   - Low stock items (85-60 available, below reorder level)
   - Critical stock items (25-15 available, well below reorder)
   - Reserved quantities (10-100 reserved)
   - Quarantine stock (RETURNS-HOLD location)
   - Damaged items (RETURNS-HOLD-DAMAGE location)

✅ **Verification Queries** included at the end to check:
   - Total inventory count
   - Inventory by RDC
   - Products with inventory
   - Quarantine/return stock
   - Low stock alerts

## After Execution

1. Run the application
2. Login as RDC Staff (North RDC user)
3. Navigate to: `/RdcStaff/Inventory`
4. You should see:
   - 17+ inventory items in the main table
   - Various stock levels (In Stock, Low Stock, Critical)
   - Some items with reserved quantities
   - Quarantine section with 5 items awaiting approval
   - Working filters (All/Damage/Return)
   - Working search functionality
   - Working Edit Stock modal

## Test the Features

### Test Edit Stock:
1. Click "Edit Stock" on any item (e.g., Rice - 5kg)
2. Modal shows: Available=1100, Reserved=10
3. Add 100 to available
4. Change reserved to 20
5. Save - should update successfully

### Test Filters:
1. Click "Damage" filter - shows 2 damaged items
2. Click "Return" filter - shows 3 return items
3. Click "All" - shows everything

### Test Search:
1. Type "rice" - filters to rice items
2. Type "warehouse" - shows all warehouse locations
3. Type "low" or "critical" - shows low/critical stock items

### Test Quarantine Actions:
1. Scroll to Quarantine section
2. Click "Approve" on an item - moves to normal inventory
3. Click "Dispose" on an item - permanently removes it

## Database Summary

Total Products: 17
Total Inventory Records: ~45
RDCs Covered: 5
Test Scenarios: 8+

Enjoy testing! 🎉
