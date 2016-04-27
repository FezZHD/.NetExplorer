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
using netExplorer;


    namespace netExplorer
    {
        /// <summary>
        /// Логика взаимодействия для ChooseForm.xaml
        /// </summary>
        public partial class ChooseForm : Window
        {
            public ChooseForm()
            {
                InitializeComponent();
            }

            private void ChooseServer_Click(object sender, RoutedEventArgs e)
            {
                MainServerWindow newWindow = new MainServerWindow();
                newWindow.Show();
                Close();
            }

            private void ChooseClient_Click(object sender, RoutedEventArgs e)
            {
                MainClientWindow newWindow = new MainClientWindow();
                newWindow.Show();
                Close();
            }
    }
    }
