using FillPatternEditor.Enums;
using FillPatternEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FillPatternEditor.Utils
{
    public static class Utils
    {
        // Converts inches to feet (double precision)
        public static double InchToFt(this double inch) => inch / 12.0;

        // Converts inches to feet (integer precision)
        public static double InchToFt(this int inch) => (double)inch / 12.0;

        // Converts feet to inches (double precision)
        public static double FtToInch(this double ft) => ft * 12.0;

        // Converts feet to inches (integer precision)
        public static double FtToInch(this int ft) => (double)ft * 12.0;

        // Rounds a double value to 4 decimal places using MidpointRounding.ToEven
        public static double RoundIt(this double value)
        {
            return Math.Round(value, 4, MidpointRounding.ToEven);
        }

        // Converts a value from feet to the appropriate unit (inches or millimeters) based on PatUnits
        public static double ConvertToPatUnit(this double valueInFeet, PatUnits patUnits)
        {
            return patUnits == PatUnits.MM ? FtToMm(valueInFeet) : valueInFeet.FtToInch();
        }

        // Converts a value from pattern units (inches or millimeters) back to feet
        public static double ConvertFromPatUnit(this double valueInPatUnits, PatUnits patUnits)
        {
            return patUnits == PatUnits.MM ? MmToFt(valueInPatUnits) : valueInPatUnits.InchToFt();
        }

        // Calculates the endpoint of a line based on a start point, angle, and length
        public static Point GetEndPoint(Point startPoint, double angleInRadians, double length)
        {
            // X coordinate: start X + (length * cos(angle))
            // Y coordinate: start Y + (length * sin(angle))
            return new Point(
                startPoint.X + length * Math.Cos(angleInRadians),
                startPoint.Y + length * Math.Sin(angleInRadians)
            );
        }

        // Converts feet to millimeters (helper method, assuming it's defined in ModPlus_Revit.Utils)
        public static double FtToMm(double feet)
        {
            return feet * 304.8; // 1 foot = 304.8 millimeters
        }

        // Converts millimeters to feet (helper method, assuming it's defined in ModPlus_Revit.Utils)
        public static double MmToFt(double mm)
        {
            return mm / 304.8;
        }

        // Convert degrees to radians
        public static double DegreeToRadian(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        // Convert radians to degrees
        public static double RadianToDegree(double radians)
        {
            return radians * (180 / Math.PI);
        }

        // Rounds a number to a specified number of decimal places (or default)
        public static double RoundIt(this double value, int decimalPlaces = 2)
        {
            return Math.Round(value, decimalPlaces);
        }

        private static double _tolerance = 1E-06; // Tolerance for geometric calculations

        /// <summary>
        /// Extends a line by a specified distance in both directions.
        /// </summary>
        public static void ExtendLine(this Line line, double length)
        {
            Point firstPoint = line.FirstPoint;
            Point secondPoint = line.SecondPoint;
            Vector vector = secondPoint - firstPoint;
            vector.Normalize();
            line.FirstPoint = firstPoint - vector * length;
            line.SecondPoint = secondPoint + vector * length;
        }

        /// <summary>
        /// Checks if a given point is one of the endpoints of the line.
        /// </summary>
        public static bool IsEndPoint(this Line line, Point point)
        {
            return Math.Abs(line.FirstPoint.DistanceTo(point)) < _tolerance || Math.Abs(line.SecondPoint.DistanceTo(point)) < _tolerance;
        }

        /// <summary>
        /// Checks if two lines are identical by comparing their endpoints.
        /// </summary>
        public static bool IsEqualTo(this Line line, Line checkedLine)
        {
            return Math.Abs(line.FirstPoint.DistanceTo(checkedLine.FirstPoint)) < _tolerance &&
                   Math.Abs(line.SecondPoint.DistanceTo(checkedLine.SecondPoint)) < _tolerance ||
                   Math.Abs(line.FirstPoint.DistanceTo(checkedLine.SecondPoint)) < _tolerance &&
                   Math.Abs(line.SecondPoint.DistanceTo(checkedLine.FirstPoint)) < _tolerance;
        }

        /// <summary>
        /// Determines if two lines intersect.
        /// </summary>
        public static bool Intersects(this Line lineA, Line lineB)
        {
            return LineIntersection.Find(lineA, lineB, _tolerance).HasValue;
        }

        /// <summary>
        /// Finds the intersection point between two lines if they intersect. Returns null if no intersection.
        /// </summary>
        public static Point? Intersection(this Line lineA, Line lineB)
        {
            return LineIntersection.Find(lineA, lineB, _tolerance);
        }

        internal static bool Intersects(this Line lineA, Line lineB, double tolerance)
        {
            return LineIntersection.FindIntersection(lineA, lineB, tolerance).HasValue;
        }

        internal static Point? Intersection(this Line lineA, Line lineB, double tolerance)
        {
            return LineIntersection.FindIntersection(lineA, lineB, tolerance);
        }

        /// <summary>
        /// Checks if two lines lie on the same straight line and returns the distance between them.
        /// </summary>
        public static bool IsLieOnOneStraightLine(this Line line, Line checkedLine, out double distanceBetween)
        {
            double[] distances = line.GetDistancesBetweenEndPoints(checkedLine);
            distanceBetween = distances.Min();
            return Math.Abs(distances.Max() - (line.Length + checkedLine.Length + distanceBetween)) < _tolerance;
        }

        /// <summary>
        /// Determines if two lines are parallel.
        /// </summary>
        public static bool IsParallelTo(this Line firstLine, Line secondLine)
        {
            Vector vector1 = firstLine.SecondPoint - firstLine.FirstPoint;
            Vector vector2 = secondLine.SecondPoint - secondLine.FirstPoint;
            vector1.Normalize();
            vector2.Normalize();
            return Math.Abs(Math.Abs(Vector.Multiply(vector1, vector2)) - 1.0) < _tolerance;
        }

        /// <summary>
        /// Gets the minimum distance between the endpoints of two lines.
        /// </summary>
        public static double GetMinDistanceBetween(this Line firstLine, Line secondLine)
        {
            return firstLine.GetDistancesBetweenEndPoints(secondLine).Min();
        }

        /// <summary>
        /// Returns an array of distances between the endpoints of two lines.
        /// </summary>
        internal static double[] GetDistancesBetweenEndPoints(this Line firstLine, Line secondLine)
        {
            return new double[]
            {
                firstLine.FirstPoint.DistanceTo(secondLine.FirstPoint),
                firstLine.FirstPoint.DistanceTo(secondLine.SecondPoint),
                firstLine.SecondPoint.DistanceTo(secondLine.FirstPoint),
                firstLine.SecondPoint.DistanceTo(secondLine.SecondPoint)
            };
        }

        /// <summary>
        /// Calculates the distance between two points.
        /// </summary>
        public static double DistanceTo(this Point p1, Point p2) => (p2 - p1).Length;

        /// <summary>
        /// Checks if a point is inside a polygon, which is represented by a collection of lines.
        /// </summary>
        public static bool IsInside(this Point point, IEnumerable<Line> polygon)
        {
            Line testLine = new Line(point, new Point(double.MaxValue, point.Y)); // Horizontal ray to the right
            int intersectionCount = 0;

            foreach (Line edge in polygon)
            {
                if (LineIntersection.Find(testLine, edge).HasValue)
                    intersectionCount++;
            }

            return intersectionCount % 2 != 0;
        }

        /// <summary>
        /// Rotates a point around a given center by a specified angle in degrees.
        /// </summary>
        public static Point Rotate(this Point point, Point center, int angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180.0);
            return point.Rotate(center, angleInRadians);
        }

        /// <summary>
        /// Rotates a point around a given center by a specified angle in radians.
        /// </summary>
        public static Point Rotate(this Point point, Point center, double angleInRadians)
        {
            double cos = Math.Cos(angleInRadians);
            double sin = Math.Sin(angleInRadians);
            return new Point(
                cos * (point.X - center.X) - sin * (point.Y - center.Y) + center.X,
                sin * (point.X - center.X) + cos * (point.Y - center.Y) + center.Y
            );
        }

        /// <summary>
        /// Rotates a vector by a specified angle in radians.
        /// </summary>
        public static Vector RotateRadians(Vector v, double radians)
        {
            double cos = Math.Cos(radians);
            double sin = Math.Sin(radians);
            return new Vector(cos * v.X - sin * v.Y, sin * v.X + cos * v.Y);
        }

        /// <summary>
        /// Returns a point on an ellipse (or circle) by a given angle.
        /// </summary>
        public static Point GetEllipsePointByAngle(Point centerPoint, double xRadius, double yRadius, double angleRadians)
        {
            return new Point(
                centerPoint.X + xRadius * Math.Cos(angleRadians),
                centerPoint.Y + yRadius * Math.Sin(angleRadians)
            );
        }

        /// <summary>
        /// Compares two double values and returns true if the first value is greater than or equal to the second, 
        /// using the default precision for tolerance.
        /// </summary>
        public static bool IsGreaterThanOrEqual(this double a, double b)
        {
            // Uses the default tolerance for comparison
            return a.IsGreaterThanOrEqual(b, Constants.DefaultPrecision);
        }


        /// <summary>
        /// Compares two double values and returns true if the first value is greater than or equal to the second,
        /// using a specified tolerance.
        /// </summary>
        public static bool IsGreaterThanOrEqual(this double a, double b, double tolerance)
        {
            double difference = a - b;
            // Returns true if 'a' is greater than 'b' by more than the tolerance, or if their difference is within the tolerance
            return difference > tolerance || Math.Abs(difference) < tolerance;
        }

        public static bool IsLessThanOrEqual(this double a, double b)
        {
            return a.IsLessThanOrEqual(b, Constants.DefaultPrecision);
        }

        public static bool IsLessThanOrEqual(this double a, double b, double tolerance)
        {
            double num = a - b;
            return num < -tolerance || Math.Abs(num) < tolerance;
        }

        public static bool DNE(double a, double b) => DNE(a, b, Constants.DefaultPrecision);

        public static bool DNE(double a, double b, double precision) => !DEQ(a, b, precision);

        public static bool DEQ(double a, double b) => DEQ(a, b, Constants.DefaultPrecision);

        public static bool DEQ(double a, double b, double precision)
        {
            return a - precision <= b && b <= a + precision;
        }

        public static bool DGT(double a, double b) => DGT(a, b, Constants.DefaultPrecision);

        public static bool DGT(double a, double b, double precision) => a - precision > b;

        public static bool DGE(double a, double b) => DGE(a, b, Constants.DefaultPrecision);

        public static bool DGE(double a, double b, double precision) => a >= b - precision;

        public static bool DLT(double a, double b) => DLT(a, b, Constants.DefaultPrecision);

        public static bool DLT(double a, double b, double precision) => !DGE(a, b, precision);

        public static bool DLE(double a, double b) => DLE(a, b, Constants.DefaultPrecision);

        public static bool DLE(double a, double b, double precision) => !DGT(a, b, precision);

        public static bool IsLessThan(this double a, double b)
        {
            return a.IsLessThan(b, Constants.DefaultPrecision);
        }

        public static bool IsLessThan(this double a, double b, double tolerance) => a - b < -tolerance;

        public static bool IsGreaterThan(this double a, double b)
        {
            return a.IsGreaterThan(b, Constants.DefaultPrecision);
        }

        public static bool IsGreaterThan(this double a, double b, double tolerance)
        {
            return a - b > tolerance;
        }
    }
}
