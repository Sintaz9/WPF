using Lab4._5.Models;  // ЭТО ВАЖНО!
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace Lab4._5.Database
{
    public class OrderItemRepository
    {
        private readonly string _connectionString;

        public OrderItemRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<OrderItem> GetAll()
        {
            var items = new List<OrderItem>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM OrderItems";
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                items.Add(new OrderItem
                {
                    Id = reader.GetInt32(0),
                    OrderId = reader.GetInt32(1),
                    ProductId = reader.GetInt32(2),
                    ProductName = reader.GetString(3),
                    Quantity = reader.GetInt32(4),
                    UnitPrice = reader.GetDecimal(5)
                });
            }
            return items;
        }

        public List<OrderItem> GetByOrderId(int orderId)
        {
            var items = new List<OrderItem>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM OrderItems WHERE OrderId = @orderId";
            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@orderId", orderId);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                items.Add(new OrderItem
                {
                    Id = reader.GetInt32(0),
                    OrderId = reader.GetInt32(1),
                    ProductId = reader.GetInt32(2),
                    ProductName = reader.GetString(3),
                    Quantity = reader.GetInt32(4),
                    UnitPrice = reader.GetDecimal(5)
                });
            }
            return items;
        }

        public int Add(OrderItem item, SqliteTransaction transaction = null)
        {
            string sql = @"
                INSERT INTO OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPrice)
                VALUES (@orderId, @productId, @productName, @quantity, @unitPrice);
                SELECT last_insert_rowid();";

            var connection = transaction?.Connection ?? new SqliteConnection(_connectionString);
            bool needClose = transaction == null;
            if (needClose) connection.Open();

            using var command = new SqliteCommand(sql, connection);
            if (transaction != null) command.Transaction = transaction;

            command.Parameters.AddWithValue("@orderId", item.OrderId);
            command.Parameters.AddWithValue("@productId", item.ProductId);
            command.Parameters.AddWithValue("@productName", item.ProductName);
            command.Parameters.AddWithValue("@quantity", item.Quantity);
            command.Parameters.AddWithValue("@unitPrice", item.UnitPrice);

            int newId = Convert.ToInt32(command.ExecuteScalar());
            if (needClose) connection.Close();
            return newId;
        }
    }
}