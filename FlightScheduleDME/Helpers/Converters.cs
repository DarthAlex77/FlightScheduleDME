using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

namespace FlightScheduleDME.Helpers
{
    public class EnumConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return DependencyProperty.UnsetValue;
            }

            return GetDescription((Enum) value);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }

        public static string GetDescription(Enum en)
        {
            Type         type    = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo is
                {
                    Length: > 0
                })
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs is
                    {
                        Length: > 0
                    })
                {
                    return ((DescriptionAttribute) attrs[0]).Description;
                }
            }
            return en.ToString();
        }
    }

    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}