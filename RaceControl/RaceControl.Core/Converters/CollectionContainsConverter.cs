using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace RaceControl.Core.Converters
{
    public class CollectionContainsConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is string identifier && values[1] is ICollection collection && parameter is string identifierName)
            {
                var list = collection.Cast<object>().ToList();

                if (list.Any())
                {
                    var property = list.First().GetType().GetProperty(identifierName);

                    if (property != null)
                    {
                        return list.Any(item => (string)property.GetValue(item) == identifier);
                    }
                }
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}