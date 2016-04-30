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
    }
}
