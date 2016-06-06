using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ListView = System.Windows.Controls.ListView;
using ListViewItem = System.Windows.Controls.ListViewItem;
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
        public static string NewName { get; set; }
        public static bool IsOk { private get; set; }
        public static string StringDownloadPath { get; private set; }
        public static Thread UploadThread, DownloadThread;
        public static List<DownloadList> DownloadList = new List<DownloadList>();
        public static List<UploadList> UploadList = new List<UploadList>();
        public static string CurrentDir { get; set; }


        public MainClientWindow()
        {
            InitializeComponent();
            TransferWindow = MainWindow;
            TranferView = DataView;
            DownloadPath.Content = Path.GetTempPath();
            StringDownloadPath = (string) DownloadPath.Content;
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
     

        private void Rename_OnClick(object sender, RoutedEventArgs e)
        {
            RenameForm renameForm = new RenameForm();
            renameForm.ShowDialog();
            if (IsOk)
            {
               _currentProtocol.Rename(DataView.SelectedIndex);
            }
            DataView.Items.Refresh();
        }

        private void NewFolder_OnClick(object sender, RoutedEventArgs e)
        {
            RenameForm newFolderForm = new RenameForm();
            newFolderForm.ShowDialog();
            if (IsOk)
            {
                _currentProtocol.MakeDir(NewName);
            }
            DataView.Items.Refresh();
        }

        private void ClientDisconnect()
        {
            if (_currentProtocol != null)
            {
                _currentProtocol.Disconnect();
            }
        }


        private void DataView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DependencyObject originalDependencyObject = (DependencyObject) e.OriginalSource;
            while ((originalDependencyObject != null) && !(originalDependencyObject is ListViewItem))
            {
                originalDependencyObject = VisualTreeHelper.GetParent(originalDependencyObject);
            }

            if (originalDependencyObject != null)
            {
                _currentProtocol.DoubleClickHeadler(DataView.SelectedIndex);
                DataView.Items.Refresh();
            }
        }


        private void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            _currentProtocol.GetList();
            DataView.Items.Refresh();
        }


        private void FolderUp_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentProtocol != null)
            {
                _currentProtocol.UpFolder();
                DataView.Items.Refresh();
            }
        }


        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            folderBrowser.Description = @"Выберите папку для загрузки";
            folderBrowser.RootFolder = Environment.SpecialFolder.MyComputer;
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DownloadPath.Content = folderBrowser.SelectedPath;
                StringDownloadPath = folderBrowser.SelectedPath;
            }
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            string uploadPath = CurrentDir;
            OpenFileDialog uploadFileDialog = new OpenFileDialog();
            if (uploadFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string newFileName = uploadFileDialog.SafeFileName;
                UploadList.Add(new UploadList(uploadPath, newFileName, uploadFileDialog.FileName));
                UploadView.ItemsSource = UploadList;
                UploadView.Items.Refresh();
                if (!_currentProtocol.IsUploading)
                {
                    _currentProtocol.IsUploading = true;
                    UploadThread = new Thread(_currentProtocol.UploadFile);
                    UploadThread.Start();
                }
            }
            
        }

    }
}
