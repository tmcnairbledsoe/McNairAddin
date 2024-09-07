using Autodesk.Revit.UI;
using System.Reflection;
using System;
namespace McNairsAddin
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                string tabName = "Pattern Editor";
                string panelName = "Tools";

                // Create a new ribbon tab
                application.CreateRibbonTab(tabName);

                // Create a new ribbon panel
                RibbonPanel panel = application.CreateRibbonPanel(tabName, panelName);

                // Define the path to the FillPatternEditor.dll
                string dllPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "FillPatternEditor.dll");

                // Create a button for the FillPatternEditor command
                PushButtonData buttonData = new PushButtonData("PatternEditor", "Edit Patterns", dllPath, "FillPatternEditor.PatternEditorCommand");
                panel.AddItem(buttonData);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", "Failed to initialize ribbon button: " + ex.Message);
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
