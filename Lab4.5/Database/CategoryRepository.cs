using Lab4._5.Models;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace Lab4._5.Database
{
    public class CategoryRepository
    {
        private readonly string _connectionString;

        public CategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Category> GetAll()
        {
            var categories = new List<Category>();
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Id, Name FROM Categories ORDER BY Id";
            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                categories.Add(new Category
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }
            return categories;
        }
    }
}