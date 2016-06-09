using System;
using System.Windows;

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
