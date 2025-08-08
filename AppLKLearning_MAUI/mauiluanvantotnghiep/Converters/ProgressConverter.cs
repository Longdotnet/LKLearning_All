using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace mauiluanvantotnghiep.Converters
{
    public class ProgressConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length >= 2 &&
                values[0] is int currentIndex &&
                values[1] is int totalQuestions &&
                totalQuestions > 0)
            {
                // Tính tỷ lệ từ 0.0 đến 1.0
                double progress = (double)(currentIndex + 1) / totalQuestions;
                return progress;
            }
            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}