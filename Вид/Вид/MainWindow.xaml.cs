using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Вид
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            Window2 win = new Window2();
            win.ShowDialog();
        }

        private void BtnReview_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Укажите файл с ответами");
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            string file_path = ofd.FileName;
            Window_Review wr = new Window_Review(file_path);
            wr.ShowDialog();
        }
    }
}
