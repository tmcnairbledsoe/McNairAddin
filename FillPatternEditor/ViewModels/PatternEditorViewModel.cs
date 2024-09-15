using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.Exceptions;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;
using FillPatternEditor.Models;
using FillPatternEditor.Utils;
using FillPatternEditor.Commands;
using FillPatternEditor.Revit;
using FillPatternEditor.Views;
using FillPatternEditor.Dialogues;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.Input;
using FillPatternEditor.Enums;
using System.Reflection.Metadata;



namespace FillPatternEditor.ViewModels
{
    public class PatternEditorViewModel : ObservableObject, IDisposable
    {
        private bool _applyIsEnable;
        private CustomPattern _currentSelectedCustomPattern;
        private bool _isEnableMenu;
        private int _filterIndex;   
        private string _searchText;
        private readonly object _collLock = new object();
        public readonly PickCrosshairWindow PickCrosshairWindow;
        private Autodesk.Revit.DB.Document _currentDocument;
        private ListCollectionView _view;
        private ObservableCollection<CustomPattern> _patterns;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public PatternEditorViewModel()
        {
            // Initialize PickCrosshairWindow, set up the collection, and subscribe to Revit events
            PickCrosshairWindow = new PickCrosshairWindow();
            PickCrosshairWindow.Show();
            Patterns = new ObservableCollection<CustomPattern>();
            BindingOperations.EnableCollectionSynchronization(Patterns, _collLock);

            RevitInterop.UiApplication.Application.DocumentChanged += ApplicationOnDocumentChanged;
            RevitInterop.UiApplication.Application.DocumentClosed += ApplicationOnDocumentClosed;
            RevitInterop.UiApplication.ViewActivated += UiApplicationOnViewActivated;
        }

        // Handles when a view is activated in Revit, triggering pattern reload
        private void UiApplicationOnViewActivated(object sender, ViewActivatedEventArgs e)
        {
            if (!Equals(RevitInterop.Document, _currentDocument))
            {
                Patterns.Clear();
                LoadPatternsFromCurrentDocument();
            }
            IsEnableMenu = !RevitInterop.UiApplication.Application.Documents.IsEmpty;
        }

        // Handles when a document is closed
        private void ApplicationOnDocumentClosed(object sender, DocumentClosedEventArgs e)
        {
            if (RevitInterop.UiApplication.Application.Documents.IsEmpty)
            {
                IsEnableMenu = false;
                Patterns.Clear();
                _currentDocument = null;
            }
            else
            {
                IsEnableMenu = true;
            }
        }

        // Handles when a document is changed (elements added/deleted)
        private void ApplicationOnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            foreach (ElementId addedId in e.GetAddedElementIds())
            {
                FillPatternElement element = RevitInterop.Document.GetElement(addedId) as FillPatternElement;
                if (element != null)
                {
                    FillPattern pattern = element.GetFillPattern();
                    if (!pattern.IsSolidFill && Patterns.All(p => p.OriginalFillPatternElementId != element.Id))
                    {
                        Patterns.Add(new CustomPattern(pattern, null, element.Id));
                    }
                }
            }

            foreach (ElementId deletedId in e.GetDeletedElementIds())
            {
                CustomPattern pattern = Patterns.FirstOrDefault(p => p.OriginalFillPatternElementId == deletedId);
                if (pattern != null)
                {
                    Patterns.Remove(pattern);
                }
            }
        }

        // Observable collection of patterns
        public ObservableCollection<CustomPattern> Patterns
        {
            get => _patterns;
            set
            {
                _patterns = value;
                _view = new ListCollectionView(_patterns);
                OnPropertyChanged(nameof(Patterns));
            }
        }

        // The current view of the patterns collection
        public ICollectionView View => _view;

        // Property to enable/disable the menu
        public bool IsEnableMenu
        {
            get => _isEnableMenu;
            set
            {
                if (!Equals(value, _isEnableMenu))
                {
                    _isEnableMenu = value;
                    OnPropertyChanged(nameof(IsEnableMenu));
                }
            }
        }

