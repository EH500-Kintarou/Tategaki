using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Globalization;
using System.Collections.Specialized;

namespace TategakiSample.Views.ValueConverters
{
	[ValueConversion(typeof(string), typeof(TextDecorations))]
	public class SelectedValueToTextDecorationsConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(value is string val) {
				var ret = new TextDecorationCollection();

				if(val.Contains("OverLine"))
					ret.Add(new TextDecoration() { Location = TextDecorationLocation.OverLine });
				if(val.Contains("Strikethrough"))
					ret.Add(new TextDecoration() { Location = TextDecorationLocation.Strikethrough });
				if(val.Contains("Baseline"))
					ret.Add(new TextDecoration() { Location = TextDecorationLocation.Baseline });
				if(val.Contains("Underline"))
					ret.Add(new TextDecoration() { Location = TextDecorationLocation.Underline });

				return ret;
			} else
				return DependencyProperty.UnsetValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
