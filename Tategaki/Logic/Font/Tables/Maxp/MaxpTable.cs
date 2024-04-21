using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.Maxp
{
	internal class MaxpTable
	{
		internal MaxpTable(ReadOnlySpan<byte> data)
		{
			MajorVersion = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
			MinorVersion = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
			NumGlyphs = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));

			if(MajorVersion == 1 && MinorVersion == 0) {
				MaxPoints = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6, 2));
				MaxContours = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(8, 2));
				MaxCompositePoints = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(10, 2));
				MaxCompositeContours = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(12, 2));
				MaxZones = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(14, 2));
				MaxTwilightPoints = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(16, 2));
				MaxStorage = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(18, 2));
				MaxFunctionDefs = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(20, 2));
				MaxInstructionDefs = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(22, 2));
				MaxStackElements = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(24, 2));
				MaxSizeOfInstructions = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(26, 2));
				MaxComponentElements = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(28, 2));
				MaxComponentDepth = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(30, 2));
			}
		}

		public ushort MajorVersion { get; }
		public ushort MinorVersion { get; }
		public ushort NumGlyphs { get; }
		public ushort MaxPoints { get; }
		public ushort MaxContours { get; }
		public ushort MaxCompositePoints { get; }
		public ushort MaxCompositeContours { get; }
		public ushort MaxZones { get; }
		public ushort MaxTwilightPoints { get; }
		public ushort MaxStorage { get; }
		public ushort MaxFunctionDefs { get; }
		public ushort MaxInstructionDefs { get; }
		public ushort MaxStackElements { get; }
		public ushort MaxSizeOfInstructions { get; }
		public ushort MaxComponentElements { get; }
		public ushort MaxComponentDepth { get; }
	}
}
