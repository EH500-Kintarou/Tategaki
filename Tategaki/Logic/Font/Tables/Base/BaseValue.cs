using System.Buffers.Binary;
using System.Collections.ObjectModel;

namespace Tategaki.Logic.Font.Tables.Base
{
	internal class BaseValue
	{
		public BaseValue(ReadOnlySpan<byte> data)
		{
			DefaultBaselineIndex = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
			BaseCoordCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));

			var coords = new BaseCoord[BaseCoordCount];
			for(int i = 0; i < BaseCoordCount; i++) {
				var offset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4 + i * 2, 2));
				coords[i] = new BaseCoord(data.Slice(offset));
			}
			BaseCoords = new ReadOnlyCollection<BaseCoord>(coords);
		}

		public ushort DefaultBaselineIndex { get; }
		public ushort BaseCoordCount { get; }
		public IReadOnlyList<BaseCoord> BaseCoords { get; }
	}
}
