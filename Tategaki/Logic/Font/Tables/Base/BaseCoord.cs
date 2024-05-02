using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.Base
{
	internal class BaseCoord
	{
		public BaseCoord(ReadOnlySpan<byte> data)
		{
			BaseCoordFormat = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
			Coordinate = BinaryPrimitives.ReadInt16BigEndian(data.Slice(2, 2));

			switch(BaseCoordFormat) {
				case 1: // Format 1
					break;
				case 2: // Format 2
					ReferenceGlyph = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));
					BaseCoordPoint = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6, 2));
					break;
				case 3: // Format 3
					DeviceTableOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));
					break;
			}
		}

		public ushort BaseCoordFormat { get; }
		public short Coordinate { get; }

		public ushort ReferenceGlyph { get; }
		public ushort BaseCoordPoint { get; }

		public ushort DeviceTableOffset { get; }
	}
}
