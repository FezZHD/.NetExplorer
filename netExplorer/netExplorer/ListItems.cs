using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace netExplorer
{
    public class ListItems
    {
        public string Type { get;}
        public string Name { get; set; }
        public string Time { get; set; }
        public string Size { get; set; }
        public string Path { get; set; }

        public ListItems(string type, string name, string time, string size, string path)
        {
            Type = type;
            Name = name;
            Time = time;
            Size = size;
            Path = path;
        }
    }
}
