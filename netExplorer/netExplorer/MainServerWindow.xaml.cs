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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;


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

        private bool IsServerEnabled = false;
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
                try
                {
                    SelectedPath = chooseFolderDialog.SelectedPath;
                    RootPathLable.Content = SelectedPath;
                }
                catch (Exception)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }

        private void ServerStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsServerEnabled)
            {
                ServerStartButton.Content = @"Остановить сервер";
                IsEnabled = true;
            }
            else
            {
                ServerStartButton.Content = @"Запустить сервер";
                IsEnabled = false;
            }
        }
    }
}
