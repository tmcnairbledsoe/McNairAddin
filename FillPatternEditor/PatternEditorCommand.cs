using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using System;
using System.Windows;
using Autodesk.Revit.DB;

namespace FillPatternEditor
{
    [Transaction(TransactionMode.Manual)]
    public class PatternEditorCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                MessageBox.Show("Please select a .pat file to edit.", "Pattern Editor", MessageBoxButton.OK, MessageBoxImage.Information);

                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "PAT files (*.pat)|*.pat",
                    Title = "Select a Pattern File"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    UIApplication uiApp = commandData.Application;
                    UIDocument uiDoc = uiApp.ActiveUIDocument;

                    PatternListWindow patternListWindow = new PatternListWindow(uiApp, uiDoc, filePath);
                    patternListWindow.ShowDialog();
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = $"An error occurred: {ex.Message}\n{ex.StackTrace}";
                TaskDialog.Show("Error", message);
                return Result.Failed;
            }
        }
    }
}
