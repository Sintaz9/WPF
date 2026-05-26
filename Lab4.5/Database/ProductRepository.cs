using Lab4._5.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lab4._5.Database
{
    public class ProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Получить все товары
        public List<Product> GetAll()
        {
            var products = new List<Product>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = @"
                SELECT p.*, c.Name as CategoryName 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                ORDER BY p.Id";

            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                products.Add(MapToProduct(reader));
            }
            return products;
        }

        // Асинхронная версия
        public async Task<List<Product>> GetAllAsync()
        {
            var products = new List<Product>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                SELECT p.*, c.Name as CategoryName 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                ORDER BY p.Id";

            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(MapToProduct(reader));
            }
            return products;
        }

        // Получить один товар по ID
        public Product GetById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = @"
                SELECT p.*, c.Name as CategoryName 
                FROM Products p
                LEFT JOIN Categories c ON p.CategoryId = c.Id
                WHERE p.Id = @id";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
                return MapToProduct(reader);

            return null;
        }

        // Добавить товар (с поддержкой транзакции)
        public int Add(Product product, SqliteTransaction transaction = null)
        {
            string sql = @"
                INSERT INTO Products (
                    Name, FullName, Description, CategoryId, 
                    Manufacturer, Country, Color, Size,
                    Price, Discount, Quantity, InStock, Rating, SoldCount,
                    ImagePaths, RelatedProductIds
                ) VALUES (
                    @name, @fullName, @desc, @catId,
                    @manufacturer, @country, @color, @size,
                    @price, @discount, @quantity, @inStock, @rating, @soldCount,
                    @imagePaths, @relatedIds
                );
                SELECT last_insert_rowid();";

            var connection = transaction?.Connection ?? new SqliteConnection(_connectionString);
            bool needClose = transaction == null;

            if (needClose)
                connection.Open();

            using var command = new SqliteCommand(sql, connection);
            if (transaction != null)
                command.Transaction = transaction;

            AddParameters(command, product);
            int newId = Convert.ToInt32(command.ExecuteScalar());

            if (needClose)
                connection.Close();

            return newId;
        }

        // Обновить товар
        public void Update(Product product, SqliteTransaction transaction = null)
        {
            string sql = @"
                UPDATE Products SET
                    Name = @name,
                    FullName = @fullName,
                    Description = @desc,
                    CategoryId = @catId,
                    Manufacturer = @manufacturer,
                    Country = @country,
                    Color = @color,
                    Size = @size,
                    Price = @price,
                    Discount = @discount,
                    Quantity = @quantity,
                    InStock = @inStock,
                    Rating = @rating,
                    SoldCount = @soldCount,
                    ImagePaths = @imagePaths,
                    RelatedProductIds = @relatedIds
                WHERE Id = @id";

            var connection = transaction?.Connection ?? new SqliteConnection(_connectionString);
            bool needClose = transaction == null;

            if (needClose)
                connection.Open();

            using var command = new SqliteCommand(sql, connection);
            if (transaction != null)
                command.Transaction = transaction;

            AddParameters(command, product);
            command.Parameters.AddWithValue("@id", product.Id);

            command.ExecuteNonQuery();

            if (needClose)
                connection.Close();
        }

        // Удалить товар
        public void Delete(int id, SqliteTransaction transaction = null)
        {
            string sql = "DELETE FROM Products WHERE Id = @id";

            var connection = transaction?.Connection ?? new SqliteConnection(_connectionString);
            bool needClose = transaction == null;

            if (needClose)
                connection.Open();

            using var command = new SqliteCommand(sql, connection);
            if (transaction != null)
                command.Transaction = transaction;

            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();

            if (needClose)
                connection.Close();
        }

        // Поиск по диапазону цен
        public List<Product> GetByPriceRange(decimal minPrice, decimal maxPrice)
        {
            var products = new List<Product>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM Products WHERE Price BETWEEN @min AND @max";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@min", minPrice);
            command.Parameters.AddWithValue("@max", maxPrice);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                products.Add(MapToProduct(reader));
            }
            return products;
        }

        // Вызов представления (эмуляция хранимой процедуры)
        public List<(string Name, int TotalSold)> GetTopProducts()
        {
            var result = new List<(string, int)>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Name, TotalSold FROM TopProductsView";
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add((reader.GetString(0), reader.GetInt32(1)));
            }
            return result;
        }

        // Маппинг из SqliteDataReader в Product
        private Product MapToProduct(SqliteDataReader reader)
        {
            var product = new Product
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                FullName = reader.IsDBNull(2) ? null : reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                CategoryId = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                Manufacturer = reader.IsDBNull(5) ? null : reader.GetString(5),
                Country = reader.IsDBNull(6) ? null : reader.GetString(6),
                Color = reader.IsDBNull(7) ? null : reader.GetString(7),
                Size = reader.IsDBNull(8) ? null : reader.GetString(8),
                Price = reader.GetDecimal(9),
                Discount = reader.IsDBNull(10) ? 0 : reader.GetDouble(10),
                Quantity = reader.GetInt32(11),
                InStock = reader.GetInt32(12) == 1,
                Rating = reader.IsDBNull(13) ? 0 : reader.GetDouble(13),
                SoldCount = reader.IsDBNull(14) ? 0 : reader.GetInt32(14),
            };

            // Десериализация JSON-полей
            if (!reader.IsDBNull(15))
            {
                var imagePathsJson = reader.GetString(15);
                try
                {
                    var paths = JsonSerializer.Deserialize<List<string>>(imagePathsJson);
                    if (paths != null)
                    {
                        foreach (var path in paths)
                            product.ImagePaths.Add(path);
                    }
                }
                catch { }
            }

            if (!reader.IsDBNull(16))
            {
                var relatedJson = reader.GetString(16);
                try
                {
                    product.RelatedProductIds = JsonSerializer.Deserialize<List<int>>(relatedJson) ?? new List<int>();
                }
                catch { }
            }

            return product;
        }

        // Добавление параметров в команду
        private void AddParameters(SqliteCommand command, Product product)
        {
            command.Parameters.AddWithValue("@name", product.Name ?? "");
            command.Parameters.AddWithValue("@fullName", product.FullName ?? "");
            command.Parameters.AddWithValue("@desc", product.Description ?? "");
            command.Parameters.AddWithValue("@catId", product.CategoryId == 0 ? (object)DBNull.Value : product.CategoryId);
            command.Parameters.AddWithValue("@manufacturer", product.Manufacturer ?? "");
            command.Parameters.AddWithValue("@country", product.Country ?? "");
            command.Parameters.AddWithValue("@color", product.Color ?? "");
            command.Parameters.AddWithValue("@size", product.Size ?? "");
            command.Parameters.AddWithValue("@price", product.Price);
            command.Parameters.AddWithValue("@discount", product.Discount);
            command.Parameters.AddWithValue("@quantity", product.Quantity);
            command.Parameters.AddWithValue("@inStock", product.InStock ? 1 : 0);
            command.Parameters.AddWithValue("@rating", product.Rating);
            command.Parameters.AddWithValue("@soldCount", product.SoldCount);

            // Сериализуем коллекции в JSON
            var imagePathsJson = JsonSerializer.Serialize(product.ImagePaths?.ToList() ?? new List<string>());
            command.Parameters.AddWithValue("@imagePaths", imagePathsJson);

            var relatedIdsJson = JsonSerializer.Serialize(product.RelatedProductIds ?? new List<int>());
            command.Parameters.AddWithValue("@relatedIds", relatedIdsJson);
        }
    }
}