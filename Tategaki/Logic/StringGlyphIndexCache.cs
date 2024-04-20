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
		public StringGlyphIndexCache(string text, FontGlyphCache fontglyph, bool halfWidthCharVertical)
		{
			Text = text;
			FontGlyph = fontglyph;
			HalfWidthCharVertical = halfWidthCharVertical;

			var indices = new ushort[text.Length];
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
							indices[i] = fontglyph.VerticalSubstitution.TryGetValue(index, out var subst) ? subst : index;
							widths[i] = fontglyph.GlyphTypeface.AdvanceHeights[index];
							isvert[i] = true;
						} else {
							indices[i] = index;
							widths[i] = fontglyph.GlyphTypeface.AdvanceWidths[index];
							isvert[i] = false;
						}
					}
					catch(KeyNotFoundException) {
						indices[i] = 0;
						widths[i] = 0;
						isvert[i] = null;
					}
				}
			}

			Indices = new ReadOnlyCollection<ushort>(indices);
			AdvanceWidths = new ReadOnlyCollection<double>(widths);
			IsVerticals = new ReadOnlyCollection<bool?>(isvert);
		}

		public string Text { get; }

		public IReadOnlyList<ushort> Indices { get; }

		public IReadOnlyList<double> AdvanceWidths { get; }

		public IReadOnlyList<bool?> IsVerticals { get; }

		public FontGlyphCache FontGlyph { get; }

		public bool HalfWidthCharVertical { get; }


		public bool ParamEquals(string text, FontNameInfo fontname, FontWeight fontweight, FontStyle style, FontStretch stretch, bool halfWidthCharVertical)
		{
			return Text == text && FontGlyph.ParamEquals(fontname, fontweight, style, stretch) && HalfWidthCharVertical == halfWidthCharVertical;
		}
	}
}
