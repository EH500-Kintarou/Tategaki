using System.Buffers.Binary;
using System.Text;

namespace Tategaki.Logic.Font.Tables
{
	internal class TableRecord
	{
		public TableRecord(ReadOnlySpan<byte> data)
		{
			TableTag = Encoding.UTF8.GetString(data.Slice(0, 4).ToArray());
			Checksum = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(4, 4));
			Offset = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(8, 4));
			Length = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(12, 4));
		}

		public string TableTag { get; }

		public uint Checksum { get; }

		public uint Offset { get; }

		public uint Length { get; }

		public override string ToString()
		{
			return $"({TableTag}, 0x{Checksum:X8}, {Offset}, {Length})";
		}
	}
}
