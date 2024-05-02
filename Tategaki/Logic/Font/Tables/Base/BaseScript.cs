using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Text;

namespace Tategaki.Logic.Font.Tables.Base
{
	internal class BaseScript
	{
		public BaseScript(ReadOnlySpan<byte> data)
		{
			BaseValueOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
			DefaultMinMaxOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
			BaseLangSysCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));

			var records = new Dictionary<string, MinMax>();
			for(int i = 0; i < BaseLangSysCount; i++) {
				var baseLangSysTag = Encoding.UTF8.GetString(data.Slice(6 + i * 6, 4).ToArray());
				var minMaxOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(10 + i * 6, 2));
				records[baseLangSysTag] = new MinMax(data.Slice(minMaxOffset));
			}
			BaseScriptRecords = new ReadOnlyDictionary<string, MinMax>(records);

			BaseValue = BaseValueOffset != 0 ? new BaseValue(data.Slice(BaseValueOffset)) : null;
			DefaultMinMax = DefaultMinMaxOffset != 0 ? new MinMax(data.Slice(DefaultMinMaxOffset)) : null;
		}

		public ushort BaseValueOffset { get; }
		public ushort DefaultMinMaxOffset { get; }
		public ushort BaseLangSysCount { get; }
		public IReadOnlyDictionary<string, MinMax> BaseScriptRecords { get; }

		public BaseValue? BaseValue { get; }
		public MinMax? DefaultMinMax { get; }
	}
}
