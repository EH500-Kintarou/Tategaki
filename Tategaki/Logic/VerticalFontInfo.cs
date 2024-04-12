﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Tategaki.Logic
{
	internal class VerticalFontInfo
	{
		public VerticalFontInfo(GlyphTypeface gtf, string sourceName, VerticalConverterType convType)
		{
			if(!convType.HasFlag(VerticalConverterType.Normal) && !convType.HasFlag(VerticalConverterType.Advanced))
				throw new ArgumentOutOfRangeException($"{nameof(convType)} must contain Normal or Advanced.", nameof(convType));

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

			if(!string.IsNullOrEmpty(curculname))
				OutstandingFamilyName = curculname;
			else if(!string.IsNullOrEmpty(invculname))
				OutstandingFamilyName = invculname;
			else if(FamilierFamilyNames.Count > 0)
				OutstandingFamilyName = FamilierFamilyNames[0].familyname;
			else
				throw new ArgumentException("No familier FamilyName was found");

			FontUri = gtf.FontUri;
			FamilyNames = new ReadOnlyDictionary<CultureInfo, string>(new Dictionary<CultureInfo, string>(gtf.FamilyNames));
			FaceNames = new ReadOnlyDictionary<CultureInfo, string>(new Dictionary<CultureInfo, string>(gtf.FaceNames));
			ConverterType = convType;
		}

		/// <summary>
		/// フォントのURI
		/// </summary>
		public Uri FontUri { get; }

		/// <summary>
		/// 代表的なフォント名（現在カルチャーでの名前→Invaliant Cultureでの名前→それ以外の何か　の優先順位で何か名前が入る）
		/// </summary>
		public string OutstandingFamilyName { get; }

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

		/// <summary>
		/// このフォントが保有している縦書きコンバーターの種類
		/// </summary>
		public VerticalConverterType ConverterType { get; }
	}

	[Flags]
	internal enum VerticalConverterType : uint
	{
		None = 0,
		Normal = 1,
		Advanced = 2,
	}
}