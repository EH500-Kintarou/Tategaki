using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace TategakiSample.Views.ValueConverters
{
	[ValueConversion(typeof(bool), typeof(FontStyle))]
	public class BooleanToFontStyleConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return (bool)value ? FontStyles.Italic : FontStyles.Normal;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return (FontStyle)value != FontStyles.Normal;
		}
	}
}