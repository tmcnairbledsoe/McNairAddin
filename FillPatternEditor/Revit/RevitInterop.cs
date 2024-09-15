using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FillPatternEditor.Revit
{
    public static class RevitInterop
    {
        // Revit's UIApplication instance
        public static UIApplication UiApplication { get; set; }

        // Get the currently active UIDocument
        public static UIDocument UiDocument => UiApplication.ActiveUIDocument;

        // Get the active Document in Revit
        public static Document Document => UiDocument.Document;

        // Get the currently active View in the Revit Document
        public static View ActiveView => Document.ActiveView;

        // Get the active UIView associated with the currently active View
        public static UIView ActiveUiView
        {
            get
            {
                View activeView = Document.ActiveView;
                foreach (UIView openUiView in UiDocument.GetOpenUIViews())
                {
                    if (openUiView.ViewId.Equals(activeView.Id))
                    {
                        return openUiView;  // Return the matching UIView
                    }
                }
                return null;  // No active UIView found
            }
        }

        // Events for handling Revit transactions and custom logic
        public static RevitEvent RevitEvent { get; private set; }
        public static RevitEventWithoutTransaction RevitEventWithoutTransaction { get; private set; }
        public static RemoveModelLinesEvent RemoveModelLinesEvent { get; private set; }
        public static ShowHelpRectangleEvent ShowHelpRectangleEvent { get; private set; }

        // List of ElementIds for model curves to be deleted (used for cleanup or UI purposes)
        public static List<ElementId> HelpModelCurvesToDelete { get; } = new List<ElementId>();

        // Store the old color of graphic styles (used when reverting UI changes)
        public static List<Tuple<ElementId, Color>> OldColorOfGraphicStyle { get; } = new List<Tuple<ElementId, Color>>();

        // Method to initialize various Revit events
        public static void InitEvents()
        {
            RevitEvent = new RevitEvent();
            RevitEventWithoutTransaction = new RevitEventWithoutTransaction();
            RemoveModelLinesEvent = new RemoveModelLinesEvent();
            ShowHelpRectangleEvent = new ShowHelpRectangleEvent();
        }
    }
}
