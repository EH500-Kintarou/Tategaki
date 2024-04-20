using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.Metrix
{
	internal class VmtxTable
	{
		internal VmtxTable(ReadOnlySpan<byte> data, ushort numberOfVerMetrics, ushort numGlyphs)
		{
			short aw = 0;
			short lsb;

			var mtx = new LongVerMetric[numGlyphs];

			for(ushort i = 0; i < numGlyphs; i++) {
				if(i < numberOfVerMetrics) {
					aw = BinaryPrimitives.ReadInt16BigEndian(data.Slice(0 + i * 4, 2));
					lsb = BinaryPrimitives.ReadInt16BigEndian(data.Slice(2 + i * 4, 2));
				} else
					lsb = BinaryPrimitives.ReadInt16BigEndian(data.Slice(numberOfVerMetrics * 2 + i * 2, 2));

				mtx[i] = new(aw, lsb);
			}

			LongVerMetrics = new ReadOnlyCollection<LongVerMetric>(mtx);
		}

		public IReadOnlyList<LongVerMetric> LongVerMetrics { get; }
	}
}
