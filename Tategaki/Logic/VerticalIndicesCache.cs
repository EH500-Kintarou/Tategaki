using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WaterTrans.TypeLoader;
using static System.Net.Mime.MediaTypeNames;

namespace Tategaki.Logic
{
	/// <summary>
	/// 縦書きグリフインデックスのキャッシュ
	/// </summary>
	internal class VerticalIndicesCache
	{
		readonly SingleGlyphConverter conv;
		readonly GlyphTypeface gtf;
		readonly SortedDictionary<char, ushort> indices = new();

		internal VerticalIndicesCache(Uri fontUri, bool advanced = false)
		{
			FontUri = fontUri;
			Advanced = advanced;

			gtf = new GlyphTypeface(fontUri);
			int num = fontUri.Fragment == "" ? 0 : int.Parse(fontUri.Fragment.Replace("#", ""));
			var info = new TypefaceInfo(gtf.GetFontStream(), num);

			if(advanced)
				conv = info.GetAdvancedVerticalGlyphConverter();
			else
				conv = info.GetVerticalGlyphConverter();
		}

		internal Uri FontUri { get; }

		internal bool Advanced { get; }

		internal ushort GetIndex(char c)
		{
			if(!indices.TryGetValue(c, out ushort index)) {
				index = conv.Convert(gtf.CharacterToGlyphMap[gtf.CharacterToGlyphMap.ContainsKey(c) ? c : '?']);
				indices.Add(c, index);
			}
			return index;
		}
		
		internal IList<ushort> GetIndices(string text)
		{
			var ret = new ushort[text.Length];

			for(int i = 0; i < ret.Length; i++)
				ret[i] = GetIndex(text[i]);
			
			return ret;
		}

		internal void Clear()
		{
			indices.Clear();
		}

		static readonly Dictionary<(Uri, bool advanced), VerticalIndicesCache> cachedic = new();

		internal static VerticalIndicesCache GetCache(Uri uri, bool advanced = false)
		{
			var pair = (uri, advanced);
			if(!cachedic.TryGetValue(pair, out var dic)) {
				dic = new VerticalIndicesCache(uri, advanced);
				cachedic[pair] = dic;
			}
			return dic;
		}
	}
}
