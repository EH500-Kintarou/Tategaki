using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Tategaki.Logic.Font;
using Tategaki.Logic.Font.Tables.Base;
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
				vpal[table.Key] = new GlyphAdjustment()
				{
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

			// 日本語フォントには、だいたい以下の2種類がある
			//  - Baseline Tagにideoのみを含む（ＭＳ 明朝など）
			//  - Baseline Tagにicfb, icft, ideo, romnを含む（游ゴシックなど）
			if(otf.Base == null) {  // BASEテーブルを持っていない / ideo Tagが無い
				VerticalDecorationMetrics = new DecorationMetrics(1, 0.5, 0, 0);    // 決め打ちする
			} else {
				var baseScriptPriority = new[] { "kana", "hani", "DFLT" };
				BaseScript? script = null;
				foreach(var tag in baseScriptPriority) {
					if(otf.Base.Vert.BaseScriptRecords.TryGetValue(tag, out script))
						break;
				}
				if(script == null && otf.Base.Vert.BaseScriptRecords.Count > 0)
					script = otf.Base.Vert.BaseScriptRecords.Values.First();

				if(script == null)
					VerticalDecorationMetrics = new DecorationMetrics(1, 0.5, 0, 0);    // 決め打ちする
				else {
					if(otf.Base.Vert.BaselineTags.Contains("icfb") &&
					   otf.Base.Vert.BaselineTags.Contains("icft") &&
					   otf.Base.Vert.BaselineTags.Contains("romn") &&
					   (script.BaseValue?.BaseCoords?.Count ?? 0) == otf.Base.Vert.BaselineTags.Count) {    // Baseline Tagにicfb, icft, romnを含む（游ゴシックなど）
						var tags = otf.Base.Vert.BaselineTags.ToList();

						var overline = script.BaseValue!.BaseCoords[tags.IndexOf("icft")].Coordinate / (double)otf.Head.UnitsPerEm;
						var baseline = script.BaseValue!.BaseCoords[tags.IndexOf("romn")].Coordinate / (double)otf.Head.UnitsPerEm;
						var underline = script.BaseValue!.BaseCoords[tags.IndexOf("icfb")].Coordinate / (double)otf.Head.UnitsPerEm;
						var strikethrough = (overline + underline) / 2;

						VerticalDecorationMetrics = new DecorationMetrics(overline, strikethrough, baseline, underline);
					} else if(!otf.Base.Vert.BaselineTags.Contains("ideo") && script.DefaultMinMax != null) {   // Baseline Tagにideoのみを含む（ＭＳ 明朝など）
						var overline = (script.DefaultMinMax.MinMaxCoord.MaxCoord?.Coordinate ?? (int)otf.Head.UnitsPerEm) / (double)otf.Head.UnitsPerEm;
						var underline = (script.DefaultMinMax.MinMaxCoord.MinCoord?.Coordinate ?? (int)otf.Head.UnitsPerEm) / (double)otf.Head.UnitsPerEm;
						var strikethrough = (overline + underline) / 2;

						VerticalDecorationMetrics = new DecorationMetrics(overline, strikethrough, underline, underline);
					} else
						VerticalDecorationMetrics = new DecorationMetrics(1, 0.5, 0, 0);    // 決め打ちする
				}
			}
		}

		public FontNameInfo FontName { get; }

		public FontWeight FontWeight { get; }

		public FontStyle FontStyle { get; }

		public FontStretch FontStretch { get; }

		public GlyphTypeface GlyphTypeface { get; }

		public IReadOnlyDictionary<ushort, ushort> VerticalSubstitution { get; }

		public IReadOnlyDictionary<ushort, GlyphAdjustment> VerticalProportionalAdjustment { get; }

		public IReadOnlyDictionary<ushort, GlyphMetrics> GlyphMetrics { get; }

		public DecorationMetrics VerticalDecorationMetrics { get; }

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
