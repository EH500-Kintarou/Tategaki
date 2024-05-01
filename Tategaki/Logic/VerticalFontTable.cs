using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Tategaki.Logic.Font;
using Tategaki.Logic.Font.Tables.GsubGpos;

namespace Tategaki.Logic
{
	internal class VerticalFontTable
	{
		static VerticalFontTable()
		{
			var fonttable = new Dictionary<string, FontNameInfo>();
			var namelist = new List<string>();
			var lockobj = new object();

			Parallel.ForEach(Fonts.SystemFontFamilies, ff => {
				var tf = new Typeface(ff.Source);
				if(!tf.TryGetGlyphTypeface(out var gtf))
					return;     // GlyphTypefaceが取得できなければ用無し

				try {
					var otf = new OpenTypeFont(gtf.FontUri);

					if((otf.Gsub?.FeatureRecords ?? Enumerable.Empty<FeatureRecord>()).Where(p => p.FeatureTag == "vert").Any()) {     // GSUBに"vert" FeatureTagがあるか判定
						var vfi = new FontNameInfo(gtf, ff.Source);

						lock(lockobj) {
							namelist.Add(vfi.OutstandingFamilyName);
							foreach(var name in vfi.FamilierFamilyNames.Select(p => p.familyname).Distinct())
								fonttable[name] = vfi;
						}
					}
				}
				catch(NotSupportedException) { }    // 中にはサポートできないフォントもある
			});

			namelist.Sort();

			Table = new ReadOnlyDictionary<string, FontNameInfo>(fonttable);
			FamilyNames = new ReadOnlyCollection<string>(namelist);
		}

		internal static IReadOnlyDictionary<string, FontNameInfo> Table { get;  }
		internal static IReadOnlyList<string> FamilyNames { get; }


		/// <summary>
		/// フォント名からUriを取得するメソッド
		/// 存在しない場合は適当なフォントにフォールバックする。
		/// </summary>
		/// <param name="name">フォント名</param>
		/// <returns>フォント情報</returns>
		internal static FontNameInfo FromName(string? name)
		{
			if(name == null)
				return Table[FamilyNames.First()];

			if(Table.TryGetValue(name, out var info))
				return info;
			else {
				var include = Table.Keys.Where(p => p.Contains(name) || name.Contains(p)).FirstOrDefault();
				if(include != null)
					return Table[include];
				else
					return Table.First().Value;
			}
		}
	}
}
