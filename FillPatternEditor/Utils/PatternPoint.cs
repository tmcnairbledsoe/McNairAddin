using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FillPatternEditor.Utils
{
    public class PatternPoint
    {
        // Tolerances and resolution for comparisons and rounding
        private const double Tolerance = 0.0001;
        private const double ZeroTolerance = 5E-06;
        private const int CoordinateResolution = 15;

        // Constructor that initializes U and V, with rounding applied
        public PatternPoint(double u, double v)
        {
            U = RoundVector(u);
            V = RoundVector(v);
        }

        public double U { get; private set; }
        public double V { get; private set; }

        // Checks if this point equals another point (within a tolerance)
        public bool Equals(PatternPoint other)
        {
            return Math.Abs(U - other.U) < Tolerance && Math.Abs(V - other.V) < Tolerance;
        }

        // Adds another point to this point
        public PatternPoint Add(PatternPoint other)
        {
            return new PatternPoint(U + other.U, V + other.V);
        }

        // Subtracts another point from this point
        public PatternPoint Sub(PatternPoint other)
        {
            return new PatternPoint(U - other.U, V - other.V);
        }

        // Calculates the Euclidean distance between this point and another
        public double DistanceTo(PatternPoint point)
        {
            return Math.Sqrt(Math.Pow(point.U - U, 2.0) + Math.Pow(point.V - V, 2.0));
        }

        // Rotates this point around a given origin by a specified angle
        public bool Rotate(double angle, PatternPoint origin = null)
        {
            origin ??= new PatternPoint(0.0, 0.0);  // Default to (0,0) if origin is not provided
            double deltaX = U - origin.U;
            double deltaY = V - origin.V;

            U = origin.U + (deltaX * Math.Cos(angle) - deltaY * Math.Sin(angle));
            V = origin.V + (deltaX * Math.Sin(angle) + deltaY * Math.Cos(angle));
            return true;
        }

        // Rounds a given coordinate value, setting it to 0 if it's below a threshold
        private double RoundVector(double length)
        {
            length = Math.Abs(length) > ZeroTolerance ? length : 0.0;
            return Math.Round(length, CoordinateResolution);
        }
    }
}
