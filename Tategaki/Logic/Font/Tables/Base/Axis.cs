using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.Base
{
	internal class Axis
	{
		internal Axis(ReadOnlySpan<byte> data)
		{
			BaseTagListOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
			BaseScriptListOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));

			var btldata = data.Slice(BaseTagListOffset);
			BaseTagCount = BinaryPrimitives.ReadUInt16BigEndian(btldata.Slice(0, 2));

			var tags = new string[BaseTagCount];
			for(int i = 0; i < BaseTagCount; i++)
				tags[i] = Encoding.UTF8.GetString(btldata.Slice(2 + i * 4, 4).ToArray());
			BaselineTags = new ReadOnlyCollection<string>(tags);

			var bsldata = data.Slice(BaseScriptListOffset);
			BaseScriptCount = BinaryPrimitives.ReadUInt16BigEndian(bsldata.Slice(0, 2));

			var records = new Dictionary<string, BaseScript>();
			for(int i = 0; i < BaseScriptCount; i++) {
				var baseScriptTag = Encoding.UTF8.GetString(bsldata.Slice(2 + i * 6, 4).ToArray());
				var baseScriptOffset = BinaryPrimitives.ReadUInt16BigEndian(bsldata.Slice(6 + i * 6, 2));
				records[baseScriptTag] = new BaseScript(bsldata.Slice(baseScriptOffset));
			}
			BaseScriptRecords = new ReadOnlyDictionary<string, BaseScript>(records);
		}

		public ushort BaseTagListOffset { get; }
		public ushort BaseScriptListOffset { get; }

		public ushort BaseTagCount { get; }
		public IReadOnlyList<string> BaselineTags { get; }

		public ushort BaseScriptCount { get; }
		public IReadOnlyDictionary<string, BaseScript> BaseScriptRecords { get; }
	}
}
