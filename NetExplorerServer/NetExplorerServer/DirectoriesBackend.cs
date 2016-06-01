using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;


namespace NetExplorerServer
{
    class DirectoriesBackend
    {
        private StreamWriter _streamWriter;
        private const int BufferSize = 4096;
        public string Response { get; set; }
        public string CurrentDirectory { get; set; }
        private readonly Stack<string> _rootStack = new Stack<string>(); 

        public DirectoriesBackend()
        {
            CurrentDirectory = ServerStart.DefaultRoot;
        }


        public string GetFile()
        {
            Response = "";
            string fileName = CurrentDirectory + "\\" + FtpBackend.TempPath;
            FileStream currentFile = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            byte[] buffer = new byte[BufferSize];
            int count;
            try
            {
                while ((count = FtpBackend.DataNetworkStream.Read(buffer, 0, buffer.Length)) > 0 &&
                       (FtpBackend.DataNetworkStream != null))

                {
                    currentFile.Write(buffer, 0, count);
                }
            }

            catch (IOException)
            {
                return "";
            }
            finally
            {
                currentFile.Close();
            }
            Response = "226 uploading complete";
            if (FtpBackend.DataNetworkStream == null)
            {
                return "";
            }

            FtpBackend.DataNetworkStream.Close();
            return Response;
        }


        public void SendFile()
        {
            string filePath = CurrentDirectory + "\\" + FtpBackend.TempPath;

            if (File.Exists(filePath))
            {
                FileStream sendFileStream = new FileStream(filePath,FileMode.Open, FileAccess.Read);
                byte[] fileBuffer = new byte[BufferSize];
                int count;
                lock (FtpBackend.DataNetworkStream)
                {
                    while ((count = sendFileStream.Read(fileBuffer, 0, fileBuffer.Length)) > 0 && (FtpBackend.DataNetworkStream != null))
                    {
                        try
                        {
                            FtpBackend.DataNetworkStream.Write(fileBuffer, 0, count);
                        }
                        catch (IOException)
                        {
                            sendFileStream.Close();
                            return;
                        }
                    }
                }
                sendFileStream.Close();
                if (FtpBackend.DataNetworkStream == null)
                {
                    return;
                }
            }
            FtpBackend.DataNetworkStream.Close();
        }

        public void GetList(NetworkStream stream)
        {
            _streamWriter = new StreamWriter(stream, Encoding.UTF8)
            {
                NewLine = "\n"
            };
            IEnumerable<string> directoriesEnumerable = Directory.EnumerateDirectories(CurrentDirectory);
            foreach (string directory in directoriesEnumerable)
            {
                DirectoryInfo currentDirectoryInfo = new DirectoryInfo(directory);
                string resultLine = string.Format("{0} {1} {2} {3} {4}", "DIR" , currentDirectoryInfo.Name.Replace(' ','|'), Directory.GetLastWriteTime(currentDirectoryInfo.FullName).ToString(CultureInfo.InvariantCulture).Replace(' ','|'), "", currentDirectoryInfo.FullName.Replace(' ','|'));
                _streamWriter.WriteLine(resultLine);
                _streamWriter.Flush();
            }

            IEnumerable<string> filesEnumerable = Directory.EnumerateFiles(CurrentDirectory);
            foreach (string file in filesEnumerable)
            {
                FileInfo currentFileInfo = new FileInfo(file);

                string resultString = string.Format("{0} {1} {2} {3} {4}", currentFileInfo.Extension, currentFileInfo.Name.Replace(' ','|'), File.GetLastWriteTime(currentFileInfo.FullName).ToString(CultureInfo.InvariantCulture).Replace(' ','|'), currentFileInfo.Length, currentFileInfo.FullName.Replace(' ','|'));
                _streamWriter.WriteLine(resultString);
                _streamWriter.Flush();
            }
            _streamWriter.Close();
        }

        public void ChangeDirectory(string path)
        {
            string newPath = null;
            if (path.Length >= CurrentDirectory.Length)
            {
                if (path.Substring(0, CurrentDirectory.Length).Equals(CurrentDirectory))
                {
                    newPath = path;
                }
            }
            if (newPath == null)
            {
                newPath = CurrentDirectory + "\\" + path;
            }
            _rootStack.Push(CurrentDirectory);
            CurrentDirectory = newPath;
        }

        public void HandleCdup()
        {
            try
            {
                CurrentDirectory = _rootStack.Pop();
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Попытка переключиться на дирректорию выше провальна, т.к вы находитесь в корневой директории");
            }
        }

        public void DeleteDirectory(string path)
        {
            string deltebleDirectory = CurrentDirectory + "\\" + path;
            if (Directory.Exists(deltebleDirectory))
            {
               DirectoryInfo ourDirectory = new DirectoryInfo(deltebleDirectory);
                ourDirectory.Delete(true);
            }
        }

        public void CreateDirectory(string path)
        {
            string newPath = CurrentDirectory + "\\" + path;
            Directory.CreateDirectory(newPath);
        }

        public void DeleteFile(string path)
        {
            string deletableFile = CurrentDirectory + "\\" + path;
            if (File.Exists(deletableFile))
            {
                FileInfo ourFile = new FileInfo(deletableFile);
                ourFile.Delete();
            }
        }
    }
}
