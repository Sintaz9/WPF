using Lab4._5.Controls;
using Lab4._5.Models;
using Lab4._5.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //  TUNNELING 
            AddHandler(UIElement.PreviewMouseDownEvent, new MouseButtonEventHandler(OnTunnelingEvent), true);
            
            //  BUBBLING
            AddHandler(UIElement.MouseDownEvent, new MouseButtonEventHandler(OnBubblingEvent), true);
        }

        // TUNNELING
        private void OnTunnelingEvent(object sender, MouseButtonEventArgs e)
        {
            // Подсвечиваем синим
            TunnelingIndicator.Background = Brushes.DarkBlue;
            BubblingIndicator.Background = Brushes.LightGreen;
            DirectIndicator.Background = Brushes.LightCoral;
            
            string elementName = (e.OriginalSource as FrameworkElement)?.Name ?? 
                                 (e.OriginalSource as FrameworkElement)?.GetType().Name ?? "неизвестно";
            
            RoutingLog.Text = $"🔵 TUNNELING: событие на окне ПЕРВЫМ! Цель: {elementName}";
            
            // Возвращаем цвет через 0.5 секунды
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += (s, args) => { TunnelingIndicator.Background = Brushes.LightBlue; timer.Stop(); };
            timer.Start();
        }

        // BUBBLING
        private void OnBubblingEvent(object sender, MouseButtonEventArgs e)
        {
            // Подсвечиваем зелёным
            TunnelingIndicator.Background = Brushes.LightBlue;
            BubblingIndicator.Background = Brushes.DarkGreen;
            DirectIndicator.Background = Brushes.LightCoral;
            
            string elementName = (e.OriginalSource as FrameworkElement)?.Name ?? 
                                 (e.OriginalSource as FrameworkElement)?.GetType().Name ?? "неизвестно";
            
            RoutingLog.Text = $"🟢 BUBBLING: событие поднялось до окна! Цель: {elementName}";
            
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += (s, args) => { BubblingIndicator.Background = Brushes.LightGreen; timer.Stop(); };
            timer.Start();
        }

        // ========== СТАРЫЕ МЕТОДЫ (НЕ ТРОГАЕМ) ==========
        
        private void HandlePreviewValueChanged(object controlSender, RoutedPropertyChangedEventArgs<int> valueArgs)
        {
            if (valueArgs.NewValue > 20)
            {
                MessageBox.Show("Нельзя заказать больше 20 товаров.", "Ограничение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                valueArgs.Handled = true;
            }
        }

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

        private void BuyProduct_CanExecute(object commandSender, CanExecuteRoutedEventArgs commandArgs)
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

        private void BuyProduct_Executed(object commandSender, ExecutedRoutedEventArgs commandArgs)
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

        // ========== ДЛЯ ДЕМОНСТРАЦИИ DIRECT ==========
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DirectIndicator.Background = Brushes.DarkRed;
            RoutingLog.Text = "🔴 DIRECT: событие только на кнопке меню!";
            
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.5);
            timer.Tick += (s, args) => { DirectIndicator.Background = Brushes.LightCoral; timer.Stop(); };
            timer.Start();
        }
    }
}