using Autodesk.Revit.DB;
using FillPatternEditor.Enums;
using FillPatternEditor.Utils;
using FillPatternEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Drawing;

namespace FillPatternEditor.Utils
{
    // Class representing line data for a pattern
    public class PatternLineData
    {
        // Constructor initializes an empty list of lines
        public PatternLineData()
        {
            Lines = new List<Models.Line>();
        }

        // List of lines representing the pattern data
        public List<Models.Line> Lines { get; }

        // Calculates the angle of the first line in the list
        public double GetAngle()
        {
            if (Lines.Count == 0)
                return 0.0;

            Models.Line line = Lines.First();

            System.Windows.Point firstPoint = line.FirstPoint;
            System.Windows.Point secondPoint = line.SecondPoint;

            // Calculate the absolute angle based on the difference between Y and X coordinates
            double angle = Math.Atan(Math.Abs(secondPoint.Y - firstPoint.Y) / Math.Abs(secondPoint.X - firstPoint.X));

            // Return 0 if the angle is almost horizontal
            if (Math.Abs(angle) < 0.001)
                return 0.0;

            // Adjust the angle if the line direction is reversed
            if ((firstPoint.X < secondPoint.X && firstPoint.Y > secondPoint.Y) ||
                (secondPoint.X < firstPoint.X && secondPoint.Y > firstPoint.Y))
            {
                angle = Math.PI - angle;
            }

            return angle;
        }

        // Determines the origin point of the pattern based on its proximity to the border origin
        public UV GetOriginPoint(PatternBorders borders, PatUnits patUnits)
        {
            List<KeyValuePair<double, System.Windows.Point>> distanceToBorderPoints = new List<KeyValuePair<double, System.Windows.Point>>();

            // Iterate over the lines and calculate the distance from their points to the border origin
            foreach (Models.Line line in Lines)
            {
                System.Windows.Point firstPoint = line.FirstPoint;
                System.Windows.Point secondPoint = line.SecondPoint;

                distanceToBorderPoints.Add(new KeyValuePair<double, System.Windows.Point>(
                    Utils.DistanceTo(borders.Origin, firstPoint), firstPoint));
                distanceToBorderPoints.Add(new KeyValuePair<double, System.Windows.Point>(
                    Utils.DistanceTo(borders.Origin, secondPoint), secondPoint));
            }

            // Find the point with the smallest distance to the border origin
            System.Windows.Point closestPoint = distanceToBorderPoints
                .OrderBy(pair => pair.Key)
                .FirstOrDefault()
                .Value;

            // Convert the point's X and Y coordinates from the specified pattern units and return as UV
            return new UV(
                closestPoint.X.ConvertFromPatUnit(patUnits),
                closestPoint.Y.ConvertFromPatUnit(patUnits)
            );
        }

        // Generates the internal segments between the lines, returning their lengths and distances
        public IList<double> GetInternalSegments(PatUnits patUnits)
        {
            List<double> internalSegments = new List<double>();

            // Iterate over the lines to get their lengths and distances between lines
            for (int i = 0; i < Lines.Count; ++i)
            {
                Models.Line currentLine = Lines[i];

                if (i == Lines.Count - 1)
                {
                    // If it's the last line, only add its length
                    double length = currentLine.Length.ConvertFromPatUnit(patUnits);
                    internalSegments.Add(length);
                }
                else
                {
                    Models.Line nextLine = Lines[i + 1];

                    // Add the current line's length and the distance to the next line
                    double currentLength = currentLine.Length.ConvertFromPatUnit(patUnits);
                    double distanceBetween = Utils.GetMinDistanceBetween(currentLine, nextLine)
                        .ConvertFromPatUnit(patUnits);

                    internalSegments.Add(currentLength);
                    internalSegments.Add(distanceBetween);
                }
            }

            return internalSegments;
        }
    }
}
