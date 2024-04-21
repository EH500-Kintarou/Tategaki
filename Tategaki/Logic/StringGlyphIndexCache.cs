using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Tategaki.Logic
{
	internal class StringGlyphIndexCache
	{
		public StringGlyphIndexCache(string text, FontGlyphCache fontglyph, bool halfWidthCharVertical, bool enableVpal)
		{
			Text = text;
			FontGlyph = fontglyph;
			HalfWidthCharVertical = halfWidthCharVertical;
			EnableProportionalAlternate = enableVpal;

			var indices = new ushort[text.Length];
			var xoffset = new double[text.Length];
			var altxoffset = new double[text.Length];
			var widths = new double[text.Length];
			var isvert = new bool?[text.Length];

			for(int i = 0; i < text.Length; i++) {
				var c = text[i];

				if(char.IsControl(c)) {
					indices[i] = 0;
					widths[i] = 0;
					isvert[i] = null;
				} else {
					try {
						if(!fontglyph.GlyphTypeface.CharacterToGlyphMap.TryGetValue(c, out var index))
							index = fontglyph.GlyphTypeface.CharacterToGlyphMap['?'];

						if(c >= 0x80 || halfWidthCharVertical) {
							index = fontglyph.VerticalSubstitution.TryGetValue(index, out var subst) ? subst : index;
							
							var adj = new GlyphAdjustment();
							if(enableVpal)
								fontglyph.VerticalProportionalAdjustment.TryGetValue(index, out adj);

							fontglyph.GlyphMetrics.TryGetValue(index, out var metrics);
							fontglyph.GlyphTypeface.TopSideBearings.TryGetValue(index, out var tsb);

							indices[i] = index;
							xoffset[i] = -adj.YPlacement;
							altxoffset[i] = -(fontglyph.GlyphTypeface.Baseline - tsb - metrics.YMax);
							widths[i] = fontglyph.GlyphTypeface.AdvanceHeights[index] + adj.YAdvance;
							isvert[i] = true;
						} else {
							indices[i] = index;
							xoffset[i] = 0;
							altxoffset[i] = 0;
							widths[i] = fontglyph.GlyphTypeface.AdvanceWidths[index];
							isvert[i] = false;
						}
					}
					catch(KeyNotFoundException) {
						indices[i] = 0;
						xoffset[i] = 0;
						widths[i] = 0;
						isvert[i] = null;
					}
				}
			}

			Indices = new ReadOnlyCollection<ushort>(indices);
			XOffset = new ReadOnlyCollection<double>(xoffset);
			AlternateRenderingXOffset = new ReadOnlyCollection<double>(altxoffset);
			AdvanceWidths = new ReadOnlyCollection<double>(widths);
			IsVerticals = new ReadOnlyCollection<bool?>(isvert);
		}

		public string Text { get; }

		public IReadOnlyList<ushort> Indices { get; }

		public IReadOnlyList<double> XOffset { get; }

		public IReadOnlyList<double> AlternateRenderingXOffset { get; }

		public IReadOnlyList<double> AdvanceWidths { get; }

		public IReadOnlyList<bool?> IsVerticals { get; }

		public FontGlyphCache FontGlyph { get; }

		public bool HalfWidthCharVertical { get; }

		public bool EnableProportionalAlternate { get; }


		public bool ParamEquals(string text, FontGlyphCache glyphcache, bool halfWidthCharVertical, bool enableVpal)
		{
			return Text == text && FontGlyph.Equals(glyphcache) && HalfWidthCharVertical == halfWidthCharVertical && EnableProportionalAlternate == enableVpal;
		}
	}
}
