Write-Host "Loading Inventory Data..." -ForegroundColor Cyan

# Read the SQL file
$sqlFile = "Database\seed_inventory_data.sql"
$sqlContent = Get-Content $sqlFile -Raw

# Execute using mysql command
$mysqlPath = "mysql"
$database = "isdn_distribution_db"
$username = "root"

Write-Host "Executing SQL script..." -ForegroundColor Yellow

try {
    # Try with common MySQL password (you may need to change this)
    $sqlContent | & $mysqlPath -u $username -proot $database 2>&1
    
    Write-Host "`n✓ Inventory data loaded successfully!" -ForegroundColor Green
    Write-Host "Refresh your browser to see the data." -ForegroundColor Cyan
}
catch {
    Write-Host "`n✗ Error loading data. Please run this command manually:" -ForegroundColor Red
    Write-Host "mysql -u root -p isdn_distribution_db < Database\seed_inventory_data.sql" -ForegroundColor Yellow
    Write-Host "`nOr open MySQL Workbench and execute the file: Database\seed_inventory_data.sql" -ForegroundColor Yellow
}

Write-Host "`nPress any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
