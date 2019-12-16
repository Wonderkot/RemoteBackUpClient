using System.Windows;
using RemoteBackUpClient.Data;
using RemoteBackUpClient.Utils;

namespace RemoteBackUpClient.Controls
{
    /// <summary>
    /// Interaction logic for GeneralSettings.xaml
    /// </summary>
    public partial class GeneralSettings
    {
        private readonly Settings _settings;
        public GeneralSettings()
        {
            InitializeComponent();
            _settings = SettingsReader.GetSettings();
            LoginTb.Text = _settings.Login;
            if (!string.IsNullOrEmpty(_settings.Password))
            {
                var secureString = SettingsReader.DecryptString(_settings.Password);
                PasswordTb.Password = SettingsReader.ToInsecureString(secureString);
            }
        }

        private void SaveBtn_OnClick(object sender, RoutedEventArgs e)
        {
            _settings.Login = LoginTb.Text;
            if (!string.IsNullOrEmpty(PasswordTb.Password))
            {
                var s = SettingsReader.ToSecureString(PasswordTb.Password);
                var secureString = SettingsReader.EncryptString(s);
                _settings.Password = secureString;
                SettingsReader.Save(_settings);
            }

            
            Close();
        }

        private void CancelBtn_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
