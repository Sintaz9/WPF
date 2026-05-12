using System.Windows;
using Lab4._5.Models;
using Lab4._5.ViewModels;

namespace Lab4._5.Views
{
    public partial class ProductEditWindow : Window
    {
        private ProductEditViewModel _viewModel;

        public ProductEditWindow(Product product = null)
        {
            InitializeComponent();
            _viewModel = new ProductEditViewModel(product);
            DataContext = _viewModel;
        }

        public Product EditedProduct { get; private set; }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsValid())
            {
                EditedProduct = _viewModel.GetProduct();
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Исправьте ошибки в форме перед сохранением.",
                              "Ошибка валидации",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.AddImage();
            UpdatePreview();
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RemoveImage();
            UpdatePreview();
        }

        private void UpdatePreview()
        {
            if (_viewModel.ImagePaths.Count > 0)
            {
                try
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri(_viewModel.ImagePaths[0], UriKind.Absolute));
                    PreviewImage.Source = bitmap;
                    PreviewImage.Visibility = Visibility.Visible;
                }
                catch
                {
                    PreviewImage.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                PreviewImage.Visibility = Visibility.Collapsed;
            }
        }
    }
}