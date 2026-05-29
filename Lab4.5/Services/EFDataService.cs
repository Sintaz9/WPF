using Lab4._5.Database;
using Lab4._5.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lab4._5.Services
{
    public class EFDataService : IDisposable
    {
        private readonly AppDbContext _context;

        public EFDataService()
        {
            _context = new AppDbContext();
        }

        // CREATE
        public async Task<Product> AddProductAsync(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.FullName))
                product.FullName = product.Name ?? "Без названия";
            if (string.IsNullOrWhiteSpace(product.Description))
                product.Description = "";
            if (string.IsNullOrWhiteSpace(product.Color))
                product.Color = "";
            if (string.IsNullOrWhiteSpace(product.Size))
                product.Size = "";
            if (string.IsNullOrWhiteSpace(product.Country))
                product.Country = "";
            if (string.IsNullOrWhiteSpace(product.Manufacturer))
                product.Manufacturer = "";
            if (product.ImagePaths == null)
                product.ImagePaths = new System.Collections.ObjectModel.ObservableCollection<string>();
            if (product.RelatedProductIds == null)
                product.RelatedProductIds = new List<int>();

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        // READ (Чтение)
        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
        }

        public List<Product> GetAllProducts()
        {
            return _context.Products
                .Include(p => p.Category)
                .ToList();
        }

        public List<Category> GetAllCategories()
        {
            return _context.Categories.ToList();
        }

        // UPDATE 
        public async Task UpdateProductAsync(Product product)
        {
            // Находим существующий товар в БД
            var existing = await _context.Products.FindAsync(product.Id);
            if (existing == null) return;

            // Обновляем свойства
            existing.Name = product.Name;
            existing.FullName = product.FullName;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Quantity = product.Quantity;
            existing.Discount = product.Discount;
            existing.CategoryId = product.CategoryId;
            existing.Rating = product.Rating;
            existing.InStock = product.InStock;
            existing.SoldCount = product.SoldCount;
            existing.Manufacturer = product.Manufacturer;
            existing.Country = product.Country;
            existing.Color = product.Color;
            existing.Size = product.Size;
            existing.BuyCount = product.BuyCount;

            // Сохраняем изменения
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public List<Product> GetFilteredProducts(
            string searchText = null,
            int? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string sortBy = null,
            bool ascending = true)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var search = searchText.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(search) ||
                                         (p.Description != null && p.Description.ToLower().Contains(search)) ||
                                         (p.Manufacturer != null && p.Manufacturer.ToLower().Contains(search)));
            }

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "name":
                        query = ascending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name);
                        break;
                    case "price":
                        query = ascending ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price);
                        break;
                    case "rating":
                        query = ascending ? query.OrderBy(p => p.Rating) : query.OrderByDescending(p => p.Rating);
                        break;
                }
            }

            return query.ToList();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}