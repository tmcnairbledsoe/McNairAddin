using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using FillPatternEditor.Models;
using FillPatternEditor.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FillPatternEditor.Dialogues
{
    public class SelectPatternsViewModel : ObservableObject
    {
        private int _filterIndex;

        // Constructor for initializing the view model with close handler and list of patterns
        public SelectPatternsViewModel(Action<SelectPatternsViewModel> closeHandler, IEnumerable<CustomPattern> patterns)
        {
            // Command for accepting the selection
            this.AcceptCommand = new RelayCommand(() =>
            {
                this.IsCanceled = false;
                closeHandler(this); // Close the dialog with the current state
            });

            // Command for canceling the selection
            this.CancelCommand = new RelayCommand(() =>
            {
                this.IsCanceled = true;
                closeHandler(this); // Close the dialog, indicating cancellation
            });

            // Initialize the collection of patterns
            this.Patterns = new ObservableCollection<PatternForSelection>();

            // Add each pattern to the collection
            foreach (IPattern pattern in patterns)
            {
                this.Patterns.Add(new PatternForSelection(pattern));
            }
        }

        // Property to track if the dialog was canceled
        public bool IsCanceled { get; private set; }

        // Command to accept the current selection
        public ICommand AcceptCommand { get; }

        // Command to cancel the selection
        public ICommand CancelCommand { get; }

        // Command to check all visible patterns
        public ICommand CheckAllCommand => new RelayCommand(() =>
        {
            foreach (PatternForSelection pattern in this.Patterns)
            {
                if (pattern.Visibility == System.Windows.Visibility.Visible)
                {
                    pattern.IsChecked = true; // Check all visible patterns
                }
            }
        });

        // Command to uncheck all visible patterns
        public ICommand UncheckAllCommand => new RelayCommand(() =>
        {
            foreach (PatternForSelection pattern in this.Patterns)
            {
                if (pattern.Visibility == System.Windows.Visibility.Visible)
                {
                    pattern.IsChecked = false; // Uncheck all visible patterns
                }
            }
        });

        // Collection of patterns available for selection
        public ObservableCollection<PatternForSelection> Patterns { get; set; }

        // Property to filter the patterns by index (0: All, 1: Drafting, 2: Model)
        public int FilterIndex
        {
            get => _filterIndex;
            set
            {
                if (_filterIndex != value)
                {
                    _filterIndex = value;
                    OnPropertyChanged(nameof(FilterIndex));
                    ChangePatternsVisibility(); // Update pattern visibility based on the filter
                }
            }
        }

        // Method to change visibility of patterns based on the current filter index
        private void ChangePatternsVisibility()
        {
            foreach (PatternForSelection pattern in this.Patterns)
            {
                ChangePatternVisibilityByFilterIndex(pattern);
            }
        }

        // Method to set the visibility of a specific pattern based on the filter index
        private void ChangePatternVisibilityByFilterIndex(PatternForSelection pattern)
        {
            switch (FilterIndex)
            {
                case 0: // Show all patterns
                    pattern.Visibility = System.Windows.Visibility.Visible;
                    break;
                case 1: // Show only drafting patterns
                    pattern.Visibility = pattern.Target == FillPatternTarget.Drafting ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
                case 2: // Show only model patterns
                    pattern.Visibility = pattern.Target == FillPatternTarget.Model ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
                    break;
            }
        }
    }
}
