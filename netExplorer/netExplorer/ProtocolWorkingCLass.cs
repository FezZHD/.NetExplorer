using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public ProtocolWorkingCLass(string server, string login, string password)
        {
            _server = server;
            _login = login;
            _password = password;    
        }

        public void Connect()
        {
            CurrentTcpClient.Connect(_server,21);
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
            //TODO authetication
            
        }

        private string GetAnswer()
        {
            string returnCommand = CommandReaderStream.ReadLine();
            return returnCommand;
        }


    }
}
