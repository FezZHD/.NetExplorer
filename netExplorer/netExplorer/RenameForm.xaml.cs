using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace netExplorer
{
    /// <summary>
    /// Логика взаимодействия для RenameForm.xaml
    /// </summary>
    public partial class RenameForm : Window
    {
        private bool _ok = false;
        public RenameForm()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainClientWindow.NewName = NewNameBox.Text.Trim();
            _ok = true;
            this.Close();
        }

        private void MainGrid_Closed(object sender, EventArgs e)
        {
            MainClientWindow.IsOk = _ok;
        }
    }
}
