using Microsoft.EntityFrameworkCore;
using TradeO.Models;
namespace TradeO.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }


        // Seeding Categories
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Electronics", DisplayOrder = 1 },
                new Category { Id = 2, Name = "Fashion", DisplayOrder = 2 },
                new Category { Id = 3, Name = "Home & Kitchen", DisplayOrder = 3 },
                new Category { Id = 4, Name = "Books", DisplayOrder = 4 },
                new Category { Id = 5, Name = "Beauty & Personal Care", DisplayOrder = 5 },
                new Category { Id = 6, Name = "Sports & Outdoors", DisplayOrder = 6 },
                new Category { Id = 7, Name = "Toys & Games", DisplayOrder = 7 },
                new Category { Id = 8, Name = "Groceries", DisplayOrder = 8 }
            );
        }
    }
}
