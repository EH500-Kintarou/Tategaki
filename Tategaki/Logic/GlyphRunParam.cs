using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace Tategaki.Logic
{
	internal class GlyphRunParam
	{
		public GlyphRunParam(FontGlyphCache glyphCache, StringGlyphIndexCache textCache, int sliceStart, int sliceEndExclusive, double size, double spacing, XmlLanguage language)
		{
			Text = textCache.Text.Substring(sliceStart, sliceEndExclusive - sliceStart);

			GlyphIndices = new ushort[sliceEndExclusive - sliceStart];  // Sliceが欲しい…
			for(int i = sliceStart; i < sliceEndExclusive; i++)
				GlyphIndices[i - sliceStart] = textCache.Indices[i];

			AdvanceWidths = new double[sliceEndExclusive - sliceStart]; // Sliceが欲しい…
			for(int i = sliceStart; i < sliceEndExclusive; i++)
				AdvanceWidths[i - sliceStart] = textCache.AdvanceWidths[i] * size;

			IsSideways = textCache.IsVerticals[sliceStart] ?? throw new ArgumentException($"{nameof(textCache)}.{nameof(textCache.IsVerticals)} must not be null", nameof(textCache));	// 簡単のため先頭だけ見る
			FontName = glyphCache.FontName;
			GlyphTypeface = glyphCache.GlyphTypeface;
			RenderingEmSize = size;
			Spacing = spacing;
			GlyphOffsets = Enumerable.Range(0, Text.Length).Select(p => new Point(p * (spacing - 100) / 100 * size, 0)).ToArray();
			Language = language;
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

		#endregion

		public GlyphRun Create(Point origin)
		{
			return new GlyphRun(GlyphTypeface, 0, IsSideways, RenderingEmSize, 1, GlyphIndices, origin, AdvanceWidths, GlyphOffsets, Text.ToArray(), FontName, null, null, Language);
		}

		public GlyphRun CreateWithOffsetY0(Point origin)
		{
			var yoffset = (IsSideways ? GlyphTypeface.Height / 2.0 : GlyphTypeface.Baseline) * RenderingEmSize;

			return Create(new Point(origin.X, origin.Y + yoffset));
		}

		public override string ToString()
		{
			return Text;
		}
	}
}
