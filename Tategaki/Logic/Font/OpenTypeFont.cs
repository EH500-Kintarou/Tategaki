using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tategaki.Logic.Font.Tables;
using Tategaki.Logic.Font.Tables.GsubGpos;
using Tategaki.Logic.Font.Tables.Head;
using Tategaki.Logic.Font.Tables.Maxp;
using Tategaki.Logic.Font.Tables.Metrix;

namespace Tategaki.Logic.Font
{
	/// <summary>
	/// OpenTypeのフォントを読み込むクラス
	/// 参考1： https://learn.microsoft.com/en-us/typography/opentype/spec/
	/// 参考2： https://aznote.jakou.com/prog/opentype/index.html
	/// </summary>
	internal class OpenTypeFont
	{
		public OpenTypeFont(Uri fontUri)
		{
			var data = new ReadOnlySpan<byte>(File.ReadAllBytes(Uri.UnescapeDataString(fontUri.AbsolutePath)));
			int index = fontUri.Fragment == "" ? 0 : int.Parse(fontUri.Fragment.Replace("#", ""));

			if(IsWOFF2(data))
				throw new NotSupportedException("WOFF is not supported.");

			NecessaryTables necessary;
			if(IsCollection(data))
				necessary = ReadCollectionTypeface(data, index);
			else
				necessary = ReadTypeface(data, 0);

			Tables = necessary.Tables;
			Maxp = necessary.Maxp;
			Head = necessary.Head;
			Hhea = necessary.Hhea;
			Hmtx = necessary.Hmtx;
		}

		private static bool IsWOFF2(ReadOnlySpan<byte> data)
			=> data.Slice(0, 4).SequenceEqual("wOF2"u8);

		private static bool IsCollection(ReadOnlySpan<byte> data)
			=> data.Slice(0, 4).SequenceEqual("ttcf"u8);

