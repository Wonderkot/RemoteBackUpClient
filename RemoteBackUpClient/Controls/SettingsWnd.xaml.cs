using System.Windows;
using RemoteBackUpClient.Data;
using RemoteBackUpClient.Utils;

namespace RemoteBackUpClient.Controls
{
    /// <summary>
    /// Interaction logic for SettingsWnd.xaml
    /// </summary>
    public partial class SettingsWnd : Window
    {
        private Settings _settings;
        public SettingsWnd()
        {
            _settings = SettingsReader.GetSettings();
            InitializeComponent();
            SettingsGrd.ItemsSource = _settings.List;
        }

        private void CancelBtn_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveBtn_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsReader.Save(_settings);
            Close();
        }

        private void ResetBtn_OnClick(object sender, RoutedEventArgs e)
        {
            _settings = SettingsReader.GetSettings();
            SettingsGrd.ItemsSource = null;
            SettingsGrd.ItemsSource = _settings.List;
        }
    }
}
