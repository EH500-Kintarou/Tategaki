using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Tategaki.Logic
{
	/// <summary>
	/// フォント名情報（カルチャー等の対応）を格納するクラス
	/// </summary>
	internal class FontNameInfo : IEquatable<FontNameInfo>
	{
		public FontNameInfo(GlyphTypeface gtf, string sourceName)
		{
			if(gtf.FamilyNames.Select(p => p.Value).Contains(sourceName))    // FamilyNameがそのまま含まれていたら、gtf.FamilyNamesをそのまま採用
				FamilierFamilyNames = gtf.FamilyNames.Select(p => (p.Key, p.Value)).ToList().AsReadOnly();
			else {
				var family_face = gtf.FamilyNames
					.Select(p => (culture: p.Key, family: (string?)p.Value, face: (string?)null))
					.Concat(gtf.FaceNames.Select(p => (culture: p.Key, family: (string?)null, face: (string?)p.Value)))
					.GroupBy(p => p.culture)
					.Select(p => (culture: p.Key, family: p.Select(p1 => p1.family).FirstOrDefault(p1 => p1 != null), face: p.Select(p1 => p1.face).FirstOrDefault(p1 => p1 != null)))
					.Where(p => p.family != null && p.face != null)
					.Select(p => (p.culture, familyname: $"{p.family} {p.face}"))
					.ToList();

				if(!family_face.Select(p => p.familyname).Contains(sourceName))
					family_face.Insert(0, (CultureInfo.InvariantCulture, sourceName));

				FamilierFamilyNames = family_face.AsReadOnly();
			}

			var curculname = FamilierFamilyNames.FirstOrDefault(p => p.culture.Equals(CultureInfo.CurrentCulture)).familyname;
			var invculname = FamilierFamilyNames.FirstOrDefault(p => p.culture.Equals(CultureInfo.InvariantCulture)).familyname;
			var enusculname = FamilierFamilyNames.FirstOrDefault(p => p.culture.Equals(CultureInfo.GetCultureInfo("en-us"))).familyname;
			var firstname = FamilierFamilyNames.FirstOrDefault().familyname;

			var outstanding = new[] { curculname, invculname, enusculname, firstname };
			var invaliant = new[] { invculname, enusculname, curculname, firstname };

			OutstandingFamilyName = outstanding.Where(p => !string.IsNullOrEmpty(p)).FirstOrDefault() ?? throw new ArgumentException("No outstanding FamilyName is found");
			InvaliantFamilyName = invaliant.Where(p => !string.IsNullOrEmpty(p)).FirstOrDefault() ?? throw new ArgumentException("No invaliant FamilyName is found");

			FamilyNames = new ReadOnlyDictionary<CultureInfo, string>(new Dictionary<CultureInfo, string>(gtf.FamilyNames));
			FaceNames = new ReadOnlyDictionary<CultureInfo, string>(new Dictionary<CultureInfo, string>(gtf.FaceNames));
		}

		/// <summary>
		/// 代表的なフォント名（現在カルチャーでの名前→Invaliant Cultureでの名前→英語での名前→それ以外の何か　の優先順位で何か名前が入る）
		/// </summary>
		public string OutstandingFamilyName { get; }

		/// <summary>
		/// 普遍的なフォント名（Invaliant Cultureでの名前→英語での名前→現在カルチャーでの名前→それ以外の何か　の優先順位で何か名前が入る）
		/// </summary>
		public string InvaliantFamilyName { get; }

		/// <summary>
		/// FontFamily名から検索を掛けるとヒットする名前
		/// </summary>
		public IReadOnlyList<(CultureInfo culture, string familyname)> FamilierFamilyNames { get; }

		/// <summary>
		/// GlyphTypeFaceのFamilyNamesそのまま
		/// </summary>
		public IReadOnlyDictionary<CultureInfo, string> FamilyNames { get; }

		/// <summary>
		/// GlyphTypeFaceのFaceNamesそのまま
		/// </summary>
		public IReadOnlyDictionary<CultureInfo, string> FaceNames { get; }

		public bool Equals(FontNameInfo? other)
		{
			if(other == null)
				return false;

			return OutstandingFamilyName == other.OutstandingFamilyName;
		}
	}
}
