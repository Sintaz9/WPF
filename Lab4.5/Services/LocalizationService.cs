using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;

namespace Lab4._5.Services
{
    public static class LocalizationService
    {
        private static readonly Dictionary<string, ResourceDictionary> _languages = new();
        private static string _currentLanguage;

        static LocalizationService()
        {
            // Регистрируем языки
            RegisterLanguage("ru-RU", "Resources/Strings.ru-RU.xaml");
            RegisterLanguage("en-US", "Resources/Strings.en-US.xaml");

            // Язык по умолчанию
            SwitchLanguage("ru-RU");
        }

        private static void RegisterLanguage(string cultureName, string resourcePath)
        {
            var dict = new ResourceDictionary
            {
                Source = new Uri(resourcePath, UriKind.Relative)
            };
            _languages[cultureName] = dict;
        }

        public static void SwitchLanguage(string cultureName)
        {
            if (_languages.ContainsKey(cultureName))
            {
                // Удаляем старый язык
                if (_currentLanguage != null && _languages.ContainsKey(_currentLanguage))
                {
                    Application.Current.Resources.MergedDictionaries.Remove(_languages[_currentLanguage]);
                }

                // Добавляем новый
                Application.Current.Resources.MergedDictionaries.Add(_languages[cultureName]);
                _currentLanguage = cultureName;

                // Устанавливаем культуру
                CultureInfo.CurrentCulture = new CultureInfo(cultureName);
                CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
            }
        }

        public static string CurrentLanguage => _currentLanguage;
    }
}