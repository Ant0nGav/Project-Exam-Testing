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

namespace Вид
{
    /// <summary>
    /// Логика взаимодействия для Window_Review.xaml
    /// </summary>
    public partial class Window_Review : Window
    {
        public Window_Review(string file_path)
        {
            InitializeComponent();

            //Получили ФИО участника
            string name = GetNameOfUser(file_path);

            //Получили его ответы
            string path = GetFileWithAnswers(name);

            //Проверяем его ответы
            ComparingAnswers(path, file_path);
        }

        //Функция определяет ФИО участника
        private string GetNameOfUser(string file_path)
        {
            int name_end = 0;
            int name_start = 0;
            for (int i = 0; i < file_path.Length; i++)
            {
                if (file_path.Substring(i, 6) == "ответы")
                {
                    name_end = i;
                    break;
                }
            }
            for (int i = name_end; i >= 0; i--)
            {
                if (file_path[i] == '\\')
                {
                    name_start = i + 1;
                    break;
                }
            }
            return file_path.Substring(name_start, name_end - 1 - name_start);
        }

        //Функция находит правильные ответы участника
        private string GetFileWithAnswers(string name)
        {
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
            path = System.IO.Path.Combine(path, $"Users_Answers\\{name}.txt");
            return path;
        }

        //Функция улучшает внешний вид правильных ответов
        private string[] GetBetterView(string right_answers)
        {
            using (StreamReader sr = new StreamReader(right_answers))
            {
                string[] better_view = new string[27];
                for (int i = 0; i < 24; i++)
                {
                    better_view[i] = sr.ReadLine();
                }
                string temp = sr.ReadLine();
                string task25 = temp;
                while (temp.Substring(0, 4) != "26 -")
                {
                    temp = sr.ReadLine();
                    if (temp == "")
                        temp = sr.ReadLine();
                    if (temp.Substring(0, 4) == "26 -")
                        break;
                    task25 += $", {temp}";
                }
                better_view[24] = task25;
                better_view[25] = temp;
                better_view[26] = sr.ReadLine();
                return better_view;
            }
        }
        //Функция сравнивает ответы участника с правильными ответами, считает количество правильных ответов и выводит неправильные ответы участника
        private void ComparingAnswers(string right_answers, string user_answers)
        {
            int total = 0;
            string[] better_view = GetBetterView(right_answers);
            using (StreamReader sr2 = new StreamReader(user_answers))
            {
                for (int i = 1; i <= 27; i++)
                {
                    string line = better_view[i - 1];
                    string line2 = sr2.ReadLine();
                    if (line == line2)
                    {
                        CheckedTasks.Items.Add(new ListBoxItem { Content = $"{i}. Верно", Foreground = Brushes.Green });
                        total += 1;
                    }
                    else
                    {
                        for (int j = 0; j < line.Length; j++)
                        {
                            if (line[j] == '-')
                            {
                                CheckedTasks.Items.Add(new ListBoxItem { Content = $"{i}. Неверно | Правильный ответ: {line.Substring(j + 1, line.Length - j - 1)} | Ответ участника: {line2.Substring(j + 1, line2.Length - j - 1)}", Foreground = Brushes.Red });
                                break;
                            }
                        }
                    }
                }
            }
            TotalPoints2.Text = $"{total} из 27";
        }
    }
}
