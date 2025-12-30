using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace GoldenStandard.Converters
{
    public class QualityColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double percentage)
            {
                if (percentage < 45)
                    return Brushes.Red;
                if (percentage <= 75)
                    return Brushes.Gold;

                return Brushes.Green;
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}