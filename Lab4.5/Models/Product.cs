using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lab4._5.Models
{
    /// <summary>
    /// Модель товара (чай или кофе)
    /// </summary>
    public class Product : INotifyPropertyChanged
    {
        private int _id;
        private string _name;              // краткое название
        private string _fullName;          // полное название
        private string _description;       // описание
        private int _categoryId;           // категория
        private double _rating;            // рейтинг 0-5
        private decimal _price;            // цена
        private int _quantity;             // количество на складе
        private string _color;             // цвет упаковки
        private string _size;              // размер (250г, 500г)
        private string _country;           // страна происхождения
        private double _discount;          // скидка %
        private bool _inStock;             // в наличии
        private int _soldCount;            // количество проданных
        private string _manufacturer;      // производитель

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public int CategoryId
        {
            get => _categoryId;
            set { _categoryId = value; OnPropertyChanged(); }
        }

        public double Rating
        {
            get => _rating;
            set { _rating = value; OnPropertyChanged(); }
        }

        public decimal Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); OnPropertyChanged(nameof(FinalPrice)); }
        }

        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(InStock)); }
        }

        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(); }
        }

        public string Size
        {
            get => _size;
            set { _size = value; OnPropertyChanged(); }
        }

        public string Country
        {
            get => _country;
            set { _country = value; OnPropertyChanged(); }
        }

        public double Discount
        {
            get => _discount;
            set { _discount = value; OnPropertyChanged(); OnPropertyChanged(nameof(FinalPrice)); }
        }

        public bool InStock
        {
            get => _inStock && Quantity > 0;
            set { _inStock = value; OnPropertyChanged(); }
        }

        public int SoldCount
        {
            get => _soldCount;
            set { _soldCount = value; OnPropertyChanged(); }
        }

        public string Manufacturer
        {
            get => _manufacturer;
            set { _manufacturer = value; OnPropertyChanged(); }
        }

        // Цена со скидкой (вычисляемое свойство)
        public decimal FinalPrice => Price - (Price * (decimal)(Discount / 100));

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}