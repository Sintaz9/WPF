using System.Windows;
using Lab4._5.Controls;
using Lab4._5.ViewModels;

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

        private bool _isInitialized = false;

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // ===== NumericUpDown =====

            // TUNNELING
            this.AddHandler(
                NumericUpDown.PreviewValueChangedEvent,
                new RoutedPropertyChangedEventHandler<int>(OnPreviewValueChanged));

            // BUBBLING
            DemoNumeric.ValueChanged += DemoNumeric_ValueChanged;

            // DIRECT
            DemoNumeric.ValueRejected += DemoNumeric_ValueRejected;


            // ===== ToggleSwitch =====

            // TUNNELING
            this.AddHandler(
                ToggleSwitch.PreviewCheckedChangedEvent,
                new RoutedEventHandler(OnPreviewCheckedChanged));

            // BUBBLING
            DemoToggle.CheckedChanged += DemoToggle_CheckedChanged;

            // DIRECT
            DemoToggle.ToggleClicked += DemoToggle_ToggleClicked;
            _isInitialized = true;
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
                "© 2026",
                "О программе",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        // Tunneling — срабатывает ПЕРВЫМ (от окна к контролу)
        private void OnPreviewValueChanged(object sender,
            RoutedPropertyChangedEventArgs<int> e)
        {
            LogEvent($"[TUNNELING] PreviewValueChanged: {e.OldValue} -> {e.NewValue}");
        }

        // Bubbling — срабатывает ВТОРЫМ (от контрола к окну)
        private void DemoNumeric_ValueChanged(object sender,
            RoutedPropertyChangedEventArgs<int> e)
        {
            if (!_isInitialized || NumericResult == null)
                return;

            NumericResult.Text = $"Значение: {e.NewValue}";
            LogEvent($"[BUBBLING] ValueChanged: {e.OldValue} -> {e.NewValue}");
        }
        // Direct — только на контроле
        private void DemoNumeric_ValueRejected(object sender, RoutedEventArgs e)
        {
            LogEvent($"[DIRECT] ValueRejected");
            MessageBox.Show("Достигнуто граничное значение!",
                           "Предупреждение",
                           MessageBoxButton.OK,
                           MessageBoxImage.Warning);
        }

        // Tunneling — срабатывает ПЕРВЫМ
        private void OnPreviewCheckedChanged(object sender, RoutedEventArgs e)
        {
            LogEvent($"[TUNNELING] PreviewCheckedChanged");
        }

        // Bubbling — срабатывает ВТОРЫМ
        private void DemoToggle_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized || ToggleResult == null)
                return;

            ToggleResult.Text = DemoToggle.IsChecked
                ? "  Включено"
                : "  Выключено";

            LogEvent($"[BUBBLING] CheckedChanged: {DemoToggle.IsChecked}");
        }

        // Direct — только на контроле
        private void DemoToggle_ToggleClicked(object sender, RoutedEventArgs e)
        {
            LogEvent($"[DIRECT] ToggleClicked");
        }

        private void LogEvent(string message)
        {
            EventLog.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            EventLog.ScrollToEnd();
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            EventLog.Text = "";
        }
    }

}