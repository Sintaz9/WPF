using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Lab4._5.Commands;
using Lab4._5.Models;
using Lab4._5.Services;
using Lab4._5.Views;

namespace Lab4._5.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly DataService _dataService;
        private ObservableCollection<Product> _products;
        private ObservableCollection<Category> _categories;
        private Product _selectedProduct;
        private Category _selectedCategory;
        private string _searchText;
        private string _statusText;
        private bool _isAdminMode;

        // Новые свойства для фильтрации
        private string _priceFromText;
        private string _priceToText;
        private bool? _inStockFilter;
        private double? _minRatingFilter;
        private string _sortBy;
        private string _sortDirection;

        public MainViewModel()
        {
            _dataService = new DataService();

            // Инициализация команд
            LoadDataCommand = new RelayCommand(_ => LoadData());
            AddProductCommand = new RelayCommand(_ => AddProduct(), _ => IsAdminMode);
            EditProductCommand = new RelayCommand(_ => EditProduct(), _ => IsAdminMode && SelectedProduct != null);
            DeleteProductCommand = new RelayCommand(_ => DeleteProduct(), _ => IsAdminMode && SelectedProduct != null);
            SaveCommand = new RelayCommand(_ => SaveData());
            FilterByCategoryCommand = new RelayCommand(_ => ApplyFilters());
            ClearFilterCommand = new RelayCommand(_ => ClearFilters());
            SearchCommand = new RelayCommand(_ => ApplyFilters());
            ToggleRoleCommand = new RelayCommand(_ => ToggleRole());
            ShowDetailsCommand = new RelayCommand(ShowDetails, CanShowDetails);
            ApplyFiltersCommand = new RelayCommand(_ => ApplyFilters());
            SortCommand = new RelayCommand(param => SortProducts(param?.ToString()));

            // Загрузка данных
            LoadData();
        }

        #region Properties
        private int _inStockFilterIndex;
        private string _ratingFilterText;

        public int InStockFilterIndex
        {
            get => _inStockFilterIndex;
            set
            {
                _inStockFilterIndex = value;
                OnPropertyChanged();
            }
        }

        public string RatingFilterText
        {
            get => _ratingFilterText;
            set { _ratingFilterText = value; OnPropertyChanged(); }
        }
        public ObservableCollection<Product> Products
        {
            get => _products;
            set { _products = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set { _categories = value; OnPropertyChanged(); }
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public bool IsAdminMode
        {
            get => _isAdminMode;
            set
            {
                _isAdminMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RoleButtonText));
                OnPropertyChanged(nameof(CurrentRole));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // Новые свойства фильтрации
        public string PriceFromText
        {
            get => _priceFromText;
            set { _priceFromText = value; OnPropertyChanged(); }
        }

        public string PriceToText
        {
            get => _priceToText;
            set { _priceToText = value; OnPropertyChanged(); }
        }

        public bool? InStockFilter
        {
            get => _inStockFilter;
            set { _inStockFilter = value; OnPropertyChanged(); }
        }

        public double? MinRatingFilter
        {
            get => _minRatingFilter;
            set { _minRatingFilter = value; OnPropertyChanged(); }
        }

        public string SortBy
        {
            get => _sortBy;
            set { _sortBy = value; OnPropertyChanged(); }
        }

        public string SortDirection
        {
            get => _sortDirection;
            set { _sortDirection = value; OnPropertyChanged(); }
        }

        public string RoleButtonText => IsAdminMode ? "Переключить на Клиента" : "Переключить на Администратора";
        public string CurrentRole => IsAdminMode ? "Администратор" : "Клиент";

        #endregion

        #region Commands

        public ICommand LoadDataCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand FilterByCategoryCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ToggleRoleCommand { get; }
        public ICommand ShowDetailsCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand SortCommand { get; }

        #endregion

        #region Methods

        private void LoadData()
        {
            try
            {
                var allProducts = _dataService.GetAllProducts();
                Products = new ObservableCollection<Product>(allProducts);

                var allCategories = _dataService.GetAllCategories();
                Categories = new ObservableCollection<Category>(allCategories);

                StatusText = $"Загружено товаров: {Products.Count}";
            }
            catch (Exception ex)
            {
                StatusText = $"Ошибка загрузки: {ex.Message}";
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveData()
        {
            try
            {
                _dataService.SaveProducts(Products.ToList());
                StatusText = $"Сохранено товаров: {Products.Count}";
                MessageBox.Show("Данные успешно сохранены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                StatusText = $"Ошибка сохранения: {ex.Message}";
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddProduct()
        {
            var editWindow = new ProductEditWindow();
            editWindow.Owner = Application.Current.MainWindow;

            if (editWindow.ShowDialog() == true && editWindow.EditedProduct != null)
            {
                _dataService.AddProduct(editWindow.EditedProduct);
                LoadData();
                StatusText = $"Товар '{editWindow.EditedProduct.Name}' добавлен";
            }
        }

        private void EditProduct()
        {
            if (SelectedProduct == null) return;

            var editWindow = new ProductEditWindow(SelectedProduct);
            editWindow.Owner = Application.Current.MainWindow;

            if (editWindow.ShowDialog() == true && editWindow.EditedProduct != null)
            {
                _dataService.UpdateProduct(editWindow.EditedProduct);
                LoadData();
                StatusText = $"Товар '{editWindow.EditedProduct.Name}' обновлен";
            }
        }

        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;

            var result = MessageBox.Show($"Удалить товар '{SelectedProduct.Name}'?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Products.Remove(SelectedProduct);
                _dataService.SaveProducts(Products.ToList());
                StatusText = $"Товар удален. Всего товаров: {Products.Count}";
            }
        }

        private void ApplyFilters()
        {
            var allProducts = _dataService.GetAllProducts();
            var filtered = allProducts.AsEnumerable();

            // Фильтр по категории
            if (SelectedCategory != null)
            {
                filtered = filtered.Where(p => p.CategoryId == SelectedCategory.Id);
            }

            // Поиск по тексту
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(p =>
                    p.Name.ToLower().Contains(searchLower) ||
                    p.FullName.ToLower().Contains(searchLower) ||
                    p.Description.ToLower().Contains(searchLower)
                );
            }

            // Фильтр по цене от
            if (!string.IsNullOrWhiteSpace(PriceFromText) && decimal.TryParse(PriceFromText, out decimal priceFrom))
            {
                filtered = filtered.Where(p => p.FinalPrice >= priceFrom);
            }

            // Фильтр по цене до
            if (!string.IsNullOrWhiteSpace(PriceToText) && decimal.TryParse(PriceToText, out decimal priceTo))
            {
                filtered = filtered.Where(p => p.FinalPrice <= priceTo);
            }

            // Фильтр по наличию
            if (InStockFilterIndex == 1) // В наличии
            {
                filtered = filtered.Where(p => p.InStock);
            }
            else if (InStockFilterIndex == 2) // Нет в наличии
            {
                filtered = filtered.Where(p => !p.InStock);
            }

            // Фильтр по рейтингу
            if (!string.IsNullOrWhiteSpace(RatingFilterText) && double.TryParse(RatingFilterText, out double minRating))
            {
                filtered = filtered.Where(p => p.Rating >= minRating);
            }

            // Сортировка
            filtered = ApplySorting(filtered);

            Products = new ObservableCollection<Product>(filtered);
            StatusText = $"Найдено товаров: {Products.Count}";
        }

        private IEnumerable<Product> ApplySorting(IEnumerable<Product> products)
        {
            if (string.IsNullOrWhiteSpace(SortBy))
                return products;

            bool ascending = SortDirection != "desc";

            switch (SortBy)
            {
                case "name":
                    return ascending ? products.OrderBy(p => p.Name) : products.OrderByDescending(p => p.Name);
                case "price":
                    return ascending ? products.OrderBy(p => p.FinalPrice) : products.OrderByDescending(p => p.FinalPrice);
                case "rating":
                    return ascending ? products.OrderBy(p => p.Rating) : products.OrderByDescending(p => p.Rating);
                case "quantity":
                    return ascending ? products.OrderBy(p => p.Quantity) : products.OrderByDescending(p => p.Quantity);
                default:
                    return products;
            }
        }

        private void ClearFilters()
        {
            SelectedCategory = null;
            SearchText = string.Empty;
            PriceFromText = string.Empty;
            PriceToText = string.Empty;
            InStockFilterIndex = 0;
            RatingFilterText = string.Empty;
            SortBy = null;
            SortDirection = null;

            LoadData();
            StatusText = $"Сброшены фильтры. Всего товаров: {Products.Count}";
        }

        private void SortProducts(string sortBy)
        {
            if (sortBy == "reset")
            {
                SortBy = null;
                SortDirection = null;
                ApplyFilters();
                StatusText = "Сортировка сброшена";
                return;
            }

            if (SortBy == sortBy)
            {
                SortDirection = SortDirection == "asc" ? "desc" : "asc";
            }
            else
            {
                SortBy = sortBy;
                SortDirection = "asc";
            }

            ApplyFilters();

            string direction = SortDirection == "asc" ? "возрастанию" : "убыванию";
            string sortName = sortBy switch
            {
                "name" => "названию",
                "price" => "цене",
                "rating" => "рейтингу",
                _ => sortBy
            };
            StatusText = $"Сортировка по {sortName} ({direction}). Найдено: {Products.Count}";
        }

        private void ToggleRole()
        {
            IsAdminMode = !IsAdminMode;
            StatusText = $"Режим: {CurrentRole}";
        }

        private bool CanShowDetails(object parameter)
        {
            return parameter is Product || SelectedProduct != null;
        }

        private void ShowDetails(object parameter)
        {
            Product productToShow = parameter as Product ?? SelectedProduct;

            if (productToShow != null)
            {
                var detailsWindow = new ProductDetailWindow(productToShow, IsAdminMode);
                detailsWindow.ShowDialog();

                if (detailsWindow.DialogResult == true)
                {
                    LoadData();
                }
            }
        }

        #endregion
    }
}