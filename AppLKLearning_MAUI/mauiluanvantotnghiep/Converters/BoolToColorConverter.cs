using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace mauiluanvantotnghiep.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string colorParams)
            {
                var colors = colorParams.Split('|');
                if (colors.Length == 2)
                {
                    var trueColor = colors[0];
                    var falseColor = colors[1];
                    
                    return Color.FromArgb(boolValue ? trueColor : falseColor);
                }
            }
            
            // Fallback về logic cũ nếu không có parameter
            if (value is bool fallbackBool)
            {
                return fallbackBool ? Colors.Green : Colors.Gray;
            }
            
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
