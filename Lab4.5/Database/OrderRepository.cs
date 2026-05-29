using Lab4._5.Models;  // ЭТО ВАЖНО!
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab4._5.Database
{
    public class OrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Order> GetAll()
        {
            var orders = new List<Order>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT * FROM Orders ORDER BY OrderDate DESC";
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                orders.Add(new Order
                {
                    Id = reader.GetInt32(0),
                    OrderDate = reader.GetDateTime(1),
                    CustomerName = reader.GetString(2),
                    TotalAmount = reader.GetDecimal(3)
                });
            }
            return orders;
        }

        public async Task<List<Order>> GetAllAsync()
        {
            var orders = new List<Order>();
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "SELECT * FROM Orders ORDER BY OrderDate DESC";
            using var command = new SqliteCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                orders.Add(new Order
                {
                    Id = reader.GetInt32(0),
                    OrderDate = reader.GetDateTime(1),
                    CustomerName = reader.GetString(2),
                    TotalAmount = reader.GetDecimal(3)
                });
            }
            return orders;
        }

        public int Add(Order order, SqliteTransaction transaction = null)
        {
            string sql = @"
                INSERT INTO Orders (CustomerName, TotalAmount)
                VALUES (@customerName, @totalAmount);
                SELECT last_insert_rowid();";

            var connection = transaction?.Connection ?? new SqliteConnection(_connectionString);
            bool needClose = transaction == null;
            if (needClose) connection.Open();

            using var command = new SqliteCommand(sql, connection);
            if (transaction != null) command.Transaction = transaction;

            command.Parameters.AddWithValue("@customerName", order.CustomerName);
            command.Parameters.AddWithValue("@totalAmount", order.TotalAmount);

            int newId = Convert.ToInt32(command.ExecuteScalar());
            if (needClose) connection.Close();
            return newId;
        }
    }
}