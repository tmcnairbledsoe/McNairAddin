using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace FillPatternEditor.Converters
{
    public class BooleanToBrushConverter : DependencyObject, IValueConverter
    {
        public Brush TrueBrush { get; set; } = (Brush)Brushes.Black;

        public Brush FalseBrush { get; set; } = (Brush)Brushes.Black;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool flag ? (flag ? (object)this.TrueBrush : (object)this.FalseBrush) : (object)false;
        }

        public object ConvertBack(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
