using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace NetExplorerServer
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            FolderBrowserDialog chooseBrowserDialog = new FolderBrowserDialog
            {
                Description = @"Выберите корневую папку",
                RootFolder = Environment.SpecialFolder.MyComputer //do not work on win 10
            };
            if (chooseBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedRoot = chooseBrowserDialog.SelectedPath;
                Console.WriteLine(@"Ваш корневой каталог - {0}",selectedRoot);
                ServerStart server = new ServerStart(selectedRoot);
                server.Start();
                while (true)
                {
                    
                }
            }
        }
    }
}
