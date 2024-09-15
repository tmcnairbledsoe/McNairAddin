using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FillPatternEditor.Converters
{
    public class BooleanToVisibilityMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object obj in values)
            {
                if (obj is bool flag && !flag)
                    return (object)Visibility.Collapsed;
            }
            return (object)Visibility.Visible;
        }

        public object[] ConvertBack(
          object value,
          Type[] targetTypes,
          object parameter,
          CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
