using System.Windows;
using Lab4._5.ViewModels;

namespace Lab4._5.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Магазин кофе и чая\n" +
                "Лабораторная работа №4-5\n" +
                "WPF приложение для продажи товаров\n\n" +
                "© 2024",
                "О программе",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}