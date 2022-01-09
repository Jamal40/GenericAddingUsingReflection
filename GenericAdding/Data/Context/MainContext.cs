using GenericAdding.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GenericAdding.Data.Context
{
    public class MainContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public MainContext(DbContextOptions<MainContext> options) : base(options) { }
    }
}
