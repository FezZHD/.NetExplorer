using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace netExplorer
{
    /// <summary>
    /// Логика взаимодействия для MainClientWindos.xaml
    /// </summary>
    public partial class MainClientWindow : Window
    {
        private ProtocolWorkingCLass _currentProtocol;

        public MainClientWindow()
        {
            InitializeComponent();
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
             _currentProtocol = new ProtocolWorkingCLass(Addres.Text,Login.Text,Password.Password);
             _currentProtocol.Connect();
             DataView.ItemsSource = _currentProtocol.List;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if(_currentProtocol != null)
            { 
                _currentProtocol.CurrentTcpClient.Close();
            }
        }
    }
}
