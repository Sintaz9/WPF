using System.Windows;
using Lab4._5.Services;

namespace Lab4._5
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LocalizationService.SwitchLanguage("ru-RU");
        }
    }
}