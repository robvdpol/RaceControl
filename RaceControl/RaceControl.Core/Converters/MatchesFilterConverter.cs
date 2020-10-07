using System;
using System.Globalization;
using System.Windows.Data;

namespace RaceControl.Core.Converters
{
    public class MatchesFilterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var item = values[0];
            var text = values[1] as string;

            if (item == null || string.IsNullOrEmpty(text))
            {
                return true;
            }

            return item.ToString().Contains(text, StringComparison.OrdinalIgnoreCase);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}