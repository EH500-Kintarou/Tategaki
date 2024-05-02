using System.Windows;
using System.Windows.Data;

namespace Tategaki.ValueConverters
{
	[Obsolete]
	[ValueConversion(typeof(double), typeof(Thickness))]
	public class LineMarginConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			double margin = (double)value / 2;
			return new Thickness(margin, 0, margin, 0);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new InvalidOperationException();
		}
	}
}
