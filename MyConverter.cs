using System;
using Avalonia.Data.Converters;
//using Avalonia.Data;
using System.Globalization;

namespace MAMEIronXP
{
    public class MyConverter : IValueConverter
    {
        public static readonly MyConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (boolValue == true)
                {
                    return boolValue ? "1" : "";
                }
            }

            throw new InvalidOperationException("Value must be a boolean");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}