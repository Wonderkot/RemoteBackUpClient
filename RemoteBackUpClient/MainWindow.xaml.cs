using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Notifications.Wpf;
using Ookii.Dialogs.Wpf;
using RemoteBackUpClient.Controls;
using RemoteBackUpClient.Data;
using RemoteBackUpClient.Utils;
using RequestProcessorLib.Classes;
using RequestProcessorLib.Interfaces;


namespace RemoteBackUpClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    enum ActionList
    {
        GetNew = 0, CheckLast = 1, GetLast = 2
    }
    public partial class MainWindow
    {
        private readonly IRequestSender _requestSender = new RequestSender();
        private Settings _settings;
        private TaskbarIcon _tbi;
        private NotificationManager _notificationManager;
        event Action<string> ShowMessage;
        private event Action<string> ShowBalloonMsg;

        public MainWindow()
        {
            InitializeComponent();
            Init();
            Closed += OnClosed;
        }

        private void OnClosed(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void Init()
        {
            _notificationManager = new NotificationManager();
            CreateTaskBarIcon();
            ShowMessage += AddTextToConsole;
            ShowBalloonMsg += ShowBalloonTip;
            _requestSender.Init(ShowMessage, ShowBalloonMsg);

            try
            {
                _settings = SettingsReader.GetSettings();
            }
            catch (Exception e)
            {
                AddTextToConsole(e.Message);
            }
            if (_settings != null)
            {
                DbList.Items.Clear();
                SelectedFolder.Text = _settings.DefaultPath;
                FileNameTb.Text = _settings.SelectedDb;

                foreach (var listItem in _settings.List)
                {
                    DbList.Items.Add(listItem.DbName);
                }

                if (!string.IsNullOrEmpty(_settings.SelectedDb))
                {
                    DbList.SelectedValue = _settings.SelectedDb;
                    UrlTb.Text = _settings.List.FirstOrDefault(i => i.DbName == _settings.SelectedDb)?.Url ?? string.Empty;
                }
            }
        }

        private void CreateTaskBarIcon()
        {
            var contextMenu = new ContextMenu();
            MenuItem item = new MenuItem()
            {
                Header = "Close"
            };
            item.Click += ItemOnClick;
            contextMenu.Items.Add(item);
            _tbi = new TaskbarIcon
            {
                Icon = Properties.Resources.cat1
            };
            _tbi.TrayLeftMouseUp += (sender, args) =>
            {
                switch (WindowState)
                {
                    case WindowState.Normal:
                        WindowState = WindowState.Minimized;
                        break;
                    case WindowState.Minimized:
                        WindowState = WindowState.Normal;
                        break;
                }

            };
            _tbi.ContextMenu = contextMenu;
        }

        private void ItemOnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveSettings()
        {
            _settings.SelectedDb = DbList.SelectionBoxItem.ToString();
            _settings.DefaultPath = SelectedFolder.Text;
            SettingsReader.Save(_settings);
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Normal:
                    ShowInTaskbar = true;
                    break;
                case WindowState.Minimized:
                    ShowInTaskbar = false;
                    break;
                case WindowState.Maximized:
                    ShowInTaskbar = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private void ExecuteBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var urlTbText = UrlTb.Text;
            var selectedFolderText = SelectedFolder.Text;
            string fileName = FileNameTb.Text;
            var dbName = DbList.SelectedItem.ToString();
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = dbName + ".7z";
            }

            try
            {
                ExecuteAction(urlTbText, dbName, selectedFolderText, fileName, ActionList.GetNew);
            }
            catch (Exception exception)
            {
                AddTextToConsole(exception.Message);
            }
        }

        private void ExecuteAction(string urlTbText, string dbName, string selectedFolderText, string fileName, ActionList action)
        {
            ExecuteBtn.IsEnabled = false;
            if (!string.IsNullOrEmpty(urlTbText) && !string.IsNullOrEmpty(dbName))
            {
                var thread = new Thread(() =>
                {
                    string data;
                    switch (action)
                    {
                        case ActionList.GetNew:
                            data = _requestSender.CreateNewBackupRequest(urlTbText, dbName);
                            break;
                        case ActionList.CheckLast:
                            data = _requestSender.CheckLastBackup(urlTbText, dbName);
                            break;
                        case ActionList.GetLast:
                            data = _requestSender.GetLastBackUp(urlTbText, dbName);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(action), action, null);
                    }

                    if (data != null)
                    {
                        FileUtils.SaveFile(data, selectedFolderText, fileName);
                        AddTextToConsole("Saved to " + selectedFolderText);
                    }

                    ExecuteBtn?.Dispatcher?.Invoke(() => { ExecuteBtn.IsEnabled = true; });
                });
                thread.Start();
            }
        }

        private void SelectFolder_OnClick(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = "Please select a folder.";
            dialog.UseDescriptionForTitle = true; // This applies to the Vista style dialog only, not the old dialog.
            if (!VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
            {
                MessageBox.Show(this, "Because you are not using Windows Vista or later, the regular folder browser dialog will be used. Please use Windows Vista to see the new dialog.", "Sample folder browser dialog");
            }

            var showDialog = dialog.ShowDialog(this);

            if (showDialog != null && showDialog == true)
            {
                SelectedFolder.Text = dialog.SelectedPath;
            }
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddTextToConsole(string msg)
        {
            Console?.Dispatcher?.Invoke(() =>
            {
                Console.Text += msg;
                Console.Text += Environment.NewLine;
            });
        }

        private void GetLastBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var urlTbText = UrlTb.Text;
            var selectedFolderText = SelectedFolder.Text;
            string fileName = FileNameTb.Text;
            var dbName = DbList.SelectedItem.ToString();
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = dbName + ".7z";
            }
            ExecuteBtn.IsEnabled = false;

            try
            {
                ExecuteAction(urlTbText, dbName, selectedFolderText, fileName, ActionList.GetLast);
            }
            catch (Exception exception)
            {
                AddTextToConsole(exception.Message);
            }
        }

        private void CheckBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var urlTbText = UrlTb.Text;
            var selectedFolderText = SelectedFolder.Text;
            string fileName = FileNameTb.Text;
            var dbName = DbList.SelectedItem.ToString();
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = dbName + ".7z";
            }
            ExecuteBtn.IsEnabled = false;

            try
            {
                ExecuteAction(urlTbText, dbName, selectedFolderText, fileName, ActionList.CheckLast);
            }
            catch (Exception exception)
            {
                AddTextToConsole(exception.Message);
            }
        }

        private void ClearBtn_OnClick(object sender, RoutedEventArgs e)
        {
            Console.Text = string.Empty;
        }


        private void SettingsItem_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsWnd settingsWnd = new SettingsWnd();
            settingsWnd.ShowDialog();
            _settings = SettingsReader.GetSettings();
            Init();
        }


        private void DbList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(DbList?.SelectedValue?.ToString()))
            {
                UrlTb.Text = _settings.List.FirstOrDefault(i => i.DbName == DbList.SelectedValue.ToString())?.Url ?? string.Empty;
                FileNameTb.Text = DbList.SelectedValue.ToString();
            }
        }

        public void ShowBalloonTip(string msg)
        {
            _tbi.ShowBalloonTip("Remote Backup", msg, BalloonIcon.Info);
            //also show notification
            _notificationManager.Show(new NotificationContent()
            {
                Title = "Remote Backup",
                Message = msg,
                Type = NotificationType.Information
            });
        }
    }
}
