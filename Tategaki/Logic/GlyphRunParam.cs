using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using WaterTrans.TypeLoader;

namespace Tategaki.Logic
{
	internal class GlyphRunParam
	{
		static KeyValuePair<(string fontname, FontWeight weight, FontStyle style), GlyphTypeface>? gtfCache = null;

		public GlyphRunParam(string text, bool isSideways, string? fontname, FontWeight weight, FontStyle style, double size, double spacing, XmlLanguage language)
		{
			if(string.IsNullOrEmpty(text))
				throw new ArgumentException("Length of text must be more zan zero.", nameof(text));

			GlyphTypeface gtf;
			if(gtfCache == null || gtfCache.Value.Key != (fontname, weight, style)) {
				var uri = FontUriTable.FromName(fontname);
				fontname ??= FontUriTable.AllVerticalFonts.Where(p => p.Value == uri).First().Key;
				gtf = new GlyphTypeface(uri, ((weight == FontWeights.Normal) ? StyleSimulations.None : StyleSimulations.BoldSimulation) | ((style == FontStyles.Normal) ? StyleSimulations.None : StyleSimulations.ItalicSimulation));

				gtfCache = new KeyValuePair<(string fontname, FontWeight weight, FontStyle style), GlyphTypeface>((fontname, weight, style), gtf);
			} else
				gtf = gtfCache.Value.Value;

			Text = text;
			GlyphIndices = GetIndices(gtfCache.Value.Value, text, isSideways);
			IsSideways = isSideways;
			FontName = fontname!;
			GlyphTypeface = gtf;
			RenderingEmSize = size;
			AdvanceWidths = GlyphIndices.Select(p => (isSideways ? gtf.AdvanceHeights[p] : gtf.AdvanceWidths[p]) * size).ToArray();
			Spacing = spacing;
			GlyphOffsets = Enumerable.Range(0, text.Length).Select(p => new Point(p * (spacing - 100) / 100 * size, 0)).ToArray();
			Language = language;

			GlyphBox = Create(new Point(0, 0)).ComputeAlignmentBox();
		}

		#region Properties

		public string Text { get; }

		public IList<ushort> GlyphIndices { get; }

		public bool IsSideways { get; }

		public string FontName { get; }

		public GlyphTypeface GlyphTypeface { get; }

		public double RenderingEmSize { get; }

		public IList<double> AdvanceWidths { get; }

		public double Spacing { get; }

		public IList<Point> GlyphOffsets { get; }

		public XmlLanguage Language { get; }

		public Rect GlyphBox { get; }

		#endregion

		public GlyphRun Create(Point origin)
		{
			return new GlyphRun(GlyphTypeface, 0, IsSideways, RenderingEmSize, 1, GlyphIndices, origin, AdvanceWidths, GlyphOffsets, Text.ToArray(), FontName, null, null, Language);
		}

		static IList<ushort> GetIndices(GlyphTypeface gtf, string text, bool vertical)
		{
			var conv = GlyphConverterCache.GetConverter(gtf);
			var ret = new ushort[text.Length];

			for(int i = 0; i < ret.Length; i++) {
				ushort index;
				try {
					index = gtf.CharacterToGlyphMap[text[i]];
				}
				catch(KeyNotFoundException) {
					index = gtf.CharacterToGlyphMap['?'];
				}

				if(vertical)
					index = conv.Convert(index);

				ret[i] = index;
			}

			return ret;
		}

		public override string ToString()
		{
			return $"{Text}; ({GlyphBox.Left}, {GlyphBox.Top}, {GlyphBox.Width}, {GlyphBox.Height})";
		}
	}
}
