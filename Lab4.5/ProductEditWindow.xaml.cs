using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
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

            // Если есть изображение — показываем его
            if (_viewModel.HasImage)
            {
                PreviewImage.Source = ConvertBytesToImage(_viewModel.Image);
                PreviewImage.Visibility = Visibility.Visible;
            }
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
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Все файлы|*.*";
            dialog.Title = "Выберите изображение для товара";

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    byte[] imageBytes = File.ReadAllBytes(dialog.FileName);
                    _viewModel.AddImage(imageBytes);
                    PreviewImage.Source = ConvertBytesToImage(imageBytes);
                    PreviewImage.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.RemoveImage();
            PreviewImage.Source = null;
            PreviewImage.Visibility = Visibility.Collapsed;
        }

        private BitmapImage ConvertBytesToImage(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return null;

            using (var stream = new MemoryStream(imageBytes))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
        }
    }
}