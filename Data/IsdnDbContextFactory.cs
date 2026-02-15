using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ISDN.Data
{
    public class IsdnDbContextFactory : IDesignTimeDbContextFactory<IsdnDbContext>
    {
        public IsdnDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IsdnDbContext>();

            // Use a connection string for design-time only
            var connectionString = "Server=localhost;Port=3306;Database=isdn_distribution_db;Uid=root;Pwd=#DRC123aa;";
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)));

            return new IsdnDbContext(optionsBuilder.Options);
        }
    }
}
