using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FillPatternEditor.Utils
{
    public class PatternLine
    {
        private const double _zeroTolerance = 5E-06;

        public PatternLine(PatternPoint start, PatternPoint end)
        {
            // Ensure Start always has a smaller V value than End
            if (start.V <= end.V)
            {
                Start = start;
                End = end;
            }
            else
            {
                Start = end;
                End = start;
            }
            UVector = new UV(1.0, 0.0);
        }

        public PatternPoint Start { get; private set; }

        public PatternPoint End { get; private set; }

        public UV UVector { get; private set; }

        // Direction vector from Start to End
        public PatternPoint Direction => new PatternPoint(End.U - Start.U, End.V - Start.V);

        // Angle between the line direction and the UVector (horizontal line)
        public double Angle => UVector.AngleTo(new UV(Direction.U, Direction.V));

        // Center point of the line
        public PatternPoint CenterPoint => new PatternPoint((End.U + Start.U) / 2.0, (End.V + Start.V) / 2.0);

        // Length of the line
        public double Length => Math.Sqrt(Math.Pow(Direction.U, 2.0) + Math.Pow(Direction.V, 2.0));

        // Check if a given point lies on the line (within a tolerance)
        public bool PointOnLine(PatternPoint point, double tolerance = _zeroTolerance)
        {
            double crossProduct = (Start.U - point.U) * (End.V - point.V) - (Start.V - point.V) * (End.U - point.U);
            return Math.Abs(crossProduct) <= tolerance;
        }

        // Find the intersection point of two lines (if it exists)
        public PatternPoint Intersect(PatternLine otherLine)
        {
            PatternPoint deltaU = new PatternPoint(Start.U - End.U, otherLine.Start.U - otherLine.End.U);
            PatternPoint deltaV = new PatternPoint(Start.V - End.V, otherLine.Start.V - otherLine.End.V);

            double determinant = Det(deltaU, deltaV);
            if (Math.Abs(determinant) < _zeroTolerance)
                return null;  // Lines are parallel or coincident

            PatternPoint detStartEnd = new PatternPoint(Det(Start, End), Det(otherLine.Start, otherLine.End));
            return new PatternPoint(Det(detStartEnd, deltaU) / determinant, Det(detStartEnd, deltaV) / determinant);
        }

        // Rotate the line by a given angle around a specified origin (default is (0,0))
        public void Rotate(double angle, PatternPoint origin = null)
        {
            Start.Rotate(angle, origin);
            End.Rotate(angle, origin);
        }

        // Helper function to calculate the determinant of two points
        private double Det(PatternPoint a, PatternPoint b)
        {
            return a.U * b.V - a.V * b.U;
        }
    }
}
