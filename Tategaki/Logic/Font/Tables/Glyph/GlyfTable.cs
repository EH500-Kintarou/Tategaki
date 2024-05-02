using System.Collections.ObjectModel;

namespace Tategaki.Logic.Font.Tables.Glyph
{
	internal class GlyfTable
	{
		internal GlyfTable(ReadOnlySpan<byte> data, LocaTable loca)
		{
			var glyphcnt = loca.Offsets.Count - 1;

			var glyphs = new GlyphData?[glyphcnt];

			for(int i = 0; i < glyphcnt; i++) {
				var offset = loca.Offsets[i];
				glyphs[i] = offset == null ? null : new GlyphData(data.Slice((int)offset.Value));
			}

			Glyphs = new ReadOnlyCollection<GlyphData?>(glyphs);
		}

		public IReadOnlyList<GlyphData?> Glyphs { get; }
	}
}
