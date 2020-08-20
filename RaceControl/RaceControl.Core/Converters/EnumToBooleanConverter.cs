using System;
using System.Globalization;
using System.Windows.Data;

namespace RaceControl.Core.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(bool) && targetType != typeof(bool?))
            {
                throw new InvalidOperationException("The target must be a boolean");
            }

            if (value is Enum enumValue && parameter is Enum enumParameter)
            {
                return enumValue.Equals(enumParameter);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}