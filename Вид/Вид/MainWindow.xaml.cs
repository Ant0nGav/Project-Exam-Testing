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
using System.Xml.Linq;

namespace Вид
{
    public partial class MainWindow : Window
    {
        private string userTasksPath;
        private string userAnswersPath;

        public MainWindow()
        {
            InitializeComponent();

            //Находим Users_Answers и Users_Tasks
            string path = Directory.GetCurrentDirectory();
            int bin = 0;
            for (int i = 0; i < path.Length; i++)
            {
                if (path.Substring(i, 3) == "bin")
                {
                    bin = i;
                    break;
                }
            }
            path = path.Substring(0, bin - 1);
            userAnswersPath = System.IO.Path.Combine(path, $"Users_Answers");
            userTasksPath = System.IO.Path.Combine(path, $"Users_Tasks");
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно создания варианта
            FioInputDialog createVariantWindow = new FioInputDialog(userTasksPath, userAnswersPath);
            createVariantWindow.Owner = this;
            createVariantWindow.ShowDialog();

        }
            

        private void BtnReview_click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Укажите файл с ответами");
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            string file_path = ofd.FileName;
            Window_Review wr = new Window_Review(file_path, userAnswersPath);
            wr.ShowDialog();
        }
    }
}
