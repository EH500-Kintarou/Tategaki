using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.GsubGpos
{
	internal class LangSysRecord
	{
		internal LangSysRecord(string scriptTag, string langSysTag, ReadOnlySpan<byte> data)
		{
			ScriptTag = scriptTag;
			LangSysTag = langSysTag;
			LookupOrder = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
			RequiredFeatureIndex = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
			FeatureIndexCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));

			var indices = new ushort[FeatureIndexCount];
			for(int i = 0; i < FeatureIndexCount; i++)
				indices[i] = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6 + i * 2, 2));
			FeatureIndices = new ReadOnlyCollection<ushort>(indices);
		}

		public string ScriptTag { get; }
		public string LangSysTag { get; }
		public ushort LookupOrder { get; }
		public ushort RequiredFeatureIndex { get; }
		public ushort FeatureIndexCount { get; }
		public IReadOnlyList<ushort> FeatureIndices { get; }

		public override string ToString()
		{
			return $"({ScriptTag}.{LangSysTag}, LookupOrder = {LookupOrder}, RequiredFeatureIndex = {RequiredFeatureIndex}, FeatureIndices = {{{string.Join(", ", FeatureIndices)}}})";
		}
	}
}
