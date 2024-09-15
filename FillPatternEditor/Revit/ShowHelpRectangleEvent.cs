using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FillPatternEditor.Revit
{
    // This class is responsible for showing a help rectangle in Revit using external events
    public class ShowHelpRectangleEvent : IExternalEventHandler
    {
        private readonly ExternalEvent _externalEvent;
        private XYZ[] _points;

        public ShowHelpRectangleEvent()
        {
            // Create an external event for Revit
            _externalEvent = ExternalEvent.Create(this);
        }

        // Method to trigger the external event with specified points
        public void RaiseEvent(XYZ[] points)
        {
            _points = points;
            _externalEvent.Raise();
        }

        // Executes the event and shows the help rectangle in Revit
        public void Execute(UIApplication app)
        {
            ShowHelpRectangle(app.ActiveUIDocument.Document, _points);
        }

        // Method to show a help rectangle in Revit based on the given points
        public static List<ElementId> ShowHelpRectangle(Autodesk.Revit.DB.Document doc, XYZ[] points)
        {
            List<ElementId> elementIds = new List<ElementId>();

            using (Transaction transaction = new Transaction(doc, "Show help rectangle"))
            {
                transaction.Start();

                // Get the minimum and maximum X and Y coordinates from the points
                List<double> xCoords = points.Select(p => p.X).ToList();
                List<double> yCoords = points.Select(p => p.Y).ToList();
                XYZ bottomLeft = new XYZ(xCoords.Min(), yCoords.Min(), 0.0);
                XYZ bottomRight = new XYZ(xCoords.Max(), yCoords.Min(), 0.0);
                XYZ topRight = new XYZ(xCoords.Max(), yCoords.Max(), 0.0);
                XYZ topLeft = new XYZ(xCoords.Min(), yCoords.Max(), 0.0);

                // Create lines to form the rectangle
                Line[] rectangleLines = new Line[]
                {
                    Line.CreateBound(bottomLeft, bottomRight),
                    Line.CreateBound(bottomRight, topRight),
                    Line.CreateBound(topRight, topLeft),
                    Line.CreateBound(topLeft, bottomLeft)
                };

                // Create detail curves and update line colors for each rectangle line
                foreach (Line line in rectangleLines)
                {
                    DetailCurve detailCurve = doc.Create.NewDetailCurve(doc.ActiveView, line);
                    RevitInterop.HelpModelCurvesToDelete.Add(detailCurve.Id);

                    if (detailCurve.LineStyle is GraphicsStyle lineStyle)
                    {
                        RevitInterop.OldColorOfGraphicStyle.Add(new Tuple<ElementId, Color>(lineStyle.Id, lineStyle.GraphicsStyleCategory.LineColor));
                        lineStyle.GraphicsStyleCategory.LineColor = new Color(250, 0, 0); // Red color for the help rectangle
                    }

                    elementIds.Add(detailCurve.Id);
                }

                transaction.Commit();
            }

            return elementIds;
        }

        // Returns the name of the event handler
        public string GetName() => nameof(ShowHelpRectangleEvent);
    }
}