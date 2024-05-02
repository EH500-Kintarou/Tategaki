using System.Buffers.Binary;
using System.Collections.ObjectModel;

namespace Tategaki.Logic.Font.Tables.Glyph
{
	internal class LocaTable
	{
		internal LocaTable(ReadOnlySpan<byte> data, ushort numGlyphs, short indexToLocFormat)
		{
			var offsets = new uint?[numGlyphs + 1];

			switch(indexToLocFormat) {
				case 0:
					for(int i = 0; i < numGlyphs + 1; i++)
						offsets[i] = (uint)(BinaryPrimitives.ReadUInt16BigEndian(data.Slice(i * 2)) * 2);
					break;
				case 1:
					for(int i = 0; i < numGlyphs + 1; i++)
						offsets[i] = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(i * 4));
					break;
			}

			for(int i = 0; i < numGlyphs; i++) {
				if(offsets[i] == offsets[i + 1])
					offsets[i] = null;
			}

			Offsets = new ReadOnlyCollection<uint?>(offsets);
		}

		public IReadOnlyList<uint?> Offsets { get; }
	}
}
