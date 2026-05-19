using System;
using System.IO;
using System.Text.Json;

namespace Lab4._5.Models
{
    public class User
    {
        private static string FilePathForRole(string role) =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"user_{role.ToLower()}.json");

        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string About { get; set; }
        public DateTime RegistrationDate { get; set; }

        public bool IsAdmin => Role == "Admin";

        private static User _currentUser;
        public static User CurrentUser
        {
            get => _currentUser;
            set => _currentUser = value;
        }

        public static User LoadFromFile(string role)
        {
            string path = FilePathForRole(role);
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    return JsonSerializer.Deserialize<User>(json);
                }
            }
            catch { }
            return null;
        }

        public static void SaveToFile(User user)
        {
            try
            {
                string path = FilePathForRole(user.Role);
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(path, JsonSerializer.Serialize(user, options));
            }
            catch { }
        }

        public static User CreateDefault(string role)
        {
            return new User
            {
                Id = role == "Admin" ? 1 : 2,
                Username = role == "Admin" ? "admin" : "client",
                Password = "",
                Role = role,
                Email = "",
                DisplayName = role == "Admin" ? "Администратор" : "Клиент",
                About = "",
                RegistrationDate = DateTime.Now
            };
        }
    }
}