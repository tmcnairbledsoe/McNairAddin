using FillPatternEditor.Utils;
using System;
using System.Linq;

namespace FillPatternEditor.Utils
{
    public class PatternSafeGrid
    {
        private const double ZeroTolerance = 5E-06;
        private const double HalfPi = Math.PI / 2.0;
        private double _offsetDirection;
        private double _angle;
        private int _uTitles;
        private int _vTitles;
        private double _domainU;
        private double _domainV;

        public PatternSafeGrid(PatternPoint domain, double diagonalAngle, int uTitles, int vTitles, bool flipped = false)
        {
            Domain = domain;
            Flipped = flipped;
            DiagonalAngle = diagonalAngle;
            AxisLine = new PatternLine(new PatternPoint(0.0, 0.0), new PatternPoint(Domain.U * uTitles, Domain.V * vTitles));
            DetermineAbstractParams(uTitles, vTitles);
        }

        public PatternPoint Domain { get; }
        public double DiagonalAngle { get; }
        public PatternLine AxisLine { get; }
        public bool Flipped { get; }

        // Calculate various grid parameters based on uTitles and vTitles
        private void DetermineAbstractParams(int uTitles, int vTitles)
        {
            if (AxisLine.Angle <= DiagonalAngle)
            {
                _offsetDirection = Flipped ? 1.0 : -1.0;
                _angle = AxisLine.Angle;
                _uTitles = uTitles;
                _vTitles = vTitles;
                _domainU = Domain.U;
                _domainV = Domain.V;
            }
            else
            {
                if (!Flipped)
                {
                    _offsetDirection = 1.0;
                    _angle = HalfPi - AxisLine.Angle;
                }
                else
                {
                    _offsetDirection = -1.0;
                    _angle = AxisLine.Angle - HalfPi;
                }
                _uTitles = vTitles;
                _vTitles = uTitles;
                _domainU = Domain.V;
                _domainV = Domain.U;
            }
        }

        public bool IsValid() => Shift.HasValue;

        public double GridAngle => Flipped ? Math.PI - AxisLine.Angle : AxisLine.Angle;

        public double Span => AxisLine.Length;

        public double Offset => Math.Abs(_angle) < ZeroTolerance
            ? _domainV * _offsetDirection
            : Math.Abs(_domainU * Math.Sin(_angle) / _vTitles) * _offsetDirection;

        public double? Shift
        {
            get
            {
                if (Math.Abs(_angle) < ZeroTolerance)
                    return 0.0;

                if (_uTitles == 1 && _vTitles == 1)
                    return Math.Abs(_domainU * Math.Cos(_angle));

                var offset = new PatternPoint(Math.Abs(Offset * Math.Sin(_angle)), -Math.Abs(Offset * Math.Cos(_angle)));
                var patternPoint1 = new PatternPoint(0.0, 0.0);
                var patternPoint2 = new PatternPoint(_domainU * _uTitles, _domainV * _vTitles);

                var offsetLine = new PatternLine(patternPoint1.Add(offset), patternPoint2.Add(offset));
                var nxtGridPoint = FindNxtGridPoint(offsetLine);

                return nxtGridPoint != null ? offsetLine.Start.DistanceTo(nxtGridPoint) : (double?)null;
            }
        }

        // Find the next grid point that lies on the offset line
        private PatternPoint FindNxtGridPoint(PatternLine offsetLine)
        {
            for (int i = 0; i < _uTitles; i++)
            {
                foreach (int j in Enumerable.Range(0, _vTitles))
                {
                    var point = new PatternPoint(_domainU * i, _domainV * j);
                    if (offsetLine.PointOnLine(point))
                        return point;
                }
            }
            return null;
        }

        // Override ToString for better debugging and output readability
        public override string ToString()
        {
            return $"<_PatternSafeGrid GridAngle: {GridAngle} Angle: {_angle} U_Titles: {_uTitles} V_Titles: {_vTitles} " +
                   $"Domain_U: {_domainU} Domain_V: {_domainV} Offset_Dir: {_offsetDirection} Span: {Span} Offset: {Offset} Shift: {Shift}>";
        }
    }
}
