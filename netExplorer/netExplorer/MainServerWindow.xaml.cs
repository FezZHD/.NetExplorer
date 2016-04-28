using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;


namespace netExplorer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainServerWindow : Window
    {
        public MainServerWindow()
        {
            InitializeComponent();
        }

        const int InputPort = 21;
        public TcpListener InputListener;
        private bool _isServerEnabled;
        public string SelectedPath { get; set; }

        private void ChooseRootButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog chooseFolderDialog = new FolderBrowserDialog
            {
                Description = @"Выберите ваш корневой каталог",
                RootFolder = Environment.SpecialFolder.MyComputer
                
            };

            DialogResult dialogResult = chooseFolderDialog.ShowDialog();

            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                    SelectedPath = chooseFolderDialog.SelectedPath;
                    RootPathLable.Content = SelectedPath;
            }
        }

        private void ServerStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isServerEnabled)
            {
                ServerStartButton.Content = @"Остановить сервер";
                _isServerEnabled = true;
                InputListener = new TcpListener(IPAddress.Any, InputPort);
                InputListener.Start();
                TcpClient tcpClient = InputListener.AcceptTcpClient();
            }
            else
            {
                ServerStartButton.Content = @"Запустить сервер";
                _isServerEnabled = false;
                InputListener.Stop();
            }
        }
    }
}
