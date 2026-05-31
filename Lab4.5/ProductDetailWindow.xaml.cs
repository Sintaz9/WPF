using System.Windows;
using Lab4._5.Models;

namespace Lab4._5.Views
{
    public partial class ProductDetailWindow : Window
    {
        public Product SelectedProduct { get; set; }
        public bool IsAdminMode { get; set; }
        public bool ShouldDelete { get; private set; }

        public ProductDetailWindow(Product product, bool isAdminMode)
        {
            InitializeComponent();

            SelectedProduct = product;
            IsAdminMode = isAdminMode;

            DataContext = this;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new ProductEditWindow(SelectedProduct);
            editWindow.Owner = this;

            if (editWindow.ShowDialog() == true && editWindow.EditedProduct != null)
            {
                SelectedProduct = editWindow.EditedProduct;
                DataContext = null;
                DataContext = this;
                DialogResult = true;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show($"Вы уверены, что хотите удалить товар '{SelectedProduct.Name}'?",
                                       "Подтверждение удаления",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                ShouldDelete = true;
                DialogResult = true;
                Close();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}