		private NecessaryTables ReadCollectionTypeface(ReadOnlySpan<byte> data, int index)
		{
			ReadOnlySpan<byte> ttcTag = data.Slice(0, 4);
			ushort majorVersion = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));
			ushort minorVersion = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6, 2));
			uint numFonts = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(8, 4));

			if(index >= numFonts || index < 0)
				throw new ArgumentOutOfRangeException(nameof(index));

			var offset = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(12 + index * 4, 4));
			return ReadTypeface(data, offset);
		}

		private NecessaryTables ReadTypeface(ReadOnlySpan<byte> data, uint offsetTablePos)
		{
			var tables = ReadOffsetTable(data.Slice((int)offsetTablePos));
			CalcAllTableChecksum(data, tables);

			var maxp = ReadMaxp(data, tables);
			var head = ReadHead(data, tables);
			var hhea = ReadHhea(data, tables);
			var hmtx = ReadHmtx(data, tables, hhea, maxp);
			Vhea = ReadVhea(data, tables);
			Vmtx = ReadVmtx(data, tables, Vhea, maxp);
			Gsub = ReadGsub(data, tables);
			Gpos = ReadGpos(data, tables);

			return new NecessaryTables(tables, maxp, head, hhea, hmtx);
		}

		private IReadOnlyDictionary<string, TableRecord> ReadOffsetTable(ReadOnlySpan<byte> data)
		{
			Version = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(0, 4));
			NumTables = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));
			SearchRange = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6, 2));
			EntrySelector = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(8, 2));
			RangeShift = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(10, 2));

			var tables = new Dictionary<string, TableRecord>();

			for(int i = 0; i < NumTables; i++) {
				var table = new TableRecord(data.Slice(12 + i * 16, 16));
				tables[table.TableTag] = table;
			}

			if(!TableNames.RequiedTables.All(p => tables.ContainsKey(p)))
				throw new NotSupportedException("This font file does not contain the required tables.");

			return new ReadOnlyDictionary<string, TableRecord>(tables);
		}

		private static void CalcAllTableChecksum(ReadOnlySpan<byte> data, IReadOnlyDictionary<string, TableRecord> tables)
		{
			foreach(var table in tables) {
				if(!CalcTableChecksum(data, table.Value))
					throw new NotSupportedException($"The checksums in the '{table.Key}' table did not match.");
			}
		}

		private static bool CalcTableChecksum(ReadOnlySpan<byte> data, TableRecord table)
		{
			uint sum = 0;

			if(table.TableTag == TableNames.HEAD) {
				for(int i = 0; i < table.Length; i += 4) {
					if(i != 8) {    // 'head' tableのcheckSumAdjustmentは無視する（すべてのチェックサム計算後に書き入れられた値なので）
						unchecked {
							sum += BinaryPrimitives.ReadUInt32BigEndian(data.Slice((int)(table.Offset + i), 4));
						}
					}
				}
			} else {
				for(int i = 0; i < table.Length; i += 4) {
					unchecked {
						sum += BinaryPrimitives.ReadUInt32BigEndian(data.Slice((int)(table.Offset + i), 4));
					}
				}
			}
			return sum == table.Checksum;
		}


		private MaxpTable ReadMaxp(ReadOnlySpan<byte> data, IReadOnlyDictionary<string, TableRecord> tables)
		{
			var table = tables[TableNames.MAXP];
			return new MaxpTable(data.Slice((int)table.Offset, (int)table.Length));
		}

		private HeadTable ReadHead(ReadOnlySpan<byte> data, IReadOnlyDictionary<string, TableRecord> tables)
		{
			var table = tables[TableNames.HEAD];
			return new HeadTable(data.Slice((int)table.Offset, (int)table.Length));
		}

		private HheaTable ReadHhea(ReadOnlySpan<byte> data, IReadOnlyDictionary<string, TableRecord> tables)
		{
			var table = tables[TableNames.HHEA];
			return new HheaTable(data.Slice((int)table.Offset, (int)table.Length));
		}

		private HmtxTable ReadHmtx(ReadOnlySpan<byte> data, IReadOnlyDictionary<string, TableRecord> tables, HheaTable hhea, MaxpTable maxp)
		{
			var table = tables[TableNames.HMTX];
			return new HmtxTable(data.Slice((int)table.Offset, (int)table.Length), hhea.NumberOfHMetrics, maxp.NumGlyphs);
		}

		private VheaTable? ReadVhea(ReadOnlySpan<byte> data, IReadOnlyDictionary<string, TableRecord> tables)
		{
			if(tables.TryGetValue(TableNames.VHEA, out var table))
				return new VheaTable(data.Slice((int)table.Offset, (int)table.Length));
			else
				return null;
		}

		private VmtxTable? ReadVmtx(ReadOnlySpan<byte> data, IReadOnlyDictionary<string, TableRecord> tables, VheaTable? vhea, MaxpTable maxp)
		{
			if(vhea != null && tables.TryGetValue(TableNames.VMTX, out var table))
				return new VmtxTable(data.Slice((int)table.Offset, (int)table.Length), vhea.NumberOfVerMetrics, maxp.NumGlyphs);
			else
				return null;
		}

		private GsubTable? ReadGsub(ReadOnlySpan<byte> data, IReadOnlyDictionary<string, TableRecord> tables)
		{
			if(tables.TryGetValue(TableNames.GSUB, out var table))
				return new GsubTable(data.Slice((int)table.Offset, (int)table.Length));
			else
				return null;
		}

		private GposTable? ReadGpos(ReadOnlySpan<byte> data, IReadOnlyDictionary<string, TableRecord> tables)
		{
			if(tables.TryGetValue(TableNames.GPOS, out var table))
				return new GposTable(data.Slice((int)table.Offset, (int)table.Length));
			else
				return null;
		}


		public uint Version { get; private set; }
		public ushort NumTables { get; private set; }
		public ushort SearchRange { get; private set; }
		public ushort EntrySelector { get; private set; }
		public ushort RangeShift { get; private set; }

		public IReadOnlyDictionary<string, TableRecord> Tables { get; }
		public MaxpTable Maxp { get; }
		public HeadTable Head { get; }
		public HheaTable Hhea { get; }
		public HmtxTable Hmtx { get; }
		public VheaTable? Vhea { get; private set; } = null;
		public VmtxTable? Vmtx { get; private set; } = null;
		public GsubTable? Gsub { get; private set; } = null;
		public GposTable? Gpos { get; private set; } = null;

		private class NecessaryTables
		{
			public NecessaryTables(IReadOnlyDictionary<string, TableRecord> tables, MaxpTable maxp, HeadTable head, HheaTable hhea, HmtxTable hmtx)
			{
				Tables = tables;
				Maxp = maxp;
				Head = head;
				Hhea = hhea;
				Hmtx = hmtx;
			}

			public IReadOnlyDictionary<string, TableRecord> Tables { get; }
			public MaxpTable Maxp { get; }
			public HeadTable Head { get; }
			public HheaTable Hhea { get; }
			public HmtxTable Hmtx { get; }
		}
	}
}
