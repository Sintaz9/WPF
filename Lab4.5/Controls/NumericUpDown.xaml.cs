using System;
using System.Windows;
using System.Windows.Controls;

namespace Lab4._5.Controls
{
    public partial class NumericUpDown : UserControl
    {
        // DependencyProperty для значения с валидацией и коррекцией
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

        // Минимальное значение
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register(
                "MinValue",
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(0, OnMinMaxChanged));

        // Максимальное значение
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(
                "MaxValue",
                typeof(int),
                typeof(NumericUpDown),
                new PropertyMetadata(100, OnMinMaxChanged));

        // Шаг изменения
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

        // BUBBLING событие — всплывает вверх
        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent(
                "ValueChanged",
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<int>),
                typeof(NumericUpDown));

        public event RoutedPropertyChangedEventHandler<int> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        // TUNNELING событие — спускается вниз
        public static readonly RoutedEvent PreviewValueChangedEvent =
            EventManager.RegisterRoutedEvent(
                "PreviewValueChanged",
                RoutingStrategy.Tunnel,
                typeof(RoutedPropertyChangedEventHandler<int>),
                typeof(NumericUpDown));

        public event RoutedPropertyChangedEventHandler<int> PreviewValueChanged
        {
            add { AddHandler(PreviewValueChangedEvent, value); }
            remove { RemoveHandler(PreviewValueChangedEvent, value); }
        }

        // DIRECT событие — только на самом контроле
        public static readonly RoutedEvent ValueRejectedEvent =
            EventManager.RegisterRoutedEvent(
                "ValueRejected",
                RoutingStrategy.Direct,
                typeof(RoutedEventHandler),
                typeof(NumericUpDown));

        public event RoutedEventHandler ValueRejected
        {
            add { AddHandler(ValueRejectedEvent, value); }
            remove { RemoveHandler(ValueRejectedEvent, value); }
        }

        public NumericUpDown()
        {
            InitializeComponent();
        }

        // ВАЛИДАЦИЯ: проверка допустимости значения
        private static bool ValidateValue(object value)
        {
            int val = (int)value;
            return val >= -1000 && val <= 1000;
        }

        // КОРРЕКЦИЯ: приведение к допустимому диапазону
        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            var control = (NumericUpDown)d;
            int val = (int)baseValue;

            if (val < control.MinValue) return control.MinValue;
            if (val > control.MaxValue) return control.MaxValue;
            return val;
        }

        private static void OnValueChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown)d;
            int oldValue = (int)e.OldValue;
            int newValue = (int)e.NewValue;

            // Вызываем Tunneling событие (сначала)
            var previewArgs = new RoutedPropertyChangedEventArgs<int>(oldValue, newValue, PreviewValueChangedEvent);
            control.RaiseEvent(previewArgs);

            // Вызываем Bubbling событие
            var bubbleArgs = new RoutedPropertyChangedEventArgs<int>(oldValue, newValue, ValueChangedEvent);
            control.RaiseEvent(bubbleArgs);
        }

        private static void OnMinMaxChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericUpDown)d;
            control.CoerceValue(ValueProperty);
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            int newValue = Value + Step;
            if (newValue <= MaxValue)
                Value = newValue;
            else
            {
                Value = MaxValue;
                // Direct событие — значение отклонено
                RaiseEvent(new RoutedEventArgs(ValueRejectedEvent, this));
            }
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            int newValue = Value - Step;
            if (newValue >= MinValue)
                Value = newValue;
            else
            {
                Value = MinValue;
                // Direct событие — значение отклонено
                RaiseEvent(new RoutedEventArgs(ValueRejectedEvent, this));
            }
        }
    }
}