namespace AvalonDock.MVVMTestApp.Converters
{
    using System;
    using System.Windows.Data;

    public class ActiveDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is FileViewModel)
                return value;

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is FileViewModel)
                return value;

            return Binding.DoNothing;
        }
    }
}
