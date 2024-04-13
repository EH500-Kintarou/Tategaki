using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Globalization;

namespace TategakiSample.Views.ValueConverters
{
	[ValueConversion(typeof(bool), typeof(TextDecoration))]

	public class BooleanToUnderlineConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is bool val)
				return val ? new TextDecorationCollection() { TextDecorations.Underline } : new TextDecorationCollection();
			else
				return DependencyProperty.UnsetValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is TextDecorationCollection val)
				return val.Count > 0;
			else
				return false;
		}
	}
}
