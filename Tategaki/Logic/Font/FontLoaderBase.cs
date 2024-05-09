using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.IO;
using Tategaki.Logic.Font.Tables;
using Tategaki.Logic.Font.Tables.Head;
using Tategaki.Logic.Font.Tables.Maxp;
using Tategaki.Logic.Font.Tables.Metrix;

namespace Tategaki.Logic.Font
{
	internal abstract class FontLoaderBase
	{
		protected FontLoaderBase(Uri fontUri, out NecessaryTables tables, IReadOnlyList<string> filter, bool validateChecksum = false)
		{
			using var stream = new FileStream(Uri.UnescapeDataString(fontUri.AbsolutePath), FileMode.Open, FileAccess.Read);
			int index = fontUri.Fragment == "" ? 0 : int.Parse(fontUri.Fragment.Replace("#", ""));

			var ttcTag = new byte[4];
			stream.Read(ttcTag, 0, 4);
			var data = ttcTag.AsSpan();

			if(IsWOFF2(data))
				throw new NotSupportedException("WOFF is not supported.");

			stream.Seek(0, SeekOrigin.Begin);
			if(IsCollection(data))
				tables = ReadCollectionTypeface(stream, index, filter, validateChecksum);
			else
				tables = ReadTypeface(stream, 0, filter, validateChecksum);
		}

		private static bool IsWOFF2(ReadOnlySpan<byte> data)
			=> data.Slice(0, 4).SequenceEqual("wOF2"u8);

		private static bool IsCollection(ReadOnlySpan<byte> data)
			=> data.Slice(0, 4).SequenceEqual("ttcf"u8);

		private NecessaryTables ReadCollectionTypeface(Stream stream, int index, IReadOnlyList<string> filter, bool validateChecksum)
		{
			var buffer = new byte[12];
			stream.Read(buffer, 0, 12);
			var data = buffer.AsSpan();

			//ReadOnlySpan<byte> ttcTag = data.Slice(0, 4);
			//ushort majorVersion = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));
			//ushort minorVersion = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6, 2));
			uint numFonts = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(8, 4));

			if(index >= numFonts || index < 0)
				throw new ArgumentOutOfRangeException(nameof(index));

			buffer = new byte[4];
			stream.Seek(12 + index * 4, SeekOrigin.Begin);
			stream.Read(buffer, 0, 4);

			var offset = BinaryPrimitives.ReadUInt32BigEndian(buffer.AsSpan());
			stream.Seek(0, SeekOrigin.Begin);
			return ReadTypeface(stream, offset, filter, validateChecksum);
		}

		private NecessaryTables ReadTypeface(Stream stream, uint offsetTablePos, IReadOnlyList<string> readingTables, bool validateChecksum)
		{
			stream.Seek(offsetTablePos, SeekOrigin.Begin);
			var ot = ReadOffsetTable(stream);

			var cmap = readingTables.Contains(TableNames.CMAP) ? new CmapTable(ReadTable(stream, ot[TableNames.CMAP], validateChecksum)) : null;
			var maxp = readingTables.Contains(TableNames.MAXP) ? new MaxpTable(ReadTable(stream, ot[TableNames.MAXP], validateChecksum)) : null;
			var head = readingTables.Contains(TableNames.HEAD) ? new HeadTable(ReadTable(stream, ot[TableNames.HEAD], validateChecksum)) : null;
			var hhea = readingTables.Contains(TableNames.HHEA) ? new HheaTable(ReadTable(stream, ot[TableNames.HHEA], validateChecksum)) : null;
			var hmtx = readingTables.Contains(TableNames.HMTX) ? new HmtxTable(ReadTable(stream, ot[TableNames.HMTX], validateChecksum), hhea?.NumberOfHMetrics ?? default, maxp?.NumGlyphs ?? default) : null;
			var post = readingTables.Contains(TableNames.POST) ? new PostTable(ReadTable(stream, ot[TableNames.POST], validateChecksum)) : null;
			var name = readingTables.Contains(TableNames.NAME) ? new NameTable(ReadTable(stream, ot[TableNames.NAME], validateChecksum)) : null;

			var nt = new NecessaryTables(cmap, maxp, head, hhea, hmtx, post, name);

			foreach(var t in readingTables.Except(TableNames.RequiedTables)) {
				if(ot.TryGetValue(t, out var tr))
					ReadArbitraryTables(ReadTable(stream, tr, validateChecksum), t, nt);
			}

			return nt;
		}

		protected virtual void ReadArbitraryTables(ReadOnlySpan<byte> data, string tableName, NecessaryTables necessary) { }

		private static IReadOnlyDictionary<string, TableRecord> ReadOffsetTable(Stream stream)
		{
			var buffer = new byte[12];
			stream.Read(buffer, 0, 12);
			var data = buffer.AsSpan();

			//var version = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(0, 4));
			var numTables = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));
			//var searchRange = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6, 2));
			//var entrySelector = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(8, 2));
			//var rangeShift = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(10, 2));

			buffer = new byte[16 * numTables];
			stream.Read(buffer, 0, buffer.Length);
			data = buffer.AsSpan();

			var tables = new Dictionary<string, TableRecord>();

			for(int i = 0; i < numTables; i++) {
				var table = new TableRecord(data.Slice(i * 16, 16));
				tables[table.TableTag] = table;
			}

			if(!TableNames.RequiedTables.All(p => tables.ContainsKey(p)))
				throw new NotSupportedException("This font file does not contain the required tables.");

			return new ReadOnlyDictionary<string, TableRecord>(tables);
		}

		private static byte[] ReadTable(Stream stream, TableRecord table, bool validateChecksum)
		{
			var buffer = new byte[table.Length];
			stream.Seek(table.Offset, SeekOrigin.Begin);
			stream.Read(buffer, 0, buffer.Length);

			if(validateChecksum) {
				if(!ValidateTableChecksum(buffer, table))
					throw new NotSupportedException($"Checksum of {table.TableTag} Table is not matching.");
			}

			return buffer;
		}

		private static bool ValidateTableChecksum(ReadOnlySpan<byte> data, TableRecord table)
		{
			uint sum = 0;

			if(table.TableTag == TableNames.HEAD) {
				for(int i = 0; i < table.Length; i += 4) {
					if(i != 8) {    // 'head' tableのcheckSumAdjustmentは無視する（すべてのチェックサム計算後に書き入れられた値なので）
						unchecked {
							sum += BinaryPrimitives.ReadUInt32BigEndian(data.Slice(i, 4));
						}
					}
				}
			} else {
				for(int i = 0; i < table.Length; i += 4) {
					unchecked {
						sum += BinaryPrimitives.ReadUInt32BigEndian(data.Slice(i, 4));
					}
				}
			}
			return sum == table.Checksum;
		}


		protected record struct NecessaryTables(
			CmapTable? Cmap,
			MaxpTable? Maxp,
			HeadTable? Head,
			HheaTable? Hhea,
			HmtxTable? Hmtx,
			PostTable? Post,
			NameTable? Name
		);
	}
}
