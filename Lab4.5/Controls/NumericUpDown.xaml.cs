using System.Windows;
using System.Windows.Controls;

namespace Lab4._5.Controls
{
    public partial class NumericUpDown : UserControl
    {
        // DependencyProperty с валидацией и коррекцией
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(int),
                typeof(NumericUpDown),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged,
                    CoerceValue),
                ValidateValue);

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register(
                "MinValue",
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(0, OnMinMaxChanged));

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(
                "MaxValue",
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(100, OnMinMaxChanged));

        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register(
                "Step",
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(1));

        // Свойства-обёртки
        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public int MinValue
        {
            get => (int)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        public int MaxValue
        {
            get => (int)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public int Step
        {
            get => (int)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        public NumericUpDown()
        {
            InitializeComponent();
        }


        // Проверяет, что значение находится в глобальном диапазоне -1000...1000
        private static bool ValidateValue(object value)
        {
            int val = (int)value;
            return val >= -1000 && val <= 1000;
        }


        // Принудительно корректирует значение, приводя к диапазону MinValue...MaxValue
        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            var control = (NumericUpDown)d;
            int val = (int)baseValue;

            if (val < control.MinValue) return control.MinValue;
            if (val > control.MaxValue) return control.MaxValue;
            return val;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Значение успешно изменилось
            var control = (NumericUpDown)d;
            System.Diagnostics.Debug.WriteLine($"Значение изменено: {e.OldValue} → {e.NewValue}");
        }

        private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown)d;
            control.CoerceValue(ValueProperty);
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            int newValue = Value + Step;
            if (newValue <= MaxValue)
            {
                Value = newValue;
            }
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            int newValue = Value - Step;
            if (newValue >= MinValue)
            {
                Value = newValue;
            }
        }
    }
}