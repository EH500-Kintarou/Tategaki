using Tategaki.Logic.Font.Tables;
using Tategaki.Logic.Font.Tables.GsubGpos;

namespace Tategaki.Logic.Font
{
	internal class GsubLoader : FontLoaderBase
	{
		readonly static string[] TableFilter = { TableNames.HEAD, TableNames.GSUB };	// Magic Numberの検証のためにHEADテーブルも読み込んでおく

		public GsubLoader(Uri fontUri) : base(fontUri, out var _, TableFilter, false) { }

		protected override void ReadArbitraryTables(ReadOnlySpan<byte> data, string tableName, NecessaryTables necessary)
		{
			switch (tableName) {
				case TableNames.GSUB:
					Gsub = new GsubTable(data);
					break;
			};
		}

		public GsubTable? Gsub { get; private set; }

		/// <summary>
		/// vert FeatureTagを持ったフォントファイルかを確かめるメソッド
		/// すべてのフォントデータを読み込んでいると処理が重いため、vertタグがあるかどうかに特化した読み込みを行う。
		/// </summary>
		/// <param name="fontUri">フォントファイルのURI</param>
		/// <returns>vertタグを持っているかどうか。</returns>
		/// <exception cref="NotSupportedException">サポートしていないフォント</exception>
		public static bool HasVert(Uri fontUri)
		{
			var font = new GsubLoader(fontUri);

			if(font.Gsub == null)
				return false;
			else
				return (font.Gsub.FeatureRecords ?? Enumerable.Empty<FeatureRecord>()).Where(p => p.FeatureTag == "vert").Any();
		}
	}
}