        // Search text for filtering patterns
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (!Equals(value, _searchText))
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    View.Filter = Filter;
                }
            }
        }

        // Filter index for the type of patterns (e.g., Drafting, Model)
        public int FilterIndex
        {
            get => _filterIndex;
            set
            {
                if (!Equals(value, _filterIndex))
                {
                    _filterIndex = value;
                    OnPropertyChanged(nameof(FilterIndex));
                    View.Filter = Filter;
                }
            }
        }

        // Currently selected pattern in the UI
        public CustomPattern CurrentSelectedCustomPattern
        {
            get => _currentSelectedCustomPattern;
            set
            {
                if (!Equals(value, _currentSelectedCustomPattern))
                {
                    _currentSelectedCustomPattern = value;
                    OnPropertyChanged(nameof(CurrentSelectedCustomPattern));
                    OnPropertyChanged(nameof(CanDeleteCurrentSelectedPattern));
                }
            }
        }

        // Property indicating if the current selected pattern can be deleted
        public bool CanDeleteCurrentSelectedPattern => CurrentSelectedCustomPattern != null;

        // Whether to show preview icons in the UI
        public bool ShowPreviewIcons()
        {
            return true;
        }

        // Event handler for property changes in the CustomPattern object
        private void PatternOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Cast the sender to a CustomPattern
            CustomPattern pattern = sender as CustomPattern;

            // If the sender isn't a CustomPattern, exit the method
            if (pattern == null)
                return;

            bool hasInvalidName = false;

            // Check if the changed property is "Name"
            if (e.PropertyName == "Name")
            {
                // Get a list of patterns that have the same target but different GUIDs
                List<CustomPattern> similarPatterns = this.Patterns
                    .Where(p => p.Guid != pattern.Guid && p.Target == pattern.Target)
                    .ToList();

                // Check if the name is empty
                if (string.IsNullOrEmpty(pattern.Name))
                {
                    // Set the pattern as having an invalid name and show an appropriate message
                    pattern.SetIsAllowableName(false, "Choose a valid name");
                    hasInvalidName = true;
                }
                // Check if there are any patterns with the same name (case insensitive)
                else if (similarPatterns.Any(p => string.Equals(p.Name, pattern.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    // Set the pattern as having an invalid name and show a duplication warning
                    pattern.SetIsAllowableName(false, "Choose a valid name");
                    hasInvalidName = true;
                }
                else
                {
                    // If the name is valid, mark it as allowable
                    pattern.SetIsAllowableName(true, string.Empty);
                }
            }

            // Enable or disable the "Apply" button based on whether any pattern is modified and there are no invalid names
            this.ApplyIsEnable = !hasInvalidName && this.Patterns.Any(p => p.IsModified);
        }

        // Method to load patterns from the current Revit document
        public void LoadPatternsFromCurrentDocument()
        {
            IsEnableMenu = false;
            Patterns.CollectionChanged -= PatternsOnCollectionChanged;

            _currentDocument = RevitInterop.Document;
            var patternElements = new FilteredElementCollector(_currentDocument).OfClass(typeof(FillPatternElement)).Cast<FillPatternElement>().ToList();

            var patternsList = new List<CustomPattern>();
            var draftingPatternNames = new HashSet<string>();
            var modelPatternNames = new HashSet<string>();

            foreach (var element in patternElements)
            {
                FillPattern pattern = element.GetFillPattern();
                if (!pattern.IsSolidFill && pattern.IsValidObject)
                {
                    // Deduplicate pattern names between drafting and model patterns
                    if (pattern.Target == FillPatternTarget.Drafting && draftingPatternNames.Add(pattern.Name) ||
                        pattern.Target == FillPatternTarget.Model && modelPatternNames.Add(pattern.Name))
                    {
                        var customPattern = new CustomPattern(pattern, null, element.Id);
                        customPattern.PropertyChanged += PatternOnPropertyChanged;
                        patternsList.Add(customPattern);
                    }
                }
            }

            // Sort and add patterns to the collection
            patternsList.Sort((p1, p2) => string.Compare(p1.Name, p2.Name, StringComparison.Ordinal));
            foreach (var pattern in patternsList)
            {
                lock (_collLock)
                {
                    Patterns.Add(pattern);
                }
            }

            IsEnableMenu = true;
            Patterns.CollectionChanged += PatternsOnCollectionChanged;
        }

        // Event handler for pattern collection changes
        private void PatternsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                SortPatterns();
            }
        }

        // Sort the patterns alphabetically
        private void SortPatterns()
        {
            var sortedPatterns = Patterns.OrderBy(p => p.Name).ToList();
            Patterns.Clear();
            foreach (var pattern in sortedPatterns)
            {
                lock (_collLock)
                {
                    Patterns.Add(pattern);
                }
            }
        }

        // Property to determine if the "Apply" button should be enabled
        public bool ApplyIsEnable
        {
            get => _applyIsEnable;
            set
            {
                if (!Equals(value, _applyIsEnable))
                {
                    _applyIsEnable = value;
                    OnPropertyChanged(nameof(ApplyIsEnable));
                }
            }
        }

        public ICommand ApplyCommand => new CommunityToolkit.Mvvm.Input.RelayCommand(Apply);

        private void Apply()
        {
            string transactionName = "Fill Patterns";

            RevitInterop.RevitEvent.Run(() =>
            {
                try
                {
                    // Store the current selected pattern's name
                    string currentSelectedPatternName = this.CurrentSelectedCustomPattern?.Name;
                    this.CurrentSelectedCustomPattern = null;

                    // Update modified but not created patterns
                    foreach (var customPattern in this.Patterns.Where(p => p.IsModified && !p.IsCreated))
                    {
                        try
                        {
                            var existingPatternElement = FillPatternElement.GetFillPatternElementByName(RevitInterop.Document, customPattern.OriginFillPattern.Target, customPattern.OriginFillPattern.Name);
                            existingPatternElement.SetFillPattern(customPattern.FillPattern);
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show(ex.Message);
                        }
                    }

                    // Create new patterns
                    foreach (var customPattern in this.Patterns.Where(p => p.IsCreated))
                    {
                        try
                        {
                            FillPatternElement.Create(RevitInterop.Document, customPattern.FillPattern);
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show(ex.Message);
                        }
                    }

                    // Clear and reload patterns from the document
                    this.Patterns.Clear();
                    this.LoadPatternsFromCurrentDocument();

                    // Restore the selected pattern
                    if (!string.IsNullOrEmpty(currentSelectedPatternName))
                    {
                        this.CurrentSelectedCustomPattern = this.Patterns.FirstOrDefault(p => p.Name == currentSelectedPatternName);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }, false, transactionName: transactionName);
        }

        public ICommand CreateEmptyPatternCommand => new CommunityToolkit.Mvvm.Input.RelayCommand(CreateEmptyPattern);

        private void CreateEmptyPattern()
        {
            try
            {
                var customPattern = new CustomPattern(null, this.GetNewPatternName(), ElementId.InvalidElementId);
                this.AddNewPatternToCollectionWithSubscribe(customPattern);
                this.CurrentSelectedCustomPattern = this.Patterns.FirstOrDefault();
                this.PatternOnPropertyChanged(customPattern, new PropertyChangedEventArgs("Name"));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        public ICommand DuplicateSelectedPatternCommand => new CommunityToolkit.Mvvm.Input.RelayCommand(DuplicateSelectedPattern);

        private void DuplicateSelectedPattern()
        {
            try
            {
                var customPattern = new CustomPattern(this.CurrentSelectedCustomPattern, this.GetNewPatternName(this.CurrentSelectedCustomPattern.Name));
                this.AddNewPatternToCollectionWithSubscribe(customPattern, this.Patterns.IndexOf(this.CurrentSelectedCustomPattern) + 1);
                this.CurrentSelectedCustomPattern = this.Patterns.FirstOrDefault();
                this.PatternOnPropertyChanged(customPattern, new PropertyChangedEventArgs("Name"));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        public ICommand DeleteSelectedPatternCommand => new CommunityToolkit.Mvvm.Input.RelayCommand(DeleteSelectedPattern);

        private async void DeleteSelectedPattern()
        {
            var result = System.Windows.MessageBox.Show(PatternEditorCommand.PatternListWindow, "Are you sure?", "Delete: " + CurrentSelectedCustomPattern.Name, MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK) return;

            if (this.CurrentSelectedCustomPattern.IsCreated)
            {
                lock (this._collLock)
                {
                    this.Patterns.Remove(this.CurrentSelectedCustomPattern);
                }
            }
            else
            {
                string transactionName = string.Format($"Delete customPattern {this.CurrentSelectedCustomPattern.Name}");
                RevitInterop.RevitEvent.Run(() =>
                {
                    try
                    {
                        var patternElement = FillPatternElement.GetFillPatternElementByName(RevitInterop.Document, this.CurrentSelectedCustomPattern.OriginFillPattern.Target, this.CurrentSelectedCustomPattern.OriginFillPattern.Name);
                        RevitInterop.Document.Delete(patternElement.Id);

                        lock (this._collLock)
                        {
                            this.Patterns.Remove(this.CurrentSelectedCustomPattern);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                    }
                }, false, transactionName: transactionName);
            }

            this.ApplyIsEnable = this.Patterns.Any(p => p.IsModified);
        }

        public ICommand DeleteAllDraftingPatternsCommand => new CommunityToolkit.Mvvm.Input.RelayCommand(DeleteAllDraftingPatterns);

        private async void DeleteAllDraftingPatterns()
        {
            var result = System.Windows.MessageBox.Show(PatternEditorCommand.PatternListWindow,"Are you sure?","Delete All Drafting Patterns",MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK) return;

            string transactionName = "Delete All Drafting Patterns";
            RevitInterop.RevitEvent.Run(() =>
            {
                try
                {
                    this.DeleteAllPatterns((FillPatternTarget)0); // Drafting patterns
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }, false, transactionName: transactionName);
        }

        public ICommand DeleteAllModelPatternsCommand => new CommunityToolkit.Mvvm.Input.RelayCommand(DeleteAllModelPatterns);

        private async void DeleteAllModelPatterns()
        {
            var result = System.Windows.MessageBox.Show(PatternEditorCommand.PatternListWindow, "Are you sure?", "Delete All Model Patterns", MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK) return;

            string transactionName = "Delete All Model Patterns";
            RevitInterop.RevitEvent.Run(() =>
            {
                try
                {
                    this.DeleteAllPatterns((FillPatternTarget)1); // Model patterns
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }, false, transactionName: transactionName);
        }

        public ICommand DeleteSolidPatternDuplicatesCommand => new CommunityToolkit.Mvvm.Input.RelayCommand(DeleteSolidPatternDuplicates);

        private async void DeleteSolidPatternDuplicates()
        {
            var fillPatternElements = new FilteredElementCollector(RevitInterop.Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(FillPatternElement))
                .Cast<FillPatternElement>()
                .ToList();

            var solidFills = fillPatternElements
                .Where(e => e.GetFillPattern().IsSolidFill)
                .ToList();

            if (solidFills.Count == 1)
            {
                return;
            }
            else
            {
                var result = System.Windows.MessageBox.Show(PatternEditorCommand.PatternListWindow, "Are you sure?", "Delete All Duplicate Patterns", MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK) return;

                RevitInterop.RevitEventWithoutTransaction.Run(() =>
                {
                    try
                    {
                        var document = RevitInterop.Document;
                        var sortedSolidFills = solidFills.Select(f => f.Name).OrderBy(n => n.Length).FirstOrDefault();

                        string transactionName = "Fix Solid Fill Patterns";
                        using (var transactionGroup = new TransactionGroup(document, transactionName))
                        {
                            transactionGroup.Start();

                            foreach (var fillPatternElement in fillPatternElements)
                            {
                                try
                                {
                                    ElementId elementId = ElementId.InvalidElementId;
                                    using (var transaction = new Transaction(document, "Change Fill Pattern"))
                                    {
                                        transaction.Start();
                                        if (fillPatternElement.Name.Contains(sortedSolidFills) && !fillPatternElement.Name.Equals(sortedSolidFills))
                                        {
                                            fillPatternElement.SetFillPattern(new FillPattern("ToFix", FillPatternTarget.Drafting, FillPatternHostOrientation.ToView, 0.0, 0.5));
                                            elementId = fillPatternElement.Id;
                                        }
                                        transaction.Commit();
                                    }

                                    if (elementId != ElementId.InvalidElementId)
                                    {
                                        using (var transaction = new Transaction(document, "Delete Fill Pattern"))
                                        {
                                            transaction.Start();
                                            document.Delete(elementId);
                                            transaction.Commit();
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Windows.MessageBox.Show(ex.Message);
                                }
                            }

                            transactionGroup.Assimilate();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                    }
                }, false);
            }
        }


        public ICommand DeleteAllUnsavedPatternsFromListCommand
        {
            get => new CommunityToolkit.Mvvm.Input.RelayCommand(async () =>
            {
                try
                {
                    // Ask for confirmation before deleting unsaved patterns
                    var result = System.Windows.MessageBox.Show(PatternEditorCommand.PatternListWindow, "Are you sure?", "Delete All Unsaved Patterns", MessageBoxButton.OKCancel);
                    if (result != MessageBoxResult.OK) return;

                    // Remove unsaved patterns
                    for (int i = this.Patterns.Count - 1; i >= 0; --i)
                    {
                        if (this.Patterns[i].OriginFillPattern == null)
                        {
                            lock (this._collLock)
                            {
                                this.Patterns.RemoveAt(i);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            });
        }

        
        public ICommand DeleteMultiplePatternsCommand
        {
            get => new CommunityToolkit.Mvvm.Input.RelayCommand(async () =>
            {
                var result = System.Windows.MessageBox.Show(PatternEditorCommand.PatternListWindow, "Are you sure?", "Delete Patterns", MessageBoxButton.OKCancel);
                if (result != MessageBoxResult.OK) return;

                var selectPatternsViewModel = new SelectPatternsViewModel(async instance =>
                {
                    if (instance.IsCanceled) return;

                    var selectedPatterns = instance.Patterns
                        .Where(p => p.Visibility == System.Windows.Visibility.Visible && p.IsChecked)
                        .ToList();

                    if (!selectedPatterns.Any()) return;

                    var transactionName = "Delete All Checked Patterns";

                    RevitInterop.RevitEvent.Run(() =>
                    {
                        try
                        {
                            var elementIdsToDelete = selectedPatterns
                                .Select(p => p.Pattern)
                                .OfType<CustomPattern>()
                                .Where(p => p.OriginalFillPatternElementId != ElementId.InvalidElementId)
                                .Select(p => p.OriginalFillPatternElementId)
                                .ToArray();

                            RevitInterop.Document.Delete(elementIdsToDelete);

                            lock (this._collLock)
                            {
                                foreach (var customPattern in selectedPatterns.Select(p => p.Pattern).OfType<CustomPattern>())
                                {
                                    this.Patterns.Remove(customPattern);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show(ex.Message);
                        }
                    }, false, transactionName: transactionName);
                }, this.Patterns);

            });
        }

        public ICommand CreatePatternFromDraftingViewCommand
        {
            get => new CommunityToolkit.Mvvm.Input.RelayCommand(CreatePatternFromDraftingView);
        }

        private void CreatePatternFromDraftingView()
        {
            if (!(RevitInterop.Document.ActiveView is ViewDrafting))
            {
                System.Windows.MessageBox.Show("Please open a drafting view");
                return;
            }

            RevitInterop.RevitEventWithoutTransaction.Run(() =>
            {
                PatternEditorCommand.PatternListWindow.Hide();

                try
                {
                    PatternEditorViewModel.SetForegroundWindow(RevitInterop.UiApplication.MainWindowHandle);
                    this.PickCrosshairWindow.InitViewRectangle(RevitInterop.ActiveUiView?.GetWindowRectangle());
                    this.PickCrosshairWindow.StartShowCrosshair();

                    RemoveModelLinesEvent.RemoveLines(RevitInterop.Document);

                    var startPoint = RevitInterop.UiDocument.Selection.PickPoint(ObjectSnapTypes.Endpoints, "Select one corner for the boundary box");
                    this.PickCrosshairWindow.ClearCanvas();
                    this.PickCrosshairWindow.StartShowCrosshair();

                    var endPoint = RevitInterop.UiDocument.Selection.PickPoint(ObjectSnapTypes.Endpoints, "Select opposite corner for the boundary box");
                    this.PickCrosshairWindow.StopShowCrosshair();

                    var borderPoints = new XYZ[] { startPoint, endPoint };

                    var availablePatterns = this.Patterns
                        .Select(p => new Tuple<string, FillPatternTarget>(p.Name, p.Target))
                        .ToList();

                    var patFromElementsViewModel = new PatFromElementsViewModel(borderPoints, this.GetNewPatternName(), availablePatterns);

                    if (patFromElementsViewModel.IsValidBorderPoints(out var errorMessage))
                    {
                        var selectedElements = RevitInterop.UiDocument.Selection
                            .PickObjects(ObjectType.Element, new ObjectsForPatternSelectionFilter(ShowHelpRectangleEvent.ShowHelpRectangle(RevitInterop.Document, borderPoints)), "")
                            .Select(r => RevitInterop.Document.GetElement(r))
                            .ToList();

                        if (selectedElements.Any())
                        {
                            patFromElementsViewModel.SelectedElements = selectedElements;
                            var fromElementsWindow = new PatFromElementsWindow
                            {
                                DataContext = patFromElementsViewModel
                            };

                            fromElementsWindow.ContentRendered += (sender, args) => patFromElementsViewModel.UpdatePreviewFillPattern();

                            if (fromElementsWindow.ShowDialog().GetValueOrDefault())
                            {
                                this.CreateEmptyPattern();
                                this.CurrentSelectedCustomPattern.SetDataFromFillPattern(patFromElementsViewModel.PreviewPattern, patFromElementsViewModel.PatternName);
                            }
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("","",MessageBoxButton.OKCancel);
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show(errorMessage);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
                finally
                {
                    this.PickCrosshairWindow.StopShowCrosshair();
                    PatternEditorCommand.PatternListWindow.Show();
                    RemoveModelLinesEvent.RemoveLines(RevitInterop.Document);
                }
            }, false);
        }

        public ICommand CreatePatternFromPatFileCommand
        {
            get => new AsyncCommand(CreatePatternFromPatFile);
        }

        private async Task CreatePatternFromPatFile()
        {
            try
            {
                var openFileDialog = new System.Windows.Forms.OpenFileDialog
                {
                    Multiselect = false,
                    CheckFileExists = true,
                    Filter = "Hatch Patterns" + " (*.pat)|*.pat"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {

                    var customDialog = new CustomDialog((Window)PatternEditorCommand.PatternListWindow)
                    {
                        Title = "Create Pattern from .pat File"
                    };

                    var selectPatternViewModel = new SelectPatPatternDialogViewModel(instance =>
                    {
                        try
                        {
                            customDialog.Close();
                            if (!instance.IsCanceled && instance.SelectedPatPattern != null)
                            {
                                var customPattern = new CustomPattern(instance.SelectedPatPattern);
                                AddNewPatternToCollectionWithSubscribe(customPattern);
                                this.CurrentSelectedCustomPattern = this.Patterns.FirstOrDefault();
                                this.PatternOnPropertyChanged(customPattern, new PropertyChangedEventArgs("Name"));
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show(ex.Message);
                        }
                    }, PatPatternConverter.ReadPatPatternsFromFile(openFileDialog.FileName));

                    var selectPatPatternDialog = new SelectPatPatternDialog
                    {
                        DataContext = selectPatternViewModel
                    };
                    customDialog.Content = selectPatPatternDialog;
                    Window.GetWindow(customDialog).Show();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        public ICommand SelectPatFileForPatternCommand
        {
            get => new CommunityToolkit.Mvvm.Input.RelayCommand<CustomPattern>(SelectPatFileForPattern);
        }

        private void SelectPatFileForPattern(CustomPattern customPattern)
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Multiselect = false,
                CheckFileExists = true,
                Filter = "Hatch Pattern" + " (*.pat)|*.pat"
            };



            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (var patPattern in PatPatternConverter.ReadPatPatternsFromFile(openFileDialog.FileName))
                {
                    customPattern.PatPatterns.Add(patPattern);
                }
            }
        }

        public ICommand ImportPatternsFromPatFileCommand
        {
            get => new CommunityToolkit.Mvvm.Input.RelayCommand(ImportPatternsFromPatFile);
        }


        private void ImportPatternsFromPatFile()
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Multiselect = false,
                CheckFileExists = true,
                Filter = "Hatch Pattern" + " (*.pat)|*.pat"
            };


            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ImportPatternsFromPatFiles(new List<string> { openFileDialog.FileName });
            }
        }

        public ICommand ImportPatternsFromPatFilesInFolderCommand
        {
            get => new CommunityToolkit.Mvvm.Input.RelayCommand(ImportPatternsFromPatFilesInFolder);
        }

        private async void ImportPatternsFromPatFilesInFolder()
        {

            var folderBrowserDialog = new FolderBrowserDialog();


            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                bool includeSubfolders = false;

                if (Directory.GetDirectories(folderBrowserDialog.SelectedPath).Any())
                {
                    var result = System.Windows.MessageBox.Show("", "Include Subfolders?", MessageBoxButton.OKCancel);

                    includeSubfolders = result == MessageBoxResult.OK;
                }

                string[] files = Directory.GetFiles(
                    folderBrowserDialog.SelectedPath,
                    "*.pat",
                    includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

                if (files.Any())
                {
                    ImportPatternsFromPatFiles(files);
                }
            }
        }

        public ICommand ShowPatternPreviewCommand
        {
            get => new CommunityToolkit.Mvvm.Input.RelayCommand(ShowPatternPreview);
        }

        private void ShowPatternPreview()
        {

            Autodesk.Revit.DB.Document document = null;
            string tempPath = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".rvt");

            try
            {
                document = this._currentDocument.Application.OpenDocumentFile("E:\\Test\\mprPatterns\\PatternPreview_2020.rvt");
                var viewElement = new FilteredElementCollector(document)
                    .OfClass(typeof(View3D))
                    .FirstOrDefault<Element>();

                var fillPatternElement = FillPatternElement.Create(document, this.CurrentSelectedCustomPattern.FillPattern);
                var material = new FilteredElementCollector(document)
                    .OfClass(typeof(Material))
                    .Cast<Material>()
                    .FirstOrDefault(m => m.Name == "ShowPattern");

                material.SurfaceBackgroundPatternId = fillPatternElement.Id;
                material.SurfaceForegroundPatternId = fillPatternElement.Id;
                material.CutBackgroundPatternId = fillPatternElement.Id;
                material.CutForegroundPatternId = fillPatternElement.Id;

                document.SaveAs(tempPath);
                new PreviewWindow(document, viewElement.Id).ShowDialog();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            finally
            {
                document?.Close(false);

                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }

        public ICommand ShowSelectedPatternWithNotepadCommand
        {
            get => new AsyncCommand(ShowSelectedPatternWithNotepad);
        }

        private async Task ShowSelectedPatternWithNotepad()
        {
            if (this.CurrentSelectedCustomPattern == null)
                return;

            try
            {
                var customDialog = new CustomDialog((Window)PatternEditorCommand.PatternListWindow)
                {
                    Title = "Show Pattern in Notepad"
                };

                var selectPatUnitsViewModel = new SelectPatUnitsViewModel(instance =>
                {
                    try
                    {
                        customDialog.Close();
                        string patternString = instance.PatUnits.GetUnitsDirective() + Environment.NewLine + Environment.NewLine +
                                               this.CurrentSelectedCustomPattern.ToPatPattern(instance.PatUnits).GetStringRepresentation();

                        // Create a temporary file
                        string tempFile = Path.GetTempFileName();

                        // Write the string to the file
                        File.WriteAllText(tempFile, patternString);

                        // Open the file with Notepad
                        Process.Start("notepad.exe", tempFile);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                    }
                }, "Show Pattern in Notepad");

                var selectPatUnitsDialog = new SelectPatUnitsDialog
                {
                    DataContext = selectPatUnitsViewModel
                };

                customDialog.Content = selectPatUnitsDialog;
                Window.GetWindow(customDialog).Show();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        public ICommand ExportPatternsToPatFileCommand
        {
            get => new AsyncCommand(ExportPatternsToPatFile);
        }

        private async Task ExportPatternsToPatFile()
        {
            try
            {
                // Create a dialog to select patterns for export
                var selectPatternsDialog = new CustomDialog((Window)PatternEditorCommand.PatternListWindow)
                {
                    Title = "Export Patterns To .pat File"
                };

                var selectPatternsViewModel = new SelectPatternsViewModel(async (instance) =>
                {
                    try
                    {
                        // Close the pattern selection dialog
                        selectPatternsDialog.Close();

                        List<PatternForSelection> selectedPatterns = instance.Patterns.Where<PatternForSelection>((Func<PatternForSelection, bool>)(p => p.Visibility == System.Windows.Visibility.Visible && p.IsChecked)).ToList<PatternForSelection>();

                        // Return if no patterns were selected or if the user canceled
                        if (instance.IsCanceled || !instance.Patterns.Any(p => p.Visibility == System.Windows.Visibility.Visible && p.IsChecked))
                            return;

                        // Create a dialog to select the unit for exporting the patterns
                        var selectUnitsDialog = new CustomDialog((Window)PatternEditorCommand.PatternListWindow)
                        {
                            Title = "Select Unit"
                        };

                        var selectPatUnitsViewModel = new SelectPatUnitsViewModel(viewModel =>
                        {
                            try
                            {
                                // Close the unit selection dialog
                                selectUnitsDialog.Close();

                                // Open SaveFileDialog for saving the .pat file
                                var saveFileDialog = new System.Windows.Forms.SaveFileDialog
                                {
                                    OverwritePrompt = true,
                                    AddExtension = true,
                                    DefaultExt = "pat",
                                    FileName = "MyPatterns",
                                    Filter = "Hatch Patterns" + " (*.pat)|*.pat"
                                };

                                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                                {
                                    // Write selected patterns to the selected file
                                    PatPatternConverter.WriteStringRepresentationsToFile(saveFileDialog.FileName, selectedPatterns, viewModel.PatUnits);

                                    // Open the explorer to show the saved file
                                    Process.Start("explorer.exe", $"/select, \"{saveFileDialog.FileName}\"");
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Windows.MessageBox.Show(ex.Message);
                            }
                        }, "Select Units");

                        // Set the DataContext and show the unit selection dialog
                        selectUnitsDialog.Content = new SelectPatUnitsDialog { DataContext = selectPatUnitsViewModel };
                        Window.GetWindow( selectUnitsDialog ).Show();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show(ex.Message);
                    }
                }, this.Patterns);

                // Set the DataContext and show the pattern selection dialog
                selectPatternsDialog.Content = new SelectPatternsDialog { DataContext = selectPatternsViewModel };
                Window.GetWindow(selectPatternsDialog).Hide();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private bool Filter(object o)
        {
            if (o is CustomPattern customPattern)
            {
                // Handle empty search text
                if (string.IsNullOrWhiteSpace(this.SearchText))
                {
                    return FilterByIndex(customPattern);
                }
                else if (customPattern.Name.ToUpper().Contains(this.SearchText.ToUpper()))
                {
                    return FilterByIndex(customPattern);
                }
            }
            return false;
        }

        private bool FilterByIndex(CustomPattern customPattern)
        {
            // Filter patterns based on FilterIndex
            return this.FilterIndex switch
            {
                0 => true,
                1 => customPattern.Target == FillPatternTarget.Drafting,
                2 => customPattern.Target == FillPatternTarget.Model,
                _ => false
            };
        }

        private void ChangePatternVisibilityByFilterIndex(CustomPattern customPattern)
        {
            switch (this.FilterIndex)
            {
                case 0:
                    customPattern.Visibility = System.Windows.Visibility.Visible;
                    break;
                case 1:
                    customPattern.Visibility = customPattern.Target == FillPatternTarget.Drafting ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case 2:
                    customPattern.Visibility = customPattern.Target == FillPatternTarget.Model ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
            }

            // Unselect pattern if it's collapsed
            if (customPattern == this.CurrentSelectedCustomPattern && customPattern.Visibility == System.Windows.Visibility.Collapsed)
            {
                this.CurrentSelectedCustomPattern = null;
            }
        }

        private string GetNewPatternName(string oldName = null)
        {
            string baseName = string.IsNullOrEmpty(oldName) ? "" : oldName;
            string newName = baseName;
            List<string> existingNames = this.Patterns.Select(p => p.Name).ToList();
            int counter = 1;

            while (existingNames.Contains(newName))
            {
                newName = $"{baseName} {counter++}";
            }

            return newName;
        }

        private void DeleteAllPatterns(FillPatternTarget target)
        {
            // Find all patterns in Revit and delete them based on the target
            var elements = new FilteredElementCollector(RevitInterop.Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(FillPatternElement))
                .ToElements();

            if (!elements.Any())
                return;

            var idsToDelete = elements
                .Where(e => ((FillPatternElement)e).GetFillPattern().Target == target && !((FillPatternElement)e).GetFillPattern().IsSolidFill)
                .Select(e => e.Id)
                .ToList();

            if (idsToDelete.Any())
            {
                RevitInterop.Document.Delete(idsToDelete);

                // Remove matching patterns from the collection
                for (int i = this.Patterns.Count - 1; i >= 0; i--)
                {
                    var pattern = this.Patterns[i];
                    if (!pattern.IsCreated && !pattern.OriginFillPattern.IsSolidFill && pattern.OriginFillPattern.Target == target)
                    {
                        lock (this._collLock)
                        {
                            this.Patterns.RemoveAt(i);
                        }
                    }
                }
            }
        }

        private void ImportPatternsFromPatFiles(IEnumerable<string> fileNames)
        {
                try
                {
                    foreach (string fileName in fileNames)
                    {
                        List<PatPattern> patPatterns = PatPatternConverter.ReadPatPatternsFromFile(fileName).ToList();

                        for (int i = 0; i < patPatterns.Count; i++)
                        {
                            var patPattern = patPatterns[i];

                            var newPattern = new CustomPattern(patPattern);
                            AddNewPatternToCollectionWithSubscribe(newPattern);
                            PatternOnPropertyChanged(newPattern, new PropertyChangedEventArgs("Name"));
                        }
                    }

                    this.CurrentSelectedCustomPattern = this.Patterns.FirstOrDefault();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
        }

        private void AddNewPatternToCollectionWithSubscribe(CustomPattern customPattern, int index = 0)
        {
            customPattern.PropertyChanged += PatternOnPropertyChanged;

            lock (this._collLock)
            {
                this.Patterns.CollectionChanged -= PatternsOnCollectionChanged;
                this.Patterns.Insert(index, customPattern);
                this.Patterns.CollectionChanged += PatternsOnCollectionChanged;
            }
        }

        // Cleanup resources when the view model is disposed
        public void Dispose()
        {
            foreach (var pattern in Patterns)
            {
                pattern.Dispose();
            }
        }
    }
}
