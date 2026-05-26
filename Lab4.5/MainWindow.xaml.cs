using Lab4._5.Controls;
using Lab4._5.Models;
using Lab4._5.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace Lab4._5.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object windowSender, RoutedEventArgs loadedArgs)
        {
            // Tunneling для NumericUpDown
            AddHandler(
                NumericUpDown.PreviewValueChangedEvent,
                new RoutedPropertyChangedEventHandler<int>(HandlePreviewValueChanged));

            // Bubbling для ToggleSwitch
            AddHandler(
                ToggleSwitch.CheckedChangedEvent,
                new RoutedEventHandler(HandleRoleChanged));
        }

        // TUNNELING
        // Проверка выполняется ДО изменения значения
        private void HandlePreviewValueChanged(object controlSender,
            RoutedPropertyChangedEventArgs<int> valueArgs)
        {
            if (valueArgs.NewValue > 20)
            {
                MessageBox.Show(
                    "Нельзя заказать больше 20 товаров.",
                    "Ограничение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                valueArgs.Handled = true;
            }
        }

        // BUBBLING
        // Срабатывает после изменения состояния ToggleSwitch
        private void HandleRoleChanged(object toggleSender, RoutedEventArgs toggleArgs)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.StatusText = $"Роль изменена: {viewModel.CurrentRole}";
            }
        }

        private void ExitMenuItem_Click(object menuSender, RoutedEventArgs clickArgs)
        {
            Application.Current.Shutdown();
        }

        private void AboutMenuItem_Click(object infoSender, RoutedEventArgs infoArgs)
        {
            MessageBox.Show(
                "Магазин кофе и чая\n" +
                "Лабораторная работа №7\n\n" +
                "Пользовательские элементы управления WPF\n" +
                "DependencyProperty, RoutedEvent, RoutedUICommand\n\n" +
                "© 2026",
                "О программе",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        // Проверяем, можно ли выполнить команду
        private void BuyProduct_CanExecute(object commandSender,
            CanExecuteRoutedEventArgs commandArgs)
        {
            if (DataContext is MainViewModel mainViewModel)
            {
                commandArgs.CanExecute =
                    mainViewModel.SelectedProduct != null &&
                    mainViewModel.SelectedProduct.InStock &&
                    !mainViewModel.IsAdminMode;
            }
            else
            {
                commandArgs.CanExecute = false;
            }
        }

        // Выполнение RoutedUICommand
        private void BuyProduct_Executed(object commandSender,
            ExecutedRoutedEventArgs commandArgs)
        {
            if (DataContext is MainViewModel mainViewModel)
            {
                Product selectedItem = mainViewModel.SelectedProduct;

                if (selectedItem != null)
                {
                    mainViewModel.BuyProductCommand.Execute(selectedItem);
                }
            }
        }
    }
}