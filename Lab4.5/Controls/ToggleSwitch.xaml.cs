using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lab4._5.Controls
{
    public partial class ToggleSwitch : UserControl
    {
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool), typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsCheckedChanged));

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public static readonly DependencyProperty OnTextProperty =
            DependencyProperty.Register("OnText", typeof(string), typeof(ToggleSwitch),
                new PropertyMetadata("ON"));

        public string OnText
        {
            get => (string)GetValue(OnTextProperty);
            set => SetValue(OnTextProperty, value);
        }

        public static readonly DependencyProperty OffTextProperty =
            DependencyProperty.Register("OffText", typeof(string), typeof(ToggleSwitch),
                new PropertyMetadata("OFF"));

        public string OffText
        {
            get => (string)GetValue(OffTextProperty);
            set => SetValue(OffTextProperty, value);
        }

        public static readonly RoutedEvent CheckedChangedEvent =
            EventManager.RegisterRoutedEvent("CheckedChanged", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(ToggleSwitch));

        public event RoutedEventHandler CheckedChanged
        {
            add => AddHandler(CheckedChangedEvent, value);
            remove => RemoveHandler(CheckedChangedEvent, value);
        }

        public ToggleSwitch()
        {
            InitializeComponent();
            UpdateVisualState();
        }

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (ToggleSwitch)d;
            control.UpdateVisualState();
            control.RaiseEvent(new RoutedEventArgs(CheckedChangedEvent, control));
        }

        private void UpdateVisualState()
        {
            if (IsChecked)
            {
                SwitchBorder.Background = new SolidColorBrush(Colors.Green);
                Thumb.Margin = new Thickness(32, 2, 2, 2);
                StateText.Text = OnText;
                StateText.HorizontalAlignment = HorizontalAlignment.Left;
                StateText.Margin = new Thickness(8, 0, 0, 0);
                StateText.Foreground = Brushes.White;
            }
            else
            {
                SwitchBorder.Background = new SolidColorBrush(Colors.LightGray);
                Thumb.Margin = new Thickness(2, 2, 32, 2);
                StateText.Text = OffText;
                StateText.HorizontalAlignment = HorizontalAlignment.Right;
                StateText.Margin = new Thickness(0, 0, 8, 0);
                StateText.Foreground = Brushes.Gray;
            }
        }

        private void SwitchBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
        }
    }
}