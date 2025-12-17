using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EgeOrganizator
{
    public class Generation
    {
        private string _selectedVariantPath;
        private string _userTasksPath;
        private string _userAnswersPath;
        private Action<string> _logCallback;

        public Generation(string userTasksPath, string userAnswersPath, Action<string> logCallback)
        {
            _userTasksPath = userTasksPath;
            _userAnswersPath = userAnswersPath;
            _logCallback = logCallback;
        }

        public string BrowseVariants()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Выберите папку с вариантами";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return dialog.SelectedPath;
                }
            }

            return null;
        }

        public List<string> LoadVariants(string variantsPath)
        {
            var variants = new List<string>();

            try
            {
                if (Directory.Exists(variantsPath))
                {
                    var variantFolders = Directory.GetDirectories(variantsPath);
                    foreach (var variantFolder in variantFolders)
                    {
                        variants.Add(Path.GetFileName(variantFolder));
                    }

                    Log($"Загружено вариантов: {variants.Count}");
                }
                else
                {
                    Log("Папка с вариантами не существует!");
                }
            }
            catch (Exception ex)
            {
                Log($"Ошибка при загрузке вариантов: {ex.Message}");
            }

            return variants;
        }

        public bool CreateVariant(string fullName, string variantsRootPath, string selectedVariant)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentException("Введите ФИО участника!");
            }

            if (string.IsNullOrWhiteSpace(variantsRootPath))
            {
                throw new ArgumentException("Выберите папку с вариантами!");
            }

            if (string.IsNullOrWhiteSpace(selectedVariant))
            {
                throw new ArgumentException("Выберите вариант!");
            }

            try
            {
                _selectedVariantPath = Path.Combine(variantsRootPath, selectedVariant);

                Log($"Начинаем создание варианта для: {fullName}");
                Log($"Выбранный вариант: {selectedVariant}");
                Log($"Задания будут сохранены в: {_userTasksPath}");
                Log($"Ответы будут сохранены в: {_userAnswersPath}");

                
                CreateUserVariant(fullName, _selectedVariantPath);

                return true;
            }
            catch (Exception ex)
            {
                Log($"Ошибка при создании варианта: {ex.Message}");
                throw;
            }
        }

        private void CreateUserVariant(string fullName, string variantPath)
        {
            Log($"Анализируем структуру папки варианта...");

            
            if (!Directory.Exists(variantPath))
            {
                throw new DirectoryNotFoundException($"Папка варианта не найдена: {variantPath}");
            }

            
            if (!Directory.Exists(_userTasksPath))
            {
                Directory.CreateDirectory(_userTasksPath);
                Log($"Создана папка для заданий: {_userTasksPath}");
            }

            if (!Directory.Exists(_userAnswersPath))
            {
                Directory.CreateDirectory(_userAnswersPath);
                Log($"Создана папка для ответов: {_userAnswersPath}");
            }

            
            string[] subDirectories = Directory.GetDirectories(variantPath);

            string tasksFolder = null;
            string answersFolder = null;
            string answersFile = null;

            foreach (var dir in subDirectories)
            {
                string dirName = Path.GetFileName(dir).ToLower();

                if (dirName.Contains("задание") || dirName.Contains("task") ||
                    dirName.Contains("задания") || dirName.Contains("tasks"))
                {
                    tasksFolder = dir;
                    Log($"Найдена папка с заданиями: {Path.GetFileName(dir)}");
                }
                else if (dirName.Contains("ответ") || dirName.Contains("answer") ||
                         dirName.Contains("answers") || dirName.Contains("решение"))
                {
                    answersFolder = dir;
                    Log($"Найдена папка с ответами: {Path.GetFileName(dir)}");
                }
            }

            if (tasksFolder == null)
            {
                throw new DirectoryNotFoundException("Не найдена папка с заданиями в выбранном варианте");
            }

            
            string userTasksFolderName = $"{fullName.Replace(" ", "")}_задания";
            string userTasksFolderPath = Path.Combine(_userTasksPath, userTasksFolderName);

            
            Log($"Копируем задания в: {userTasksFolderPath}");

            if (Directory.Exists(userTasksFolderPath))
            {
                Directory.Delete(userTasksFolderPath, true);
                Log($"Удалена существующая папка: {userTasksFolderName}");
            }

            CopyDirectory(tasksFolder, userTasksFolderPath);

            
            Log($"Создаем файл ответов...");

            string userAnswersFileName = $"{fullName.Replace(" ", "")}.txt";
            string userAnswersFilePath = Path.Combine(_userAnswersPath, userAnswersFileName);

            if (answersFolder != null)
            {
                
                var answerFiles = Directory.GetFiles(answersFolder, "*.txt");
                if (answerFiles.Length > 0)
                {
                    answersFile = answerFiles[0];
                    File.Copy(answersFile, userAnswersFilePath, true);
                    Log($"Скопирован файл ответов: {Path.GetFileName(answersFile)} -> {userAnswersFileName}");
                }
                else
                {
                    
                    var allFiles = Directory.GetFiles(answersFolder);
                    if (allFiles.Length > 0)
                    {
                        
                        answersFile = allFiles[0];
                        File.Copy(answersFile, userAnswersFilePath, true);
                        Log($"Скопирован файл: {Path.GetFileName(answersFile)} -> {userAnswersFileName}");
                    }
                    else
                    {
                        CreateAnswerFile(userAnswersFilePath, fullName, variantPath);
                        Log($"Создан новый файл ответов: {userAnswersFileName}");
                    }
                }
            }
            else
            {
                
                CreateAnswerFile(userAnswersFilePath, fullName, variantPath);
                Log($"Создан новый файл ответов: {userAnswersFileName} (папка ответов не найдена)");
            }

            Log($"\n=== Вариант успешно создан! ===");
            Log($"ФИО участника: {fullName}");
            Log($"Задания сохранены в: {userTasksFolderPath}");
            Log($"Ответы сохранены в: {userAnswersFilePath}");
        }

        private void CreateAnswerFile(string filePath, string fullName, string variantPath)
        {
            File.WriteAllText(filePath,
                $"Ответы для: {fullName}\n" +
                $"Вариант: {Path.GetFileName(variantPath)}\n" +
                $"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}\n" +
                $"--------------------------------------------------\n" +
                $"Ответы будут добавлены позже");
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Исходная папка не найдена: {dir.FullName}");

            Directory.CreateDirectory(destinationDir);

            
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
                Log($"  Скопирован файл: {file.Name}");
            }

            
            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
            }
        }

        private void Log(string message)
        {
            _logCallback?.Invoke($"{DateTime.Now:HH:mm:ss} - {message}");
        }

    }
}
