using System.Collections.Generic;

namespace Lab4._5.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static List<Category> GetCategories()
        {
            return new List<Category>
            {
                new Category { Id = 1, Name = "Кофе в зернах" },
                new Category { Id = 2, Name = "Молотый кофе" },
                new Category { Id = 3, Name = "Кофе в капсулах" },
                new Category { Id = 4, Name = "Черный чай" },
                new Category { Id = 5, Name = "Зеленый чай" },
                new Category { Id = 6, Name = "Чай улун" },
                new Category { Id = 7, Name = "Белый чай" },
                new Category { Id = 8, Name = "Травяной чай" },
                new Category { Id = 9, Name = "Аксессуары" }
            };
        }
    }
}