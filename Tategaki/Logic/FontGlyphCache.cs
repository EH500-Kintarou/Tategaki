using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using Tategaki.Logic.Font;
using Tategaki.Logic.Font.Tables.GsubGpos;
using System.Collections.ObjectModel;

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

			var subst = new Dictionary<ushort, ushort>();
			foreach(var table in (otf.Gsub?.GetSubstitution<SingleSubstitution>("vert") ?? Enumerable.Empty<SingleSubstitution>()).SelectMany(p => p.Table))
				subst[table.Key] = table.Value;
			VerticalSubstitution = new ReadOnlyDictionary<ushort, ushort>(subst);
		}

		public FontNameInfo FontName { get; }

		public FontWeight FontWeight { get; }

		public FontStyle FontStyle { get; }

		public FontStretch FontStretch { get; }

		public GlyphTypeface GlyphTypeface { get; }

		public IReadOnlyDictionary<ushort, ushort> VerticalSubstitution { get; }

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
