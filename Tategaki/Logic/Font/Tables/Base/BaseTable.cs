using System.Buffers.Binary;

namespace Tategaki.Logic.Font.Tables.Base
{
	internal class BaseTable
	{
		internal BaseTable(ReadOnlySpan<byte> data)
		{
			MajorVersion = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
			MinorVersion = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
			HorizAxisOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));
			VertAxisOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6, 2));

			Horiz = new Axis(data.Slice(HorizAxisOffset));
			Vert = new Axis(data.Slice(VertAxisOffset));
		}

		public ushort MajorVersion { get; }
		public ushort MinorVersion { get; }
		public ushort HorizAxisOffset { get; }
		public ushort VertAxisOffset { get; }

		public Axis Horiz { get; }
		public Axis Vert { get; }
	}
}
