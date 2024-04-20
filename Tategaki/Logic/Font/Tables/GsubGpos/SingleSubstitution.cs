using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.GsubGpos
{
	internal class SingleSubstitution : IGlyphSubstitution
	{
		internal SingleSubstitution(ReadOnlySpan<byte> data)
		{
			SubstFormat = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
			var table = new Dictionary<ushort, ushort>();

			switch(SubstFormat) {
				case 1:
					CoverageOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
					DeltaGlyphID = BinaryPrimitives.ReadInt16BigEndian(data.Slice(4, 2));

					var coverage = TableBase.ReadCoverage(data.Slice(CoverageOffset));
					foreach(var id in coverage)
						table[id] = (ushort)(id + DeltaGlyphID);
					break;
				case 2:
					CoverageOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
					GlyphCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));

					coverage = TableBase.ReadCoverage(data.Slice(CoverageOffset));
					var cnt = Math.Min(GlyphCount, coverage.Count);
					for(int i = 0; i < cnt; i++)
						table[coverage[i]] = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6 + i * 2, 2));
					break;
			}

			Table = new ReadOnlyDictionary<ushort, ushort>(table);
		}

		public ushort SubstFormat { get; }
		public ushort CoverageOffset { get; }
		public short DeltaGlyphID { get; }
		public ushort GlyphCount { get; }
		public IReadOnlyDictionary<ushort, ushort> Table { get; }
	}
}
