using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.Metrix
{
	internal class HmtxTable
	{
		internal HmtxTable(ReadOnlySpan<byte> data, ushort numberOfHMetrics, ushort numGlyphs)
		{
			short aw = 0;
			short lsb;

			var mtx = new LongHorMetric[numGlyphs];

			for(ushort i = 0; i < numGlyphs; i++) {
				if(i < numberOfHMetrics) {
					aw = BinaryPrimitives.ReadInt16BigEndian(data.Slice(0 + i * 4, 2));
					lsb = BinaryPrimitives.ReadInt16BigEndian(data.Slice(2 + i * 4, 2));
				} else
					lsb = BinaryPrimitives.ReadInt16BigEndian(data.Slice(numberOfHMetrics * 2 + i * 2, 2));

				mtx[i] = new(aw, lsb);
			}

			LongHorMetrics = new ReadOnlyCollection<LongHorMetric>(mtx);
		}

		public IReadOnlyList<LongHorMetric> LongHorMetrics { get; }
	}
}
