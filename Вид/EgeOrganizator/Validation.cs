using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EgeOrganizator
{
    public class Validation
    {
        //Функция определяет ФИО участника
        static public string GetNameOfUser(string file_path)
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
        static public string GetFileWithAnswers(string name, string answers)
        {
            string path = System.IO.Path.Combine(answers, $"{name}.txt");
            if (File.Exists(path))
            {
                return path;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        //Функция улучшает внешний вид правильных ответов
        static public string[] GetBetterView(string right_answers)
        {
            using (StreamReader sr = new StreamReader(right_answers))
            {
                string[] better_view = new string[27];
                for (int i = 0; i < 24; i++)
                {
                    better_view[i] = sr.ReadLine();
                }
                string temp = sr.ReadLine();
                if (temp.Length > 28)
                {
                    temp = temp.Substring(5, temp.Length - 5);
                    string[] split = temp.Split(' ');
                    string answer = "25 - ";
                    for (int i = 0; i < split.Length; i++)
                    {
                        answer += split[i];
                        if (i != split.Length - 1)
                        {
                            if (i % 2 == 0 && i != split.Length - 1)
                            {
                                answer += " ";
                            }
                            else if (i % 2 == 1 && i != split.Length - 1)
                            {
                                answer = answer + ", ";
                            }
                        }
                    }
                    better_view[24] = answer;
                    better_view[25] = sr.ReadLine();
                    better_view[26] = sr.ReadLine();
                    return better_view;
                }
                else
                {
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
        }
        //Функция сравнивает ответы участника с правильными ответами и выдаёт результат в виде массива строк
        static public string[] ComparingAnswers(string right_answers, string user_answers)
        {
            string[] better_view = GetBetterView(right_answers);
            string[] checkedtasks = new string[27];
            using (StreamReader sr2 = new StreamReader(user_answers))
            {
                for (int i = 1; i <= 27; i++)
                {
                    string line = better_view[i - 1];
                    string line2 = sr2.ReadLine();
                    if (line == line2)
                    {
                        checkedtasks[i - 1] = $"{i}. Верно";
                    }
                    else
                    {
                        if (i <= 25)
                        {
                            for (int j = 0; j < line.Length; j++)
                            {
                                if (line[j] == '-')
                                {
                                    if (line2.Contains("%noanswer%"))
                                    {
                                        checkedtasks[i - 1] = $"{i}. Неверно | Правильный ответ: {line.Substring(j + 1, line.Length - j - 1)} | Ответ участника: Ответ не дан";
                                    }
                                    else
                                    {
                                        checkedtasks[i - 1] = $"{i}. Неверно | Правильный ответ: {line.Substring(j + 1, line.Length - j - 1)} | Ответ участника: {line2.Substring(j + 1, line2.Length - j - 1)}";
                                    }
                                    break;
                                }
                            }
                        }
                        else
                        {
                            string[] check_right = line.Substring(5, line.Length - 5).Split(' ');
                            string[] check_wrong = line2.Substring(5, line2.Length - 5).Split(' ');
                            if ((check_right[0] == check_wrong[1] && check_right[1] == check_wrong[0]) | (check_right[0] == check_wrong[0]) | (check_right[1] == check_wrong[1]))
                            {
                                for (int j = 0; j < line.Length; j++)
                                {
                                    if (line[j] == '-')
                                    {
                                        checkedtasks[i - 1] = $"{i}. Частично верно | Правильный ответ: {line.Substring(j + 1, line.Length - j - 1)} | Ответ участника: {line2.Substring(j + 1, line2.Length - j - 1)}";
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                for (int j = 0; j < line.Length; j++)
                                {
                                    if (line[j] == '-')
                                    {
                                        checkedtasks[i - 1] = $"{i}. Неверно | Правильный ответ: {line.Substring(j + 1, line.Length - j - 1)} | Ответ участника: {line2.Substring(j + 1, line2.Length - j - 1)}";
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return checkedtasks;
        }

        //Функция рассчитывает баллы
        static public string TotalPoints(string[] checkedtasks)
        {
            int total = 0;
            foreach (string task in checkedtasks)
            {
                if (!task.Contains("Неверно"))
                {
                    if (task.Substring(0,2) == "26" | task.Substring(0, 2) == "27")
                    {
                        if (task.Contains("Частично"))
                        {
                            total++;
                        }
                        else
                        {
                            total += 2;
                        }
                    }
                    else
                    {
                        total++;
                    }
                }
            }
            return $"{total} из 29";
        }
    }
}
