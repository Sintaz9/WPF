using Lab4._5.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab4._5.Database
{
    public class AppDbContext : DbContext
    {
        // Таблицы БД (только нужные для EF, без Orders и OrderItems)
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        // Отдельный файл БД для EF (не трогает CoffeeShop.db из лабы 8)
        private static readonly string DbPath = "CoffeeShop.db";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={DbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Связь "один ко многим": Category → Products
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Начальные данные для категорий
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Кофе в зернах" },
                new Category { Id = 2, Name = "Молотый кофе" },
                new Category { Id = 3, Name = "Кофе в капсулах" },
                new Category { Id = 4, Name = "Черный чай" },
                new Category { Id = 5, Name = "Зеленый чай" },
                new Category { Id = 6, Name = "Чай улун" },
                new Category { Id = 7, Name = "Белый чай" },
                new Category { Id = 8, Name = "Травяной чай" },
                new Category { Id = 9, Name = "Аксессуары" }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}