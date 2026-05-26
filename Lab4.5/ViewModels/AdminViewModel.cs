using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Lab4._5.Database;
using Lab4._5.Models;
using Lab4._5.Services;
using Lab4._5.Views;
using Microsoft.Data.Sqlite;

namespace Lab4._5.ViewModels
{
    public class AdminViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;
        private ObservableCollection<Product> _products;
        private ObservableCollection<Category> _categories;
        private Product _selectedProduct;
        private Category _selectedCategory;
        private string _searchText;
        private string _priceFromText;
        private string _priceToText;
        private string _statusText;

        // Сортировка
        private bool _sortByNameAsc;
        private bool _sortByNameDesc;
        private bool _sortByPriceAsc;
        private bool _sortByPriceDesc;
        private bool _sortByRatingAsc;
        private bool _sortByRatingDesc;

        public AdminViewModel()
        {
            _dataService = new DataService();
            _dataService.UseDatabase = true; // Админ всегда работает с БД

            _categories = new ObservableCollection<Category>();
            _products = new ObservableCollection<Product>();

            // Команды
            AddProductCommand = new RelayCommand(_ => AddProduct());
            EditProductCommand = new RelayCommand(_ => EditProduct(), _ => SelectedProduct != null);
            DeleteProductCommand = new RelayCommand(_ => DeleteProduct(), _ => SelectedProduct != null);
            RefreshCommand = new RelayCommand(_ => LoadProductsAsync());
            ApplyFiltersCommand = new RelayCommand(_ => ApplyFilters());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            ShowTopProductsCommand = new RelayCommand(_ => ShowTopProducts());
            SyncFromJsonCommand = new RelayCommand(_ => SyncFromJson());

            // Загрузка данных
            LoadCategories();
            LoadProductsAsync();
        }

