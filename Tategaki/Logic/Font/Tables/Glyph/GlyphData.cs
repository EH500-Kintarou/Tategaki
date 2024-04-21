using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.Glyph
{
	internal class GlyphData
	{
		internal GlyphData(ReadOnlySpan<byte> data)
		{
			NumberOfContours = BinaryPrimitives.ReadInt16BigEndian(data.Slice(0, 2));
			XMin = BinaryPrimitives.ReadInt16BigEndian(data.Slice(2, 2));
			YMin = BinaryPrimitives.ReadInt16BigEndian(data.Slice(4, 2));
			XMax = BinaryPrimitives.ReadInt16BigEndian(data.Slice(6, 2));
			YMax = BinaryPrimitives.ReadInt16BigEndian(data.Slice(8, 2));
		}

		public short NumberOfContours { get; }
		public short XMin { get; }
		public short YMin { get; }
		public short XMax { get; }
		public short YMax { get; }

		// これ以降は省略
	}
}
