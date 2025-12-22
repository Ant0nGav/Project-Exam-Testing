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

            Organizator_Facade facade = new Organizator_Facade(new EgeOrganizator.Validation());

            string[] comparing = facade.Compare(file_path, answers);

            //Выводим результаты на экран
            foreach (string task in comparing)
            {
                if (!task.Contains("Неверно"))
                {
                    if (task.Contains("Частично"))
                    {
                        CheckedTasks.Items.Add(new ListBoxItem { Content = task, Foreground = Brushes.YellowGreen });
                    }
                    else
                    {
                        CheckedTasks.Items.Add(new ListBoxItem { Content = task, Foreground = Brushes.Green });
                    }
                }
                else
                {
                    CheckedTasks.Items.Add(new ListBoxItem { Content = task, Foreground = Brushes.Red });
                }
            }

            //Считаем количество баллов
            TotalPoints2.Text = facade.Result(comparing);
        }
    }
}