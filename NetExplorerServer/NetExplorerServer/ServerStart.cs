using System;
using System.Net;
using System.Net.Sockets;


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
            _serverListiner.BeginAcceptTcpClient(AcceptCallback,_serverListiner);
        }

        private void AcceptCallback(IAsyncResult result)
        {
            TcpClient serverClient = _serverListiner.EndAcceptTcpClient(result);
            _serverListiner.BeginAcceptTcpClient(AcceptCallback, _serverListiner);
            FtpBackend ftpBackend = new FtpBackend(serverClient);
            while (true)
            {
                ftpBackend.HandleFtp();
                if (!serverClient.Connected)
                {
                    break;
                }
            }
        }
    }
}
