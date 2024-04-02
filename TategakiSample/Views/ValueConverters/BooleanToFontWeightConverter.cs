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
			return (bool)value ? FontWeights.Bold : FontWeights.Normal;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return (FontWeight)value != FontWeights.Normal;
		}
	}
}