        // ========== СВОЙСТВА ==========

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
            set { _selectedProduct = value; OnPropertyChanged(); }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); ApplyFilters(); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
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

        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public int ProductsCount => Products?.Count ?? 0;

        // Свойства для сортировки (RadioButton binding)
        public bool SortByNameAsc { get => _sortByNameAsc; set { _sortByNameAsc = value; if (value) ApplySorting("Name", true); OnPropertyChanged(); } }
        public bool SortByNameDesc { get => _sortByNameDesc; set { _sortByNameDesc = value; if (value) ApplySorting("Name", false); OnPropertyChanged(); } }
        public bool SortByPriceAsc { get => _sortByPriceAsc; set { _sortByPriceAsc = value; if (value) ApplySorting("Price", true); OnPropertyChanged(); } }
        public bool SortByPriceDesc { get => _sortByPriceDesc; set { _sortByPriceDesc = value; if (value) ApplySorting("Price", false); OnPropertyChanged(); } }
        public bool SortByRatingAsc { get => _sortByRatingAsc; set { _sortByRatingAsc = value; if (value) ApplySorting("Rating", true); OnPropertyChanged(); } }
        public bool SortByRatingDesc { get => _sortByRatingDesc; set { _sortByRatingDesc = value; if (value) ApplySorting("Rating", false); OnPropertyChanged(); } }

        // ========== КОМАНДЫ ==========

        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand ShowTopProductsCommand { get; }
        public ICommand SyncFromJsonCommand { get; }

        // ========== ЗАГРУЗКА ДАННЫХ ==========

        private void LoadCategories()
        {
            var cats = _dataService.GetAllCategories();
            Categories.Clear();
            foreach (var cat in cats)
                Categories.Add(cat);
        }

        private async void LoadProductsAsync()
        {
            StatusText = "Загрузка товаров из БД...";
            var products = await _dataService.GetAllProductsAsync();
            Products = new ObservableCollection<Product>(products);
            OnPropertyChanged(nameof(ProductsCount));
            StatusText = $"Загружено {Products.Count} товаров из базы данных";
        }

        // ========== CRUD ОПЕРАЦИИ ==========

        private void AddProduct()
        {
            var editWindow = new ProductEditWindow();
            editWindow.Owner = Application.Current.MainWindow;

            if (editWindow.ShowDialog() == true && editWindow.EditedProduct != null)
            {
                _dataService.AddProduct(editWindow.EditedProduct);
                LoadProductsAsync();
                StatusText = $"Товар '{editWindow.EditedProduct.Name}' добавлен в БД";
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
                LoadProductsAsync();
                StatusText = $"Товар '{editWindow.EditedProduct.Name}' обновлён в БД";
            }
        }

        private void DeleteProduct()
        {
            if (SelectedProduct == null) return;

            var result = MessageBox.Show($"Удалить товар '{SelectedProduct.Name}' из БД?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _dataService.DeleteProduct(SelectedProduct.Id);
                LoadProductsAsync();
                StatusText = $"Товар удалён из БД (запись в логе)";
            }
        }

        // ========== ФИЛЬТРАЦИЯ (ЧЕРЕЗ SQL/РЕПОЗИТОРИЙ) ==========

        private void ApplyFilters()
        {
            // Получаем все товары
            var allProducts = _dataService.GetAllProducts();
            var filtered = allProducts.AsEnumerable();

            // Фильтр по поиску
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filtered = filtered.Where(p => p.Name.ToLower().Contains(search) ||
                                               (p.Description?.ToLower().Contains(search) ?? false));
            }

            // Фильтр по категории
            if (SelectedCategory != null)
            {
                filtered = filtered.Where(p => p.CategoryId == SelectedCategory.Id);
            }

            // Фильтр по цене
            if (!string.IsNullOrWhiteSpace(PriceFromText) && decimal.TryParse(PriceFromText, out decimal from))
            {
                filtered = filtered.Where(p => p.Price >= from);
            }
            if (!string.IsNullOrWhiteSpace(PriceToText) && decimal.TryParse(PriceToText, out decimal to))
            {
                filtered = filtered.Where(p => p.Price <= to);
            }

            Products = new ObservableCollection<Product>(filtered);
            OnPropertyChanged(nameof(ProductsCount));
            StatusText = $"Найдено {Products.Count} товаров";
        }

        private void ClearFilters()
        {
            SearchText = "";
            SelectedCategory = null;
            PriceFromText = "";
            PriceToText = "";
            ApplyFilters();
            StatusText = "Фильтры сброшены";
        }

        // ========== СОРТИРОВКА (ЧЕРЕЗ LINQ к БД) ==========

        private void ApplySorting(string field, bool ascending)
        {
            var products = _dataService.GetAllProducts();
            var sorted = products.AsEnumerable();

            switch (field)
            {
                case "Name":
                    sorted = ascending ? products.OrderBy(p => p.Name) : products.OrderByDescending(p => p.Name);
                    break;
                case "Price":
                    sorted = ascending ? products.OrderBy(p => p.Price) : products.OrderByDescending(p => p.Price);
                    break;
                case "Rating":
                    sorted = ascending ? products.OrderBy(p => p.Rating) : products.OrderByDescending(p => p.Rating);
                    break;
            }

            Products = new ObservableCollection<Product>(sorted);
            StatusText = $"Сортировка: {field} {(ascending ? "↑" : "↓")}";
        }

        // ========== ХРАНИМАЯ ПРОЦЕДУРА / ТОП ТОВАРОВ ==========

        private void ShowTopProducts()
        {
            var topProducts = _dataService.GetTopProducts();
            string message = "🏆 ТОП-5 САМЫХ ПРОДАВАЕМЫХ ТОВАРОВ 🏆\n\n";

            for (int i = 0; i < topProducts.Count; i++)
            {
                message += $"{i + 1}. {topProducts[i].Name}\n   Продано: {topProducts[i].TotalSold} шт.\n\n";
            }

            MessageBox.Show(message, "Хранимая процедура (TopProductsView)",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ========== СИНХРОНИЗАЦИЯ ИЗ JSON ==========

        private void SyncFromJson()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Выберите JSON файл для импорта в БД",
                Filter = "JSON файлы (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _dataService.LoadFromFile(dialog.FileName);
                    LoadProductsAsync();
                    StatusText = $"Импорт из JSON выполнен. Загружено {Products.Count} товаров";
                    MessageBox.Show("Данные успешно импортированы из JSON в базу данных!",
                        "Импорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // ========== INotifyPropertyChanged ==========

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Простая реализация RelayCommand
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}