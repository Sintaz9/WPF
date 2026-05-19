using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Lab4._5.Controls
{
    public partial class RatingControl : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(RatingControl),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(RatingControl),
                new PropertyMetadata(true));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public RatingControl()
        {
            InitializeComponent();
            Loaded += RatingControl_Loaded;
        }

        private void RatingControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (StarsPanel.Children.Count == 0)
            {
                for (int i = 1; i <= 5; i++)
                {
                    var star = new TextBlock
                    {
                        Text = "★",
                        FontSize = 22,
                        Cursor = IsReadOnly ? Cursors.Arrow : Cursors.Hand,
                        Margin = new Thickness(2, 0, 2, 0),
                        Tag = i,
                        Foreground = new SolidColorBrush(Colors.LightGray),
                        RenderTransformOrigin = new Point(0.5, 0.5),
                        RenderTransform = new ScaleTransform(1, 1)
                    };

                    star.MouseEnter += Star_MouseEnter;
                    star.MouseLeave += Star_MouseLeave;
                    star.MouseDown += Star_MouseDown;

                    StarsPanel.Children.Add(star);
                }
            }
            UpdateStars(Value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RatingControl control && control.IsLoaded)
                control.UpdateStars((double)e.NewValue);
        }

        private void UpdateStars(double val)
        {
            int idx = 0;
            foreach (TextBlock star in StarsPanel.Children)
            {
                idx++;
                var brush = star.Foreground as SolidColorBrush;
                if (brush != null)
                    brush.Color = (idx <= val) ? Colors.Gold : Colors.LightGray;
            }
        }

        private void AnimateStar(TextBlock star, double toScale)
        {
            var scaleTransform = star.RenderTransform as ScaleTransform;
            if (scaleTransform != null)
            {
                var animation = new DoubleAnimation
                {
                    To = toScale,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new QuadraticEase()
                };
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
            }
        }

        private void Star_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is TextBlock star && int.TryParse(star.Tag.ToString(), out int max))
            {
                // Подсвечиваем звёзды
                int i = 0;
                foreach (TextBlock s in StarsPanel.Children)
                {
                    i++;
                    var brush = s.Foreground as SolidColorBrush;
                    if (brush != null)
                        brush.Color = (i <= max) ? Colors.Gold : Colors.LightGray;

                    // Анимация увеличения
                    if (i <= max)
                        AnimateStar(s, 1.3);
                    else
                        AnimateStar(s, 1.0);
                }
            }
        }

        private void Star_MouseLeave(object sender, MouseEventArgs e)
        {
            // Возвращаем исходное состояние
            UpdateStars(Value);

            // Возвращаем обычный размер всем звёздам
            foreach (TextBlock star in StarsPanel.Children)
            {
                AnimateStar(star, 1.0);
            }
        }

        private void Star_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsReadOnly) return;

            if (sender is TextBlock star && int.TryParse(star.Tag.ToString(), out int val))
            {
                Value = val;
                UpdateStars(val);
            }
        }
    }
}