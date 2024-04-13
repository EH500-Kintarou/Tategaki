using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace TategakiSample.Views.ValueConverters
{
	[ValueConversion(typeof(bool), typeof(FontWeight))]
	public class BooleanToFontWeightConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if(value is bool val)
				return val ? FontWeights.Bold : FontWeights.Normal;
			else
				return DependencyProperty.UnsetValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if(value is FontWeight val)
				return !val.Equals(FontWeights.Normal);
			else
				return false;
		}
	}
}