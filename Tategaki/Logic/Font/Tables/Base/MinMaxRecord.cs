namespace Tategaki.Logic.Font.Tables.Base
{
	internal struct MinMaxRecord
	{
		public MinMaxRecord(ReadOnlySpan<byte> data, ushort minCoordOffset, ushort maxCoordOffset)
		{
			MinCoord = minCoordOffset != 0 ? new BaseCoord(data.Slice(minCoordOffset)) : null;
			MaxCoord = maxCoordOffset != 0 ? new BaseCoord(data.Slice(maxCoordOffset)) : null;
		}

		public BaseCoord? MinCoord { get; }
		public BaseCoord? MaxCoord { get; }
	}
}
