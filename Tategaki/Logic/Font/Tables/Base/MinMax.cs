using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.Base
{
	internal class MinMax
	{
		public MinMax(ReadOnlySpan<byte> data)
		{
			MinCoordOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
			MaxCoordOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
			FeatMinMaxCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));

			var records = new Dictionary<string, MinMaxRecord>();
			for(int i = 0; i < FeatMinMaxCount; i++) {
				var featureTableTag = Encoding.UTF8.GetString(data.Slice(6 + i * 8, 4).ToArray());
				var minCoordOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(10 + i * 8, 2));
				var maxCoordOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(12 + i * 8, 2));

				records[featureTableTag] = new MinMaxRecord(data, MinCoordOffset, MaxCoordOffset);
			}
			FeatMinMaxRecords = new ReadOnlyDictionary<string, MinMaxRecord>(records);

			MinMaxCoord = new MinMaxRecord(data, MinCoordOffset, MaxCoordOffset);
		}

		public ushort MinCoordOffset { get; }
		public ushort MaxCoordOffset { get; }
		public ushort FeatMinMaxCount { get; }
		public IReadOnlyDictionary<string, MinMaxRecord> FeatMinMaxRecords{ get; }

		public MinMaxRecord MinMaxCoord { get; }
	}
}
