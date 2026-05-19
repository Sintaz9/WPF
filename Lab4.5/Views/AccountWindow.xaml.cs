using System.Windows;
using Lab4._5.ViewModels;

namespace Lab4._5.Views
{
    public partial class AccountWindow : Window
    {
        private AccountViewModel _viewModel;

        public AccountWindow(string role)
        {
            InitializeComponent();
            _viewModel = new AccountViewModel(role);
            DataContext = _viewModel;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Save();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}