using System;
using System.Linq;
using System.Windows;
using Lab4._5.Database;

namespace Lab4._5
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации базы данных:\n{ex.Message}",
                    "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static void SwitchLanguage(string lang)
        {
            var oldDict = Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Strings"));

            if (oldDict != null)
                Current.Resources.MergedDictionaries.Remove(oldDict);

            // Создать новый словарь
            var dict = new ResourceDictionary();
            dict.Source = new Uri(lang == "en-US"
                ? "Resources/Strings.en-US.xaml"
                : "Resources/Strings.ru-RU.xaml", UriKind.Relative);

            // Добавить в начало, чтобы языковые строки имели приоритет
            Current.Resources.MergedDictionaries.Insert(0, dict);
        }

        public static void SwitchTheme(string themeName)
        {
            var oldDict = Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Themes/"));

            if (oldDict != null)
                Current.Resources.MergedDictionaries.Remove(oldDict);

            var dict = new ResourceDictionary();
            dict.Source = new Uri($"Themes/{themeName}.xaml", UriKind.Relative);

            Current.Resources.MergedDictionaries.Insert(1, dict);
        }
    }
}