using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using FillPatternEditor.Enums;
using FillPatternEditor.Utils;

namespace FillPatternEditor.Models
{
    public class CustomPattern : ObservableObject, IPattern, IDisposable
    {
        // Fields for storing pattern properties
        private System.Windows.Visibility _visibility = System.Windows.Visibility.Visible;
        private string _name;
        private bool _isAllowableName = true;
        private string _notAllowableNameMessage;
        private bool _isModified;
        private FillPatternCreationType _creationType;
        private bool _basicEditIsEnableSecondOffset;
        private double _basicEditAngle;
        private double _basicEditFirstOffset;
        private double _basicEditSecondOffset;
        private PatPattern _selectedPatPattern;
        private double _patPatternImportScale = 1.0;
        private string _patPatternSearchText;
        private bool _isEnableIncreasePatternScale = true;
        private bool _isEnableDecreasePatternScale = true;
        private readonly FillPatternCreationType _originFillPatternCreationType;
        private FillPattern _fillPattern;

        public System.Windows.Visibility Visibility
        {
            get => this._visibility;
            set
            {
                if (object.Equals((object)value, (object)this._visibility))
                    return;
                this._visibility = value;
                this.OnPropertyChanged(nameof(System.Windows.Visibility));
            }
        }

        public double BasicEditOffsetMinimum
        {
            get => this.Target != null ? Constants.ModelPatternMinSize : Constants.DraftingPatternMinSize;
        }

        public double BasicEditOffsetMaximum
        {
            get => this.Target != null ? Constants.ModelPatternMaxSize : Constants.DraftingPatternMaxSize;
        }

        // Constructor for creating a CustomPattern based on a FillPattern
        public CustomPattern(FillPattern fillPattern, string newPatternName, ElementId fillPatternElementId)
        {
            Guid = Guid.NewGuid();
            OriginalFillPatternElementId = fillPatternElementId;

            if (fillPattern != null)
            {
                OriginFillPattern = fillPattern;
                FillPattern = new FillPattern(fillPattern);
            }
            else
            {
                OriginFillPattern = null;
                FillPattern = new FillPattern(newPatternName, FillPatternTarget.Drafting, FillPatternHostOrientation.ToView, Utils.Utils.DegreeToRadian(45), Utils.Utils.MmToFt(3));
                IsModified = true;
            }

            _basicEditSecondOffset = 3.0;
            _name = FillPattern.Name;
            _creationType = _originFillPatternCreationType = FillPattern.DetectCreationType();

            if (_creationType == FillPatternCreationType.Basic)
                ReadValuesFromBasicCreationType(false);
        }

        // Constructor for creating a CustomPattern from a PatPattern
        public CustomPattern(PatPattern patPattern)
        {
            Guid = Guid.NewGuid();
            OriginFillPattern = null;
            FillPattern = new FillPattern();
            FillPattern.HostOrientation = FillPatternHostOrientation.ToView;
            SetPatternFromPat(patPattern);
            _basicEditSecondOffset = 3.0;
            _basicEditFirstOffset = 3.0;
            _creationType = FillPatternCreationType.Custom;
        }

        // Constructor for duplicating an existing CustomPattern
        public CustomPattern(CustomPattern sourceCustomPattern, string newPatternName)
        {
            Guid = Guid.NewGuid();
            OriginFillPattern = null;
            _basicEditFirstOffset = sourceCustomPattern._basicEditFirstOffset;
            _basicEditSecondOffset = sourceCustomPattern._basicEditSecondOffset;
            _creationType = sourceCustomPattern.CreationType;
            SetDataFromFillPattern(sourceCustomPattern.FillPattern, newPatternName);

            if (_creationType == FillPatternCreationType.Basic)
                ReadValuesFromBasicCreationType(false);

            IsModified = true;
        }

        // Properties

        public Guid Guid { get; }


        public FillPattern FillPattern
        {
            get => _fillPattern;
            set
            {
                _fillPattern = value;
                OnPropertyChanged(nameof(FillPattern));
            }
        }

        public FillPattern OriginFillPattern { get; }

        public ElementId OriginalFillPatternElementId { get; }

        public bool IsCreated => OriginFillPattern == null;

        public string Name
        {
            get => _name;
            set
            {
                if (Equals(value, _name)) return;
                _name = value;
                FillPattern.Name = value;
                OnPropertyChanged(nameof(Name));
                DetectIsModified();
            }
        }

