using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FillPatternEditor.Utils
{
    public class PatternGrid
    {
        private readonly PatternDomain _domain;
        private readonly List<PatternLine> _segmentLines = new List<PatternLine>();

        public PatternGrid(PatternDomain domain, PatternLine initialLine)
        {
            _domain = domain;

            // Get the best angle for the grid
            PatternSafeGrid bestAngle = domain.GetBestAngle(initialLine.Angle);
            Angle = bestAngle.GridAngle;
            Span = bestAngle.Span;
            Offset = bestAngle.Offset;

            // If a shift value is provided, set it
            if (bestAngle.Shift.HasValue)
                Shift = bestAngle.Shift.Value;

            // Rotate the initial line to the calculated angle and add it to the segment list
            initialLine.Rotate(Angle - initialLine.Angle, initialLine.CenterPoint);
            _segmentLines.Add(initialLine);
        }

        public double Angle { get; }

        public double Span { get; }

        public double Offset { get; }

        public double Shift { get; }

        public override string ToString()
        {
            return $"<_PatternGrid Angle: {Angle}, Span: {Span}, Offset: {Offset}, Shift: {Shift}>";
        }

        public PatternPoint Origin
        {
            get
            {
                // Get all segment points (start and end)
                List<PatternPoint> points = _segmentLines.SelectMany(line => new[] { line.Start, line.End }).ToList();

                // Set the reference point for distance comparison based on angle
                PatternPoint referencePoint = Angle <= Math.PI / 2.0
                    ? new PatternPoint(0.0, 0.0)
                    : new PatternPoint(_domain.UVec.Length, 0.0);

                // Find the closest point to the reference point
                return points.OrderBy(point => point.DistanceTo(referencePoint)).FirstOrDefault();
            }
        }

        public List<double> Segments
        {
            get
            {
                // Get the first line length and calculate remaining segment length
                double lineLength = _segmentLines.First().Length;
                return new List<double> { lineLength, Span - lineLength };
            }
        }
    }
}
