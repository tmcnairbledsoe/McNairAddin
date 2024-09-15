using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using System;
using System.Windows;
using Autodesk.Revit.DB;
using FillPatternEditor.Views;
using FillPatternEditor.Models;
using FillPatternEditor.ViewModels;
using FillPatternEditor.Revit;

namespace FillPatternEditor
{
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class PatternEditorCommand : IExternalCommand
    {
        // A static reference to the MainWindow
        public static PatternListWindow PatternListWindow { get; private set; }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // If the MainWindow is not already open
            if (PatternEditorCommand.PatternListWindow == null)
            {

                RevitInterop.UiApplication = commandData.Application;
                RevitInterop.InitEvents();
                // Create a new instance of the MainWindow and store it in the static property
                PatternEditorCommand.PatternListWindow = new PatternListWindow();

                // Handle the Closed event to clean up when the window is closed
                PatternEditorCommand.PatternListWindow.Closed += (sender, e) =>
                {
                    PatternEditorCommand.PatternListWindow = null;  // Clear the reference to the MainWindow
                    RevitInterop.RemoveModelLinesEvent.RiseEvent();
                };

                // Create a new view model for the Patterns Editor and assign it to the DataContext of the MainWindow
                PatternEditorViewModel viewModel = new PatternEditorViewModel();
                PatternEditorCommand.PatternListWindow.DataContext = viewModel;

                // Once the window is rendered, load patterns from the current Revit document
                PatternEditorCommand.PatternListWindow.ContentRendered += (sender, e) => viewModel.LoadPatternsFromCurrentDocument();
                Window.GetWindow(PatternEditorCommand.PatternListWindow).Show();
            }
            else
            {
                // If the window is already open, bring it to the front
                PatternEditorCommand.PatternListWindow.Activate();
            }

            // Return success
            return Result.Succeeded;
        }
    }
}
