using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FillPatternEditor.Utils;

namespace FillPatternEditor.Models
{
    public class PatternBorders
    {
        /// <summary>
        /// Constructor that creates PatternBorders from a collection of border lines.
        /// Adjusts the points relative to the minimum X and Y.
        /// </summary>
        public PatternBorders(IEnumerable<Autodesk.Revit.DB.Line> borderLines)
        {
            var source = new List<XYZ>();

            // Collect all points from the border lines
            foreach (Autodesk.Revit.DB.Line borderLine in borderLines)
            {
                XYZ endPoint1 = ((Curve)borderLine).GetEndPoint(0);
                XYZ endPoint2 = ((Curve)borderLine).GetEndPoint(1);
                source.Add(endPoint1);
                source.Add(endPoint2);
            }

            // Get minimum X and Y coordinates
            double minX = source.Select(p => p.X).Min();
            double minY = source.Select(p => p.Y).Min();

            // Create borders based on the adjusted points
            this.BorderLines = GetBordersLines(source
                            .Select(xyz => new System.Windows.Point(xyz.X - minX, xyz.Y - minY))
                            .ToList())
                            .ToList();

            CalculateProperties();
        }

        /// <summary>
        /// Constructor that creates rectangular PatternBorders given width, height, minX, and minY.
        /// </summary>
        public PatternBorders(double width, double height, double minX, double minY)
        {
            this.BorderLines = GetBordersLines(width, height, minX, minY).ToList();
            CalculateProperties();
        }

        /// <summary>
        /// Constructor that creates PatternBorders from a list of points.
        /// </summary>
        public PatternBorders(IList<System.Windows.Point> points)
        {
            this.BorderLines = GetBordersLines(points).ToList();
            CalculateProperties();
        }

        // Properties
        public List<Line> BorderLines { get; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public List<System.Windows.Point> AllPoints { get; private set; }
        public List<double> AllX { get; private set; }
        public List<double> AllY { get; private set; }
        public double MinX { get; private set; }
        public double MinY { get; private set; }
        public double MaxX { get; private set; }
        public double MaxY { get; private set; }
        public System.Windows.Point Origin { get; private set; }

        /// <summary>
        /// Determines if a point is inside the borders.
        /// </summary>
        public bool IsPointInside(System.Windows.Point point)
        {
            return point.X >= this.MinX && point.X <= this.MaxX && point.Y >= this.MinY && point.Y <= this.MaxY;
        }

        /// <summary>
        /// Calculates various properties such as width, height, and origin based on the border lines.
        /// </summary>
        private void CalculateProperties()
        {
            // Gather all points from the border lines
            this.AllPoints = this.BorderLines
                                .SelectMany(l => new System.Windows.Point[] { l.FirstPoint, l.SecondPoint })
                                .ToList();

            // Extract distinct X and Y coordinates
            this.AllX = this.AllPoints.Select(p => p.X).Distinct().ToList();
            this.AllY = this.AllPoints.Select(p => p.Y).Distinct().ToList();

            // Calculate min and max coordinates
            this.MinX = this.AllX.Min();
            this.MinY = this.AllY.Min();
            this.MaxX = this.AllX.Max();
            this.MaxY = this.AllY.Max();

            // Calculate origin, width, and height
            this.Origin = new System.Windows.Point(this.MinX, this.MinY);
            this.Width = Math.Abs(this.MaxX - this.MinX);
            this.Height = Math.Abs(this.MaxY - this.MinY);
        }

        /// <summary>
        /// Creates border lines for a rectangular region given width, height, minX, and minY.
        /// </summary>
        private IEnumerable<Line> GetBordersLines(double width, double height, double minX, double minY)
        {
            return new Line[]
            {
                new Line(new System.Windows.Point(minX, minY), new System.Windows.Point(minX + width, minY)),            // Bottom
                new Line(new System.Windows.Point(minX + width, minY), new System.Windows.Point(minX + width, minY + height)),  // Right
                new Line(new System.Windows.Point(minX, minY), new System.Windows.Point(minX, minY + height)),           // Left
                new Line(new System.Windows.Point(minX, minY + height), new System.Windows.Point(minX + width, minY + height)) // Top
            };
        }

        /// <summary>
        /// Creates border lines for an arbitrary set of points.
        /// </summary>
        private IEnumerable<Line> GetBordersLines(IList<System.Windows.Point> points)
        {
            double minX = points.Select(p => p.X).Min();
            double maxX = points.Select(p => p.X).Max();
            double minY = points.Select(p => p.Y).Min();
            double maxY = points.Select(p => p.Y).Max();

            return GetBordersLines(Math.Abs(maxX - minX), Math.Abs(maxY - minY), minX, minY);
        }
    }
}
