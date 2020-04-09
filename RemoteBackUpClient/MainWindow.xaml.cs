using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Hardcodet.Wpf.TaskbarNotification;
using Notifications.Wpf;
using Ookii.Dialogs.Wpf;
using RemoteBackUpClient.Controls;
using RemoteBackUpClient.Data;
using RemoteBackUpClient.Jobs;
using RemoteBackUpClient.Utils;
using RequestProcessorLib.Classes;
using RequestProcessorLib.Interfaces;


namespace RemoteBackUpClient
{
    enum CloseReason
    {
        EndTask,
        Logoff,
        User,
        Manually
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly IRequestSender _requestSender = new RequestSender();
        private Settings _settings;
        private TaskbarIcon _tbi;
        private NotificationManager _notificationManager;
        event Action<string> ShowMessage;
        private event Action<string> ShowBalloonMsg;
        private CloseReason _closeReason;

        public MainWindow()
        {
            InitializeComponent();
            Init();
            Closed += OnClosed;

            BackupScheduler.Start();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void Init()
        {
            if (_tbi == null)
            {
                _notificationManager = new NotificationManager();
                CreateTaskBarIcon();
                ShowMessage += AddTextToConsole;
                ShowBalloonMsg += ShowBalloonTip;
            }

            try

            {
                _settings = SettingsReader.GetSettings();

                if (string.IsNullOrEmpty(_settings.Password))
                {
                    MessageBox.Show("Password is empty!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                var secureString = SettingsReader.DecryptString(_settings.Password);
                var rawPass = SettingsReader.ToInsecureString(secureString);

                _requestSender.Init(ShowMessage, ShowBalloonMsg, _settings.Login, rawPass);
            }
            catch (Exception e)
            {
                AddTextToConsole(e.Message);
            }
            if (_settings != null)
            {
                //bind controls to settings
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

            //hide instead of close
            Loaded += delegate
            {
                HwndSource source = (HwndSource)PresentationSource.FromDependencyObject(this);
                source?.AddHook(WindowProc);
            };
            Closing += (x, y) =>
            {
                switch (_closeReason)
                {
                    case CloseReason.EndTask:
                        break;
                    case CloseReason.Logoff:
                        break;
                    case CloseReason.User:
                        WindowState = WindowState.Minimized;
                        y.Cancel = true;
                        break;
                    case CloseReason.Manually:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
        }

        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x11:
                case 0x16:
                    _closeReason = CloseReason.Logoff;
                    break;

                case 0x112:
                    if (((ushort)wParam & 0xfff0) == 0xf060)
                        _closeReason = CloseReason.User;
                    break;

                    // CloseReason.EndTask gets a 0x10 windows message which is got by CloseReason.User too,
                    // so we have no way to identify it,
                    // except knowing that we did not got any of the specific messages of the other CloseReasons
            }
            return IntPtr.Zero;

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
            _closeReason = CloseReason.Manually;
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
            var dbName = DbList.SelectedItem?.ToString();
            try
            {
                ExecuteAction(urlTbText, dbName, selectedFolderText, fileName, ActionList.CreateNewBackup);
            }
            catch (Exception exception)
            {
                AddTextToConsole(exception.Message);
            }
        }

        private void ExecuteAction(string urlTbText, string dbName, string selectedFolderText, string fileName, ActionList action)
        {
            Check(urlTbText, dbName, selectedFolderText, fileName, action);
            ExecuteBtn.IsEnabled = false;
            CheckBtn.IsEnabled = false;
            GetLastBtn.IsEnabled = false;
            ClearBtn.IsEnabled = false;

            if (!string.IsNullOrEmpty(urlTbText) && !string.IsNullOrEmpty(dbName))
            {
                var thread = new Thread(() =>
                {
                    string path;
                    switch (action)
                    {
                        case ActionList.CreateNewBackup:
                            path = Path.Combine(selectedFolderText, fileName);
                            _requestSender.InvokeAction(urlTbText, dbName, path, action);
                            break;
                        case ActionList.CheckExistFile:
                            _requestSender.InvokeAction(urlTbText, dbName, null, action);
                            break;
                        case ActionList.GetLastBackUp:
                            path = Path.Combine(selectedFolderText, fileName);
                            _requestSender.InvokeAction(urlTbText, dbName, path, action);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(action), action, null);
                    }

                    //if (data != null)
                    //{
                    //    FileUtils.SaveFile(data, selectedFolderText, fileName);
                    //    AddTextToConsole("Saved to " + selectedFolderText);
                    //}

                    ExecuteBtn?.Dispatcher?.Invoke(() => { ExecuteBtn.IsEnabled = true; });
                    CheckBtn?.Dispatcher?.Invoke(() => { CheckBtn.IsEnabled = true; });
                    GetLastBtn?.Dispatcher?.Invoke(() => { GetLastBtn.IsEnabled = true; });
                    ClearBtn?.Dispatcher?.Invoke(() => { ClearBtn.IsEnabled = true; });
                });
                thread.Start();
            }
        }

        private static void Check(string urlTbText, string dbName, string selectedFolderText, string fileName, ActionList action)
        {

            switch (action)
            {
                case ActionList.CreateNewBackup:
                    if (string.IsNullOrEmpty(urlTbText) || string.IsNullOrEmpty(dbName) || string.IsNullOrEmpty(fileName))
                    {
                        throw new Exception("Fields DB, URL and FileName is Required!");
                    }

                    if (string.IsNullOrEmpty(selectedFolderText))
                    {
                        throw new Exception("Please, select folder.");
                    }
                    break;
                case ActionList.CheckExistFile:
                    if (string.IsNullOrEmpty(urlTbText) || string.IsNullOrEmpty(dbName) || string.IsNullOrEmpty(fileName))
                    {
                        throw new Exception("Fields DB, URL and FileName is Required!");
                    }
                    break;
                case ActionList.GetLastBackUp:
                    if (string.IsNullOrEmpty(urlTbText) || string.IsNullOrEmpty(dbName) || string.IsNullOrEmpty(fileName))
                    {
                        throw new Exception("Fields DB, URL and FileName is Required!");
                    }

                    if (string.IsNullOrEmpty(selectedFolderText))
                    {
                        throw new Exception("Please, select folder.");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
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
                _settings.DefaultPath = SelectedFolder.Text;
                SettingsReader.Save(_settings);
            }
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            _closeReason = CloseReason.Manually;
            Close();
        }

        /// <summary>
        /// Write messages from other services into text block
        /// </summary>
        /// <param name="msg"></param>
        private void AddTextToConsole(string msg)
        {
            Console?.Dispatcher?.Invoke(() =>
            {
                Console.Text += msg;
                Console.Text += Environment.NewLine;
            });
            ScrollViewer?.Dispatcher?.Invoke(() => ScrollViewer.ScrollToEnd());
        }

        /// <summary>
        /// Get last created back up file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetLastBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var urlTbText = UrlTb.Text;
            var selectedFolderText = SelectedFolder.Text;
            string fileName = FileNameTb.Text;
            var dbName = DbList.SelectedItem?.ToString();
            try
            {
                ExecuteAction(urlTbText, dbName, selectedFolderText, fileName, ActionList.GetLastBackUp);
            }
            catch (Exception exception)
            {
                AddTextToConsole(exception.Message);
            }
        }

        /// <summary>
        /// Check if exist last back up file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var urlTbText = UrlTb.Text;
            var selectedFolderText = SelectedFolder.Text;
            string fileName = FileNameTb.Text;
            var dbName = DbList.SelectedItem?.ToString();

            try
            {
                ExecuteAction(urlTbText, dbName, selectedFolderText, fileName, ActionList.CheckExistFile);
            }
            catch (Exception exception)
            {
                AddTextToConsole(exception.Message);
            }
        }

        /// <summary>
        /// Clear console text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearBtn_OnClick(object sender, RoutedEventArgs e)
        {
            Console.Text = string.Empty;
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
            //_tbi.ShowBalloonTip("Remote Backup", msg, BalloonIcon.Info);
            //also show notification
            _notificationManager.Show(new NotificationContent()
            {
                Title = "Remote Backup",
                Message = msg,
                Type = NotificationType.Information
            });
        }

        private void ServerList_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsWnd settingsWnd = new SettingsWnd();
            settingsWnd.ShowDialog();
            _settings = SettingsReader.GetSettings();
            Init();
        }

        private void General_OnClick(object sender, RoutedEventArgs e)
        {
            GeneralSettings generalSettings = new GeneralSettings();
            generalSettings.ShowDialog();
            _settings = SettingsReader.GetSettings();
            Init();
        }

        private void AboutItem_OnClick(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }
    }
}
