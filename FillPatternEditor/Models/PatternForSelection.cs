using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FillPatternEditor.Models
{
    // The class represents a pattern with properties used for selection in the UI.
    public class PatternForSelection : ObservableObject, IPattern
    {
        private bool _isChecked;
        private System.Windows.Visibility _visibility = System.Windows.Visibility.Visible;

        // Constructor to initialize the pattern with its relevant properties.
        public PatternForSelection(IPattern pattern)
        {
            // Initialize pattern properties from the given IPattern object
            this.Pattern = pattern;
            this.FillPattern = pattern.FillPattern;
            this.Name = pattern.Name;
            this.Target = pattern.Target;
        }

        // Property to store the underlying pattern object.
        public IPattern Pattern { get; }

        // Property to check if the pattern is selected in the UI.
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked)); // Notify when the property changes
                }
            }
        }

        // Property to control the visibility of the pattern in the UI.
        public System.Windows.Visibility Visibility
        {
            get => _visibility;
            set
            {
                if (_visibility != value)
                {
                    _visibility = value;
                    OnPropertyChanged(nameof(Visibility)); // Notify when the property changes
                }
            }
        }

        // Property for the name of the pattern.
        public string Name { get; set; }

        // Property for the FillPattern associated with the pattern.
        public FillPattern FillPattern { get; set; }

        // Property to define whether the pattern is a Drafting or Model pattern.
        public FillPatternTarget Target { get; set; }
    }
}
