using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace mauiluanvantotnghiep.Converters
{
    public class StarConverter : IValueConverter
    {
        // input: int hoặc double 0–5, output: chuỗi ★/☆
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double ratingValue = 0;

            // Lấy số sao từ value
            if (value is int intRating)
                ratingValue = intRating;
            else if (value is double dblRating)
                ratingValue = dblRating;
            else
                return "☆☆☆☆☆";

            // Lấy phần nguyên (nếu muốn làm tròn: Math.Round)
            int fullStars = (int)Math.Floor(ratingValue);

            var stars = new System.Text.StringBuilder(5);
            for (int idx = 1; idx <= 5; idx++)
            {
                stars.Append(idx <= fullStars ? "★" : "☆");
            }

            return stars.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
