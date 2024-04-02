using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WaterTrans.TypeLoader;

namespace Tategaki.Logic
{
	internal class GlyphConverters
	{
		public GlyphConverters(Uri FontUri)
		{
			GlyphTypeface = new GlyphTypeface(FontUri);
			TypefaceInfo ti = new TypefaceInfo(GlyphTypeface.GetFontStream(), string.IsNullOrEmpty(FontUri.Fragment) ? 0 : int.Parse(FontUri.Fragment.Replace("#", "")));
			VerticalConverter = ti.GetVerticalGlyphConverter();
		}

		public GlyphTypeface GlyphTypeface { get; private set; }
		public SingleGlyphConverter VerticalConverter { get; private set; }
	}
}
