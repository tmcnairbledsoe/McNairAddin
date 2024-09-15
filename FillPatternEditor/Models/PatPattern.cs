using Autodesk.Revit.DB;
using FillPatternEditor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FillPatternEditor.Models
{
    // Class representing a pattern used in Revit
    public class PatPattern : ObservableObject
    {
        // Private backing field for Visibility property
        private System.Windows.Visibility _visibility;

        // Property to get and set the visibility of the pattern
        public System.Windows.Visibility Visibility
        {
            get => _visibility;
            set
            {
                // Only set if the new value is different from the current one
                if (!Equals(value, _visibility))
                {
                    _visibility = value;
                    // Notify listeners that the property has changed
                    OnPropertyChanged(nameof(Visibility));
                }
            }
        }

        // The name of the pattern (defaulting to an empty string)
        public string Name { get; set; } = string.Empty;

        // Additional comments about the pattern (defaulting to an empty string)
        public string Comment { get; set; } = string.Empty;

        // Units used for the pattern, defaulting to inches (from the PatUnits enum)
        public PatUnits PatUnits { get; set; } = PatUnits.INCH;

        // The target type for the fill pattern, defaulting to 0 (probably an unspecified target)
        public FillPatternTarget Target { get; set; } = FillPatternTarget.Model;

        // List of line families used in the pattern
        public List<PatLineFamily> LineFamilies { get; set; } = new List<PatLineFamily>();

        // Boolean property indicating if any line family needs to expand its dots
        public bool NeedExpandDots
        {
            get
            {
                // Check if any of the PatLineFamily objects in the list require expanded dots
                return LineFamilies.Any(f => f.NeedExpandDots);
            }
        }
    }
}
