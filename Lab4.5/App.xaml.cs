using System;
using System.Windows;

namespace Lab4._5
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var dict = new ResourceDictionary();
            dict.Source = new Uri("Resources/Strings.ru-RU.xaml", UriKind.Relative);

            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(dict);
        }

        public static void SwitchLanguage(string lang)
        {
            var dict = new ResourceDictionary();

            if (lang == "en-US")
                dict.Source = new Uri("Resources/Strings.en-US.xaml", UriKind.Relative);
            else
                dict.Source = new Uri("Resources/Strings.ru-RU.xaml", UriKind.Relative);

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }
    }
}