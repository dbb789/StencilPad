using System.Globalization;
using System.Windows.Data;

namespace StencilPad.UI;

public class EnumBindingExtension : Binding
{
    private class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value,
                              Type targetType,
                              object parameter,
                              CultureInfo culture)
        {
            return value?.Equals(parameter) ?? false;
        }

        public object ConvertBack(object value,
                                  Type targetType,
                                  object parameter,
                                  CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : Binding.DoNothing;
        }
    }

    public EnumBindingExtension(string path) : base(path)
    {
        Converter = new EnumToBoolConverter();
        Mode = BindingMode.TwoWay;
    }

    public object TargetValue
    {
        get => ConverterParameter;
        set => ConverterParameter = value;
    }

}
