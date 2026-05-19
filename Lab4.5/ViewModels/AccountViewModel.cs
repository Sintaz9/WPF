using System.Windows.Input;
using Lab4._5.Commands;
using Lab4._5.Models;

namespace Lab4._5.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        private string _displayName;
        private string _email;
        private string _about;
        private string _role;
        private string _selectedLanguage;
        private string _selectedTheme;

        public AccountViewModel(string role)
        {
            _role = role;
            var user = User.LoadFromFile(role);

            if (user == null)
                user = User.CreateDefault(role);

            User.CurrentUser = user;

            _displayName = user.DisplayName;
            _email = user.Email;
            _about = user.About ?? "";
            _selectedLanguage = "ru-RU";
            _selectedTheme = "LightGreenTheme";

            SaveCommand = new RelayCommand(_ => Save());
            SwitchThemeCommand = new RelayCommand(theme => SwitchTheme(theme?.ToString()));
            SwitchLanguageCommand = new RelayCommand(lang => SwitchLanguage(lang?.ToString()));
        }

        public string DisplayName { get => _displayName; set { _displayName = value; OnPropertyChanged(); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
        public string About { get => _about; set { _about = value; OnPropertyChanged(); } }
        public string Username => User.CurrentUser?.Username ?? "";
        public string Role => User.CurrentUser?.Role ?? "";

        public string SelectedLanguage { get => _selectedLanguage; set { _selectedLanguage = value; OnPropertyChanged(); } }
        public string SelectedTheme { get => _selectedTheme; set { _selectedTheme = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand SwitchThemeCommand { get; }
        public ICommand SwitchLanguageCommand { get; }

        public void Save()
        {
            var user = User.CurrentUser;
            if (user == null) return;

            user.DisplayName = _displayName;
            user.Email = _email;
            user.About = _about;

            User.SaveToFile(user);
        }

        private void SwitchTheme(string themeName)
        {
            SelectedTheme = themeName;
            App.SwitchTheme(themeName);
        }

        private void SwitchLanguage(string lang)
        {
            SelectedLanguage = lang;
            App.SwitchLanguage(lang);
        }
    }
}