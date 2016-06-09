namespace netExplorer
{
    public class ListItems
    {
        public string Type { get; set; }
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