        public PatPattern SelectedPatPattern
        {
            get => this._selectedPatPattern;
            set
            {
                if (object.Equals((object)value, (object)this._selectedPatPattern))
                    return;
                this._selectedPatPattern = value;
                this.OnPropertyChanged(nameof(SelectedPatPattern));
                this._patPatternImportScale = 1.0;
                this.OnPropertyChanged("PatPatternImportScale");
                this.OnPropertyChanged("PatPatternUnits");
                this.SetPatternFormSelectedPat();
            }
        }

        public bool IsAllowableName
        {
            get => _isAllowableName;
            set
            {
                if (Equals(value, _isAllowableName)) return;
                _isAllowableName = value;
                OnPropertyChanged(nameof(IsAllowableName));
            }
        }

        public string NotAllowableNameMessage
        {
            get => _notAllowableNameMessage;
            set
            {
                if (Equals(value, _notAllowableNameMessage)) return;
                _notAllowableNameMessage = value;
                OnPropertyChanged(nameof(NotAllowableNameMessage));
            }
        }

        public bool IsModified
        {
            get => _isModified;
            set
            {
                if (Equals(value, _isModified)) return;
                _isModified = value;
                OnPropertyChanged(nameof(IsModified));
            }
        }

        public FillPatternHostOrientation HostOrientation
        {
            get => FillPattern.HostOrientation;
            set
            {
                if (FillPattern.HostOrientation == value) return;
                FillPattern.HostOrientation = value;
                OnPropertyChanged(nameof(HostOrientation));
                DetectIsModified();
            }
        }

        public bool HostOrientationIsEnabled => Target == FillPatternTarget.Drafting;

        public ObservableCollection<PatPattern> PatPatterns { get; set; } = new ObservableCollection<PatPattern>();

        public string PatPatternSearchText
        {
            get => this._patPatternSearchText;
            set
            {
                if (object.Equals((object)value, (object)this._patPatternSearchText))
                    return;
                this._patPatternSearchText = value;
                this.OnPropertyChanged(nameof(PatPatternSearchText));
                this.ChangePatPatternsVisibilityBySearchString(value);
            }
        }


        public FillPatternTarget Target
        {
            get => FillPattern.Target;
            set
            {
                if (FillPattern.Target == value) return;
                FillPattern.Target = value;
                OnPropertyChanged(nameof(Target));
                DetectIsModified();
                OnPropertyChanged(nameof(HostOrientationIsEnabled));
                OnPropertyChanged(nameof(BasicEditOffsetMinimum));
                OnPropertyChanged(nameof(BasicEditOffsetMaximum));
                OnTargetChange();
                ChangePatPatternsVisibilityBySearchString(PatPatternSearchText);
            }
        }

        public FillPatternCreationType CreationType
        {
            get => _creationType;
            set
            {
                _creationType = value;
                OnPropertyChanged(nameof(CreationType));
                DetectIsModified();
                AcceptingNewCreationType(value);
                OnPropertyChanged(nameof(IsEnableBasicParameters));
            }
        }

        public bool IsEnableBasicParameters => CreationType == FillPatternCreationType.Basic;

        public bool IsEnableIncreasePatternScale
        {
            get => _isEnableIncreasePatternScale;
            set
            {
                if (Equals(value, _isEnableIncreasePatternScale)) return;
                _isEnableIncreasePatternScale = value;
                OnPropertyChanged(nameof(IsEnableIncreasePatternScale));
            }
        }

        public bool IsEnableDecreasePatternScale
        {
            get => _isEnableDecreasePatternScale;
            set
            {
                if (Equals(value, _isEnableDecreasePatternScale)) return;
                _isEnableDecreasePatternScale = value;
                OnPropertyChanged(nameof(IsEnableDecreasePatternScale));
            }
        }

        // Command for resetting the pattern to its original state
        public ICommand ResetToOriginalFillPatternCommand => new RelayCommand(ResetToOriginalFillPattern);

        // Other commands and methods (such as for scaling the pattern) omitted for brevity

        // Method to reset the pattern to its original fill pattern
        private void ResetToOriginalFillPattern()
        {
            FillPattern = new FillPattern(OriginFillPattern);
            OnPropertyChanged(nameof(FillPattern));
            DetectIsModified();
            OnPropertyChanged(nameof(Target));
            _creationType = FillPattern.DetectCreationType();
            OnPropertyChanged(nameof(CreationType));
        }


