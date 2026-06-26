using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ConfigEditor.App.Theme;

/// <summary>
/// Converts string values to boolean for CheckBox interaction and back.
/// </summary>
public class StringToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            if (bool.TryParse(str, out bool b))
                return b;
            
            if (str.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                str.Equals("yes", StringComparison.OrdinalIgnoreCase) ||
                str.Equals("on", StringComparison.OrdinalIgnoreCase) ||
                str.Equals("1"))
            {
                return true;
            }
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? "true" : "false";
        }
        return "false";
    }
}

/// <summary>
/// Converts a boolean value to Visibility.
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool invert = parameter is string str && str.Equals("Invert", StringComparison.OrdinalIgnoreCase);
        if (value is bool b)
        {
            if (invert)
                b = !b;

            return b ? Visibility.Visible : Visibility.Collapsed;
        }
        return invert ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Visibility v && v == Visibility.Visible;
    }
}
