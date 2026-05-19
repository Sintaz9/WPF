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
        private List<Product> _products;
        private List<Category> _categories;

        public DataService()
        {
            _products = new List<Product>();
            _categories = Category.GetCategories();
        }

        // Получить текущие данные из памяти
        public List<Product> GetAllProducts()
        {
            return _products.ToList();
        }

        public List<Category> GetAllCategories()
        {
            return _categories.ToList();
        }

        // Операции с данными в памяти
        public void AddProduct(Product product)
        {
            if (product.Id == 0)
            {
                product.Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
            }
            _products.Add(product);
        }

        public void UpdateProduct(Product updatedProduct)
        {
            var existing = _products.FirstOrDefault(p => p.Id == updatedProduct.Id);
            if (existing != null)
            {
                var index = _products.IndexOf(existing);
                _products[index] = updatedProduct;
            }
        }

        public bool DeleteProduct(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _products.Remove(product);
                return true;
            }
            return false;
        }

        // Сохранить в файл
        public void SaveToFile(string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_products, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка сохранения: {ex.Message}");
            }
        }

        // Загрузить из файла
        public void LoadFromFile(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                _products = JsonSerializer.Deserialize<List<Product>>(json, options) ?? new List<Product>();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки: {ex.Message}");
            }
        }
    }
}