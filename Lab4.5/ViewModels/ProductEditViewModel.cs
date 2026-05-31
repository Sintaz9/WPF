using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Lab4._5.Models;

namespace Lab4._5.ViewModels
{
    public class ProductEditViewModel : INotifyPropertyChanged
    {
        private Product _originalProduct;
        private bool _isEditMode;
        private byte[] _image;

        private string _name;
        private string _fullName;
        private string _description;
        private Category _selectedCategory;
        private string _priceText;
        private string _discountText;
        private string _quantityText;
        private string _country;
        private string _color;
        private string _size;
        private string _manufacturer;
        private string _ratingText;

        private string _nameError;
        private string _fullNameError;
        private string _descriptionError;
        private string _priceError;
        private string _discountError;
        private string _quantityError;
        private string _ratingError;

        public ProductEditViewModel(Product product = null)
        {
            Categories = new ObservableCollection<Category>(Category.GetCategories());

            if (product != null)
            {
                _originalProduct = product;
                _isEditMode = true;
                LoadProduct(product);
            }
            else
            {
                _isEditMode = false;
                SelectedCategory = Categories.FirstOrDefault();
                RatingText = "0";
                DiscountText = "0";
                QuantityText = "0";
                PriceText = "0";
            }
        }

        public ObservableCollection<Category> Categories { get; set; }

        public string WindowTitle => _isEditMode ? "Редактирование товара" : "Добавление товара";

