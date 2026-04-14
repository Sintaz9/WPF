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

        public MainViewModel()
        {
            _dataService = new DataService();

            // Инициализация команд
            LoadDataCommand = new RelayCommand(_ => LoadData());
            AddProductCommand = new RelayCommand(_ => AddProduct(), _ => IsAdminMode);
            EditProductCommand = new RelayCommand(_ => EditProduct(), _ => IsAdminMode && SelectedProduct != null);
            DeleteProductCommand = new RelayCommand(_ => DeleteProduct(), _ => IsAdminMode && SelectedProduct != null);
            SaveCommand = new RelayCommand(_ => SaveData());
            FilterByCategoryCommand = new RelayCommand(_ => FilterByCategory());
            ClearFilterCommand = new RelayCommand(_ => ClearFilter());
            SearchCommand = new RelayCommand(_ => PerformSearch());
            ToggleRoleCommand = new RelayCommand(_ => ToggleRole());
            ShowDetailsCommand = new RelayCommand(ShowDetails, CanShowDetails);  // ← ВОТ ЭТА СТРОКА

            // Загрузка данных
            LoadData();
        }

        #region Properties

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
        public ICommand ShowDetailsCommand { get; }  // ← И СВОЙСТВО КОМАНДЫ

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
            MessageBox.Show("Добавление товара будет реализовано в Этапе 3", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EditProduct()
        {
            if (SelectedProduct == null) return;

            // Открываем окно детализации (редактирование будет в Этапе 3)
            var detailsWindow = new ProductDetailWindow(SelectedProduct, IsAdminMode);
            detailsWindow.ShowDialog();
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

        private void FilterByCategory()
        {
            if (SelectedCategory == null)
            {
                ClearFilter();
                return;
            }

            var allProducts = _dataService.GetAllProducts();
            var filtered = allProducts.Where(p => p.CategoryId == SelectedCategory.Id).ToList();
            Products = new ObservableCollection<Product>(filtered);
            StatusText = $"Найдено товаров в категории '{SelectedCategory.Name}': {filtered.Count}";
        }

        private void ClearFilter()
        {
            LoadData();
            SelectedCategory = null;
            SearchText = string.Empty;
            StatusText = $"Сброшены фильтры. Всего товаров: {Products.Count}";
        }

        private void PerformSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                ClearFilter();
                return;
            }

            var allProducts = _dataService.GetAllProducts();
            var searchLower = SearchText.ToLower();
            var filtered = allProducts.Where(p =>
                p.Name.ToLower().Contains(searchLower) ||
                p.FullName.ToLower().Contains(searchLower) ||
                p.Description.ToLower().Contains(searchLower)
            ).ToList();

            Products = new ObservableCollection<Product>(filtered);
            StatusText = $"Найдено по запросу '{SearchText}': {filtered.Count} товаров";
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

                // Если в окне детализации произошли изменения (например, удаление)
                if (detailsWindow.DialogResult == true)
                {
                    LoadData(); // Перезагружаем данные
                }
            }
        }

        #endregion
    }
}