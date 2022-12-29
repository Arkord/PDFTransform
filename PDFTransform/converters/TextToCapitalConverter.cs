using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PDFTransform.Converters
{
    internal class TextToCapitalConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            TextInfo textInfo = culture.TextInfo;
            string titleCase = textInfo.ToTitleCase(value.ToString().ToLower());

            return new Avalonia.Data.BindingNotification(titleCase);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return new Avalonia.Data.BindingNotification(value);
        }
    }
}
