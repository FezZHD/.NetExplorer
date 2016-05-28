using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace netExplorer
{
    class ListItems
    {
        public string Type;
        public string Name;
        public string Time;
        public string Size;
        public string Path;

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
