using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FillPatternEditor.Models;

namespace FillPatternEditor.Utils
{
    class LineIntersection
    {
        /// <summary>
        /// Returns Point of intersection if do intersect otherwise default Point (null)
        /// </summary>
        /// <param name="lineA">First line</param>
        /// <param name="lineB">Second line</param>
        /// <param name="tolerance">Tolerance</param>
        /// <returns>The point of intersection</returns>
        public static Point? Find(Line lineA, Line lineB, double tolerance = 1E-05)
        {
            return LineIntersection.FindIntersection(lineA, lineB, tolerance);
        }

        internal static Point? FindIntersection(Line lineA, Line lineB, double tolerance)
        {
            if (lineA == lineB)
                throw new Exception("Both lines are the same.");
            double num1 = lineA.FirstPoint.X;
            ref double local1 = ref num1;
            Point point = lineB.FirstPoint;
            double x1 = point.X;
            if (local1.CompareTo(x1) > 0)
            {
                Line line = lineA;
                lineA = lineB;
                lineB = line;
            }
            else
            {
                point = lineA.FirstPoint;
                num1 = point.X;
                ref double local2 = ref num1;
                point = lineB.FirstPoint;
                double x2 = point.X;
                if (local2.CompareTo(x2) == 0)
                {
                    point = lineA.FirstPoint;
                    num1 = point.Y;
                    ref double local3 = ref num1;
                    point = lineB.FirstPoint;
                    double y = point.Y;
                    if (local3.CompareTo(y) > 0)
                    {
                        Line line = lineA;
                        lineA = lineB;
                        lineB = line;
                    }
                }
            }
            point = lineA.FirstPoint;
            double x3 = point.X;
            point = lineA.FirstPoint;
            double y1 = point.Y;
            point = lineA.SecondPoint;
            double x4 = point.X;
            point = lineA.SecondPoint;
            double y2 = point.Y;
            point = lineB.FirstPoint;
            double x5 = point.X;
            point = lineB.FirstPoint;
            double y3 = point.Y;
            point = lineB.SecondPoint;
            double x6 = point.X;
            point = lineB.SecondPoint;
            double y4 = point.Y;
            if (Math.Abs(x3 - x4) < tolerance && Math.Abs(x5 - x6) < tolerance && Math.Abs(x3 - x5) < tolerance)
            {
                Point p = new Point(x5, y3);
                if (LineIntersection.IsInsideLine(lineA, p, tolerance) && LineIntersection.IsInsideLine(lineB, p, tolerance))
                    return new Point?(new Point(x5, y3));
            }
            if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance && Math.Abs(y1 - y3) < tolerance)
            {
                Point p = new Point(x5, y3);
                if (LineIntersection.IsInsideLine(lineA, p, tolerance) && LineIntersection.IsInsideLine(lineB, p, tolerance))
                    return new Point?(new Point(x5, y3));
            }
            if (Math.Abs(x3 - x4) < tolerance && Math.Abs(x5 - x6) < tolerance)
                return new Point?();
            if (Math.Abs(y1 - y2) < tolerance && Math.Abs(y3 - y4) < tolerance)
                return new Point?();
            double x7;
            double y5;
            if (Math.Abs(x3 - x4) < tolerance)
            {
                double num2 = (y4 - y3) / (x6 - x5);
                double num3 = -num2 * x5 + y3;
                x7 = x3;
                y5 = num3 + num2 * x3;
            }
            else if (Math.Abs(x5 - x6) < tolerance)
            {
                double num4 = (y2 - y1) / (x4 - x3);
                double num5 = -num4 * x3 + y1;
                x7 = x5;
                y5 = num5 + num4 * x5;
            }
            else
            {
                double num6 = (y2 - y1) / (x4 - x3);
                double num7 = -num6 * x3 + y1;
                double num8 = (y4 - y3) / (x6 - x5);
                double num9 = -num8 * x5 + y3;
                x7 = (num7 - num9) / (num8 - num6);
                y5 = num9 + num8 * x7;
                if (Math.Abs(-num6 * x7 + y5 - num7) >= tolerance || Math.Abs(-num8 * x7 + y5 - num9) >= tolerance)
                    return new Point?();
            }
            Point p1 = new Point(x7, y5);
            return LineIntersection.IsInsideLine(lineA, p1, tolerance) && LineIntersection.IsInsideLine(lineB, p1, tolerance) ? new Point?(p1) : new Point?();
        }

        /// <summary>
        /// Returns true if given point(x,y) is inside the given line segment.
        /// </summary>
        private static bool IsInsideLine(Line line, Point p, double tolerance)
        {
            double x1 = p.X;
            double y1 = p.Y;
            double x2 = line.FirstPoint.X;
            double y2 = line.FirstPoint.Y;
            double x3 = line.SecondPoint.X;
            double y3 = line.SecondPoint.Y;
            return (x1.IsGreaterThanOrEqual(x2, tolerance) && x1.IsLessThanOrEqual(x3, tolerance) || x1.IsGreaterThanOrEqual(x3, tolerance) && x1.IsLessThanOrEqual(x2, tolerance)) && (y1.IsGreaterThanOrEqual(y2, tolerance) && y1.IsLessThanOrEqual(y3, tolerance) || y1.IsGreaterThanOrEqual(y3, tolerance) && y1.IsLessThanOrEqual(y2, tolerance));
        }
    }
}
