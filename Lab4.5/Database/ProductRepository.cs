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

        public List<Product> GetAll()
        {
            var products = new List<Product>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM Products ORDER BY Id";
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                products.Add(MapToProduct(reader));
            }
            return products;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            var products = new List<Product>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "SELECT * FROM Products ORDER BY Id";
            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(MapToProduct(reader));
            }
            return products;
        }

        public Product GetById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM Products WHERE Id = @id";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
                return MapToProduct(reader);
            return null;
        }

        public int Add(Product product, SqliteTransaction transaction = null)
        {
            string sql = @"
                INSERT INTO Products (
                    Name, FullName, Description, CategoryId, 
                    Manufacturer, Country, Color, Size,
                    Price, Discount, Quantity, InStock, Rating, SoldCount,
                    Image, RelatedProductIds, BuyCount
                ) VALUES (
                    @name, @fullName, @desc, @catId,
                    @manufacturer, @country, @color, @size,
                    @price, @discount, @quantity, @inStock, @rating, @soldCount,
                    @image, @relatedIds, @buyCount
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
                    Image = @image,
                    RelatedProductIds = @relatedIds,
                    BuyCount = @buyCount
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

            // Чтение BLOB изображения
            if (!reader.IsDBNull(15))
            {
                long blobLength = reader.GetBytes(15, 0, null, 0, 0);
                if (blobLength > 0)
                {
                    byte[] imageBytes = new byte[blobLength];
                    reader.GetBytes(15, 0, imageBytes, 0, imageBytes.Length);
                    product.Image = imageBytes;
                }
            }

            // Десериализация RelatedProductIds
            if (!reader.IsDBNull(16))
            {
                var relatedJson = reader.GetString(16);
                try
                {
                    product.RelatedProductIds = JsonSerializer.Deserialize<List<int>>(relatedJson) ?? new List<int>();
                }
                catch { }
            }

            // BuyCount
            if (!reader.IsDBNull(17))
            {
                product.BuyCount = reader.GetInt32(17);
            }

            return product;
        }

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
            command.Parameters.AddWithValue("@buyCount", product.BuyCount);

            // BLOB параметр
            if (product.Image != null && product.Image.Length > 0)
            {
                command.Parameters.AddWithValue("@image", product.Image);
            }
            else
            {
                command.Parameters.AddWithValue("@image", DBNull.Value);
            }

            var relatedIdsJson = JsonSerializer.Serialize(product.RelatedProductIds ?? new List<int>());
            command.Parameters.AddWithValue("@relatedIds", relatedIdsJson);
        }
        // ========== ДОБАВИТЬ ПЕРЕД ПОСЛЕДНЕЙ СКОБКОЙ ==========

        // Получить топ-товары (из представления TopProductsView)
        public List<(string Name, int TotalSold)> GetTopProducts()
        {
            var result = new List<(string, int)>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Name, TotalSold FROM TopProductsView LIMIT 10";
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add((reader.GetString(0), reader.GetInt32(1)));
            }
            return result;
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
    }
}