using System.Buffers.Binary;

namespace Tategaki.Logic.Font.Tables.GsubGpos
{
	internal struct ValueRecord
	{
		public ValueRecord(ReadOnlySpan<byte> data, ValueFormat format)
		{
			int i = 0;

			if(format.HasFlag(ValueFormat.XPlacement))
				XPlacement = BinaryPrimitives.ReadInt16BigEndian(data.Slice(i++ * 2, 2));
			if(format.HasFlag(ValueFormat.YPlacement))
				YPlacement = BinaryPrimitives.ReadInt16BigEndian(data.Slice(i++ * 2, 2));
			if(format.HasFlag(ValueFormat.XAdvance))
				XAdvance = BinaryPrimitives.ReadInt16BigEndian(data.Slice(i++ * 2, 2));
			if(format.HasFlag(ValueFormat.YAdvance))
				YAdvance = BinaryPrimitives.ReadInt16BigEndian(data.Slice(i++ * 2, 2));
			if(format.HasFlag(ValueFormat.XPlaDeviceOffset))
				XPlaDeviceOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(i++ * 2, 2));
			if(format.HasFlag(ValueFormat.YPlaDeviceOffset))
				YPlaDeviceOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(i++ * 2, 2));
			if(format.HasFlag(ValueFormat.XAdvDeviceOffset))
				XAdvDeviceOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(i++ * 2, 2));
			if(format.HasFlag(ValueFormat.YAdvDeviceOffset))
				YAdvDeviceOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(i++ * 2, 2));
		}

		public short XPlacement { get; }
		public short YPlacement { get; }
		public short XAdvance { get; }
		public short YAdvance { get; }
		public ushort XPlaDeviceOffset { get; }
		public ushort YPlaDeviceOffset { get; }
		public ushort XAdvDeviceOffset { get; }
		public ushort YAdvDeviceOffset { get; }

		public override string ToString()
		{
			return $"({XPlacement}, {YPlacement}, {XAdvance}, {YAdvance})";
		}
	}

	[Flags]
	public enum ValueFormat : ushort
	{
		XPlacement = 0x0001,
		YPlacement = 0x0002,
		XAdvance = 0x0004,
		YAdvance = 0x0008,
		XPlaDeviceOffset = 0x0010,
		YPlaDeviceOffset = 0x0020,
		XAdvDeviceOffset = 0x0040,
		YAdvDeviceOffset = 0x0080,
	}
}
