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
        public ProtocolWorkingCLass CurrentProtocol;

        public MainClientWindow()
        {
            InitializeComponent();
        }

        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
             CurrentProtocol = new ProtocolWorkingCLass(Addres.Text,Login.Text,Password.Password);
             CurrentProtocol.Connect();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if(CurrentProtocol.CurrentTcpClient != null)
            { 

                CurrentProtocol.CurrentTcpClient.Close();
            }
        }
    }
}