        public void SetDataFromFillPattern(FillPattern sourceFillPattern, string name = null)
        {
            if (!string.IsNullOrEmpty(name))
            {
                this._name = name;
                this.OnPropertyChanged("Name");
            }
            this.FillPattern = new FillPattern(sourceFillPattern)
            {
                Target = sourceFillPattern.Target,
                Name = this._name
            };
            this._creationType = this.FillPattern.DetectCreationType();
            this.OnPropertyChanged("Target");
            this.OnPropertyChanged("CreationType");
            this.OnPropertyChanged("IsEnableBasicParameters");
            this.OnPropertyChanged("HostOrientationIsEnabled");
            this.DetectIsModified();
            this.OnPropertyChanged("FillPattern");
            PatternEditorCommand.PatternListWindow.PatternControl.FillPatternViewerControl.Regenerate();
            this.CheckIsAvailableIncreaseOrDecreaseScale();
        }

        // Additional helper methods for pattern manipulation, scaling, and applying changes omitted for brevity
        private void CheckIsAvailableIncreaseOrDecreaseScale()

        {
            if (this.Target == 0)
            {
                this.IsEnableIncreasePatternScale = this._basicEditFirstOffset < this.BasicEditOffsetMaximum && this._basicEditSecondOffset < this.BasicEditOffsetMaximum;
                if (this._basicEditFirstOffset <= this.BasicEditOffsetMinimum || this._basicEditSecondOffset <= this.BasicEditOffsetMinimum)
                    this.IsEnableDecreasePatternScale = false;
                else
                    this.IsEnableDecreasePatternScale = true;
            }
            else
            {
                this.IsEnableIncreasePatternScale = true;
                this.IsEnableDecreasePatternScale = true;
            }
        }

        private void SetPatternFormSelectedPat()
        {
            try
            {
                if (this._selectedPatPattern != null)
                    this.SetPatternFromPat(this._selectedPatPattern);
                else
                    this.FillPattern.SetFillGrids((IList<FillGrid>)new List<FillGrid>());
            }
            catch (Exception ex)
            {
                //TODO
                //ExceptionBox.Show(ex);
            }
        }

        private void SetPatternFromPat(PatPattern patPattern)
        {
            List<FillGrid> fillGrids = patPattern.ToFillGrids(this._patPatternImportScale);
            if (!fillGrids.Any<FillGrid>())
                throw new Exception();
            this.FillPattern.SetFillGrids((IList<FillGrid>)fillGrids);
            this.FillPattern.Target = patPattern.Target;
            if (patPattern.NeedExpandDots)
                this.FillPattern.ExpandDots();
            this.OnPropertyChanged("FillPattern");
            this.OnPropertyChanged("Target");
            this.Name = patPattern.Name;
            PatternEditorCommand.PatternListWindow.PatternControl.FillPatternViewerControl.Regenerate();
        }

        private void AcceptingNewCreationType(FillPatternCreationType newCreationType)
        {
            if (newCreationType == FillPatternCreationType.Custom)
            {
                if (!this.IsCreated)
                {
                    if (this._originFillPatternCreationType == FillPatternCreationType.Custom)
                        this.FillPattern.SetFillGrids(this.OriginFillPattern.GetFillGrids());
                    else
                        this.FillPattern.SetFillGrids((IList<FillGrid>)new List<FillGrid>());
                }
                this.SetPatternFormSelectedPat();
                this.HostOrientation = (FillPatternHostOrientation)0;
            }
            else if (!this.IsCreated)
            {
                if (this._originFillPatternCreationType == FillPatternCreationType.Custom)
                {
                    this.FillPattern.SetFillGrids((IList<FillGrid>)new List<FillGrid>()
          {
            new FillGrid(Utils.Utils.DegreeToRadian(45), Utils.Utils.MmToFt(3))
          });
                    this.HostOrientation = (FillPatternHostOrientation)0;
                }
                else
                {
                    this.FillPattern.SetFillGrids(this.OriginFillPattern.GetFillGrids());
                    this.HostOrientation = this.OriginFillPattern.HostOrientation;
                }
            }
            else
                this.AcceptingBasicCreationTypeEdit();
            this.OnPropertyChanged("FillPattern");
            PatternEditorCommand.PatternListWindow.PatternControl.FillPatternViewerControl.Regenerate();
        }

