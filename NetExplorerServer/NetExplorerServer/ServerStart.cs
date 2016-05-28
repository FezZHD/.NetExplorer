using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetExplorerServer
{
    internal class ServerStart
    {
        public static string DefaultRoot { get; set; }
        public static int Port = 21;
        private TcpListener _serverListiner;


        public ServerStart(string root)
        {
            DefaultRoot = root;
        }

        public void Start()
        {
            _serverListiner = new TcpListener(IPAddress.Any, Port);
            _serverListiner.Start();
            TcpClient serverClient = null;
            while (true)
            {
                try
                {
                    serverClient = _serverListiner.AcceptTcpClient();
                    FtpBackend ftpBackend = new FtpBackend(serverClient);
                    new Thread(ftpBackend.HandleFtp).Start();
                }
                catch (IOException e)
                {
                    if (serverClient != null)
                    {
                        serverClient.Close();
                    }
                }
            }
        }
    }
}
