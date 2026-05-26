using System.Windows.Input;

namespace Lab4._5.Commands
{
    public static class CustomCommands
    {
        public static readonly RoutedUICommand BuyProduct =
            new RoutedUICommand(
                "Купить товар",
                "BuyProduct",
                typeof(CustomCommands),
                new InputGestureCollection
                {
                    new KeyGesture(Key.B, ModifierKeys.Control)
                });    
    }
}