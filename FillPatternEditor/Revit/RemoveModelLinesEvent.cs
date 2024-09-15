using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FillPatternEditor.Revit
{
    // Handles the removal of model lines in Revit
    public class RemoveModelLinesEvent : IExternalEventHandler
    {
        private readonly ExternalEvent _externalEvent;

        // Constructor to create the ExternalEvent
        public RemoveModelLinesEvent()
        {
            _externalEvent = ExternalEvent.Create(this);
        }

        // Method to raise the event
        public void RiseEvent() => _externalEvent.Raise();

        // Executes the removal of model lines in the active document
        public void Execute(UIApplication app)
        {
            RemoveLines(app.ActiveUIDocument.Document);
        }

        // Static method to remove lines from the document
        public static void RemoveLines(Document doc)
        {
            if (!RevitInterop.HelpModelCurvesToDelete.Any())
                return;

            using (Transaction transaction = new Transaction(doc, "Remove help lines"))
            {
                transaction.Start();

                // Delete the elements identified for removal
                foreach (ElementId elementId in RevitInterop.HelpModelCurvesToDelete)
                {
                    if (doc.GetElement(elementId) != null)
                        doc.Delete(elementId);
                }

                // Clear the list after deletion
                RevitInterop.HelpModelCurvesToDelete.Clear();

                // Restore the original line colors of graphic styles
                for (int i = RevitInterop.OldColorOfGraphicStyle.Count - 1; i >= 0; i--)
                {
                    GraphicsStyle element = doc.GetElement(RevitInterop.OldColorOfGraphicStyle[i].Item1) as GraphicsStyle;
                    if (element != null)
                    {
                        element.GraphicsStyleCategory.LineColor = RevitInterop.OldColorOfGraphicStyle[i].Item2;
                    }

                    RevitInterop.OldColorOfGraphicStyle.RemoveAt(i);
                }

                transaction.Commit(); // Commit the changes
            }
        }

        // Returns the name of the event handler
        public string GetName() => nameof(RemoveModelLinesEvent);
    }
}
