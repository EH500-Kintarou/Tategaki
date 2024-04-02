using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WaterTrans.TypeLoader;

namespace Tategaki.Logic
{
	internal static class Util
	{
		static Cache cache = new Cache(SearchFontNamePathPair(new[] { CultureInfo.CurrentUICulture }));

		#region Font Family and Uri

		public static string[] AvailableFonts
		{
			get { return cache.AvailableFonts; }
		}

		public static Uri[] AvailableFontUris
		{
			get { return cache.AvailableFontUris; }
		}

		public static Uri GetFontUri(string FamilyName)
		{
			return cache.FontUriDictionary[FamilyName];
		}

		/// <summary>
		/// 有効なフォントの一覧を取得するメソッド
		/// </summary>
		/// <param name="cultures">フォントのカルチャの配列</param>
		/// <returns>ファミリ名とstringのDictionary</returns>
		internal static IDictionary<string, Uri> SearchFontNamePathPair(IEnumerable<CultureInfo> cultures)
		{
			IDictionary<string, Uri> dic = new SortedDictionary<string, Uri>();
			string FontDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

			var uris = Directory.GetFiles(FontDir, "*.ttf").Concat(Directory.GetFiles(FontDir, "*.otf")).Select(p => new Uri(p))
				.Concat(Directory.GetFiles(FontDir, "*.ttc").SelectMany(p => {
					using(var fs = new FileStream(p, FileMode.Open, FileAccess.Read)) {
						return Enumerable.Range(0, TypefaceInfo.GetCollectionCount(fs)).Select(i => new UriBuilder("file", "", -1, p, "#" + i).Uri);
					}
				})
			);

			foreach(Uri uri in uris) {
				try {
					GlyphTypeface gtf = new GlyphTypeface(uri);
					if(cultures.Where(p => gtf.FamilyNames.ContainsKey(p)).Count() > 0) {
						foreach(string FamilyName in gtf.FamilyNames.Values) {
							if(!dic.ContainsKey(FamilyName))
								dic.Add(FamilyName, uri);
						}
					}
				}
				catch(NullReferenceException) { }
			}

			return dic;
		}

		#endregion

		#region Glyph Indices

		/// <summary>
		/// Glyphsクラス用のIndicesを取得するメソッド
		/// </summary>
		/// <param name="Text">テキスト</param>
		/// <param name="FontFamilyName">ファミリ名</param>
		/// <param name="Spacing">Spacing（100で標準的な値）</param>
		/// <returns>Indicesのテキスト</returns>
		public static string GetIndices(string Text, string FontFamilyName, double Spacing)
		{
			ushort[] glyphs = GetVerticalGlyphIndex(Text, FontFamilyName);

			IEnumerable<string> IndicesTexts = glyphs.Select((p, i) => {
				StringBuilder sb = new StringBuilder();
				sb.Append(p);

				string option;
				if(i < glyphs.Length - 1)
					option = ",{0}";
				else
					option = ",";

				sb.AppendFormat(option, cache.GlyphConverters[FontFamilyName].GlyphTypeface.AdvanceHeights[p] * Spacing);

				return sb.ToString();
			}).ToArray();

			return string.Join(";", IndicesTexts);
		}

		/// <summary>
		/// 縦書きのグリフインデックスを取得するメソッド
		/// </summary>
		/// <param name="Text">取得したいテキスト</param>
		/// <param name="FontFamilyName">フォントファミリ名</param>
		/// <returns>グリフインデックスの配列</returns>
		public static ushort[] GetVerticalGlyphIndex(string Text, string FontFamilyName)
		{
			IDictionary<char, ushort> dic = cache.GetGlyphIndicesDictionary(FontFamilyName);

			string text = new string(Text.Distinct().Except(dic.Keys).ToArray());

			if(text.Length > 0) {
				foreach(var p in text.Zip(InternalGetVerticalGlyphIndex(text, FontFamilyName), (c, i) => new { Char = c, GlyphIndex = i }))
					dic.Add(p.Char, p.GlyphIndex);
			}

			return Text.Select(p => dic[p]).ToArray();
		}

		static ushort[] InternalGetVerticalGlyphIndex(string Text, string FontFamilyName)
		{
			GlyphTypeface gtf;
			SingleGlyphConverter vert;

			if(!cache.GlyphConverters.ContainsKey(FontFamilyName))
				cache.GlyphConverters.Add(FontFamilyName, new GlyphConverters(cache.FontUriDictionary[FontFamilyName]));

			gtf = cache.GlyphConverters[FontFamilyName].GlyphTypeface;
			vert = cache.GlyphConverters[FontFamilyName].VerticalConverter;

			return Text.Select(p => vert.Convert(gtf.CharacterToGlyphMap[gtf.CharacterToGlyphMap.ContainsKey(p) ? p : '?'])).ToArray();
		}

		/// <summary>
		/// グリフインデックスのキャッシュを作っておくメソッド
		/// </summary>
		/// <param name="Text">テキスト</param>
		/// <param name="FontFamilyName">フォントファミリ名</param>
		public static void MakeCache(string Text, string FontFamilyName)
		{
			if(!string.IsNullOrEmpty(Text))
				GetVerticalGlyphIndex(Text.Replace("\n", ""), FontFamilyName);
		}

		#endregion
	}
}