        // Applies edits to the fill pattern for the basic creation type
        private void AcceptingBasicCreationTypeEdit()
        {
            try
            {
                // Create fill grids based on the edited values
                var fillGridList = new List<FillGrid>
        {
            new FillGrid(Utils.Utils.DegreeToRadian(this._basicEditAngle),
                         Utils.Utils.MmToFt(this._basicEditFirstOffset))
        };

                if (this._basicEditIsEnableSecondOffset)
                {
                    fillGridList.Add(new FillGrid(Utils.Utils.DegreeToRadian(this._basicEditAngle + 90.0),
                                                  Utils.Utils.MmToFt(this._basicEditSecondOffset)));
                }

                // Set the updated grids to the fill pattern
                this.FillPattern.SetFillGrids(fillGridList);

                if (!this.IsCreated)
                {
                    this.DetectIsModified();
                }

                // Notify UI of changes
                OnPropertyChanged("FillPattern");
                PatternEditorCommand.PatternListWindow.PatternControl.FillPatternViewerControl.Regenerate();
            }
            catch (Exception ex)
            {
                //TODO
                //ExceptionBox.Show(ex);
            }
        }

        // Determines if the pattern has been modified based on creation state or differences from the original fill pattern
        private void DetectIsModified()
        {
            this.IsModified = this.IsCreated || this.IsDifferentToOriginFillPattern();
        }

        private void ReadValuesFromBasicCreationType(bool raisePropertyChanged)
        {
            try
            {
                // Retrieve and convert angle and offsets
                var firstGrid = this.FillPattern.GetFillGrid(0);
                this._basicEditAngle = Utils.Utils.RadianToDegree(firstGrid.Angle).RoundIt();
                this._basicEditFirstOffset = Utils.Utils.FtToMm(firstGrid.Offset).RoundIt();

                // Check if there's a second grid
                if (this.FillPattern.GridCount == 2)
                {
                    var secondGrid = this.FillPattern.GetFillGrid(1);
                    this._basicEditSecondOffset = Utils.Utils.FtToMm(secondGrid.Offset).RoundIt();
                    this._basicEditIsEnableSecondOffset = true;
                }
                else
                {
                    this._basicEditIsEnableSecondOffset = false;
                }

                // Notify UI if required
                if (raisePropertyChanged)
                {
                    OnPropertyChanged("BasicEditAngle");
                    OnPropertyChanged("BasicEditFirstOffset");
                    OnPropertyChanged("BasicEditSecondOffset");
                    OnPropertyChanged("BasicEditIsEnableSecondOffset");
                }
            }
            catch (Exception ex)
            {
                //TODO
                //ExceptionBox.Show(ex);
            }
        }

        private void ChangePatPatternsVisibilityBySearchString(string searchString)
        {
            if (!this.PatPatterns.Any<PatPattern>())
                return;
            if (string.IsNullOrEmpty(searchString))
            {
                foreach (PatPattern patPattern in (Collection<PatPattern>)this.PatPatterns)
                    patPattern.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                string upper = searchString.ToUpper();
                foreach (PatPattern patPattern in (Collection<PatPattern>)this.PatPatterns)
                {
                    int num = patPattern.Name.ToUpper().Contains(upper) || patPattern.Comment.ToUpper().Contains(upper) ? (patPattern.Target == this.Target ? 1 : 0) : 0;
                    patPattern.Visibility = num == 0 ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                }
            }
        }

        public void SetIsAllowableName(bool isAllowable, string message)
        {
            this.IsAllowableName = isAllowable;
            this.NotAllowableNameMessage = message;
        }


