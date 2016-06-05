using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetExplorerServer
{
    internal class FtpBackend
    {
        private TcpClient _commandClient;
        private TcpClient   _dataClient;
        private readonly NetworkStream _commandNetworkStream;
        public static NetworkStream DataNetworkStream;
        private StreamWriter _commandStreamWriter;
        private StreamReader _commadStreamReader;
        private const ushort ClientPort = 20;
        private DirectoriesBackend _directoriesBackend;
        private Thread _fileThread;

        public FtpBackend(TcpClient commandClient)
        {
            _commandClient = commandClient;
            _commandNetworkStream = _commandClient.GetStream();
            _commandStreamWriter = new StreamWriter(_commandNetworkStream);
            _commadStreamReader = new StreamReader(_commandNetworkStream);
            _directoriesBackend = new DirectoriesBackend();
        }

        public void HandleFtp()
        {
            try
            {
                _commandStreamWriter.WriteLine("220 OK");
                _commandStreamWriter.Flush();
                Console.WriteLine("220 OK");
            }
            catch (Exception)
            {
                _commandClient.Close();
                return;
            }
            try
            {
                string commandLine;
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
                    switch (realCommand)
                    {
                        case "USER":
                            commandResponse = HandleUser(arguments);
                            break;
                        case "QUIT":
                            CloseConnetcion();
                            break;
                        case "PASS":
                            commandResponse = HandlePass();
                            break;
                        case "TYPE":
                            commandResponse = HandleType(arguments);
                            break;
                        case "PWD":
                            commandResponse = HandlePwd();
                            break;
                        case "LIST":
                            commandResponse = HandleList();
                            break;
                        case "CWD":
                            commandResponse = HandleCwd(arguments.Replace('|', ' '));
                            break;
                        case "CDUP":
                            commandResponse = HandleCdup();
                            break;
                        case "MKD":
                            commandResponse = HandleMkd(arguments);
                            break;
                        case "RMD":
                            commandResponse = HandleRmd(arguments.Replace('|',' '));
                            break;
                        case "DELE":
                            commandResponse = HandleDele(arguments.Replace('|',' '));
                            break;
                        case "RETR":
                            commandResponse = HandleRetr(arguments);
                            break;
                        case "STOR":
                            commandResponse = HandleStor(arguments);
                            //todo: update thread working for stor and retr
                            break;
                        case "NOOP":
                            commandResponse = "200 OK";
                            break;
                        case "AROR":
                            commandResponse = HandleAbor();
                            break;
                        case "RENAMEFOLDER":
                            commandResponse = RenameFolder(commandStringsArray[1].Replace('|',' '), commandStringsArray[2].Replace('|',' ')); 
                            break;
                        case "RENAMEFILE":
                            commandResponse = RenameFile(commandStringsArray[1].Replace('|', ' '), commandStringsArray[2].Replace('|', ' '));
                            break;
                        default:
                            commandResponse = "502 command not implemented\n";
                            break;
                    }

                    if ((_commandClient == null) || !_commandClient.Connected)
                    {
                        break;
                    }
                    else
                    {
                        _commandStreamWriter.WriteLine(commandResponse);
                        Console.WriteLine("{0} : {1} ", realCommand, commandResponse);
                        _commandStreamWriter.Flush();
                        if(commandResponse != null && commandResponse.StartsWith("221"))
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (_commandClient != null)
                {
                    _commandClient.Close();
                }
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

        private string HandlePass()
        {
            return "230 login success";
        }

        private string HandleType(string argument)
        {
            string response;
            switch (argument)
            {
                case "I":
                case "A":
                    response = "220 OK\n";
                    break;
                default:
                    response = "501 Unknow argument";
                    break;
            }
            return response;
        }


        private string HandlePwd()
        {
            if (_directoriesBackend == null)
            {
                _directoriesBackend = new DirectoriesBackend();
            }
            return "257 \"" + _directoriesBackend.CurrentDirectory + "\" is currentd directory";
        }

        private string HandleList()
        {
            _commandStreamWriter.WriteLine("150 ready to send\n");
            _commandStreamWriter.Flush();
            DataNetworkStream = CreateNetworkStream();
            _directoriesBackend.GetList(DataNetworkStream);
            return "226 transfer complete";
        }

        private NetworkStream CreateNetworkStream()
        {
            string endPoint = _commandClient.Client.RemoteEndPoint.ToString();
            string ipAddress = endPoint.Split(':')[0];
            if ((_dataClient != null) && (_dataClient.Connected))
            {
                _dataClient.Close();
            }
            try
            {
                _dataClient = new TcpClient(ipAddress ,ClientPort);
            }
            catch (Exception)
            {
                Console.WriteLine("Происходит отладка или ошибка подключение. Отключение клиента");
                throw;
            }
            return _dataClient.GetStream();
        }


        private string HandleCwd(string argument)
        {
            _directoriesBackend.ChangeDirectory(argument);
            return "250 directiry changed";
        }


        private string HandleCdup()
        {
            try
            {
                _directoriesBackend.HandleCdup();
            }
            catch (Exception)
            {
                return "552 Error changing directory";
            }
            return "250 directory changed";
        }

        private string HandleRetr(string path)
        {
            string filePath = path;
            _commandStreamWriter.WriteLine("150 ready to send\n");
            _commandStreamWriter.Flush();
            DataNetworkStream = CreateNetworkStream();
            _fileThread = new Thread(
                () =>
                    _directoriesBackend.Response = _directoriesBackend.SendFile(filePath)
                );
            _fileThread.Start();
            _fileThread.Join();
            return _directoriesBackend.Response;
        }

        private string HandleStor(string path)
        {
            string filePath = path;
            _commandStreamWriter.WriteLine("150 ready to recieve\n");
            _commandStreamWriter.Flush();
            DataNetworkStream = CreateNetworkStream();
            _fileThread = new Thread(
                () =>
                _directoriesBackend.Response = _directoriesBackend.GetFile(filePath)
            );
            _fileThread.Start();
            _fileThread.Join();
            return _directoriesBackend.Response;
        }

        private string HandleAbor()
        {
            DataNetworkStream.Close();
            _fileThread.Abort();
            return "226 transfer abort";
        }

        private string HandleDele(string path)
        {
            return _directoriesBackend.DeleteFile(path);
        }


        private string HandleRmd(string path)
        {
            return _directoriesBackend.DeleteDirectory(path);
        }

        private string HandleMkd(string path)
        {
            _directoriesBackend.CreateDirectory(path);
            return "250 directory created";
        }

        private string RenameFolder(string path, string newName)
        {
            return _directoriesBackend.RenameCurrentFolder(path, newName);
        }

        private string RenameFile(string path, string newFileName)
        {
            return _directoriesBackend.RemameFile(path, newFileName);
        }
    }
}
