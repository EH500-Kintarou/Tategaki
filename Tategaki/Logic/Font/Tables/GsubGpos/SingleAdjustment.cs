using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.GsubGpos
{
	internal class SingleAdjustment : IGlyphPositioning
	{
		internal SingleAdjustment(ReadOnlySpan<byte> data)
		{
			PosFormat = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
			var table = new Dictionary<ushort, ValueRecord>();

			switch(PosFormat) {
				case 1:
					CoverageOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
					ValueFormat = (ValueFormat)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));
					var value = new ValueRecord(data.Slice(6), ValueFormat);

					var coverage = TableBase.ReadCoverage(data.Slice(CoverageOffset));
					foreach(var id in coverage)
						table[id] = value;
					break;
				case 2:
					CoverageOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
					ValueFormat = (ValueFormat)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));
					ValueCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6, 2));

					coverage = TableBase.ReadCoverage(data.Slice(CoverageOffset));

					var vrsize = 0;
					for(int i = 0; i < 16; i++) {
						if(((ushort)ValueFormat & (0x01 << i)) != 0)
							vrsize += 2;
					}

					var cnt = Math.Min(ValueCount, coverage.Count);
					for(int i = 0; i < cnt; i++)
						table[coverage[i]] = new ValueRecord(data.Slice(8 + vrsize * i), ValueFormat);
					break;
			}

			Table = new ReadOnlyDictionary<ushort, ValueRecord>(table);
		}

		public ushort PosFormat { get; }
		public ushort CoverageOffset { get; }
		public ValueFormat ValueFormat { get; }
		public ushort ValueCount { get; }
		public IReadOnlyDictionary<ushort, ValueRecord> Table { get; }
	}
}
