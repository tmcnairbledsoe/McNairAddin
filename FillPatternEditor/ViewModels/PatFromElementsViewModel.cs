using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using FillPatternEditor.Enums;
using FillPatternEditor.Models;
using FillPatternEditor.Revit;
using FillPatternEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FillPatternEditor.ViewModels
{
    public class PatFromElementsViewModel : ObservableObject
    {
        private string _patternName;
        private XYZ[] _borderPoints;
        private Autodesk.Revit.DB.Line[] _originBorders;
        private List<Element> _selectedElements;
        private List<Tuple<string, FillPatternTarget>> _existingPatterns;
        private string _previewPatternStringRepresentation;
        private bool _canAccept;
        private PatUnits _patUnits = PatUnits.MM;
        private string _errorMessage;
        private double _scaleMultiple = 1.0;
        private int _rotationAngle;
        private bool _flipHorizontal;
        private bool _flipVertical;
        private double _minX;
        private double _minY;
        private PatternGeometryService _patternGeometryService;

        public PatFromElementsViewModel(XYZ[] borderPoints, string patternName, List<Tuple<string, FillPatternTarget>> existingPatterns)
        {
            _patternName = patternName;
            _borderPoints = borderPoints;
            _existingPatterns = existingPatterns;
            PreviewPattern = new FillPattern { Target = FillPatternTarget.Model };
        }

        public List<Element> SelectedElements
        {
            get => _selectedElements;
            set => SetProperty(ref _selectedElements, value);
        }

        public FillPattern PreviewPattern { get; set; }

        public string PreviewPatternStringRepresentation
        {
            get => _previewPatternStringRepresentation;
            set => SetProperty(ref _previewPatternStringRepresentation, value);
        }

        public bool CanAccept
        {
            get => _canAccept;
            set => SetProperty(ref _canAccept, value);
        }

        public PatUnits PatUnits
        {
            get => _patUnits;
            set
            {
                SetProperty(ref _patUnits, value);
                UpdatePreviewFillPattern();
            }
        }

        public string PatternName
        {
            get => _patternName;
            set
            {
                SetProperty(ref _patternName, value);
                UpdatePreviewFillPattern();
            }
        }

        public FillPatternTarget FillPatternTarget
        {
            get => PreviewPattern.Target;
            set
            {
                if (this.PreviewPattern.Target == value)
                    return;
                this.PreviewPattern.Target = value;
                this.OnPropertyChanged(nameof(FillPatternTarget));
                this.UpdatePreviewFillPattern();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public double ScaleMultiple
        {
            get => _scaleMultiple;
            set
            {
                if (SetProperty(ref _scaleMultiple, value))
                {
                    UpdatePreviewFillPattern();
                }
            }
        }

        public int ViewScale => RevitInterop.Document.ActiveView.Scale;

        public int RotationAngle
        {
            get => _rotationAngle;
            set
            {
                if (SetProperty(ref _rotationAngle, value))
                {
                    UpdatePreviewFillPattern();
                }
            }
        }

        public bool FlipHorizontal
        {
            get => _flipHorizontal;
            set
            {
                if (SetProperty(ref _flipHorizontal, value))
                {
                    UpdatePreviewFillPattern();
                }
            }
        }

        public bool FlipVertical
        {
            get => _flipVertical;
            set
            {
                if (SetProperty(ref _flipVertical, value))
                {
                    UpdatePreviewFillPattern();
                }
            }
        }

        public bool IsValidBorderPoints(out string errorMessage)
        {
            errorMessage = string.Empty;
            var xPoints = _borderPoints.Select(p => p.X).ToList();
            var yPoints = _borderPoints.Select(p => p.Y).ToList();

            _minX = xPoints.Min();
            _minY = yPoints.Min();

            var bottomLeft = new XYZ(_minX, _minY, 0);
            var bottomRight = new XYZ(xPoints.Max(), _minY, 0);
            var topRight = new XYZ(xPoints.Max(), yPoints.Max(), 0);
            var topLeft = new XYZ(_minX, yPoints.Max(), 0);

            if (bottomLeft.DistanceTo(bottomRight) < Constants.DraftingPatternMinSize || bottomLeft.DistanceTo(topLeft) < Constants.DraftingPatternMinSize)
            {
                errorMessage = "Pattern size is too small.";
                return false;
            }

            _originBorders = new[] {
                Autodesk.Revit.DB.Line.CreateBound(bottomLeft, bottomRight),
                Autodesk.Revit.DB.Line.CreateBound(bottomRight, topRight),
                Autodesk.Revit.DB.Line.CreateBound(topRight, topLeft),
                Autodesk.Revit.DB.Line.CreateBound(topLeft, bottomLeft)
            };

            return true;
        }

        public void UpdatePreviewFillPattern()
        {
            if (!IsValidEnteredData())
            {
                PreviewPattern.SetFillGrids(new List<FillGrid>());
                PreviewPatternStringRepresentation = string.Empty;
                CanAccept = false;
                return;
            }

            try
            {
                var fillGrids = ConvertToFillGrids();
                if (fillGrids != null)
                {
                    PreviewPattern.SetFillGrids(fillGrids);
                    PreviewPattern.Target = FillPatternTarget;
                    PreviewPatternStringRepresentation = PreviewPattern.ToPatPattern(PatUnits, PatternName, PreviewPattern.Target).GetStringRepresentation();
                    CanAccept = true;
                }
                else
                {
                    ErrorMessage = "Failed to generate fill pattern.";
                    CanAccept = false;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
                CanAccept = false;
            }
        }

        private bool IsValidEnteredData()
        {
            if (string.IsNullOrEmpty(PatternName))
            {
                ErrorMessage = "Pattern name cannot be empty.";
                return false;
            }

            if (_existingPatterns.Any(p => p.Item1.Equals(PatternName, StringComparison.InvariantCultureIgnoreCase) && p.Item2 == FillPatternTarget))
            {
                ErrorMessage = $"Pattern name '{PatternName}' already exists.";
                return false;
            }

            ErrorMessage = string.Empty;
            return true;
        }

        private List<FillGrid> ConvertToFillGrids()
        {
            var lines = new List<Autodesk.Revit.DB.Line>();
            foreach (var element in SelectedElements)
            {
                Curve curve = element is DetailCurve detailCurve ? detailCurve.GeometryCurve :
                              element is ModelCurve modelCurve ? modelCurve.GeometryCurve : null;

                if (curve != null)
                {
                    XYZ lastPoint = null;
                    foreach (var point in curve.Tessellate())
                    {
                        if (lastPoint != null)
                        {
                            lines.Add(Autodesk.Revit.DB.Line.CreateBound(GetPointInPatUnits(lastPoint), GetPointInPatUnits(point)));
                        }
                        lastPoint = point;
                    }
                }
            }

            _patternGeometryService = _patternGeometryService ?? GetPatternGeometryService();

            return _patternGeometryService.ConvertToFillGrids(lines, PatUnits, GetPatternScale(), RotationAngle, FlipHorizontal, FlipVertical);
        }

        private XYZ GetPointInPatUnits(XYZ point) => new XYZ((point.X - _minX).ConvertToPatUnit(PatUnits), (point.Y - _minY).ConvertToPatUnit(PatUnits), 0);

        private PatternGeometryService GetPatternGeometryService()
        {
            var borders = new PatternBorders(GetBorderLinesInPatUnits());
            var domain = new PatternDomain(borders, PatUnits, FillPatternTarget == FillPatternTarget.Model,false);
            domain.CalculateSafeAngles();
            return new PatternGeometryService(domain, borders);
        }

        private IEnumerable<Autodesk.Revit.DB.Line> GetBorderLinesInPatUnits()
        {
            return _originBorders.Select(line =>
            {
                var start = new XYZ(line.GetEndPoint(0).X.ConvertToPatUnit(PatUnits), line.GetEndPoint(0).Y.ConvertToPatUnit(PatUnits), 0);
                var end = new XYZ(line.GetEndPoint(1).X.ConvertToPatUnit(PatUnits), line.GetEndPoint(1).Y.ConvertToPatUnit(PatUnits), 0);
                return Autodesk.Revit.DB.Line.CreateBound(start, end);
            });
        }

        private double GetPatternScale()
        {
            return FillPatternTarget == FillPatternTarget.Model ? 1.0 * ScaleMultiple : (1.0 / ViewScale) * ScaleMultiple;
        }
    }
}
