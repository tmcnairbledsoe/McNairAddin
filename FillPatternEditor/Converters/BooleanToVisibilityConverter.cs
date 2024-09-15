using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace FillPatternEditor.Converters
{
    /// <summary>
    /// Converts a boolean value to a Visibility value.
    /// True will result in Visible, False will result in Collapsed or Hidden based on the converter parameters.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the visibility value when the boolean is true. Defaults to Visible.
        /// </summary>
        public Visibility TrueVisibility { get; set; } = Visibility.Visible;

        /// <summary>
        /// Gets or sets the visibility value when the boolean is false. Defaults to Collapsed.
        /// </summary>
        public Visibility FalseVisibility { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueVisibility : FalseVisibility;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == TrueVisibility;
            }

            return DependencyProperty.UnsetValue;
        }
    }
}
