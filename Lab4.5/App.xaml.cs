using System;
using System.Linq;
using System.Windows;

namespace Lab4._5
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Язык уже загружен в App.xaml, ничего не делаем
        }

        public static void SwitchLanguage(string lang)
        {
            // Найти и удалить старый языковой словарь (содержит "Strings" в пути)
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
            // Найти и удалить старый словарь темы (содержит "Themes/" в пути)
            var oldDict = Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Themes/"));

            if (oldDict != null)
                Current.Resources.MergedDictionaries.Remove(oldDict);

            // Создать новый словарь темы
            var dict = new ResourceDictionary();
            dict.Source = new Uri($"Themes/{themeName}.xaml", UriKind.Relative);

            // Вставить после языкового словаря
            Current.Resources.MergedDictionaries.Insert(1, dict);
        }
    }
}