using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Media;
using WaterTrans.TypeLoader;

namespace Tategaki.Logic
{
	/// <summary>
	/// GlyphConverterのキャッシュ
	/// </summary>
	internal class GlyphConverterCache
	{
		static KeyValuePair<(Uri uri, bool advanced), SingleGlyphConverter>? convCache = null;

		public static SingleGlyphConverter GetConverter(GlyphTypeface gtf, bool advanced = false)
		{
			var key = (gtf.FontUri, advanced);

			if(convCache == null || convCache.Value.Key != key) {
				int num = gtf.FontUri.Fragment == "" ? 0 : int.Parse(gtf.FontUri.Fragment.Replace("#", ""));
				var info = new TypefaceInfo(gtf.GetFontStream(), num);

				SingleGlyphConverter conv;
				if(advanced)
					conv = info.GetAdvancedVerticalGlyphConverter();
				else
					conv = info.GetVerticalGlyphConverter();

				convCache = new KeyValuePair<(Uri uri, bool advanced), SingleGlyphConverter>(key, conv);
			}

			return convCache.Value.Value;
		}
	}
}
