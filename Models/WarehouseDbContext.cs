using System.Data.Entity;

namespace Warehouse_25DH110382.Models
{
    public class WarehouseDbContext : DbContext
    {
        public WarehouseDbContext() : base("name=WarehouseConnection")
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}