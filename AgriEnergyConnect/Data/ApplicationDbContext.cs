using AgriEnergyConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriEnergyConnect.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Farmer> Farmers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Farmer)
                .WithMany(f => f.Products)
                .HasForeignKey(p => p.FarmerId);

            // Seed initial data for testing
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "employee1", Password = "password123", Role = "Employee" },
                new User { Id = 2, Username = "farmer1", Password = "password123", Role = "Farmer", FarmerId = 1 }
            );

            modelBuilder.Entity<Farmer>().HasData(
                new Farmer { Id = 1, FirstName = "John", LastName = "Doe", Area = "Pretoria", ContactNumber = "0123456789" }
            );
        }
    }
}