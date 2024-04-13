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
			if(value is  bool val)
				return val ? FontStyles.Italic : FontStyles.Normal;
			else
				return DependencyProperty.UnsetValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if(value is FontStyle val)
				return !val.Equals(FontStyles.Normal);
			else
				return false;
		}
	}
}