using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Tategaki.Logic.Font.Tables;
using Tategaki.Logic.Font.Tables.GsubGpos;
using Tategaki.Logic.Font.Tables.Head;

namespace Tategaki.Logic.Font
{
	internal partial class OpenTypeFont
	{
		/// <summary>
		/// vert FeatureTagを持ったフォントファイルかを確かめるメソッド
		/// すべてのフォントデータを読み込んでいると処理が重いため、vertタグがあるかどうかに特化した読み込みを行う。
		/// </summary>
		/// <param name="fontUri">フォントファイルのURI</param>
		/// <returns>vertタグを持っているかどうか。</returns>
		/// <exception cref="NotSupportedException">サポートしていないフォント</exception>
		public static bool HasVert(Uri fontUri)
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
				return ReadCollectionTypeface(stream, index);
			else
				return ReadTypeface(stream, 0);
		}

		private static bool ReadCollectionTypeface(Stream stream, int index)
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
			return ReadTypeface(stream, offset);
		}

		private static bool ReadTypeface(Stream stream, uint offsetTablePos)
		{
			stream.Seek(offsetTablePos, SeekOrigin.Begin);
			var tables = ReadOffsetTable(stream);
			//CalcAllTableChecksum(data, tables);	// Checksumの検証をすると起動に時間がかかる

			ReadHead(stream, tables);   // MagicNumberの検証が入るためHeadは読み込む
			var gsub = ReadGsub(stream, tables);

			if(gsub == null)
				return false;
			else
				return (gsub.FeatureRecords ?? Enumerable.Empty<FeatureRecord>()).Where(p => p.FeatureTag == "vert").Any();
		}

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

		private static HeadTable ReadHead(Stream stream, IReadOnlyDictionary<string, TableRecord> tables)
		{
			var table = tables[TableNames.HEAD];

			var buffer = new byte[table.Length];
			stream.Seek(table.Offset, SeekOrigin.Begin);
			stream.Read(buffer, 0, buffer.Length);

			return new HeadTable(buffer.AsSpan());
		}

		private static GsubTable? ReadGsub(Stream stream, IReadOnlyDictionary<string, TableRecord> tables)
		{
			if(tables.TryGetValue(TableNames.GSUB, out var table)) {
				var buffer = new byte[table.Length];
				stream.Seek(table.Offset, SeekOrigin.Begin);
				stream.Read(buffer, 0, buffer.Length);

				return new GsubTable(buffer.AsSpan());
			} else
				return null;
		}
	}
}
