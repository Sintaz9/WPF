using Lab4._5.ViewModels;
using System.Windows;

namespace Lab4._5.Views
{
    public partial class EFDemoWindow : Window
    {
        public EFDemoWindow()
        {
            InitializeComponent();
            DataContext = new EFDemoViewModel();
        }
    }
}