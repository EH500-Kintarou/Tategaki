using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WaterTrans.TypeLoader;

namespace Tategaki.Logic
{
	internal class VerticalFontTable
	{
		static VerticalFontTable()
		{
			var fonttable = new Dictionary<string, VerticalFontInfo>();
			var namelist = new List<string>();

			foreach(var ff in Fonts.SystemFontFamilies) {
				var tf = new Typeface(ff.Source);
				if(!tf.TryGetGlyphTypeface(out var gtf))	// GlyphTypefaceが取得できなければ用無し
					continue;

				int num = gtf.FontUri.Fragment == "" ? 0 : int.Parse(gtf.FontUri.Fragment.Replace("#", ""));
				var tfi = new TypefaceInfo(gtf.GetFontStream(), num);

				VerticalConverterType convtype = VerticalConverterType.None;
				if(tfi.GetVerticalGlyphConverter().Count > 0)
					convtype |= VerticalConverterType.Normal;
				if(tfi.GetAdvancedVerticalGlyphConverter().Count > 0)
					convtype |= VerticalConverterType.Advanced;
				if(convtype == VerticalConverterType.None)	// 縦書きコンバーターが取得できなければ用無し
					continue;

				var vfi = new VerticalFontInfo(gtf, ff.Source, convtype);
				namelist.Add(vfi.OutstandingFamilyName);
				foreach(var name in vfi.FamilierFamilyNames.Select(p => p.familyname).Distinct())
					fonttable[name] = vfi;
			}

			namelist.Sort();

			Table = new ReadOnlyDictionary<string, VerticalFontInfo>(fonttable);
			FamilyNames = new ReadOnlyCollection<string>(namelist);
		}

		internal static IReadOnlyDictionary<string, VerticalFontInfo> Table { get;  }
		internal static IReadOnlyList<string> FamilyNames { get; }


		/// <summary>
		/// フォント名からUriを取得するメソッド
		/// 存在しない場合は適当なフォントにフォールバックする。
		/// </summary>
		/// <param name="name">フォント名</param>
		/// <returns>フォント情報</returns>
		internal static VerticalFontInfo FromName(string? name)
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
