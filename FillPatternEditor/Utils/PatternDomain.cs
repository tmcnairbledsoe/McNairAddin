using FillPatternEditor.Enums;
using FillPatternEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FillPatternEditor.Utils
{
    public class PatternDomain
    {
        private const double MaxModelDomain = 100.0;
        private const double MaxDetailDomain = 10.0;
        private const int RatioResolution = 2;
        private const int MaxDomainMultiplicity = 8;
        private const double AngleCorrRatio = 0.01;

        private readonly PatternPoint _origin;
        private readonly PatternPoint _bounds;
        private readonly List<PatternSafeGrid> _safeAngles = new List<PatternSafeGrid>();
        private readonly PatternLine _diagonal;
        private readonly bool _expandable;
        private readonly double _maxDomain;
        private double _targetDomain;

        public PatternDomain(
            PatternBorders borders,
            PatUnits patUnits,
            bool modelPattern,
            bool expandable)
        {
            _origin = new PatternPoint(borders.MinX.ConvertFromPatUnit(patUnits), borders.MinY.ConvertFromPatUnit(patUnits));
            _bounds = new PatternPoint(borders.MaxX.ConvertFromPatUnit(patUnits), borders.MaxY.ConvertFromPatUnit(patUnits)).Sub(_origin);

            if (ZeroDomain())
                throw new Exception("Cannot process zero domain.");

            UVec = new PatternLine(new PatternPoint(0.0, 0.0), new PatternPoint(_bounds.U, 0.0));
            VVec = new PatternLine(new PatternPoint(0.0, 0.0), new PatternPoint(0.0, _bounds.V));

            _maxDomain = modelPattern ? MaxModelDomain : MaxDetailDomain;
            _expandable = expandable;
            _targetDomain = _maxDomain;
            _diagonal = new PatternLine(new PatternPoint(0.0, 0.0), new PatternPoint(_bounds.U, _bounds.V));
        }

        public PatternLine UVec { get; }

        public PatternLine VVec { get; }

        public override string ToString()
        {
            return $"<_PatternDomain U: {_bounds.U}, V: {_bounds.V}, SafeAngles: {_safeAngles.Count}>";
        }

        private bool ZeroDomain()
        {
            return Math.Abs(_bounds.U) < 0.001 || Math.Abs(_bounds.V) < 0.001;
        }

        public void CalculateSafeAngles()
        {
            int uTitles = 1;
            int vTitles = 1;
            var uniqueRatios = new HashSet<double> { 1.0 };

            // Add base angles
            _safeAngles.Add(new PatternSafeGrid(_bounds, _diagonal.Angle, uTitles, 0));
            _safeAngles.Add(new PatternSafeGrid(_bounds, _diagonal.Angle, uTitles, 0, true));
            _safeAngles.Add(new PatternSafeGrid(_bounds, _diagonal.Angle, uTitles, vTitles));
            _safeAngles.Add(new PatternSafeGrid(_bounds, _diagonal.Angle, uTitles, vTitles, true));
            _safeAngles.Add(new PatternSafeGrid(_bounds, _diagonal.Angle, 0, vTitles));

            // Add angles based on ratio of U and V
            while (_bounds.U * uTitles <= _targetDomain / 2.0)
            {
                for (int v = 1; _bounds.V * v <= _targetDomain / 2.0; v++)
                {
                    double ratio = Math.Round((double)v / uTitles, RatioResolution);

                    if (uniqueRatios.Add(ratio))
                    {
                        var grid1 = new PatternSafeGrid(_bounds, _diagonal.Angle, uTitles, v);
                        var grid2 = new PatternSafeGrid(_bounds, _diagonal.Angle, uTitles, v, true);

                        if (grid1.IsValid() && grid2.IsValid())
                        {
                            _safeAngles.Add(grid1);
                            _safeAngles.Add(grid2);
                        }
                    }
                }
                uTitles++;
            }
        }

        public bool Expand()
        {
            if (_targetDomain > _maxDomain * MaxDomainMultiplicity)
                return false;

            _targetDomain += _maxDomain / 2.0;
            CalculateSafeAngles();
            return true;
        }

        public PatternLine GetDomainCoords(PatternLine patLine)
        {
            return new PatternLine(patLine.Start.Sub(_origin), patLine.End.Sub(_origin));
        }

        public PatternSafeGrid GetGridParams(double axisAngle)
        {
            return _safeAngles.OrderBy(grid => Math.Abs(grid.GridAngle - axisAngle)).FirstOrDefault();
        }

        public double GetRequiredCorrection(double axisAngle)
        {
            return Math.Abs(axisAngle - GetGridParams(axisAngle).GridAngle);
        }

        public PatternSafeGrid GetBestAngle(double axisAngle)
        {
            if (!_expandable)
                return GetGridParams(axisAngle);

            while (GetRequiredCorrection(axisAngle) >= AngleCorrRatio && Expand())
                ;

            return GetGridParams(axisAngle);
        }
    }
}
