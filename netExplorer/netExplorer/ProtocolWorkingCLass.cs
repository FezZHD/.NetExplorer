using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace netExplorer
{
    public class ProtocolWorkingClass
    {
        public TcpClient CurrentTcpClient = new TcpClient(); 
        private readonly string _server;
        private readonly string _login;
        private readonly string _password;
        public StreamWriter CommandStream;
        public StreamReader CommandReaderStream;
        public NetworkStream CommandNetworkStream;
        private string _answer;
        private TcpListener _tcpListiner;
        public List<ListItems> List = new List<ListItems>();
        private Thread _listThread;
        private const int BufferSize = 4096;
        private bool IsDownload { get; set; }
        private int CurrentIndex { get; set; }
        public bool IsUploading { get; set; }

        public ProtocolWorkingClass(string server, string login, string password)
        {
            _server = server;
            _login = login;
            _password = password;    
        }

        public void Connect()
        {
            try
            {
                CurrentTcpClient.Connect(_server, 21);
            }
            catch (Exception)
            {
                MessageBox.Show(@"Сервер недоступен или порт используется другой программой","Ошибка");
                return;
            }
            CommandNetworkStream = CurrentTcpClient.GetStream();
            CommandStream = new StreamWriter(CommandNetworkStream);
            CommandReaderStream = new StreamReader(CommandNetworkStream);
            _answer = CommandReaderStream.ReadLine();
            CheckAutherization();
        }

        private void CheckAutherization()
        {
            CommandStream.WriteLine("USER {0}",_login);
            CommandStream.Flush();
            if (!GetAnswer().Contains("311"))
            {
                MessageBox.Show("Неверный логин");
                return;
            }
            CommandStream.WriteLine("PASS {0}",_password);
            CommandStream.Flush();
            _answer = GetAnswer();
            _listThread = new Thread(GetList);
            _listThread.Start();
            //TODO authetication       
        }

        private string GetAnswer()
        {
            CommandReaderStream.DiscardBufferedData();
            string returnCommand = CommandReaderStream.ReadLine();
            return returnCommand;
        }

        public void GetList()
        {      
            CommandStream.WriteLine("LIST {0}",20);
            CommandStream.Flush();
            _answer = GetAnswer();
            TcpClient listClient;
            #pragma warning disable 618
            try
            {
                _tcpListiner = new TcpListener(20);
                _tcpListiner.Start();
               
                listClient = _tcpListiner.AcceptTcpClient();
            }
            catch (Exception)
            {
                return;
            }
            #pragma warning restore 618
            NetworkStream listNetwork = listClient.GetStream();
            StreamReader listWriter = new StreamReader(listNetwork);
            ListWorking(listWriter);
            _answer = GetAnswer();
            _tcpListiner.Stop();
            MainClientWindow.TransferWindow.Dispatcher.Invoke(new ThreadStart(delegate
            {
                MainClientWindow.TranferView.ItemsSource = List;
            }));
            CommandStream.WriteLine("PWD");
            CommandStream.Flush();
            _answer = GetAnswer();
            while (_answer.Contains("226"))
            {
                CommandStream.WriteLine("PWD");
                CommandStream.Flush();
                _answer = GetAnswer().Replace('|', ' ');
            }
            MainClientWindow.CurrentDir = _answer;
            _listThread.Abort();
        }

        private void ListWorking(StreamReader stream)
        {
            string answer;
            List.Clear();
            while (!string.IsNullOrEmpty(answer = stream.ReadLine()))
            {
                string[] answerArray = answer.Split(' ');
                List.Add(new ListItems(answerArray[0].Replace(".",""),answerArray[1].Replace('|',' '),answerArray[2].Replace('|',' '),answerArray[3],answerArray[4].Replace('|',' ')));
            }
        }

        public void Delete(int listIndex)
        {
            try
            {
                if (List[listIndex].Type.Equals("DIR"))
                {
                    DeleteSmth("RMD", listIndex);
                }
                else
                {
                    DeleteSmth("DELE", listIndex);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }
        }
        private void DeleteSmth(string command ,int index)
        {
            CommandStream.WriteLine("{0} {1}",command, List[index].Path.Replace(' ','|'));
            CommandStream.Flush();
            GetList();
            CommandStream.Flush();
        }

        public void Disconnect()
        {
            CurrentTcpClient.Close();
        }

        public void Rename(int index)
        {
            if (List[index].Type.Equals("DIR"))
            {
                RenameSmth("RENAMEFOLDER",index, MainClientWindow.NewName);
            }
            else
            {
                RenameSmth("RENAMEFILE", index, MainClientWindow.NewName + '.' + List[index].Type);
            }
        }

        private void RenameSmth(string command, int index, string newName)
        {
            CommandStream.WriteLine("{0} {1} {2}", command, List[index].Path.Replace(' ','|'), newName.Replace(' ','|'));
            CommandStream.Flush();
            GetList(); 
            CommandStream.Flush();
        }


        public void DoubleClickHeadler(int index)
        {
            if (List[index].Type.Equals("DIR"))
            {
                ChangeDirDown(List[index].Path);
            }
            else
            {
                CurrentIndex = index;
                MainClientWindow.DownloadList.Add(new DownloadList(List[index].Path,List[index].Name)); 
                MainClientWindow.TransferWindow.DowloadView.ItemsSource = MainClientWindow.DownloadList;
                MainClientWindow.TransferWindow.DowloadView.Items.Refresh();
                if (!IsDownload)
                {
                    IsDownload = true;
                    MainClientWindow.DownloadThread = new Thread(DownloadFile);
                    MainClientWindow.DownloadThread.Start();
                }
            }
        }
        
        private void ChangeDirDown(string path)
        {
            CommandStream.WriteLine("CWD {0}",path.Replace('|',' '));         
            CommandStream.Flush();
            GetList();
            CommandStream.Flush();
        }


        public void MakeDir(string newName)
        {
            CommandStream.WriteLine("MKD {0}", newName);    
            CommandStream.Flush();
            GetList(); 
            CommandStream.Flush();
        }

        public void UpFolder()
        {
            CommandStream.WriteLine("CDUP");
            CommandStream.Flush();
            GetList(); 
            CommandStream.Flush();
        }



        private void SendFile(string path, NetworkStream dataNetworkStream)
        {
            if (File.Exists(path))
            {
                FileStream sendFileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
                byte[] fileBuffer = new byte[BufferSize];

                try
                {
                    int count;
                    while ((count = sendFileStream.Read(fileBuffer, 0, fileBuffer.Length)) > 0 &&
                           (dataNetworkStream != null))
                    {
                        dataNetworkStream.Write(fileBuffer, 0, count);
                    }
                }
                catch (IOException)
                {

                }
                finally
                {
                    sendFileStream.Close();
                }
                dataNetworkStream.Close();
            }
        }


        private void GetFile(string path, NetworkStream dataNetworkStream)
        {
            FileStream newFile = new FileStream(path, FileMode.Create, FileAccess.Write);
            byte[] buffer = new byte[BufferSize];
            try
            {
                int count;
                while ((count = dataNetworkStream.Read(buffer, 0, buffer.Length)) > 0 && (dataNetworkStream != null))
                {
                    newFile.Write(buffer, 0 , count);
                }
            }
            catch (IOException)
            {

            }
            finally
            {
                newFile.Close();
            }
            dataNetworkStream.Close();
        }


        private void DownloadFile()
        {
            TcpClient dataClient;
            NetworkStream dataNetworkStream;
            TcpListener dataListener;
            int index = 0;
            while (MainClientWindow.DownloadList.Count != 0)
            {
                string currentPath =  MainClientWindow.DownloadList[index].DownloadPath; 
                CommandStream.WriteLine("RETR {0} {1}", currentPath.Replace(' ', '|'), 22);
                CommandStream.Flush();
                _answer = GetAnswer();
                string newPath = MainClientWindow.StringDownloadPath + "\\" + MainClientWindow.DownloadList[index].FileName;
                #pragma warning disable 618
                dataListener = new TcpListener(22);
                #pragma warning restore 618
                dataListener.Start();
                dataClient = dataListener.AcceptTcpClient();
                dataNetworkStream = dataClient.GetStream();
                GetFile(newPath, dataNetworkStream);
                _answer = GetAnswer();                
                dataListener.Stop();
                dataClient.Close();
                MainClientWindow.DownloadList.Remove(MainClientWindow.DownloadList[index]);
                MainClientWindow.TransferWindow.Dispatcher.Invoke(new ThreadStart(delegate
                {
                    MainClientWindow.TransferWindow.DowloadView.Items.Refresh();
                }));
            }
            IsDownload = false;
            MainClientWindow.DownloadList.Clear();
            MainClientWindow.DownloadThread.Abort();          
        }

        public void UploadFile()
        {
            TcpClient dataClient;
            NetworkStream dataNetworkStream;
            TcpListener dataListener;
            while (MainClientWindow.UploadList.Count != 0)
            {
                CommandStream.WriteLine("STOR {0} {1}", MainClientWindow.UploadList[0].DownloadPath.Replace(' ','|') + "\\" +MainClientWindow.UploadList[0].FileName.Replace(' ','|'), 23);
                CommandStream.Flush();
                _answer = GetAnswer();
                #pragma warning disable 618
                dataListener = new TcpListener(23);
                #pragma warning restore 618
                dataListener.Start();
                dataClient = dataListener.AcceptTcpClient();
                dataNetworkStream = dataClient.GetStream();
                SendFile(MainClientWindow.UploadList[0].LocalFilePath, dataNetworkStream);
                _answer = GetAnswer();
                dataListener.Stop();
                dataClient.Close();
                MainClientWindow.UploadList.Remove(MainClientWindow.UploadList[0]);
                MainClientWindow.TransferWindow.Dispatcher.Invoke(new ThreadStart(delegate
                {
                    MainClientWindow.TransferWindow.UploadView.Items.Refresh();
                    MainClientWindow.TransferWindow.DataView.Items.Refresh();
                }));
            }
            IsUploading = false;
            MainClientWindow.UploadList.Clear();
            MainClientWindow.UploadThread.Abort();   
        }
    }
}
