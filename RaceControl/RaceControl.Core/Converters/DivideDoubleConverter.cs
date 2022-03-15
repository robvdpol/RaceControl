namespace RaceControl.Core.Converters;

[ValueConversion(typeof(double), typeof(double))]
public class DivideDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value != null)
        {
            var dblValue = System.Convert.ToDouble(value);

            if (parameter is double dblDivide)
            {
                return dblValue / dblDivide;
            }
        }

        return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}