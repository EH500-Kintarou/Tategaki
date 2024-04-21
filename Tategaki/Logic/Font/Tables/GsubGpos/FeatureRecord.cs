using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.GsubGpos
{
	internal class FeatureRecord
	{
		internal FeatureRecord(string featureTag, ReadOnlySpan<byte> data)
		{
			FeatureTag = featureTag;
			FeatureParamsOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
			LookupIndexCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));

			var indices = new ushort[LookupIndexCount];
			for(int i = 0; i < LookupIndexCount; i++)
				indices[i] = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4 + i * 2, 2));
			LookupListIndices = new ReadOnlyCollection<ushort>(indices);
		}

		public string FeatureTag { get; }
		public ushort FeatureParamsOffset { get; }
		public ushort LookupIndexCount { get; }
		public IReadOnlyList<ushort> LookupListIndices { get; }

		public override string ToString()
		{
			return $"({FeatureTag}, FeatureParamsOffset = {FeatureParamsOffset}, LookupListIndices = {{{string.Join(", ", LookupListIndices)}}})";
		}
	}
}
