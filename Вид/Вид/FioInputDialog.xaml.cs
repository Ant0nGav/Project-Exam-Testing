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

namespace Вид
{
    /// <summary>
    /// Логика взаимодействия для FioInputDialog.xaml
    /// </summary>
    public partial class FioInputDialog : Window
    {
        private string selectedVariantPath;

        public FioInputDialog()
        {
            InitializeComponent();
            Loaded += CreateVariantWindow_Loaded;
        }

        private void CreateVariantWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Инициализация компонентов
        }

        private void btnBrowseVariants_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Выберите папку с вариантами";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtVariantsPath.Text = dialog.SelectedPath;
                LoadVariants(dialog.SelectedPath);
            }
        }

        private void LoadVariants(string variantsPath)
        {
            cmbVariants.Items.Clear();

            try
            {
                if (Directory.Exists(variantsPath))
                {
                    var variantFolders = Directory.GetDirectories(variantsPath);
                    foreach (var variantFolder in variantFolders)
                    {
                        cmbVariants.Items.Add(System.IO.Path.GetFileName(variantFolder));
                    }

                    if (cmbVariants.Items.Count > 0)
                    {
                        cmbVariants.SelectedIndex = 0;
                    }
                }
                else
                {
                    AppendLog("Папка с вариантами не существует!");
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Ошибка при загрузке вариантов: {ex.Message}");
            }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введите ФИО участника!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtVariantsPath.Text))
            {
                MessageBox.Show("Выберите папку с вариантами!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbVariants.SelectedItem == null)
            {
                MessageBox.Show("Выберите вариант!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string fullName = txtFullName.Text.Trim();
                string variantsRootPath = txtVariantsPath.Text;
                string selectedVariant = cmbVariants.SelectedItem.ToString();
                selectedVariantPath = System.IO.Path.Combine(variantsRootPath, selectedVariant);

                AppendLog($"Начинаем создание варианта для: {fullName}");
                AppendLog($"Выбранный вариант: {selectedVariant}");

                // Создаем структуру папок для пользователя
                CreateUserVariant(fullName, selectedVariantPath);

                MessageBox.Show($"Вариант успешно создан для {fullName}!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Можно закрыть окно после успешного создания
                // this.DialogResult = true;
                // this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании варианта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                AppendLog($"Ошибка: {ex.Message}");
            }
        }

        private void CreateUserVariant(string fullName, string variantPath)
        {
            AppendLog($"Анализируем структуру папки варианта...");

            // Проверяем существование папки варианта
            if (!Directory.Exists(variantPath))
            {
                throw new DirectoryNotFoundException($"Папка варианта не найдена: {variantPath}");
            }

            // Ищем папки "задание" и "ответы" (учтем возможные варианты написания)
            string[] subDirectories = Directory.GetDirectories(variantPath);

            string tasksFolder = null;
            string answersFolder = null;
            string answersFile = null;

            foreach (var dir in subDirectories)
            {
                string dirName = System.IO.Path.GetFileName(dir).ToLower();

                if (dirName.Contains("задание") || dirName.Contains("task") ||
                    dirName.Contains("задания") || dirName.Contains("tasks"))
                {
                    tasksFolder = dir;
                    AppendLog($"Найдена папка с заданиями: {System.IO.Path.GetFileName(dir)}");
                }
                else if (dirName.Contains("ответ") || dirName.Contains("answer") ||
                         dirName.Contains("answers") || dirName.Contains("решение"))
                {
                    answersFolder = dir;
                    AppendLog($"Найдена папка с ответами: {System.IO.Path.GetFileName(dir)}");
                }
            }

            if (tasksFolder == null)
            {
                throw new DirectoryNotFoundException("Не найдена папка с заданиями в выбранном варианте");
            }

            // Создаем корневую папку пользователя на том же уровне, где лежат варианты
            string userRootPath = System.IO.Path.GetDirectoryName(variantPath);
            string userFolderName = $"{fullName.Replace(" ", "_")}";
            string userFolderPath = System.IO.Path.Combine(userRootPath, userFolderName);

            AppendLog($"Создаем папку пользователя: {userFolderPath}");

            if (Directory.Exists(userFolderPath))
            {
                AppendLog($"Папка уже существует, очищаем...");
                Directory.Delete(userFolderPath, true);
            }

            Directory.CreateDirectory(userFolderPath);

            // Создаем подпапки
            string userTasksPath = System.IO.Path.Combine(userFolderPath, "Users_Tasks");
            string userAnswersPath = System.IO.Path.Combine(userFolderPath, "Users_answers");

            Directory.CreateDirectory(userTasksPath);
            Directory.CreateDirectory(userAnswersPath);

            // Копируем задания
            AppendLog($"Копируем задания...");
            CopyDirectory(tasksFolder, System.IO.Path.Combine(userTasksPath, $"{fullName.Replace(" ", "_")}_задания"));

            // Копируем или создаем файл с ответами
            AppendLog($"Создаем файл ответов...");

            if (answersFolder != null)
            {
                // Ищем текстовый файл с ответами в папке answers
                var answerFiles = Directory.GetFiles(answersFolder, "*.txt");
                if (answerFiles.Length > 0)
                {
                    answersFile = answerFiles[0];
                    string destAnswersFile = System.IO.Path.Combine(userAnswersPath, $"{fullName.Replace(" ", "_")}.txt");
                    File.Copy(answersFile, destAnswersFile, true);
                    AppendLog($"Скопирован файл ответов: {System.IO.Path.GetFileName(answersFile)}");
                }
                else
                {
                    // Создаем пустой файл ответов
                    string destAnswersFile = System.IO.Path.Combine(userAnswersPath, $"{fullName.Replace(" ", "_")}.txt");
                    File.WriteAllText(destAnswersFile, $"Ответы для {fullName}\nВариант: {System.IO.Path.GetFileName(variantPath)}");
                    AppendLog($"Создан новый файл ответов");
                }
            }
            else
            {
                // Создаем пустой файл ответов
                string destAnswersFile = System.IO.Path.Combine(userAnswersPath, $"{fullName.Replace(" ", "_")}.txt");
                File.WriteAllText(destAnswersFile, $"Ответы для {fullName}\nВариант: {System.IO.Path.GetFileName(variantPath)}");
                AppendLog($"Создан новый файл ответов (папка ответов не найдена)");
            }

            AppendLog($"Вариант успешно создан в папке: {userFolderPath}");
            AppendLog($"Задания: {System.IO.Path.Combine(userTasksPath, $"{fullName.Replace(" ", "_")}_задания")}");
            AppendLog($"Ответы: {System.IO.Path.Combine(userAnswersPath, $"{fullName.Replace(" ", "_")}.txt")}");
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            Directory.CreateDirectory(destinationDir);

            // Копируем все файлы
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = System.IO.Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
                AppendLog($"Скопирован файл: {file.Name}");
            }

            // Рекурсивно копируем поддиректории
            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newDestinationDir = System.IO.Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }

        private void AppendLog(string message)
        {
            txtLog.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
            txtLog.ScrollToEnd();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
