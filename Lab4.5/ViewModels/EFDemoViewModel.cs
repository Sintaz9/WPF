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
using System.Windows.Input;

namespace Lab4._5.ViewModels
{
    public class EFDemoViewModel : INotifyPropertyChanged
    {
        private readonly EFDataService _service;
        private ObservableCollection<Product> _products;
        private ObservableCollection<Category> _categories;
        private Product _selectedProduct;
        private Category _selectedCategory;
        private string _searchText;
        private string _statusText;
        private string _selectedSort;

        public EFDemoViewModel()
        {
            _service = new EFDataService();
            _products = new ObservableCollection<Product>();
            _categories = new ObservableCollection<Category>();

            // Команды
            AddCommand = new RelayCommand(_ => AddProduct());
            EditCommand = new RelayCommand(_ => EditProduct(), _ => SelectedProduct != null);
            DeleteCommand = new RelayCommand(_ => DeleteProduct(), _ => SelectedProduct != null);
            RefreshCommand = new RelayCommand(_ => LoadData());
            ApplyFiltersCommand = new RelayCommand(_ => ApplyFilters());
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters());
            ApplySortCommand = new RelayCommand(_ => ApplySorting());

            LoadData();
            LoadCategories();
        }

        // Свойства
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

        public string SelectedSort
        {
            get => _selectedSort;
            set { _selectedSort = value; OnPropertyChanged(); }
        }

        // Команды
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand ApplySortCommand { get; }

        // Загрузка данных через EF
        private async void LoadData()
        {
            StatusText = "📥 Загрузка через Entity Framework...";
            var products = await _service.GetAllProductsAsync();
            Products = new ObservableCollection<Product>(products);

            if (Products.Count == 0)
                StatusText = "⚠️ База данных пуста. Нажмите '➕ Добавить товар'.";
            else
                StatusText = $"✅ Загружено {Products.Count} товаров (EF Core)";
        }

        private async void LoadCategories()
        {
            var cats = await Task.Run(() => _service.GetAllCategories());
            Categories.Clear();
            foreach (var cat in cats)
                Categories.Add(cat);
        }

        // CREATE (добавление)
        private async void AddProduct()
        {
            var editWindow = new ProductEditWindow();
            editWindow.Owner = Application.Current.MainWindow;

            if (editWindow.ShowDialog() == true && editWindow.EditedProduct != null)
            {
                var product = editWindow.EditedProduct;

                try
                {
                    await _service.AddProductAsync(product);
                    await LoadDataAsync();
                    StatusText = $"✅ CREATE: добавлен товар '{product.Name}' через EF";
                }
                catch (Exception ex)
                {
                    StatusText = $"❌ Ошибка: {ex.Message}";
                    MessageBox.Show($"Ошибка при добавлении: {ex.InnerException?.Message ?? ex.Message}",
                        "EF Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // UPDATE (редактирование)
        // UPDATE (редактирование через существующее окно)
        private async void EditProduct()
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Выберите товар для редактирования.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Создаём копию для возможной отмены (как в админ-панели)
            var originalProduct = new Product
            {
                Id = SelectedProduct.Id,
                Name = SelectedProduct.Name,
                FullName = SelectedProduct.FullName,
                Description = SelectedProduct.Description,
                CategoryId = SelectedProduct.CategoryId,
                Manufacturer = SelectedProduct.Manufacturer,
                Country = SelectedProduct.Country,
                Color = SelectedProduct.Color,
                Size = SelectedProduct.Size,
                Price = SelectedProduct.Price,
                Discount = SelectedProduct.Discount,
                Quantity = SelectedProduct.Quantity,
                InStock = SelectedProduct.InStock,
                Rating = SelectedProduct.Rating,
                SoldCount = SelectedProduct.SoldCount,
                ImagePaths = SelectedProduct.ImagePaths != null
                    ? new System.Collections.ObjectModel.ObservableCollection<string>(SelectedProduct.ImagePaths)
                    : new System.Collections.ObjectModel.ObservableCollection<string>()
            };

            var editWindow = new ProductEditWindow(SelectedProduct);
            editWindow.Owner = Application.Current.MainWindow;

            if (editWindow.ShowDialog() == true && editWindow.EditedProduct != null)
            {
                var editedProduct = editWindow.EditedProduct;

                try
                {
                    await _service.UpdateProductAsync(editedProduct);
                    await LoadDataAsync();
                    StatusText = $"✏️ UPDATE: отредактирован товар '{editedProduct.Name}'";
                }
                catch (Exception ex)
                {
                    StatusText = $"❌ Ошибка: {ex.Message}";
                    MessageBox.Show($"Ошибка при редактировании: {ex.InnerException?.Message ?? ex.Message}",
                        "EF Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // DELETE (удаление)
        private async void DeleteProduct()
        {
            if (SelectedProduct == null) return;

            var result = MessageBox.Show($"Удалить товар '{SelectedProduct.Name}'?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _service.DeleteProductAsync(SelectedProduct.Id);
                    await LoadDataAsync();
                    StatusText = $"🗑️ DELETE: удалён товар";
                }
                catch (Exception ex)
                {
                    StatusText = $"❌ Ошибка: {ex.Message}";
                }
            }
        }

        // LINQ: фильтрация
        private void ApplyFilters()
        {
            try
            {
                int? categoryId = SelectedCategory?.Id;

                var filtered = _service.GetFilteredProducts(
                    searchText: SearchText,
                    categoryId: categoryId);

                Products = new ObservableCollection<Product>(filtered);
                StatusText = $"🔍 LINQ: найдено {Products.Count} товаров (фильтрация)";
            }
            catch (Exception ex)
            {
                StatusText = $"❌ Ошибка: {ex.Message}";
            }
        }

        private void ClearFilters()
        {
            SearchText = "";
            SelectedCategory = null;
            LoadData();
            StatusText = "❌ Фильтры сброшены";
        }

        // LINQ: сортировка
        private void ApplySorting()
        {
            if (string.IsNullOrEmpty(SelectedSort)) return;

            try
            {
                string sortBy = "";
                bool ascending = true;

                if (SelectedSort.Contains("имени") && SelectedSort.Contains("↑")) { sortBy = "name"; ascending = true; }
                else if (SelectedSort.Contains("имени") && SelectedSort.Contains("↓")) { sortBy = "name"; ascending = false; }
                else if (SelectedSort.Contains("цене") && SelectedSort.Contains("↑")) { sortBy = "price"; ascending = true; }
                else if (SelectedSort.Contains("цене") && SelectedSort.Contains("↓")) { sortBy = "price"; ascending = false; }
                else if (SelectedSort.Contains("рейтингу") && SelectedSort.Contains("↑")) { sortBy = "rating"; ascending = true; }
                else if (SelectedSort.Contains("рейтингу") && SelectedSort.Contains("↓")) { sortBy = "rating"; ascending = false; }

                var sorted = _service.GetFilteredProducts(sortBy: sortBy, ascending: ascending);
                Products = new ObservableCollection<Product>(sorted);
                StatusText = $"📊 LINQ: сортировка {SelectedSort}";
            }
            catch (Exception ex)
            {
                StatusText = $"❌ Ошибка: {ex.Message}";
            }
        }

        private async Task LoadDataAsync()
        {
            var products = await _service.GetAllProductsAsync();
            Products = new ObservableCollection<Product>(products);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}