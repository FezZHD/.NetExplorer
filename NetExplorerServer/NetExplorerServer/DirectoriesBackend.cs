using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetExplorerServer
{
    class DirectoriesBackend
    {
        private DirectoryInfo _directoryInfo;
        private StreamWriter _streamWriter;
        private const int BufferSize = 4096;
        private string CurrentDirectory { get; set; }
        private string RootDirectory { get; set; }

        public DirectoriesBackend()
        {
            CurrentDirectory = ServerStart.DefaultRoot;
            RootDirectory = ServerStart.DefaultRoot;
        }


        public void GetFile(string name, NetworkStream stream)
        {
            string fileName = CurrentDirectory + "\\" + name;
            FileStream currentFile = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            byte[] buffer = new byte[BufferSize];
            int count;
            while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                currentFile.Write(buffer, 0, count);
            }
            currentFile.Close();
            stream.Close();
        }


        public void SendFile(string path, NetworkStream stream)
        {
            string filePath = CurrentDirectory + "\\" + path;

            if (File.Exists(filePath))
            {
                FileStream sendFileStream = new FileStream(filePath,FileMode.Open, FileAccess.Read);
                byte[] fileBuffer = new byte[BufferSize];
                int count;
                while ((count = sendFileStream.Read(fileBuffer, 0, fileBuffer.Length)) > 0)
                {
                    stream.Write(fileBuffer, 0 ,count);
                }
                sendFileStream.Close();
            }
            stream.Close();
        }

        public void GetList(NetworkStream stream)
        {
            _streamWriter = new StreamWriter(stream, Encoding.UTF8)
            {
                NewLine = "\n"
            };

            _streamWriter.WriteLine("drwxrwxrwx 1   owner   group   {0,8}   {1}", "4096", "..");
            IEnumerable<string> directoriesEnumerable = Directory.EnumerateDirectories(CurrentDirectory);
            foreach (string directory in directoriesEnumerable)
            {
                DirectoryInfo currentDirectoryInfo = new DirectoryInfo(directory);
                string resultLine = string.Format("drwxrwxrwx 1   owner   group   {0,8}   {1}", "4096",
                    currentDirectoryInfo.Name);
                _streamWriter.WriteLine(resultLine);
                _streamWriter.Flush();
            }

            IEnumerable<string> filesEnumerable = Directory.EnumerateFiles(CurrentDirectory);
            foreach (string file in filesEnumerable)
            {
                FileInfo currentFileInfo = new FileInfo(file);

                string resultString = String.Format("-rw-r--r-- 1   owner   group   {0,8} {1}", currentFileInfo.Length,
                    currentFileInfo.Name);
                _streamWriter.WriteLine(resultString);
                _streamWriter.Flush();
            }
            _streamWriter.Close();
        }
    }
}
