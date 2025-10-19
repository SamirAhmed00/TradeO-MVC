using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TradeO.Models;

namespace TradeO.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seeding Categories
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
            // Seeding Products
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Name = "Wireless Bluetooth Headphones",
                    Description = "High-quality wireless headphones with noise cancellation and long battery life.",
                    Seller = "TechStore",
                    Price = 99.99m,
                    DiscountPrice = 79.99m,
                    ImageUrl = "/images/products/t1.jpg",
                    CategoryId = 1
                },
                new Product
                {
                    Id = 2,
                    Name = "Smartwatch with Fitness Tracker",
                    Description = "Feature-packed smartwatch with heart rate monitor, GPS, and fitness tracking.",
                    Seller = "GadgetWorld",
                    Price = 199.99m,
                    DiscountPrice = 149.99m,
                    ImageUrl = "/images/products/t2.jpg",
                    CategoryId = 1
                },
                new Product
                {
                    Id = 3,
                    Name = "4K Ultra HD TV",
                    Description = "Experience stunning visuals with this 55-inch 4K Ultra HD television.",
                    Seller = "HomeElectronics",
                    Price = 499.99m,
                    DiscountPrice = null,
                    ImageUrl = "/images/products/t3.jpg",
                    CategoryId = 1
                },
                new Product
                {
                    Id = 4,
                    Name = "Gaming Laptop",
                    Description = "Powerful gaming laptop with high-end graphics and fast performance.",
                    Seller = "GameTech",
                    Price = 1299.99m,
                    DiscountPrice = 1199.99m,
                    ImageUrl = "/images/products/t4.jpg",
                    CategoryId = 1
                },
                new Product
                {
                    Id = 5,
                    Name = "Digital Camera",
                    Description = "Capture stunning photos and videos with this compact digital camera.",
                    Seller = "CameraShop",
                    Price = 299.99m,
                    DiscountPrice = 249.99m,
                    ImageUrl = "/images/products/t5.jpg",
                    CategoryId = 1
                }
            );
            
            // Seeding Companies            
            modelBuilder.Entity<Company>().HasData(
                new Company
                {
                    Id = 1,
                    Name = "TechCorp Solutions",
                    StreetAddress = "123 Main Street",
                    City = "Cairo",
                    State = "C",
                    PostalCode = "11511",
                    PhoneNumber = "01012345678"
                },
                new Company
                {
                    Id = 2,
                    Name = "GreenMart Supermarket",
                    StreetAddress = "45 Nile Avenue",
                    City = "Giza",
                    State = "G",
                    PostalCode = "12511",
                    PhoneNumber = "01098765432"
                },
                new Company
                {
                    Id = 3,
                    Name = "SmartHome Innovations",
                    StreetAddress = "78 Smart St",
                    City = "Alexandria",
                    State = "A",
                    PostalCode = "21111",
                    PhoneNumber = "01234567890"
                },
                new Company
                {
                    Id = 4,
                    Name = "BookWorld Publishing",
                    StreetAddress = "22 Library Road",
                    City = "Mansoura",
                    State = "M",
                    PostalCode = "35611",
                    PhoneNumber = "01122334455"
                },
                new Company
                {
                    Id = 5,
                    Name = "BeautyLine Cosmetics",
                    StreetAddress = "9 Fashion Blvd",
                    City = "Tanta",
                    State = "T",
                    PostalCode = "31711",
                    PhoneNumber = "01011223344"
                }
            );

        }

    }
}
