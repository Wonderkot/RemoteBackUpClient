using System;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Ookii.Dialogs.Wpf;
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
                Header = "Выход"
            };
            item.Click += ItemOnClick;
            contextMenu.Items.Add(item);
            TaskbarIcon tbi = new TaskbarIcon
            {
                Icon = Properties.Resources.tray
            };
            tbi.TrayLeftMouseUp += (sender, args) => { this.Close(); };
            tbi.ContextMenu = contextMenu;
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
            ExecuteBtn.IsEnabled = false;
            if (!string.IsNullOrEmpty(urlTbText))
            {
                var thread = new Thread(() =>
                {
                    _requestSender.SendRequest(urlTbText);
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