        private async void OnTargetChange()
        {
            //TODO
            // If Target is 0 (Drafting pattern)
            //if (this.Target == 0)
            //{
            //    // Show a confirmation message to the user
            //    MessageDialogResult messageDialogResult = await PatternsEditorCommand.MainWindow
            //        .ShowMessageAsync(Language.GetItem(this._langItem, "m1"), Language.GetItem(this._langItem, "m2"), MessageDialogStyle.AffirmativeAndNegative);

            //    if (messageDialogResult == MessageDialogResult.Affirmative)
            //    {
            //        // Handle the case for Basic creation type
            //        if (this.CreationType == FillPatternCreationType.Basic)
            //        {
            //            // Adjust the FillGrid size by dividing by 10, ensuring that the offset remains greater than a minimum value
            //            List<FillGrid> newFillGrids = CustomPatternUtils.ChangeFillGridSize(
            //                this.FillPattern.GetFillGrids(),
            //                x => x / 10.0,
            //                d => d > ModPlus_Revit.Utils.NumericExtensions.MmToFt(this.BasicEditOffsetMinimum));

            //            // If the new FillGrids are valid, update the FillPattern and notify property changes
            //            if (newFillGrids != null)
            //            {
            //                this.FillPattern.SetFillGrids(newFillGrids);
            //                this.OnPropertyChanged("FillPattern");
            //                this.ReadValuesFromBasicCreationType(true);
            //            }
            //            else
            //            {
            //                // If adjustment fails, show an error message
            //                await PatternsEditorCommand.MainWindow.ShowMessageAsync(Language.GetItem(this._langItem, "m3"), Language.GetItem(this._langItem, "m4"));
            //            }
            //        }
            //        else
            //        {
            //            // For Custom creation type, just adjust the FillGrid size
            //            this.FillPattern.SetFillGrids(CustomPatternUtils.ChangeFillGridSize(
            //                this.FillPattern.GetFillGrids(),
            //                x => x / 10.0));

            //            this.OnPropertyChanged("FillPattern");
            //        }
            //    }
            //}
            //// If Target is 1 (Model pattern)
            //else if (this.Target == 1)
            //{
            //    // Show a confirmation message for the model pattern
            //    MessageDialogResult messageDialogResult = await PatternsEditorCommand.MainWindow
            //        .ShowMessageAsync(Language.GetItem(this._langItem, "m5"), Language.GetItem(this._langItem, "m6"), MessageDialogStyle.AffirmativeAndNegative)
            //        .ConfigureAwait(true);

            //    if (messageDialogResult == MessageDialogResult.Affirmative)
            //    {
            //        // Handle the case for Basic creation type
            //        if (this.CreationType == FillPatternCreationType.Basic)
            //        {
            //            // Adjust the FillGrid size by multiplying by 10, ensuring the offset is less than the maximum allowed value
            //            List<FillGrid> newFillGrids = CustomPatternUtils.ChangeFillGridSize(
            //                this.FillPattern.GetFillGrids(),
            //                x => x * 10.0,
            //                d => d < ModPlus_Revit.Utils.NumericExtensions.MmToFt(this.BasicEditOffsetMaximum));

            //            if (newFillGrids != null)
            //            {
            //                this.FillPattern.SetFillGrids(newFillGrids);
            //                this.OnPropertyChanged("FillPattern");
            //                this.ReadValuesFromBasicCreationType(true);
            //            }
            //            else
            //            {
            //                // If adjustment fails, show an error message
            //                await PatternsEditorCommand.MainWindow.ShowMessageAsync(Language.GetItem(this._langItem, "m7"), Language.GetItem(this._langItem, "m8"));
            //            }
            //        }
            //        else
            //        {
            //            // For Custom creation type, adjust the FillGrid size by multiplying by 10
            //            this.FillPattern.SetFillGrids(CustomPatternUtils.ChangeFillGridSize(
            //                this.FillPattern.GetFillGrids(),
            //                x => x * 10.0));

            //            this.OnPropertyChanged("FillPattern");
            //        }
            //    }
            //}

            //// Regenerate the FillPattern view after making changes
            //PatternsEditorCommand.MainWindow.PatternControl.FillPatternViewerControl.Regenerate();

            //// Check if increasing or decreasing the scale is allowed
            //this.CheckIsAvailableIncreaseOrDecreaseScale();
        }


        private bool IsDifferentToOriginFillPattern()
        {
            // Compare name and creation type
            if (!string.Equals(this.Name, this.OriginFillPattern?.Name, StringComparison.CurrentCultureIgnoreCase) ||
                this.CreationType != this._originFillPatternCreationType)
            {
                return true;
            }

            // Compare target, orientation, and fill pattern equality
            var originTarget = this.OriginFillPattern?.Target;
            return this.Target != originTarget.GetValueOrDefault() ||
                   originTarget.HasValue == false ||
                   this.HostOrientation != this.OriginFillPattern.HostOrientation ||
                   !this.FillPattern.IsEqual(this.OriginFillPattern);
        }

        // Dispose method to clean up resources
        public void Dispose()
        {
            FillPattern?.Dispose();
            OriginFillPattern?.Dispose();
        }
    }
}
