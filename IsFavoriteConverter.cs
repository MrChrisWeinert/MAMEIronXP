using System;
using Avalonia.Data.Converters;
using System.Globalization;

namespace MAMEIronXP
{
    public class IsFavoriteConverter : IValueConverter
    {
        public static readonly IsFavoriteConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (boolValue == true)
                {
                    //Display the PacMan icon if the game is marked as a Favorite.
                    //When using the Pac-Font font, a "1" corresponds to the PacMan icon.
                    return "1";
                }
                //Otherewise, don't display the icon.
                else return " ";
            }

            throw new InvalidOperationException("Value must be a boolean");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}