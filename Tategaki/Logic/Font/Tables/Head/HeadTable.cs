using System.Buffers.Binary;

namespace Tategaki.Logic.Font.Tables.Head
{
	internal class HeadTable
	{
		internal HeadTable(ReadOnlySpan<byte> data)
		{
			MajorVersion = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
			MinorVersion = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
			MajorFontRevision = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));
			MinorFontRevision = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6, 2));
			CheckSumAdjustment = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(8, 4));
			MagicNumber = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(12, 4));
			Flags = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(16, 2));
			UnitsPerEm = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(18, 2));
			var created = BinaryPrimitives.ReadUInt64BigEndian(data.Slice(20, 8));
			var modified = BinaryPrimitives.ReadUInt64BigEndian(data.Slice(28, 8));
			XMin = BinaryPrimitives.ReadInt16BigEndian(data.Slice(36, 2));
			YMin = BinaryPrimitives.ReadInt16BigEndian(data.Slice(38, 2));
			XMax = BinaryPrimitives.ReadInt16BigEndian(data.Slice(40, 2));
			YMax = BinaryPrimitives.ReadInt16BigEndian(data.Slice(42, 2));
			MacStyle = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(44, 2));
			LowestRecPPEM = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(46, 2));
			FontDirectionHint = BinaryPrimitives.ReadInt16BigEndian(data.Slice(48, 2));
			IndexToLocFormat = BinaryPrimitives.ReadInt16BigEndian(data.Slice(50, 2));
			GlyphDataFormat = BinaryPrimitives.ReadInt16BigEndian(data.Slice(52, 2));

			if(MagicNumber != 0x5F0F3CF5)
				throw new NotSupportedException("Magic number of HEAD table is not 0x5F0F3CF5.");
			if(created > TimeSpan.MaxValue.TotalSeconds)
				throw new NotSupportedException($"Created Time of HEAD table is too much big. {created} seconds");
			if(modified > TimeSpan.MaxValue.TotalSeconds)
				throw new NotSupportedException($"Modified Time HEAD table is too much big. {modified} seconds");

			DateTime start = new DateTime(1904, 1, 1);
			Created = start + TimeSpan.FromSeconds(created);
			Modified = start + TimeSpan.FromSeconds(modified);
		}

		public ushort MajorVersion { get; }
		public ushort MinorVersion { get; }
		public ushort MajorFontRevision { get; }
		public ushort MinorFontRevision { get; }
		public uint CheckSumAdjustment { get; }
		public uint MagicNumber { get; }
		public ushort Flags { get; }
		public ushort UnitsPerEm { get; }
		public DateTime Created { get; }
		public DateTime Modified { get; }
		public short XMin { get; }
		public short YMin { get; }
		public short XMax { get; }
		public short YMax { get; }
		public ushort MacStyle { get; }
		public ushort LowestRecPPEM { get; }
		public short FontDirectionHint { get; }
		public short IndexToLocFormat { get; }
		public short GlyphDataFormat { get; }
	}
}
