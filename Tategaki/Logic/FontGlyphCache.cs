using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using WaterTrans.TypeLoader;

namespace Tategaki.Logic
{
	internal class FontGlyphCache
	{
		readonly TypefaceInfo info;

		public FontGlyphCache(string? fontname, FontWeight weight, FontStyle style)
		{
			var uri = FontUriTable.FromName(fontname);
			fontname ??= FontUriTable.AllVerticalFonts.Where(p => p.Value == uri).First().Key;
			var gtf = new GlyphTypeface(uri, ((weight == FontWeights.Normal) ? StyleSimulations.None : StyleSimulations.BoldSimulation) | ((style == FontStyles.Normal) ? StyleSimulations.None : StyleSimulations.ItalicSimulation));

			int num = uri.Fragment == "" ? 0 : int.Parse(uri.Fragment.Replace("#", ""));
			info = new TypefaceInfo(gtf.GetFontStream(), num);

			FontName = fontname;
			FontWeight = weight;
			FontStyle = style;
			GlyphTypeface = gtf;
		}

		public string FontName { get; }

		public FontWeight FontWeight { get; }

		public FontStyle FontStyle { get; }

		public GlyphTypeface GlyphTypeface { get; }

		public SingleGlyphConverter AdvancedVerticalGlyphConverter
		{
			get
			{
				if(_AdvancedVerticalGlyphConverter == null)
					_AdvancedVerticalGlyphConverter = info.GetAdvancedVerticalGlyphConverter();
				return _AdvancedVerticalGlyphConverter;
			}
		}
		SingleGlyphConverter? _AdvancedVerticalGlyphConverter = null;

		public SingleGlyphConverter VerticalGlyphConverter
		{
			get
			{
				if(_VerticalGlyphConverter == null)
					_VerticalGlyphConverter = info.GetVerticalGlyphConverter();
				return _VerticalGlyphConverter;
			}
		}
		SingleGlyphConverter? _VerticalGlyphConverter = null;

		public bool ParamEquals(string? fontname, FontWeight weight, FontStyle style)
		{
			if(fontname == null) {
				var uri = FontUriTable.FromName(fontname);
				fontname ??= FontUriTable.AllVerticalFonts.Where(p => p.Value == uri).First().Key;
			}

			return FontName == fontname && FontWeight == weight && FontStyle == style;
		}

		#region GlobalCache

		static FontGlyphCache? cache = null;

		public static FontGlyphCache GetCache(string? fontname, FontWeight weight, FontStyle style)
		{
			if(cache == null || !cache.ParamEquals(fontname, weight, style))
				cache = new FontGlyphCache(fontname, weight, style);

			return cache;
		}

		#endregion
	}
}
