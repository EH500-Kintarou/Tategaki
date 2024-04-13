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
		readonly TypefaceInfo typeinfo;
		readonly VerticalFontInfo fontinfo;

		public FontGlyphCache(string? fontname, FontWeight weight, FontStyle style)
		{
			fontinfo = VerticalFontTable.FromName(fontname);
			var stylesim = ((weight == FontWeights.Normal) ? StyleSimulations.None : StyleSimulations.BoldSimulation) | ((style == FontStyles.Normal) ? StyleSimulations.None : StyleSimulations.ItalicSimulation);
			var gtf = new GlyphTypeface(fontinfo.FontUri, stylesim);

			int num = fontinfo.FontUri.Fragment == "" ? 0 : int.Parse(fontinfo.FontUri.Fragment.Replace("#", ""));
			typeinfo = new TypefaceInfo(gtf.GetFontStream(), num);

			FontName = fontinfo.OutstandingFamilyName;
			FontWeight = weight;
			FontStyle = style;
			GlyphTypeface = gtf;
		}

		public string FontName { get; }

		public FontWeight FontWeight { get; }

		public FontStyle FontStyle { get; }

		public GlyphTypeface GlyphTypeface { get; }

		public SingleGlyphConverter VerticalGlyphConverter
		{
			get
			{
				if(_VerticalGlyphConverter == null) {
					if(fontinfo.ConverterType.HasFlag(VerticalConverterType.Advanced))
						_VerticalGlyphConverter = typeinfo.GetAdvancedVerticalGlyphConverter();
					else if(fontinfo.ConverterType.HasFlag(VerticalConverterType.Normal))
						_VerticalGlyphConverter = typeinfo.GetVerticalGlyphConverter();
					else
						throw new InvalidOperationException("No font converter found");
				}
				return _VerticalGlyphConverter;
			}
		}
		SingleGlyphConverter? _VerticalGlyphConverter = null;

		public bool ParamEquals(string? fontname, FontWeight weight, FontStyle style)
		{
			var uri = VerticalFontTable.FromName(fontname);

			return FontName == uri.OutstandingFamilyName && FontWeight == weight && FontStyle == style;
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
