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

            var mainDict = new ResourceDictionary();
            mainDict.MergedDictionaries.Add(dict);

            Application.Current.Resources = mainDict;
        }

        public static void SwitchLanguage(string lang)
        {
            var dict = new ResourceDictionary();

            if (lang == "en-US")
                dict.Source = new Uri("Resources/Strings.en-US.xaml", UriKind.Relative);
            else
                dict.Source = new Uri("Resources/Strings.ru-RU.xaml", UriKind.Relative);

            var mainDict = new ResourceDictionary();
            mainDict.MergedDictionaries.Add(dict);

            Application.Current.Resources = mainDict;
        }
    }
}