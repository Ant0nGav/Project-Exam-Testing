using EgeOrganizator;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

namespace Вид
{
    public partial class FioInputDialog : Window
    {
        private Generation _generator;

        public FioInputDialog(string tasks, string answers)
        {
            InitializeComponent();
            _generator = new Generation(tasks, answers, AppendLog);
        }

        private void btnBrowseVariants_Click(object sender, RoutedEventArgs e)
        {
            string selectedPath = _generator.BrowseVariants();
            if (!string.IsNullOrEmpty(selectedPath))
            {
                txtVariantsPath.Text = selectedPath;
                LoadVariants(selectedPath);
            }
        }

        private void LoadVariants(string variantsPath)
        {
            cmbVariants.Items.Clear();

            List<string> variants = _generator.LoadVariants(variantsPath);
            foreach (var variant in variants)
            {
                cmbVariants.Items.Add(variant);
            }

            if (cmbVariants.Items.Count > 0)
            {
                cmbVariants.SelectedIndex = 0;
            }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
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
                
                string fullName = txtFullName.Text.Trim();
                string variantsRootPath = txtVariantsPath.Text;
                string selectedVariant = cmbVariants.SelectedItem.ToString();

                bool success = _generator.CreateVariant(fullName, variantsRootPath, selectedVariant);

                if (success)
                {
                    MessageBox.Show($"Вариант успешно создан для {fullName}!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании варианта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AppendLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtLog.AppendText($"{message}\n");
                txtLog.ScrollToEnd();
            });
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void txtFullName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void txtFullName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
          
            foreach (char c in e.Text)
            {
                if (!(char.IsLetter(c) || c == ' ' || c == '-' || c == '.' || c == 'ё' || c == 'Ё'))
                {
                    e.Handled = true;
                    return;
                }
            }

            if (txtFullName.Text.Length + e.Text.Length > 100)
            {
                e.Handled = true;
                return;
            }
        }

        private void txtFullName_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string pasteText = (string)e.DataObject.GetData(typeof(string));

             
                foreach (char c in pasteText)
                {
                    if (!(char.IsLetter(c) || c == ' ' || c == '-' || c == '.' || c == 'ё' || c == 'Ё'))
                    {
                        e.CancelCommand();
                        return;
                    }
                }

              
                if (txtFullName.Text.Length + pasteText.Length > 100)
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
