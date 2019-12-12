using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Ookii.Dialogs.Wpf;
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
    public partial class MainWindow : Window
    {
        private readonly IRequestSender _requestSender = new RequestSender();
        private readonly Settings _settings;
        public MainWindow()
        {
            InitializeComponent();
            CreateTaskBarIcon();
            _requestSender.ShowMessage += AddTextToConsole;
            _settings = SettingsReader.GetSettings();
            var x = _settings.List;
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
            TaskbarIcon tbi = new TaskbarIcon
            {
                Icon = Properties.Resources.cat1
            };
            tbi.TrayLeftMouseUp += (sender, args) =>
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
            tbi.ContextMenu = contextMenu;
            //TODO rework!
            var cbItem = new ComboBoxItem();
            cbItem.Content = "RioVista";
            cbItem.IsSelected = true;
            DbList.Items.Add(cbItem);

            SelectedFolder.Text = @"C:\db";
        }

        private void ItemOnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
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
            var dbName = ((ListBoxItem)DbList.SelectedItem).Content.ToString();
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
            var dbName = ((ListBoxItem)DbList.SelectedItem).Content.ToString();
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
            var dbName = ((ListBoxItem)DbList.SelectedItem).Content.ToString();
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
    }
}
