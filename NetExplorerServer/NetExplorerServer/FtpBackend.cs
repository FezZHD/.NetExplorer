using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetExplorerServer
{
    internal class FtpBackend
    {
        private TcpClient _commandClient, _dataClient;
        private NetworkStream _commandNetworkStream, _dataNetworkStream;
        private StreamWriter _commandStreamWriter;
        private StreamReader _commadStreamReader;
        private ushort _clientPort;
        private string _ipAdress;

        public FtpBackend(TcpClient commandClient)
        {
            _commandClient = commandClient;
            _commandNetworkStream = _commandClient.GetStream();
            _commandStreamWriter = new StreamWriter(_commandNetworkStream);
            _commadStreamReader = new StreamReader(_commandNetworkStream);
        }

        public void HandleFtp()
        {
            _commandStreamWriter.WriteLine("220 OK");
            Console.WriteLine("220 OK");
            _commandStreamWriter.Flush();

            string commandLine;
            try
            {
                while (!string.IsNullOrEmpty(commandLine = _commadStreamReader.ReadLine()))
                {
                    string commandResponse = null;
                    string[] commandStringsArray = commandLine.Split(' ');
                    string realCommand = commandStringsArray[0].ToUpperInvariant();
                    string arguments = commandStringsArray.Length > 1
                        ? commandLine.Substring(commandStringsArray[0].Length + 1)
                        : null;//TODO make array
                    if (string.IsNullOrWhiteSpace(arguments))
                    {
                        arguments = null;
                        
                    }
                    if (commandResponse == null)
                    {
                        switch (realCommand)
                        {
                            case "USER":
                                commandResponse = HandleUser(arguments);
                                break;
                            case "QUIT":
                                CloseConnetcion();
                                break;
                            default:
                                commandResponse = "502 command not implemented\n";
                                break;
                        }
                    }

                    if ((_commandClient == null) || !_commandClient.Connected)
                    {
                        break;
                    }
                    else
                    {
                        _commandStreamWriter.WriteLine(commandResponse);
                        Console.WriteLine("{0} : {1} ",realCommand, commandResponse);
                        _commandStreamWriter.Flush();
                        if(commandResponse.StartsWith("221"))
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        private string HandleUser(string argument)
        {
            if(argument.ToLower().Equals("admin"))
            {
                return "311 Admin access allowed";
            }
            return "530 Not allowed user";//todo make user check
        }

        private void CloseConnetcion()
        {
            _commandNetworkStream.Close();
        }
    }
}
