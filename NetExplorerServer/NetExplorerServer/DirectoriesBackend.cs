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
        public string CurrentDirectory { get; private set; }
        private readonly Stack<string> _rootStack = new Stack<string>(); 

        public DirectoriesBackend()
        {
            CurrentDirectory = ServerStart.DefaultRoot;
        }


        public string GetFile(string path)
        {
            Response = "";
            string fileName = path;
            FileStream currentFile = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            byte[] buffer = new byte[BufferSize];
            try
            {
                int count;
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


        public string SendFile(string path)
        {
            string filePath = path;

            if (File.Exists(filePath))
            {
                FileStream sendFileStream = new FileStream(filePath,FileMode.Open, FileAccess.Read);
                byte[] fileBuffer = new byte[BufferSize];
                lock (FtpBackend.DataNetworkStream)
                {
                    int count;
                    while ((count = sendFileStream.Read(fileBuffer, 0, fileBuffer.Length)) > 0 && (FtpBackend.DataNetworkStream != null))
                    {
                        try
                        {
                            FtpBackend.DataNetworkStream.Write(fileBuffer, 0, count);
                        }
                        catch (IOException)
                        {                           
                            return "";
                        }
                        finally
                        {
                            sendFileStream.Close();
                        }
                    }
                    Response = "226 sending complete";
                }

                if (FtpBackend.DataNetworkStream == null)
                {
                    return "";
                }
            }
            FtpBackend.DataNetworkStream.Close();
            return Response;

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
            string newPath = path;
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

        public string DeleteDirectory(string path)
        {
            string deltebleDirectory = path;
            if (Directory.Exists(deltebleDirectory))
            {
                try
                {
                    DirectoryInfo ourDirectory = new DirectoryInfo(deltebleDirectory);
                    ourDirectory.Delete(true);
                }
                catch (Exception)
                {
                    return "Ошибка удаления";
                }
            }
            return "250 directory deleted";
        }

        public void CreateDirectory(string path)
        {
            string newPath = CurrentDirectory + "\\" + path;
            Directory.CreateDirectory(newPath);
        }

        public string DeleteFile(string path)
        {
            string deletableFile = path;
            if (File.Exists(deletableFile))
            {
                try
                {
                    FileInfo ourFile = new FileInfo(deletableFile);
                    ourFile.Delete();
                }
                catch (Exception)
                {
                    return "Ошибка удаления";
                }
            }
            return "250 file deleted";
        }

        public string RenameCurrentFolder(string path, string newName)
        {
            DirectoryInfo oldDirectory = new DirectoryInfo(path);
            if (oldDirectory.Parent != null)
            {              
                try
                {
                    string newPath = oldDirectory.FullName.Substring(0, oldDirectory.FullName.ToString().Length - oldDirectory.Name.Length) + newName;
                    if (oldDirectory.Exists)
                    {
                        oldDirectory.MoveTo(newPath);
                    }
                }
                catch (Exception)
                {
                    return "550 Ошибка при работе с папкой";
                }
            }
            return "250 папка переименована";
        }


        public string RemameFile(string path, string newFileName)
        {
            FileInfo currentFile = new FileInfo(path);
            string newFilePath = currentFile.DirectoryName + "\\" + newFileName;
            try
            {
                if (currentFile.Exists)
                {
                    currentFile.MoveTo(newFilePath);
                }
            }
            catch (Exception)
            {

                return "550 Ошибка при работе с файлом";
            }
            return "250 файл переименован";
        }
    }
}
