using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Lab4._5.Database;
using Lab4._5.Models;
using Microsoft.Data.Sqlite;

namespace Lab4._5.Services
{
    public class DataService
    {
        private List<Product> _products;
        private List<Category> _categories;
        private readonly ProductRepository _productRepo;
        private readonly CategoryRepository _categoryRepo;
        private bool _useDatabase = true;  // По умолчанию работаем с БД

        public DataService()
        {
            _productRepo = new ProductRepository(DatabaseHelper.GetConnectionString());
            _categoryRepo = new CategoryRepository(DatabaseHelper.GetConnectionString());
            _products = new List<Product>();
            _categories = new List<Category>();

            // Загружаем данные из БД при создании сервиса
            LoadFromDatabase();
        }

        // ========== РЕЖИМ РАБОТЫ ==========
        public bool UseDatabase
        {
            get => _useDatabase;
            set
            {
                _useDatabase = value;
                if (_useDatabase)
                    LoadFromDatabase();
                else
                    LoadFromMemory(); // из последней памяти
            }
        }

        // ========== ЗАГРУЗКА ИЗ БД ==========
        public void LoadFromDatabase()
        {
            try
            {
                _products = _productRepo.GetAll();
                _categories = _categoryRepo.GetAll();

                // Если категорий нет в БД — добавляем из статического списка
                if (_categories.Count == 0)
                {
                    var defaultCategories = Category.GetCategories();
                    // Для SQLite нужно вручную добавить через репозиторий
                    using var connection = new SqliteConnection(DatabaseHelper.GetConnectionString());
                    connection.Open();
                    foreach (var cat in defaultCategories)
                    {
                        string sql = "INSERT OR IGNORE INTO Categories (Id, Name) VALUES (@id, @name)";
                        using var cmd = new SqliteCommand(sql, connection);
                        cmd.Parameters.AddWithValue("@id", cat.Id);
                        cmd.Parameters.AddWithValue("@name", cat.Name);
                        cmd.ExecuteNonQuery();
                    }
                    _categories = _categoryRepo.GetAll();
                }

                System.Diagnostics.Debug.WriteLine($"Загружено из БД: {_products.Count} товаров, {_categories.Count} категорий");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки из БД: {ex.Message}\nРаботаем в режиме JSON",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                _useDatabase = false;
                LoadFromMemory();
            }
        }

        // ========== ЗАГРУЗКА ИЗ ПАМЯТИ (для JSON режима) ==========
        private void LoadFromMemory()
        {
            // Если в памяти пусто — создаём тестовые данные
            if (_products.Count == 0)
            {
                CreateSampleProducts();
            }
        }

        // ========== ТЕСТОВЫЕ ДАННЫЕ ДЛЯ JSON РЕЖИМА ==========
        private void CreateSampleProducts()
        {
            _products = new List<Product>
            {
                new Product { Id = 1, Name = "Эфиопия Иргачеф", Price = 450, Quantity = 10, CategoryId = 1, Rating = 4.5, Discount = 10, InStock = true },
                new Product { Id = 2, Name = "Цейлонский чай", Price = 350, Quantity = 0, CategoryId = 4, Rating = 4.2, Discount = 0, InStock = false },
                new Product { Id = 3, Name = "Кения АА", Price = 520, Quantity = 15, CategoryId = 1, Rating = 4.8, Discount = 5, InStock = true }
            };
            _categories = Category.GetCategories();
        }

        // ========== ПОЛУЧЕНИЕ ДАННЫХ ==========
        public List<Product> GetAllProducts()
        {
            return _useDatabase ? _productRepo.GetAll() : _products.ToList();
        }

        public List<Category> GetAllCategories()
        {
            return _useDatabase ? _categoryRepo.GetAll() : _categories.ToList();
        }

        // ========== CRUD ОПЕРАЦИИ ==========
        public void AddProduct(Product product)
        {
            if (_useDatabase)
            {
                using var connection = new SqliteConnection(DatabaseHelper.GetConnectionString());
                connection.Open();
                using var transaction = connection.BeginTransaction();
                try
                {
                    int newId = _productRepo.Add(product, transaction);
                    product.Id = newId;
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            else
            {
                if (product.Id == 0)
                {
                    product.Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
                }
                _products.Add(product);
            }
        }

        public void UpdateProduct(Product updatedProduct)
        {
            if (_useDatabase)
            {
                using var connection = new SqliteConnection(DatabaseHelper.GetConnectionString());
                connection.Open();
                using var transaction = connection.BeginTransaction();
                try
                {
                    _productRepo.Update(updatedProduct, transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            else
            {
                var existing = _products.FirstOrDefault(p => p.Id == updatedProduct.Id);
                if (existing != null)
                {
                    var index = _products.IndexOf(existing);
                    _products[index] = updatedProduct;
                }
            }
        }

        public bool DeleteProduct(int id)
        {
            if (_useDatabase)
            {
                using var connection = new SqliteConnection(DatabaseHelper.GetConnectionString());
                connection.Open();
                using var transaction = connection.BeginTransaction();
                try
                {
                    _productRepo.Delete(id, transaction);
                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    return false;
                }
            }
            else
            {
                var product = _products.FirstOrDefault(p => p.Id == id);
                if (product != null)
                {
                    _products.Remove(product);
                    return true;
                }
                return false;
            }
        }

        // ========== JSON ОПЕРАЦИИ (импорт/экспорт) ==========
        public void SaveToFile(string filePath)
        {
            try
            {
                var productsToSave = _useDatabase ? _productRepo.GetAll() : _products;
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(productsToSave, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка сохранения: {ex.Message}");
            }
        }

        public void LoadFromFile(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var loadedProducts = JsonSerializer.Deserialize<List<Product>>(json, options) ?? new List<Product>();

                if (_useDatabase)
                {
                    // Импортируем JSON в БД
                    using var connection = new SqliteConnection(DatabaseHelper.GetConnectionString());
                    connection.Open();
                    using var transaction = connection.BeginTransaction();
                    try
                    {
                        foreach (var product in loadedProducts)
                        {
                            // Проверяем, есть ли уже такой товар
                            var existing = _productRepo.GetById(product.Id);
                            if (existing != null)
                                _productRepo.Update(product, transaction);
                            else
                                _productRepo.Add(product, transaction);
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
                else
                {
                    _products = loadedProducts;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки: {ex.Message}");
            }
        }

        // ========== НОВЫЕ МЕТОДЫ ДЛЯ БД (для лабы 8) ==========

        // Асинхронная загрузка товаров
        public async System.Threading.Tasks.Task<List<Product>> GetAllProductsAsync()
        {
            if (_useDatabase)
                return await _productRepo.GetAllAsync();
            return await System.Threading.Tasks.Task.Run(() => _products.ToList());
        }

        // Получить топ товаров (вызов хранимой процедуры/представления)
        public List<(string Name, int TotalSold)> GetTopProducts()
        {
            if (_useDatabase)
                return _productRepo.GetTopProducts();
            // Для JSON режима — эмуляция
            return _products.OrderByDescending(p => p.SoldCount).Take(5)
                .Select(p => (p.Name, p.SoldCount)).ToList();
        }

        // Поиск по диапазону цен
        public List<Product> GetByPriceRange(decimal min, decimal max)
        {
            if (_useDatabase)
                return _productRepo.GetByPriceRange(min, max);
            return _products.Where(p => p.Price >= min && p.Price <= max).ToList();
        }
    }
}