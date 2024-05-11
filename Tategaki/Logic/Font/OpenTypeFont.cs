using Tategaki.Logic.Font.Tables;
using Tategaki.Logic.Font.Tables.Base;
using Tategaki.Logic.Font.Tables.Glyph;
using Tategaki.Logic.Font.Tables.GsubGpos;
using Tategaki.Logic.Font.Tables.Head;
using Tategaki.Logic.Font.Tables.Maxp;

namespace Tategaki.Logic.Font
{
	/// <summary>
	/// OpenTypeのフォントを読み込むクラス
	/// 参考1： https://learn.microsoft.com/en-us/typography/opentype/spec/
	/// 参考2： https://aznote.jakou.com/prog/opentype/index.html
	/// </summary>
	internal class OpenTypeFont : FontLoaderBase
	{
		readonly static string[] TableFilter = { TableNames.MAXP, TableNames.HEAD, TableNames.LOCA, TableNames.GSUB, TableNames.GPOS, TableNames.GLYF, TableNames.BASE };

		public OpenTypeFont(Uri fontUri) : base(fontUri, out var necessary, TableFilter, false)
		{
			if(necessary.Maxp == null || necessary.Head == null)
				throw new NotSupportedException("MAXP or HEAD table does not exists");

			Maxp = necessary.Maxp;
			Head = necessary.Head;
		}

		protected override void ReadArbitraryTables(ReadOnlySpan<byte> data, string tableName, NecessaryTables necessary)
		{
			switch(tableName) {
				case TableNames.LOCA:
					Loca = new LocaTable(data, necessary.Maxp!.NumGlyphs, necessary.Head!.IndexToLocFormat);
					break;
				case TableNames.GSUB:
					Gsub = new GsubTable(data);
					break;
				case TableNames.GPOS:
					Gpos = new GposTable(data);
					break;
				case TableNames.GLYF:
					if(Loca != null)
						Glyf = new GlyfTable(data, Loca);
					break;
				case TableNames.BASE:
					Base = new BaseTable(data);
					break;
			}
		}

		public MaxpTable Maxp { get; }
		public HeadTable Head { get; }
		public LocaTable? Loca { get; private set; }
		public GsubTable? Gsub { get; private set; }
		public GposTable? Gpos { get; private set; }
		public GlyfTable? Glyf { get; private set; }
		public BaseTable? Base { get; private set; }
	}
}
