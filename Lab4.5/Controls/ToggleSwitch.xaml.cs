using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Lab4._5.Controls
{
    public partial class ToggleSwitch : UserControl
    {
        // DependencyProperty для состояния
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                "IsChecked",
                typeof(bool),
                typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnIsCheckedChanged,
                    CoerceIsChecked),
                ValidateIsChecked);

        public static readonly DependencyProperty OnColorProperty =
            DependencyProperty.Register("OnColor", typeof(Brush), typeof(ToggleSwitch),
                new PropertyMetadata(new SolidColorBrush(Colors.Green)));

        public static readonly DependencyProperty OffColorProperty =
            DependencyProperty.Register("OffColor", typeof(Brush), typeof(ToggleSwitch),
                new PropertyMetadata(new SolidColorBrush(Colors.LightGray)));

        public static readonly DependencyProperty OnTextProperty =
            DependencyProperty.Register("OnText", typeof(string), typeof(ToggleSwitch),
                new PropertyMetadata("ON"));

        public static readonly DependencyProperty OffTextProperty =
            DependencyProperty.Register("OffText", typeof(string), typeof(ToggleSwitch),
                new PropertyMetadata("OFF"));

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public Brush OnColor
        {
            get => (Brush)GetValue(OnColorProperty);
            set => SetValue(OnColorProperty, value);
        }

        public Brush OffColor
        {
            get => (Brush)GetValue(OffColorProperty);
            set => SetValue(OffColorProperty, value);
        }

        public string OnText
        {
            get => (string)GetValue(OnTextProperty);
            set => SetValue(OnTextProperty, value);
        }

        public string OffText
        {
            get => (string)GetValue(OffTextProperty);
            set => SetValue(OffTextProperty, value);
        }

        // BUBBLING событие (всплывает вверх)
        public static readonly RoutedEvent CheckedChangedEvent =
            EventManager.RegisterRoutedEvent("CheckedChanged", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(ToggleSwitch));

        public event RoutedEventHandler CheckedChanged
        {
            add => AddHandler(CheckedChangedEvent, value);
            remove => RemoveHandler(CheckedChangedEvent, value);
        }

        // TUNNELING событие (спускается вниз)
        public static readonly RoutedEvent PreviewCheckedChangedEvent =
            EventManager.RegisterRoutedEvent("PreviewCheckedChanged", RoutingStrategy.Tunnel,
                typeof(RoutedEventHandler), typeof(ToggleSwitch));

        public event RoutedEventHandler PreviewCheckedChanged
        {
            add => AddHandler(PreviewCheckedChangedEvent, value);
            remove => RemoveHandler(PreviewCheckedChangedEvent, value);
        }

        // DIRECT событие (только на контроле)
        public static readonly RoutedEvent ToggleClickedEvent =
            EventManager.RegisterRoutedEvent("ToggleClicked", RoutingStrategy.Direct,
                typeof(RoutedEventHandler), typeof(ToggleSwitch));

        public event RoutedEventHandler ToggleClicked
        {
            add => AddHandler(ToggleClickedEvent, value);
            remove => RemoveHandler(ToggleClickedEvent, value);
        }

        public ToggleSwitch()
        {
            InitializeComponent();
            UpdateVisualState(false);
        }

        // ВАЛИДАЦИЯ (формальная, всегда true)
        private static bool ValidateIsChecked(object value) => value is bool;

        // КОРРЕКЦИЯ (пропускаем как есть)
        private static object CoerceIsChecked(DependencyObject d, object baseValue) => baseValue;

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ToggleSwitch)d;
            bool isChecked = (bool)e.NewValue;

            control.UpdateVisualState(isChecked);
            control.RaiseEvent(new RoutedEventArgs(PreviewCheckedChangedEvent, control)); // Tunneling
            control.RaiseEvent(new RoutedEventArgs(CheckedChangedEvent, control));       // Bubbling
        }

        private void UpdateVisualState(bool isChecked)
        {
            if (isChecked)
            {
                BackgroundBrush.Color = (OnColor as SolidColorBrush)?.Color ?? Colors.Green;
                Thumb.Margin = new Thickness(30, 2, 2, 2);
                StateText.Text = OnText;
                StateText.Margin = new Thickness(-20, 0, 0, 0);
            }
            else
            {
                BackgroundBrush.Color = (OffColor as SolidColorBrush)?.Color ?? Colors.LightGray;
                Thumb.Margin = new Thickness(2, 2, 30, 2);
                StateText.Text = OffText;
                StateText.Margin = new Thickness(20, 0, 0, 0);
            }
        }

        private void SwitchBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
            RaiseEvent(new RoutedEventArgs(ToggleClickedEvent, this)); // Direct
        }
    }
}