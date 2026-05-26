using Lab4._5.Database;
using Lab4._5.Models;
using Lab4._5.Services;
using Lab4._5.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Lab4._5.ViewModels
{
    public class AdminViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;

        // Коллекции для таблиц
        private ObservableCollection<Product> _products;
        private ObservableCollection<Category> _categories;
        private ObservableCollection<Order> _orders;
        private ObservableCollection<OrderItem> _orderItems;

        private Product _selectedProduct;
        private Category _selectedCategory;
        private Order _selectedOrder;
        private string _searchText;
        private string _priceFromText;
        private string _priceToText;
        private string _statusText;

        // Переключение таблиц
        private bool _showProductsTable = true;
        private bool _showCategoriesTable;
        private bool _showOrdersTable;
        private bool _showOrderItemsTable;

        // Сортировка
        private bool _sortByNameAsc, _sortByNameDesc, _sortByPriceAsc, _sortByPriceDesc, _sortByRatingAsc, _sortByRatingDesc;

        public AdminViewModel()
        {
            _dataService = new DataService();
            _dataService.UseDatabase = true;

            _products = new ObservableCollection<Product>();
            _categories = new ObservableCollection<Category>();
            _orders = new ObservableCollection<Order>();
            _orderItems = new ObservableCollection<OrderItem>();

            // Команды
            AddProductCommand = new RelayCommand(_ => AddProduct());
            EditProductCommand = new RelayCommand(_ => EditProduct(), _ => SelectedProduct != null);
            DeleteProductCommand = new RelayCommand(_ => DeleteProduct(), _ => SelectedProduct != null);
            RefreshCommand = new RelayCommand(_ => LoadAllData());
            ApplyFiltersCommand = new RelayCommand(_ => ApplyFilters());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            ShowTopProductsCommand = new RelayCommand(_ => ShowTopProducts());
            SyncFromJsonCommand = new RelayCommand(_ => SyncFromJson());
            ShowOrderItemsCommand = new RelayCommand(order => ShowOrderItems(order as Order));

            LoadAllData();
        }

        // ========== СВОЙСТВА ДЛЯ ПЕРЕКЛЮЧЕНИЯ ТАБЛИЦ ==========
        public bool ShowProductsTable { get => _showProductsTable; set { _showProductsTable = value; OnPropertyChanged(); OnPropertyChanged(nameof(ProductsTableVisible)); OnPropertyChanged(nameof(ProductsPanelVisible)); } }
        public bool ShowCategoriesTable { get => _showCategoriesTable; set { _showCategoriesTable = value; OnPropertyChanged(); OnPropertyChanged(nameof(CategoriesTableVisible)); } }
        public bool ShowOrdersTable { get => _showOrdersTable; set { _showOrdersTable = value; OnPropertyChanged(); OnPropertyChanged(nameof(OrdersTableVisible)); LoadOrdersAsync(); } }
        public bool ShowOrderItemsTable { get => _showOrderItemsTable; set { _showOrderItemsTable = value; OnPropertyChanged(); OnPropertyChanged(nameof(OrderItemsTableVisible)); LoadOrderItemsAsync(); } }

        public Visibility ProductsTableVisible => ShowProductsTable ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProductsPanelVisible => ShowProductsTable ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CategoriesTableVisible => ShowCategoriesTable ? Visibility.Visible : Visibility.Collapsed;
        public Visibility OrdersTableVisible => ShowOrdersTable ? Visibility.Visible : Visibility.Collapsed;
        public Visibility OrderItemsTableVisible => ShowOrderItemsTable ? Visibility.Visible : Visibility.Collapsed;

        public int RecordsCount => ShowProductsTable ? Products?.Count ?? 0 : (ShowCategoriesTable ? Categories?.Count ?? 0 : (ShowOrdersTable ? Orders?.Count ?? 0 : OrderItems?.Count ?? 0));

        // ========== КОЛЛЕКЦИИ ==========
        public ObservableCollection<Product> Products { get => _products; set { _products = value; OnPropertyChanged(); OnPropertyChanged(nameof(RecordsCount)); } }
        public ObservableCollection<Category> Categories { get => _categories; set { _categories = value; OnPropertyChanged(); OnPropertyChanged(nameof(RecordsCount)); } }
        public ObservableCollection<Order> Orders { get => _orders; set { _orders = value; OnPropertyChanged(); OnPropertyChanged(nameof(RecordsCount)); } }
        public ObservableCollection<OrderItem> OrderItems { get => _orderItems; set { _orderItems = value; OnPropertyChanged(); OnPropertyChanged(nameof(RecordsCount)); } }

        public Product SelectedProduct { get => _selectedProduct; set { _selectedProduct = value; OnPropertyChanged(); } }
        public Category SelectedCategory { get => _selectedCategory; set { _selectedCategory = value; OnPropertyChanged(); ApplyFilters(); } }
        public Order SelectedOrder { get => _selectedOrder; set { _selectedOrder = value; OnPropertyChanged(); } }

        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }
        public string PriceFromText { get => _priceFromText; set { _priceFromText = value; OnPropertyChanged(); } }
        public string PriceToText { get => _priceToText; set { _priceToText = value; OnPropertyChanged(); } }
        public string StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged(); } }

        // Свойства сортировки
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
        public ICommand ShowOrderItemsCommand { get; }

        // ========== ЗАГРУЗКА ДАННЫХ ==========
        private async void LoadAllData()
        {
            await LoadProductsAsync();
            await LoadCategoriesAsync();
            if (ShowOrdersTable) await LoadOrdersAsync();
            if (ShowOrderItemsTable) await LoadOrderItemsAsync();
        }

        private async Task LoadProductsAsync()
        {
            var products = await _dataService.GetAllProductsAsync();
            Products = new ObservableCollection<Product>(products);
            StatusText = $"Товаров: {Products.Count}";
        }

        private async Task LoadCategoriesAsync()
        {
            var cats = _dataService.GetAllCategories();
            Categories.Clear();
            foreach (var cat in cats) Categories.Add(cat);
        }

        private async Task LoadOrdersAsync()
        {
            var orders = await _dataService.GetAllOrdersAsync();
            Orders = new ObservableCollection<Order>(orders);
            StatusText = $"Заказов: {Orders.Count}";
        }

        private async Task LoadOrderItemsAsync()
        {
            var items = _dataService.GetAllOrderItems();
            OrderItems = new ObservableCollection<OrderItem>(items);
            StatusText = $"Позиций заказов: {OrderItems.Count}";
        }

        private void ShowOrderItems(Order order)
        {
            if (order == null) return;
            var items = _dataService.GetOrderItemsByOrderId(order.Id);
            OrderItems = new ObservableCollection<OrderItem>(items);
            ShowOrderItemsTable = true;
            StatusText = $"Заказ #{order.Id} - {order.CustomerName}, позиций: {items.Count}";
        }

        // ========== CRUD ==========
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
            if (MessageBox.Show($"Удалить товар '{SelectedProduct.Name}' из БД?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _dataService.DeleteProduct(SelectedProduct.Id);
                LoadProductsAsync();
                StatusText = $"Товар удалён из БД (запись в логе)";
            }
        }

        // ========== ФИЛЬТРАЦИЯ ==========
        private void ApplyFilters()
        {
            var allProducts = _dataService.GetAllProducts();
            var filtered = allProducts.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.ToLower();
                filtered = filtered.Where(p => p.Name.ToLower().Contains(search) || (p.Description?.ToLower().Contains(search) ?? false));
            }
            if (SelectedCategory != null) filtered = filtered.Where(p => p.CategoryId == SelectedCategory.Id);
            if (!string.IsNullOrWhiteSpace(PriceFromText) && decimal.TryParse(PriceFromText, out decimal from)) filtered = filtered.Where(p => p.Price >= from);
            if (!string.IsNullOrWhiteSpace(PriceToText) && decimal.TryParse(PriceToText, out decimal to)) filtered = filtered.Where(p => p.Price <= to);
            Products = new ObservableCollection<Product>(filtered);
            StatusText = $"Найдено {Products.Count} товаров";
        }

        private void ClearFilters()
        {
            SearchText = ""; SelectedCategory = null; PriceFromText = ""; PriceToText = "";
            ApplyFilters();
            StatusText = "Фильтры сброшены";
        }

        private void ApplySorting(string field, bool ascending)
        {
            var products = _dataService.GetAllProducts();
            var sorted = products.AsEnumerable();
            switch (field)
            {
                case "Name": sorted = ascending ? products.OrderBy(p => p.Name) : products.OrderByDescending(p => p.Name); break;
                case "Price": sorted = ascending ? products.OrderBy(p => p.Price) : products.OrderByDescending(p => p.Price); break;
                case "Rating": sorted = ascending ? products.OrderBy(p => p.Rating) : products.OrderByDescending(p => p.Rating); break;
            }
            Products = new ObservableCollection<Product>(sorted);
            StatusText = $"Сортировка: {field} {(ascending ? "↑" : "↓")}";
        }

        private void ShowTopProducts()
        {
            var topProducts = _dataService.GetTopProducts();
            string message = "🏆 ТОП-5 САМЫХ ПРОДАВАЕМЫХ ТОВАРОВ 🏆\n\n";
            for (int i = 0; i < topProducts.Count; i++)
                message += $"{i + 1}. {topProducts[i].Name}\n   Продано: {topProducts[i].TotalSold} шт.\n\n";
            MessageBox.Show(message, "Хранимая процедура (TopProductsView)", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SyncFromJson()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Title = "Выберите JSON файл для импорта в БД", Filter = "JSON файлы (*.json)|*.json" };
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _dataService.LoadFromFile(dialog.FileName);
                    LoadProductsAsync();
                    StatusText = $"Импорт из JSON выполнен. Загружено {Products.Count} товаров";
                    MessageBox.Show("Данные успешно импортированы из JSON в базу данных!", "Импорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) { MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;
        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null) { _execute = execute; _canExecute = canExecute; }
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
        public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
    }
}