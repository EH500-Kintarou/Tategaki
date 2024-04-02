using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic
{
	internal class Cache
	{
		public Cache(IDictionary<string, Uri> FontUriDictionary)
		{
			this.FontUriDictionary = FontUriDictionary;
		}


		#region Font Family and Uri

		/// <summary>
		/// フォントファミリ名とUriを結び付けたディクショナリー
		/// </summary>
		internal IDictionary<string, Uri> FontUriDictionary { get; private set; }

		internal string[] AvailableFonts
		{
			get { return FontUriDictionary.Keys.ToArray(); }
		}

		#endregion

		#region Glyph

		/// <summary>
		/// フォントファミリ名とGlyph変換を結び付けたディクショナリー
		/// </summary>
		internal IDictionary<string, GlyphConverters> GlyphConverters
		{
			get { return glyphConverters; }
		}
		Dictionary<string, GlyphConverters> glyphConverters = new Dictionary<string, GlyphConverters>();

		/// <summary>
		/// 文字とグリフインデックスを変換するディクショナリーを取得するメソッド
		/// </summary>
		/// <param name="FamilyName">フォントファミリ名</param>
		/// <returns>ディクショナリー</returns>
		internal IDictionary<char, ushort> GetGlyphIndicesDictionary(string FamilyName)
		{
			if(!glyphIndices.ContainsKey(FamilyName))
				glyphIndices.Add(FamilyName, new Dictionary<char, ushort>());

			return glyphIndices[FamilyName];
		}
		Dictionary<string, Dictionary<char, ushort>> glyphIndices = new Dictionary<string, Dictionary<char, ushort>>();

		#endregion
	}
}
