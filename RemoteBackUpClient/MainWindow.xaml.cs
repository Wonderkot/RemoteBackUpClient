using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Ookii.Dialogs.Wpf;
using RemoteBackUpClient.Utils;
using RequestProcessorLib.Classes;
using RequestProcessorLib.Interfaces;


namespace RemoteBackUpClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IRequestSender _requestSender = new RequestSender();
        public MainWindow()
        {
            InitializeComponent();
            CreateTaskBarIcon();
            _requestSender.ShowMessage += AddTextToConsole;
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
            string fileName = FileNameTB.Text;
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = ((ListBoxItem)DbList.SelectedItem).Content + ".7z";
            }
            ExecuteBtn.IsEnabled = false;
            if (!string.IsNullOrEmpty(urlTbText))
            {
                var thread = new Thread(() =>
                {
                    var data = _requestSender.CreateNewBackupRequest(urlTbText);
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
            this.Close();
        }

        private void AddTextToConsole(string msg)
        {
            Console?.Dispatcher?.Invoke(() =>
            {
                Console.Text += msg;
                Console.Text += Environment.NewLine;
            });
        }
    }
}
