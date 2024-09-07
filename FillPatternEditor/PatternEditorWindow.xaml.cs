using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace FillPatternEditor
{
    public partial class PatternEditorWindow : Window
    {
        private string _filePath;
        private string _patternName;
        private string[] _patternData;

        public PatternEditorWindow(string filePath, string patternName)
        {
            InitializeComponent();
            _filePath = filePath;
            _patternName = patternName;

            LoadAndDisplayPattern();
        }

        private void LoadAndDisplayPattern()
        {
            try
            {
                // Read the pattern from the .pat file
                _patternData = File.ReadAllLines(_filePath);
                bool patternFound = false;

                foreach (string line in _patternData)
                {
                    if (line.StartsWith($"*{_patternName}"))
                    {
                        patternFound = true;
                        continue;
                    }

                    if (patternFound)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("*"))
                        {
                            break; // End of the pattern definition
                        }

                        // Use the existing FillPatternViewerControlWpf to display the pattern
                        //PatternViewer.DisplayPattern(line);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading pattern: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SavePattern(_filePath);
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PAT files (*.pat)|*.pat",
                Title = "Save Pattern As"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                SavePattern(saveFileDialog.FileName);
            }
        }

        private void SavePattern(string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (string line in _patternData)
                    {
                        writer.WriteLine(line);
                    }
                }

                MessageBox.Show("Pattern saved successfully.", "Save", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving pattern: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
