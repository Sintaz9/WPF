using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Lab4._5.Commands;
using Lab4._5.Models;
using Lab4._5.Services;
using Lab4._5.Views;
using Microsoft.Win32;

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
        private string _priceFromText;
        private string _priceToText;
        private int _inStockFilterIndex;
        private string _ratingFilterText;
        private string _sortBy;
        private string _sortDirection;
        public MainViewModel()
        {
            _dataService = new DataService();

            // Инициализация команд
            LoadDataCommand = new RelayCommand(_ => LoadFromFile());
            SaveCommand = new RelayCommand(_ => SaveToFile());
            AddProductCommand = new RelayCommand(_ => AddProduct(), _ => IsAdminMode);
            EditProductCommand = new RelayCommand(_ => EditProduct(), _ => IsAdminMode && SelectedProduct != null);
            DeleteProductCommand = new RelayCommand(_ => DeleteProduct(), _ => IsAdminMode && SelectedProduct != null);
            FilterByCategoryCommand = new RelayCommand(_ => ApplyFilters());
            ClearFilterCommand = new RelayCommand(_ => ClearFilters());
            SearchCommand = new RelayCommand(_ => ApplyFilters());
            ToggleRoleCommand = new RelayCommand(_ => ToggleRole());
            ShowDetailsCommand = new RelayCommand(ShowDetails, CanShowDetails);
            ApplyFiltersCommand = new RelayCommand(_ => ApplyFilters());
            SortCommand = new RelayCommand(param => SortProducts(param?.ToString()));
            SwitchLanguageCommand = new RelayCommand(lang => SwitchLanguage(lang?.ToString()));

            // Загружаем категории
            _categories = new ObservableCollection<Category>(_dataService.GetAllCategories());
            _products = new ObservableCollection<Product>();

            StatusText = "Готов к работе. Создайте товары или загрузите из файла.";
        }

        #region Properties

        public string RoleButtonText => IsAdminMode ?
        Application.Current.TryFindResource("SwitchToClient") as string ?? "Переключить на Клиента" :
        Application.Current.TryFindResource("SwitchToAdmin") as string ?? "Переключить на Администратора";

        public string CurrentRole => IsAdminMode ?
            Application.Current.TryFindResource("RoleAdmin") as string ?? "Администратор" :
            Application.Current.TryFindResource("RoleClient") as string ?? "Клиент";

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

        public int InStockFilterIndex
        {
            get => _inStockFilterIndex;
            set { _inStockFilterIndex = value; OnPropertyChanged(); }
        }

        public string RatingFilterText
        {
            get => _ratingFilterText;
            set { _ratingFilterText = value; OnPropertyChanged(); }
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


        #endregion

        #region Commands

        public ICommand LoadDataCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand FilterByCategoryCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ToggleRoleCommand { get; }
        public ICommand ShowDetailsCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand SortCommand { get; }
        public ICommand SwitchLanguageCommand { get; }
        #endregion

        #region File Operations

        private void LoadFromFile()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите файл с товарами",
                Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = ".json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _dataService.LoadFromFile(dialog.FileName);
                    RefreshProductsList();
                    StatusText = $"Загружено товаров: {Products.Count} из файла {System.IO.Path.GetFileName(dialog.FileName)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке файла:\n{ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveToFile()
        {
            if (Products.Count == 0)
            {
                MessageBox.Show("Нет товаров для сохранения. Сначала добавьте товары.",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "Сохранить товары в файл",
                Filter = "JSON файлы (*.json)|*.json|Все файлы (*.*)|*.*",
                DefaultExt = ".json",
                FileName = "products.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _dataService.SaveToFile(dialog.FileName);
                    StatusText = $"Сохранено {Products.Count} товаров в файл {System.IO.Path.GetFileName(dialog.FileName)}";
                    MessageBox.Show($"Товары успешно сохранены в файл:\n{dialog.FileName}",
                        "Сохранение", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении:\n{ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        #endregion

        #region Product Operations

        private void AddProduct()
        {
            var editWindow = new ProductEditWindow();
            editWindow.Owner = Application.Current.MainWindow;

            if (editWindow.ShowDialog() == true && editWindow.EditedProduct != null)
            {
                _dataService.AddProduct(editWindow.EditedProduct);
                RefreshProductsList();
                StatusText = $"Товар '{editWindow.EditedProduct.Name}' добавлен (не сохранен в файл)";
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
                RefreshProductsList();
                StatusText = $"Товар '{editWindow.EditedProduct.Name}' обновлен (не сохранен в файл)";
            }
        }

        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;

            var result = MessageBox.Show($"Удалить товар '{SelectedProduct.Name}'?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _dataService.DeleteProduct(SelectedProduct.Id);
                RefreshProductsList();
                StatusText = $"Товар удален (изменения не сохранены в файл)";
            }
        }

        #endregion

        #region Filters

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
            if (InStockFilterIndex == 1)
            {
                filtered = filtered.Where(p => p.InStock);
            }
            else if (InStockFilterIndex == 2)
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

            RefreshProductsList();
            StatusText = "Фильтры сброшены";
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
        }

        #endregion

        #region Helpers

        private void RefreshProductsList()
        {
            Products = new ObservableCollection<Product>(_dataService.GetAllProducts());
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
        private void SwitchLanguage(string lang)
        {
            LocalizationService.SwitchLanguage(lang);
            StatusText = $"Язык изменен на {(lang == "ru-RU" ? "Русский" : "English")}";
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
                    RefreshProductsList();
                }
            }
        }

        #endregion
    }
}