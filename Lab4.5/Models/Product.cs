using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Lab4._5.Models
{
    public class Product : INotifyPropertyChanged
    {
        private int _id;
        private string _name;
        private string _fullName;
        private string _description;
        private int _categoryId;
        private double _rating;
        private decimal _price;
        private int _quantity;
        private string _color;
        private string _size;
        private string _country;
        private double _discount;
        private bool _inStock;
        private int _soldCount;
        private string _manufacturer;
        private int _buyCount = 1;
        private byte[] _image; // ← НОВОЕ: массив байтов для картинки

        public int BuyCount
        {
            get => _buyCount;
            set { _buyCount = value; OnPropertyChanged(); }
        }

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

        public List<int> RelatedProductIds { get; set; } = new List<int>();

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
            set
            {
                _quantity = value;
                if (BuyCount > _quantity)
                {
                    BuyCount = _quantity > 0 ? _quantity : 1;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(InStock));
            }
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

        // НОВОЕ: картинка как массив байтов
        public byte[] Image
        {
            get => _image;
            set { _image = value; OnPropertyChanged(); }
        }

        public decimal FinalPrice => Price - (Price * (decimal)(Discount / 100));

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Buy(int count = 1)
        {
            if (Quantity >= count)
            {
                Quantity -= count;
                SoldCount += count;
                return true;
            }
            return false;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public virtual Category Category { get; set; }
    }
}