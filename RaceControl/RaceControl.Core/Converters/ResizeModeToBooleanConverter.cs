using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RaceControl.Core.Converters
{
    [ValueConversion(typeof(ResizeMode), typeof(bool))]
    public class ResizeModeToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool) && targetType != typeof(bool?))
            {
                throw new InvalidOperationException("The target must be a boolean");
            }

            if (value is ResizeMode resizeMode)
            {
                return resizeMode == ResizeMode.CanResize;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(ResizeMode))
            {
                throw new InvalidOperationException("The target must be a ResizeMode");
            }

            if (value is bool boolValue)
            {
                return boolValue ? ResizeMode.CanResize : ResizeMode.NoResize;
            }

            return null;
        }
    }
}