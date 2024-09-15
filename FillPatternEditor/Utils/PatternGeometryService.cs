using Autodesk.Revit.DB;
using FillPatternEditor.Enums;
using FillPatternEditor.Models;
using FillPatternEditor.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FillPatternEditor.Utils
{
    public class PatternGeometryService
    {
        private readonly PatternDomain _patternDomain;
        private readonly PatternBorders _borders;

        public PatternGeometryService(PatternDomain patternDomain, PatternBorders borders)
        {
            _patternDomain = patternDomain ?? throw new ArgumentNullException(nameof(patternDomain));
            _borders = borders ?? throw new ArgumentNullException(nameof(borders));
        }

        public List<FillGrid> ConvertToFillGrids(
            IList<IGeometry> geometries,
            PatUnits patUnits,
            double scale,
            double rotationAngle,
            bool flipU,
            bool flipV,
            double approximationAccuracy = 0.1)
        {
            approximationAccuracy = Math.Clamp(approximationAccuracy, 0.1, 1.0);

            var trimmedLines = new List<Models.Line>();
            foreach (var geometry in geometries)
            {
                var clonedGeometry = geometry.Clone();
                if (clonedGeometry is Models.Line line && line.Length > 0.0)
                {
                    var trimmedLine = TrimLine(line, _borders);
                    if (trimmedLine != null)
                        trimmedLines.Add(trimmedLine);
                }
                else if (clonedGeometry is IGeometryApproximation approximation)
                {
                    var approximationPoints = approximation.GetApproximationPoints(approximationAccuracy);
                    for (int i = 1; i < approximationPoints.Count; i++)
                    {
                        var lineSegment = new Models.Line(approximationPoints[i - 1], approximationPoints[i]);
                        var trimmedLine = TrimLine(lineSegment, _borders);
                        if (trimmedLine != null)
                            trimmedLines.Add(trimmedLine);
                    }
                }
            }

            if (!trimmedLines.Any()) return new List<FillGrid>();

            return CreateFillGrids(trimmedLines, patUnits, scale, rotationAngle, flipU, flipV);
        }

        public List<FillGrid> ConvertToFillGrids(
            IList<Autodesk.Revit.DB.Line> lines,
            PatUnits patUnits,
            double scale,
            double rotationAngle,
            bool flipU,
            bool flipV)
        {
            var geometries = lines.Select(line =>
            {
                //TODO Double check
                var start = line.GetEndPoint(0);
                var end = line.GetEndPoint(1);
                return new Models.Line(new System.Windows.Point(start.X, start.Y), new System.Windows.Point(end.X, end.Y)) as IGeometry;
            }).ToList();

            return ConvertToFillGrids(geometries, patUnits, scale, rotationAngle, flipU, flipV);
        }

        private List<FillGrid> CreateFillGrids(
            List<Models.Line> lines,
            PatUnits patUnits,
            double scale,
            double rotateAngle,
            bool flipU,
            bool flipV)
        {
            var fillGrids = new List<FillGrid>();
            var patternGridList = new List<PatternGrid>();

            foreach (var line in lines)
            {
                var start = new PatternPoint(line.FirstPoint.X.ConvertFromPatUnit(patUnits), line.FirstPoint.Y.ConvertFromPatUnit(patUnits));
                var end = new PatternPoint(line.SecondPoint.X.ConvertFromPatUnit(patUnits), line.SecondPoint.Y.ConvertFromPatUnit(patUnits));

                try
                {
                    var patternGrid = new PatternGrid(_patternDomain, _patternDomain.GetDomainCoords(new PatternLine(start, end)));
                    patternGridList.Add(patternGrid);
                }
                catch (Exception)
                {
                    // Handle exceptions gracefully if needed
                }
            }

            foreach (var patternGrid in patternGridList)
            {
                var fillGrid = new FillGrid
                {
                    Angle = CalculateAngle(patternGrid.Angle, rotateAngle, flipU, flipV),
                    Origin = CalculateOrigin(patternGrid.Origin, rotateAngle, scale, flipU, flipV),
                    Offset = CalculateOffset(patternGrid.Offset, scale, flipU, flipV),
                    Shift = patternGrid.Shift * scale
                };

                if (patternGrid.Segments.Any())
                {
                    var scaledSegments = patternGrid.Segments.Select(s => s * scale).ToList();
                    fillGrid.SetSegments(scaledSegments);
                }

                fillGrids.Add(fillGrid);
            }

            return fillGrids;
        }

        private static double CalculateAngle(double patternAngle, double rotateAngle, bool flipU, bool flipV)
        {
            double angle = flipU && !flipV || flipV && !flipU ? -rotateAngle : rotateAngle;

            return flipU && flipV ? Math.PI + patternAngle + angle :
                   flipU ? Math.PI - patternAngle + angle :
                   flipV ? -patternAngle + angle : patternAngle + angle;
        }

        private static UV CalculateOrigin(PatternPoint origin, double rotateAngle, double scale, bool flipU, bool flipV)
        {
            var adjustedOrigin = new PatternPoint(flipU ? -origin.U : origin.U, flipV ? -origin.V : origin.V);
            if (Math.Abs(rotateAngle) > 0.001)
                adjustedOrigin.Rotate(rotateAngle);

            return new UV(adjustedOrigin.U * scale, adjustedOrigin.V * scale);
        }

        private static double CalculateOffset(double patternOffset, double scale, bool flipU, bool flipV)
        {
            return flipU && flipV || !flipU && !flipV ? patternOffset * scale : -patternOffset * scale;
        }

        private static Models.Line TrimLine(Models.Line line, PatternBorders borders)
        {
            var startInside = borders.IsPointInside(line.FirstPoint);
            var endInside = borders.IsPointInside(line.SecondPoint);

            if (startInside && endInside)
                return line;

            var intersectionPoints = borders.BorderLines
                .Select(borderLine => line.Intersection(borderLine))
                .Where(intersection => intersection.HasValue)
                .Select(intersection => intersection.Value)
                .ToList();

            if (!startInside && !endInside)
            {
                if (intersectionPoints.Count < 2) return null;

                line.FirstPoint = intersectionPoints.OrderBy(p => p.DistanceTo(line.FirstPoint)).First();
                line.SecondPoint = intersectionPoints.OrderBy(p => p.DistanceTo(line.SecondPoint)).First();
            }
            else if (startInside)
            {
                line.SecondPoint = intersectionPoints.OrderBy(p => p.DistanceTo(line.SecondPoint)).First();
            }
            else
            {
                line.FirstPoint = intersectionPoints.OrderBy(p => p.DistanceTo(line.FirstPoint)).First();
            }

            return intersectionPoints.Any() ? line : null;
        }
    }
}
