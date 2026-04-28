using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Lab4._5.Models;

namespace Lab4._5.Services
{
    public class DataService
    {
        private readonly string _dataFilePath;
        private List<Product> _products;
        private List<Category> _categories;

        public DataService()
        {
            string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(dataDir))
                Directory.CreateDirectory(dataDir);

            _dataFilePath = Path.Combine(dataDir, "products.json");
            LoadData();
        }

        private void LoadData()
        {
            _categories = Category.GetCategories();

            if (!File.Exists(_dataFilePath))
            {
                CreateSampleData();
                SaveData();
            }
            else
            {
                try
                {
                    string json = File.ReadAllText(_dataFilePath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    _products = JsonSerializer.Deserialize<List<Product>>(json, options) ?? new List<Product>();
                }
                catch
                {
                    CreateSampleData();
                }
            }
        }

        private void CreateSampleData()
        {
            _products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Эфиопская Иргачефф",
                    FullName = "Кофе в зернах Эфиопская Иргачефф, 250г",
                    Description = "Изысканный кофе с нотами жасмина и цитрусов.",
                    CategoryId = 1,
                    Rating = 4.8,
                    Price = 890m,
                    Quantity = 45,
                    Color = "Коричневый",
                    Size = "250г",
                    Country = "Эфиопия",
                    Discount = 0,
                    InStock = true,
                    SoldCount = 128,
                    Manufacturer = "Ethiopian Coffee"
                },
                new Product
                {
                    Id = 2,
                    Name = "Колумбия Супремо",
                    FullName = "Кофе в зернах Колумбия Супремо, 500г",
                    Description = "Классическая арабика с нотами карамели и шоколада.",
                    CategoryId = 1,
                    Rating = 4.6,
                    Price = 1250m,
                    Quantity = 32,
                    Color = "Коричневый",
                    Size = "500г",
                    Country = "Колумбия",
                    Discount = 10,
                    InStock = true,
                    SoldCount = 95,
                    Manufacturer = "Colombian Coffee"
                },
                new Product
                {
                    Id = 3,
                    Name = "Дай Хун Пао",
                    FullName = "Красный халат, улун 100г",
                    Description = "Знаменитый китайский улун с глубоким вкусом.",
                    CategoryId = 6,
                    Rating = 4.9,
                    Price = 2350m,
                    Quantity = 18,
                    Color = "Темно-коричневый",
                    Size = "100г",
                    Country = "Китай",
                    Discount = 0,
                    InStock = true,
                    SoldCount = 42,
                    Manufacturer = "Wuyi Star Tea"
                }
            };
        }

        private void SaveData()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_products, options);
                File.WriteAllText(_dataFilePath, json);
            }
            catch { }
        }

        public void AddProduct(Product product)
        {
            if (product.Id == 0)
            {
                product.Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
            }
            _products.Add(product);
            SaveData();
        }

        public void UpdateProduct(Product updatedProduct)
        {
            var existing = _products.FirstOrDefault(p => p.Id == updatedProduct.Id);
            if (existing != null)
            {
                var index = _products.IndexOf(existing);
                _products[index] = updatedProduct;
                SaveData();
            }
        }

        public List<Product> GetAllProducts()
        {
            return _products.ToList();
        }

        public List<Category> GetAllCategories()
        {
            return _categories.ToList();
        }

        public void SaveProducts(List<Product> products)
        {
            _products = products;
            SaveData();
        }
    }
}