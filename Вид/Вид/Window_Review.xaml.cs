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
using System.Windows.Shapes;
using System.Xml.Linq;
using EgeOrganizator;

namespace Вид
{
    public partial class Window_Review : Window
    {
        public Window_Review(string file_path, string answers)
        {
            InitializeComponent();

            //Получили ФИО участника
            string name = EgeOrganizator.Validation.GetNameOfUser(file_path);

            //Получили его ответы
            string path = EgeOrganizator.Validation.GetFileWithAnswers(name, answers);

            //Проверяем ответы
            string[] check = EgeOrganizator.Validation.ComparingAnswers(path, file_path);

            //Выводим результаты на экран
            foreach (string task in check)
            {
                if (!task.Contains("Неверно"))
                {
                    CheckedTasks.Items.Add(new ListBoxItem { Content = task, Foreground = Brushes.Green });
                }
                else
                {
                    CheckedTasks.Items.Add(new ListBoxItem { Content = task, Foreground = Brushes.Red });
                }
            }

            //Считаем количество баллов
            TotalPoints2.Text = EgeOrganizator.Validation.TotalPoints(check);
        }
    }
}
