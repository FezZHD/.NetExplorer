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
            string returnCommand = CommandReaderStream.ReadLine();
            return returnCommand;
        }

        public void GetList()
        {
            TcpClient listClient;
            CommandStream.WriteLine("LIST");
            CommandStream.Flush();
            _answer = GetAnswer();
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
            _answer = GetAnswer();
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
            _answer = GetAnswer();
            CommandStream.Flush();
            GetList();
            CommandStream.Flush();
        }


        public void DoubleClickHeadler(int index)
        {
            if (List[index].Type.Equals("DIR"))
            {
                ChangeDirUp(List[index].Path);
            }
        }
        
        private void ChangeDirUp(string path)
        {
            CommandStream.WriteLine("CWD {0}",path.Replace('|',' '));
            _answer = GetAnswer();
            CommandStream.Flush();
            GetList();
            CommandStream.Flush();
        }


        public void MakeDir(string newName)
        {
            CommandStream.WriteLine("MKD {0}", newName);
            _answer = GetAnswer();
            CommandStream.Flush();
            GetList();
            CommandStream.Flush();
        }

        public void UpFolder()
        {
            CommandStream.WriteLine("CDUP");
            _answer = GetAnswer();
            CommandStream.Flush();
            GetList();
            CommandStream.Flush();
        }
    }
}
