using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace mauiluanvantotnghiep.Converters
{
    public class MessageVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length >= 2 && 
                values[0] is int currentUserId && 
                values[1] is int senderId)
            {
                bool isCurrentUser = currentUserId == senderId;
                
                // parameter sẽ là "user" hoặc "admin" 
                string messageType = parameter?.ToString();
                
                if (messageType == "user")
                    return isCurrentUser;
                else if (messageType == "admin")
                    return !isCurrentUser;
            }
            
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
