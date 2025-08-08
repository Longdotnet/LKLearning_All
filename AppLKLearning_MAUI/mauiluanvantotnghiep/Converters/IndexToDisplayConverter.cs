using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace mauiluanvantotnghiep.Converters
{
    public class IndexToDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return $"Câu {index + 1}"; // Trực tiếp format luôn
            }
            return "Câu 1";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
