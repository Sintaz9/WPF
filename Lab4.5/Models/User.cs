using System;

namespace Lab4._5.Models
{
    /// <summary>
    /// Пользователь приложения (администратор или клиент)
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }     // В реальном проекте нужно хешировать!
        public string Role { get; set; }         // "Admin" или "Client"
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Получить тестового администратора
        /// </summary>
        public static User GetAdminUser()
        {
            return new User
            {
                Id = 1,
                Username = "admin",
                Password = "admin123",
                Role = "Admin",
                Email = "admin@Lab4._5.com",
                RegistrationDate = DateTime.Now
            };
        }

        /// <summary>
        /// Получить тестового клиента
        /// </summary>
        public static User GetClientUser()
        {
            return new User
            {
                Id = 2,
                Username = "client",
                Password = "client123",
                Role = "Client",
                Email = "client@example.com",
                RegistrationDate = DateTime.Now
            };
        }

        /// <summary>
        /// Проверка, является ли пользователь администратором
        /// </summary>
        public bool IsAdmin => Role == "Admin";
    }
}