        public string Name { get => _name; set { _name = value; OnPropertyChanged(); ValidateName(); } }
        public string FullName { get => _fullName; set { _fullName = value; OnPropertyChanged(); ValidateFullName(); } }
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); ValidateDescription(); } }
        public Category SelectedCategory { get => _selectedCategory; set { _selectedCategory = value; OnPropertyChanged(); } }
        public string PriceText { get => _priceText; set { _priceText = value; OnPropertyChanged(); ValidatePrice(); } }
        public string DiscountText { get => _discountText; set { _discountText = value; OnPropertyChanged(); ValidateDiscount(); } }
        public string QuantityText { get => _quantityText; set { _quantityText = value; OnPropertyChanged(); ValidateQuantity(); } }
        public string Country { get => _country; set { _country = value; OnPropertyChanged(); } }
        public string Color { get => _color; set { _color = value; OnPropertyChanged(); } }
        public string Size { get => _size; set { _size = value; OnPropertyChanged(); } }
        public string Manufacturer { get => _manufacturer; set { _manufacturer = value; OnPropertyChanged(); } }
        public string RatingText { get => _ratingText; set { _ratingText = value; OnPropertyChanged(); ValidateRating(); } }

        public byte[] Image { get => _image; set { _image = value; OnPropertyChanged(); } }
        public bool HasImage => _image != null && _image.Length > 0;

        public string NameError { get => _nameError; set { _nameError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNameError)); } }
        public bool HasNameError => !string.IsNullOrEmpty(NameError);
        public string FullNameError { get => _fullNameError; set { _fullNameError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasFullNameError)); } }
        public bool HasFullNameError => !string.IsNullOrEmpty(FullNameError);
        public string DescriptionError { get => _descriptionError; set { _descriptionError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasDescriptionError)); } }
        public bool HasDescriptionError => !string.IsNullOrEmpty(DescriptionError);
        public string PriceError { get => _priceError; set { _priceError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasPriceError)); } }
        public bool HasPriceError => !string.IsNullOrEmpty(PriceError);
        public string DiscountError { get => _discountError; set { _discountError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasDiscountError)); } }
        public bool HasDiscountError => !string.IsNullOrEmpty(DiscountError);
        public string QuantityError { get => _quantityError; set { _quantityError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasQuantityError)); } }
        public bool HasQuantityError => !string.IsNullOrEmpty(QuantityError);
        public string RatingError { get => _ratingError; set { _ratingError = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasRatingError)); } }
        public bool HasRatingError => !string.IsNullOrEmpty(RatingError);

        private void ValidateName()
        {
            if (string.IsNullOrWhiteSpace(Name))
                NameError = "Название обязательно";
            else if (Name.Length < 2)
                NameError = "Название должно быть не менее 2 символов";
            else
                NameError = null;
        }

        private void ValidateFullName()
        {
            if (string.IsNullOrWhiteSpace(FullName))
                FullNameError = "Полное название обязательно";
            else
                FullNameError = null;
        }

        private void ValidateDescription()
        {
            if (string.IsNullOrWhiteSpace(Description))
                DescriptionError = "Описание обязательно";
            else if (Description.Length < 10)
                DescriptionError = "Описание должно быть не менее 10 символов";
            else
                DescriptionError = null;
        }

        private void ValidatePrice()
        {
            if (string.IsNullOrWhiteSpace(PriceText))
                PriceError = "Цена обязательна";
            else if (!decimal.TryParse(PriceText, out decimal price) || price < 0)
                PriceError = "Введите корректную цену";
            else
                PriceError = null;
        }

        private void ValidateDiscount()
        {
            if (string.IsNullOrWhiteSpace(DiscountText))
                DiscountError = null;
            else if (!double.TryParse(DiscountText, out double discount) || discount < 0 || discount > 100)
                DiscountError = "Скидка должна быть от 0 до 100";
            else
                DiscountError = null;
        }

        private void ValidateQuantity()
        {
            if (string.IsNullOrWhiteSpace(QuantityText))
                QuantityError = "Количество обязательно";
            else if (!int.TryParse(QuantityText, out int quantity) || quantity < 0)
                QuantityError = "Введите корректное количество";
            else
                QuantityError = null;
        }

        private void ValidateRating()
        {
            if (string.IsNullOrWhiteSpace(RatingText))
                RatingError = null;
            else if (!double.TryParse(RatingText, out double rating) || rating < 0 || rating > 5)
                RatingError = "Рейтинг должен быть от 0 до 5";
            else
                RatingError = null;
        }

        public bool IsValid()
        {
            ValidateName();
            ValidateFullName();
            ValidateDescription();
            ValidatePrice();
            ValidateDiscount();
            ValidateQuantity();
            ValidateRating();
            return !HasNameError && !HasFullNameError && !HasDescriptionError &&
                   !HasPriceError && !HasDiscountError && !HasQuantityError && !HasRatingError;
        }

        public void AddImage(byte[] imageBytes)
        {
            Image = imageBytes;
        }

        public void RemoveImage()
        {
            Image = null;
        }

        private void LoadProduct(Product product)
        {
            Name = product.Name;
            FullName = product.FullName;
            Description = product.Description;
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == product.CategoryId);
            PriceText = product.Price.ToString();
            DiscountText = product.Discount.ToString();
            QuantityText = product.Quantity.ToString();
            Country = product.Country;
            Color = product.Color;
            Size = product.Size;
            Manufacturer = product.Manufacturer;
            RatingText = product.Rating.ToString();
            Image = product.Image;
        }

        public Product GetProduct()
        {
            var product = _originalProduct ?? new Product();

            product.Name = Name;
            product.FullName = FullName;
            product.Description = Description;
            product.CategoryId = SelectedCategory?.Id ?? 1;
            product.Price = decimal.TryParse(PriceText, out decimal price) ? price : 0;
            product.Discount = double.TryParse(DiscountText, out double discount) ? discount : 0;
            product.Quantity = int.TryParse(QuantityText, out int quantity) ? quantity : 0;
            product.InStock = product.Quantity > 0;
            product.Country = Country ?? "";
            product.Color = Color ?? "";
            product.Size = Size ?? "";
            product.Manufacturer = Manufacturer ?? "";
            product.Rating = double.TryParse(RatingText, out double rating) ? rating : 0;
            product.Image = Image;

            if (!_isEditMode)
            {
                product.Id = new Random().Next(1000, 9999);
            }

            return product;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}