using System;
using System.Windows;
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

        private void TaskBarIcon_OnTrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(UrlTb.Text))
            {
                _requestSender.SendRequest(UrlTb.Text);
            }
        }
    }
}
