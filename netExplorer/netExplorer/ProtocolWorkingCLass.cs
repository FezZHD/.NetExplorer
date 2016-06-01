using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace netExplorer
{
    public class ProtocolWorkingCLass
    {
        public TcpClient CurrentTcpClient = new TcpClient(); 
        private string _server;
        private string _login;
        private string _password;
        public StreamWriter CommandStream;
        public StreamReader CommandReaderStream;
        public NetworkStream CommandNetworkStream;
        private string _answer;
        private TcpListener TcpListiner;
        public List<ListItems> List = new List<ListItems>();

        public ProtocolWorkingCLass(string server, string login, string password)
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
            catch (Exception e)
            {
                MessageBox.Show("Ошибка",@"Сервер недоступен или порт используется другой программой");
            }
            CommandNetworkStream = CurrentTcpClient.GetStream();
                CommandStream = new StreamWriter(CommandNetworkStream);
                CommandReaderStream = new StreamReader(CommandNetworkStream);
                _answer = CommandReaderStream.ReadLine();
                CheckAutherization();

        }

        public void CheckAutherization()
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
            GetList();
            //TODO authetication       
        }

        private string GetAnswer()
        {
            string returnCommand = CommandReaderStream.ReadLine();
            return returnCommand;
        }

        private void GetList()
        {
            CommandStream.WriteLine("LIST");
            CommandStream.Flush();
            _answer = GetAnswer();
            #pragma warning disable 618
            try
            {
                TcpListiner = new TcpListener(20);
                TcpListiner.Start();
            }
            catch (Exception)
            {
                MessageBox.Show(@"Ошибка подключения");
                return;
            }
            #pragma warning restore 618
            TcpClient listClient = TcpListiner.AcceptTcpClient();
            NetworkStream listNetwork = listClient.GetStream();
            StreamReader listWriter = new StreamReader(listNetwork);
            ListWorking(listWriter);
            _answer = GetAnswer();
            TcpListiner.Stop();
        }

        private void ListWorking(StreamReader stream)
        {
            string answer;
            while (!string.IsNullOrEmpty(answer = stream.ReadLine()))
            {
                string[] answerArray = answer.Split(' ');
                List.Add(new ListItems(answerArray[0].Replace(".",""),answerArray[1].Replace('|',' '),answerArray[2].Replace('|',' '),answerArray[3],answerArray[4].Replace('|',' ')));
            }
        }

    }
}
