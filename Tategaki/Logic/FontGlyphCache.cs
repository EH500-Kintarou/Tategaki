using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Tategaki.Logic.Font;
using Tategaki.Logic.Font.Tables.Glyph;
using Tategaki.Logic.Font.Tables.GsubGpos;
using Tategaki.Logic.Font.Tables.Metrix;

namespace Tategaki.Logic
{
	internal class FontGlyphCache : IEquatable<FontGlyphCache>
	{
		readonly OpenTypeFont otf;

		public FontGlyphCache(FontNameInfo fontname, FontWeight weight, FontStyle style, FontStretch stretch)
		{
			var tf = new Typeface(new FontFamily(fontname.InvaliantFamilyName), style, weight, stretch);
			if(!tf.TryGetGlyphTypeface(out var gtf))
				throw new NotSupportedException("Cannot load GlyphTypeface");

			otf = new OpenTypeFont(gtf.FontUri);

			FontName = fontname;
			FontWeight = weight;
			FontStyle = style;
			FontStretch = stretch;
			GlyphTypeface = gtf;

			var vert = new Dictionary<ushort, ushort>();
			foreach(var table in (otf.Gsub?.GetSubstitution<SingleSubstitution>("vert") ?? Enumerable.Empty<SingleSubstitution>()).SelectMany(p => p.Table))
				vert[table.Key] = table.Value;
			VerticalSubstitution = new ReadOnlyDictionary<ushort, ushort>(vert);

			var vpal = new Dictionary<ushort, GlyphAdjustment>();
			foreach(var table in (otf.Gpos?.GetPositioning<SingleAdjustment>("vpal") ?? Enumerable.Empty<SingleAdjustment>()).SelectMany(p => p.Table))
				vpal[table.Key] = new GlyphAdjustment() {
					XPlacement = (double)table.Value.XPlacement / otf.Head.UnitsPerEm,
					YPlacement = (double)table.Value.YPlacement / otf.Head.UnitsPerEm,
					XAdvance = (double)table.Value.XAdvance / otf.Head.UnitsPerEm,
					YAdvance = (double)table.Value.YAdvance / otf.Head.UnitsPerEm,
				};
			VerticalProportionalAdjustment = new ReadOnlyDictionary<ushort, GlyphAdjustment>(vpal);

			GlyphMetrics = new ReadOnlyDictionary<ushort, GlyphMetrics>((otf.Glyf?.Glyphs ?? Enumerable.Empty<GlyphData?>())
				.Select((gd, index) => (gd, index))
				.Where(p => p.gd != null)
				.ToDictionary(
					p => (ushort)p.index,
					p => new GlyphMetrics((double)p.gd!.XMin / otf.Head.UnitsPerEm, (double)p.gd.YMin / otf.Head.UnitsPerEm, (double)p.gd.XMax / otf.Head.UnitsPerEm, (double)p.gd.YMax / otf.Head.UnitsPerEm)));
		}

		public FontNameInfo FontName { get; }

		public FontWeight FontWeight { get; }

		public FontStyle FontStyle { get; }

		public FontStretch FontStretch { get; }

		public GlyphTypeface GlyphTypeface { get; }

		public IReadOnlyDictionary<ushort, ushort> VerticalSubstitution { get; }

		public IReadOnlyDictionary<ushort, GlyphAdjustment> VerticalProportionalAdjustment { get; }

		public IReadOnlyDictionary<ushort, GlyphMetrics> GlyphMetrics { get; }

		public bool Equals(FontGlyphCache? other)
		{
			if(other == null)
				return false;

			return ParamEquals(other.FontName, other.FontWeight, other.FontStyle, other.FontStretch);
		}

		public bool ParamEquals(FontNameInfo fontname, FontWeight weight, FontStyle style, FontStretch stretch)
		{
			return FontName.Equals(fontname) && FontWeight.Equals(weight) && FontStyle.Equals(style) && FontStretch.Equals(stretch);
		}

		#region GlobalCache

		static FontGlyphCache? cache = null;

		public static FontGlyphCache GetCache(FontNameInfo fontname, FontWeight weight, FontStyle style, FontStretch stretch)
		{
			if(cache == null || !cache.ParamEquals(fontname, weight, style, stretch))
				cache = new FontGlyphCache(fontname, weight, style, stretch);

			return cache;
		}

		#endregion
	}
}
