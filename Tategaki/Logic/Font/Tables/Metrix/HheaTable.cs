using System.Buffers.Binary;

namespace Tategaki.Logic.Font.Tables.Metrix
{
	internal class HheaTable
	{
		internal HheaTable(ReadOnlySpan<byte> data)
		{
			MajorVersion = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
			MinorVersion = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
			Ascender = BinaryPrimitives.ReadInt16BigEndian(data.Slice(4, 2));
			Descender = BinaryPrimitives.ReadInt16BigEndian(data.Slice(6, 2));
			LineGap = BinaryPrimitives.ReadInt16BigEndian(data.Slice(8, 2));
			AdvanceWidthMax = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(10, 2));
			MinLeftSideBearing = BinaryPrimitives.ReadInt16BigEndian(data.Slice(12, 2));
			MinRightSideBearing = BinaryPrimitives.ReadInt16BigEndian(data.Slice(14, 2));
			XMaxExtent = BinaryPrimitives.ReadInt16BigEndian(data.Slice(16, 2));
			CaretSlopeRise = BinaryPrimitives.ReadInt16BigEndian(data.Slice(18, 2));
			CaretSlopeRun = BinaryPrimitives.ReadInt16BigEndian(data.Slice(20, 2));
			CaretOffset = BinaryPrimitives.ReadInt16BigEndian(data.Slice(22, 2));
			Reserved0 = BinaryPrimitives.ReadInt16BigEndian(data.Slice(24, 2));
			Reserved1 = BinaryPrimitives.ReadInt16BigEndian(data.Slice(26, 2));
			Reserved2 = BinaryPrimitives.ReadInt16BigEndian(data.Slice(28, 2));
			Reserved3 = BinaryPrimitives.ReadInt16BigEndian(data.Slice(30, 2));
			MetricDataFormat = BinaryPrimitives.ReadInt16BigEndian(data.Slice(32, 2));
			NumberOfHMetrics = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(34, 2));
		}

		public ushort MajorVersion { get; }
		public ushort MinorVersion { get; }
		public short Ascender { get; }
		public short Descender { get; }
		public short LineGap { get; }
		public ushort AdvanceWidthMax { get; }
		public short MinLeftSideBearing { get; }
		public short MinRightSideBearing { get; }
		public short XMaxExtent { get; }
		public short CaretSlopeRise { get; }
		public short CaretSlopeRun { get; }
		public short CaretOffset { get; }
		public short Reserved0 { get; }
		public short Reserved1 { get; }
		public short Reserved2 { get; }
		public short Reserved3 { get; }
		public short MetricDataFormat { get; }
		public ushort NumberOfHMetrics { get; }
	}
}
