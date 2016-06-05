using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace netExplorer
{
    /// <summary>
    /// Логика взаимодействия для MainClientWindos.xaml
    /// </summary>
    public partial class MainClientWindow : Window
    {
        private ProtocolWorkingClass _currentProtocol;
        public static MainClientWindow TransferWindow { get; private set; }
        public static ListView TranferView { get; private set; }

        public MainClientWindow()
        {
            InitializeComponent();
            TransferWindow = MainWindow;
            TranferView = DataView;
            DownloadPath.Content = Path.GetTempPath();
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
             ClientDisconnect();
             _currentProtocol = new ProtocolWorkingClass(Addres.Text,Login.Text,Password.Password);
             _currentProtocol.Connect();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
           ClientDisconnect();
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            _currentProtocol.Delete(DataView.SelectedIndex);
            DataView.Items.Refresh();
        }

        private void Dowload_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void Rename_OnClick(object sender, RoutedEventArgs e)
        {
        }

        private void NewFolder_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void ClientDisconnect()
        {
            if (_currentProtocol != null)
            {
                _currentProtocol.Disconnect();
            }
        }
    }
}
