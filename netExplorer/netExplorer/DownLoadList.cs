using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace netExplorer
{
    public class DownloadList
    {
        public string DownloadPath { get; set; }
        public string FileName { get; set; }

        public DownloadList(string downloadPath, string fileName)
        {
            DownloadPath = downloadPath;
            FileName = fileName;
        }
    }
}
