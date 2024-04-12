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
					ushort index;
					try {
						index = fontglyph.GlyphTypeface.CharacterToGlyphMap[c];
					}
					catch(KeyNotFoundException) {
						index = fontglyph.GlyphTypeface.CharacterToGlyphMap['c'];
					}

					if(c >= 0x80 || halfWidthCharVertical) {
						indices[i] = fontglyph.VerticalGlyphConverter.Convert(index);
						widths[i] = fontglyph.GlyphTypeface.AdvanceHeights[index];
						isvert[i] = true;
					} else {
						indices[i] = index;
						widths[i] = fontglyph.GlyphTypeface.AdvanceWidths[index];
						isvert[i] = false;
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


		public bool ParamEquals(string text, string? fontname, FontWeight fontweight, FontStyle style, bool halfWidthCharVertical)
		{
			return Text == text && FontGlyph.ParamEquals(fontname, fontweight, style) && HalfWidthCharVertical == halfWidthCharVertical;
		}
	}
}
