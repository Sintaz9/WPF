using Microsoft.Data.Sqlite;
using System;
using System.Configuration;
using System.IO;
using System.Windows;

namespace Lab4._5.Database
{
    public static class DatabaseHelper
    {
        private static string _connectionString;

        static DatabaseHelper()
        {
            // Читаем строку подключения из App.config
            var connectionStringSettings = ConfigurationManager.ConnectionStrings["CoffeeShopDb"];
            if (connectionStringSettings == null)
            {
                // Если нет в конфиге — используем значение по умолчанию
                _connectionString = "Data Source=CoffeeShop.db;Version=3;";
            }
            else
            {
                _connectionString = connectionStringSettings.ConnectionString;
            }
        }

        public static string GetConnectionString() => _connectionString;

        public static void InitializeDatabase()
        {
            try
            {
                if (!File.Exists("CoffeeShop.db"))
                {
                    CreateDatabase();
                }
                else
                {
                    // Добавляем недостающие колонки в существующую БД
                    EnsureMissingColumns();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации БД: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void EnsureMissingColumns()
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                connection.Open();

                // Проверяем и добавляем колонку BuyCount
                bool hasBuyCount = false;
                string pragmaSql = "PRAGMA table_info(Products)";
                using var pragmaCommand = new SqliteCommand(pragmaSql, connection);
                using var reader = pragmaCommand.ExecuteReader();

                while (reader.Read())
                {
                    if (reader["name"].ToString() == "BuyCount")
                    {
                        hasBuyCount = true;
                        break;
                    }
                }

                if (!hasBuyCount)
                {
                    string addColumnSql = "ALTER TABLE Products ADD COLUMN BuyCount INTEGER DEFAULT 1";
                    using var addCommand = new SqliteCommand(addColumnSql, connection);
                    addCommand.ExecuteNonQuery();
                    System.Diagnostics.Debug.WriteLine("Колонка BuyCount добавлена в таблицу Products");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при добавлении колонки: {ex.Message}");
            }
        }

        private static void CreateDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string createTablesSql = @"
                -- Таблица категорий
                CREATE TABLE Categories (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE
                );

                -- Таблица товаров (соответствует вашей модели Product)
                CREATE TABLE Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    FullName TEXT,
                    Description TEXT,
                    CategoryId INTEGER,
                    Manufacturer TEXT,
                    Country TEXT,
                    Color TEXT,
                    Size TEXT,
                    Price DECIMAL(10,2) NOT NULL,
                    Discount REAL DEFAULT 0,
                    Quantity INTEGER DEFAULT 0,
                    InStock INTEGER DEFAULT 0,
                    Rating REAL DEFAULT 0,
                    SoldCount INTEGER DEFAULT 0,
                    ImagePaths TEXT,
                    RelatedProductIds TEXT,
                    BuyCount INTEGER DEFAULT 1,
                    FOREIGN KEY(CategoryId) REFERENCES Categories(Id) ON DELETE SET NULL
                );

                -- Таблица заказов
                CREATE TABLE Orders (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    OrderDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    CustomerName TEXT NOT NULL,
                    TotalAmount DECIMAL(10,2) NOT NULL
                );

                -- Таблица позиций заказа
                CREATE TABLE OrderItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    OrderId INTEGER NOT NULL,
                    ProductId INTEGER NOT NULL,
                    ProductName TEXT NOT NULL,
                    Quantity INTEGER NOT NULL,
                    UnitPrice DECIMAL(10,2) NOT NULL,
                    FOREIGN KEY(OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
                    FOREIGN KEY(ProductId) REFERENCES Products(Id) ON DELETE RESTRICT
                );

                -- Таблица для лога удалённых товаров
                CREATE TABLE DeletedProductsLog (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductName TEXT NOT NULL,
                    ProductId INTEGER,
                    DeletedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    DeletedBy TEXT DEFAULT 'System'
                );

                -- Триггер: при удалении товара записываем в лог
                CREATE TRIGGER LogDeletedProduct
                AFTER DELETE ON Products
                BEGIN
                    INSERT INTO DeletedProductsLog (ProductName, ProductId, DeletedBy)
                    VALUES (OLD.Name, OLD.Id, 'Admin');
                END;

                -- Представление для топ-товаров
                CREATE VIEW TopProductsView AS
                SELECT 
                    p.Id,
                    p.Name,
                    COALESCE(SUM(oi.Quantity), 0) as TotalSold
                FROM Products p
                LEFT JOIN OrderItems oi ON p.Id = oi.ProductId
                GROUP BY p.Id
                ORDER BY TotalSold DESC
                LIMIT 10;
            ";

            using var command = new SqliteCommand(createTablesSql, connection);
            command.ExecuteNonQuery();

            // Заполняем категории начальными данными
            InsertDefaultCategories(connection);
        }

        private static void InsertDefaultCategories(SqliteConnection connection)
        {
            var categories = new[]
            {
                "Кофе в зернах", "Молотый кофе", "Кофе в капсулах",
                "Черный чай", "Зеленый чай", "Чай улун", "Белый чай", "Травяной чай", "Аксессуары"
            };

            foreach (var cat in categories)
            {
                string sql = "INSERT OR IGNORE INTO Categories (Name) VALUES (@name)";
                using var command = new SqliteCommand(sql, connection);
                command.Parameters.AddWithValue("@name", cat);
                command.ExecuteNonQuery();
            }
        }
    }
}