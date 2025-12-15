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
        private string userTasksPath;
        private string userAnswersPath;

        public FioInputDialog()
        {
            InitializeComponent();
            Loaded += CreateVariantWindow_Loaded;
        }

        private void CreateVariantWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Устанавливаем начальные пути (можно изменить на часто используемые)
            txtTasksSavePath.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            txtAnswersSavePath.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
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

        private void btnBrowseTasksSave_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Выберите папку для сохранения заданий";
            dialog.SelectedPath = txtTasksSavePath.Text;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtTasksSavePath.Text = dialog.SelectedPath;
            }
        }

        private void btnBrowseAnswersSave_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Выберите папку для сохранения ответов";
            dialog.SelectedPath = txtAnswersSavePath.Text;

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtAnswersSavePath.Text = dialog.SelectedPath;
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

            if (string.IsNullOrWhiteSpace(txtTasksSavePath.Text))
            {
                MessageBox.Show("Выберите папку для сохранения заданий!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtAnswersSavePath.Text))
            {
                MessageBox.Show("Выберите папку для сохранения ответов!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string fullName = txtFullName.Text.Trim();
                string variantsRootPath = txtVariantsPath.Text;
                string selectedVariant = cmbVariants.SelectedItem.ToString();
                selectedVariantPath = System.IO.Path.Combine(variantsRootPath, selectedVariant);
                userTasksPath = txtTasksSavePath.Text;
                userAnswersPath = txtAnswersSavePath.Text;

                AppendLog($"Начинаем создание варианта для: {fullName}");
                AppendLog($"Выбранный вариант: {selectedVariant}");
                AppendLog($"Задания будут сохранены в: {userTasksPath}");
                AppendLog($"Ответы будут сохранены в: {userAnswersPath}");

                // Создаем структуру для пользователя
                CreateUserVariant(fullName, selectedVariantPath);

                MessageBox.Show($"Вариант успешно создан для {fullName}!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
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

            // Проверяем существование папок для сохранения
            if (!Directory.Exists(userTasksPath))
            {
                var result = MessageBox.Show($"Папка для заданий не существует:\n{userTasksPath}\n\nСоздать папку?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Directory.CreateDirectory(userTasksPath);
                    AppendLog($"Создана папка для заданий: {userTasksPath}");
                }
                else
                {
                    AppendLog("Операция отменена пользователем");
                    return;
                }
            }

            if (!Directory.Exists(userAnswersPath))
            {
                var result = MessageBox.Show($"Папка для ответов не существует:\n{userAnswersPath}\n\nСоздать папку?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Directory.CreateDirectory(userAnswersPath);
                    AppendLog($"Создана папка для ответов: {userAnswersPath}");
                }
                else
                {
                    AppendLog("Операция отменена пользователем");
                    return;
                }
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

            // Создаем имя для папки заданий
            string userTasksFolderName = $"{fullName.Replace(" ", "_")}_задания";
            string userTasksFolderPath = System.IO.Path.Combine(userTasksPath, userTasksFolderName);

            // Копируем задания
            AppendLog($"Копируем задания в: {userTasksFolderPath}");

            if (Directory.Exists(userTasksFolderPath))
            {
                var result = MessageBox.Show($"Папка с заданиями уже существует:\n{userTasksFolderName}\n\nПерезаписать?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Directory.Delete(userTasksFolderPath, true);
                    AppendLog($"Удалена существующая папка: {userTasksFolderName}");
                }
                else
                {
                    AppendLog("Операция отменена пользователем");
                    return;
                }
            }

            CopyDirectory(tasksFolder, userTasksFolderPath);

            // Копируем или создаем файл с ответами
            AppendLog($"Создаем файл ответов...");

            string userAnswersFileName = $"{fullName.Replace(" ", "_")}.txt";
            string userAnswersFilePath = System.IO.Path.Combine(userAnswersPath, userAnswersFileName);

            if (answersFolder != null)
            {
                // Ищем текстовый файл с ответами в папке answers
                var answerFiles = Directory.GetFiles(answersFolder, "*.txt");
                if (answerFiles.Length > 0)
                {
                    answersFile = answerFiles[0];
                    File.Copy(answersFile, userAnswersFilePath, true);
                    AppendLog($"Скопирован файл ответов: {System.IO.Path.GetFileName(answersFile)} -> {userAnswersFileName}");
                }
                else
                {
                    // Ищем любые файлы в папке answers
                    var allFiles = Directory.GetFiles(answersFolder);
                    if (allFiles.Length > 0)
                    {
                        // Копируем первый найденный файл
                        answersFile = allFiles[0];
                        File.Copy(answersFile, userAnswersFilePath, true);
                        AppendLog($"Скопирован файл: {System.IO.Path.GetFileName(answersFile)} -> {userAnswersFileName}");
                    }
                    else
                    {
                        // Создаем пустой файл ответов
                        File.WriteAllText(userAnswersFilePath,
                            $"Ответы для: {fullName}\n" +
                            $"Вариант: {System.IO.Path.GetFileName(variantPath)}\n" +
                            $"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}\n" +
                            $"--------------------------------------------------\n" +
                            $"Ответы будут добавлены позже");
                        AppendLog($"Создан новый файл ответов: {userAnswersFileName}");
                    }
                }
            }
            else
            {
                // Создаем пустой файл ответов
                File.WriteAllText(userAnswersFilePath,
                    $"Ответы для: {fullName}\n" +
                    $"Вариант: {System.IO.Path.GetFileName(variantPath)}\n" +
                    $"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}\n" +
                    $"--------------------------------------------------\n" +
                    $"Ответы будут добавлены позже");
                AppendLog($"Создан новый файл ответов: {userAnswersFileName} (папка ответов не найдена)");
            }

            AppendLog($"\n=== Вариант успешно создан! ===");
            AppendLog($"ФИО участника: {fullName}");
            AppendLog($"Задания сохранены в: {userTasksFolderPath}");
            AppendLog($"Ответы сохранены в: {userAnswersFilePath}");

            // Предлагаем открыть папки
            AppendLog($"\nХотите открыть папки с результатами?");
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Исходная папка не найдена: {dir.FullName}");

            Directory.CreateDirectory(destinationDir);

            // Копируем все файлы
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = System.IO.Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
                AppendLog($"  Скопирован файл: {file.Name}");
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
