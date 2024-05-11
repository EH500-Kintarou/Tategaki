using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using Tategaki.Logic.Font;

namespace Tategaki.Logic
{
	internal class VerticalFontTable : IReadOnlyDictionary<string, FontNameInfo>
	{
		private VerticalFontTable() { }

		#region Font Loading

		readonly object tablelock = new();
		readonly Dictionary<string, FontNameInfo> internalTable = new();
		bool allLoaded = false;

		private void GetAllFontNameInfo()
		{
			if(allLoaded) return;

			Parallel.ForEach(Fonts.SystemFontFamilies, ff => GetFontNameInfo(ff.Source));

			allLoaded = true;
		}

		private FontNameInfo? GetFontNameInfo(string familyname)
		{
			lock(tablelock) {
				if(internalTable.TryGetValue(familyname, out var fni))
					return fni;
			}

			var tf = new Typeface(familyname);
			if(!tf.TryGetGlyphTypeface(out var gtf))
				return null;    // GlyphTypefaceが取得できなかった

			try {
				if(GsubLoader.HasVert(gtf.FontUri)) {       // GSUBに"vert"FeatureTagがあるか判定
					var fni = new FontNameInfo(gtf, familyname);

					lock(tablelock) {
						familyNames.Add(fni.OutstandingFamilyName);
						foreach(var name in fni.FamilierFamilyNames.Select(p => p.familyname).Distinct())
							internalTable[name] = fni;
					}

					return fni;
				}
			}
			catch(NotSupportedException) { }    // 中にはサポートできないフォントもある

			return null;
		}

		readonly List<string> familyNames = new();
		IReadOnlyList<string>? readOnlyFamilyNames = null;
		public IReadOnlyList<string> FamilyNames
		{
			get
			{
				if(readOnlyFamilyNames == null) {
					GetAllFontNameInfo();
					familyNames.Sort();
					readOnlyFamilyNames = familyNames.AsReadOnly();
				}
				return readOnlyFamilyNames;
			}
		}

		#endregion

		#region Dictionary Implement

		public FontNameInfo this[string key]
		{
			get
			{
				var fni = GetFontNameInfo(key);
				if(fni == null)
					throw new KeyNotFoundException($"Key \"{key}\" is not found");
				else
					return fni;
			}
		}

		public IEnumerable<string> Keys
		{
			get
			{
				GetAllFontNameInfo();
				return internalTable.Keys;
			}
		}

		public IEnumerable<FontNameInfo> Values
		{
			get
			{
				GetAllFontNameInfo();
				return internalTable.Values;
			}
		}

		public int Count
		{
			get
			{
				GetAllFontNameInfo();
				return internalTable.Count;
			}
		}

		public bool ContainsKey(string key)
		{
			return GetFontNameInfo(key) != null;
		}

#if NETCOREAPP3_0_OR_GREATER
		public bool TryGetValue(string key, [MaybeNullWhen(false)] out FontNameInfo value)
		{
			value = GetFontNameInfo(key);
			return value != null;
		}
#else
		public bool TryGetValue(string key, out FontNameInfo value)	// MaybeNullWhenは.NET 4.7.2では未対応
		{
			value = GetFontNameInfo(key)!;
			return value != null;
		}
#endif

		public FontNameInfo GetValueWithFallback(string? familyname)
		{
			// フォント名が無かったらデフォルトのフォントを返す
			if(familyname == null)
				return Default;

			// 一致するフォントが取得できればそれを返す
			var fni = GetFontNameInfo(familyname);
			if(fni != null)
				return fni;

			// 部分一致のフォントがあるか探す。あるなら文字数が最も近いのが優先。
			var include = Keys.Where(p => p.Contains(familyname) || familyname.Contains(p)).OrderBy(p => Math.Abs(p.Length - familyname.Length)).FirstOrDefault();
			if(include != null)
				return internalTable[include];
			else
				return Default;
		}

		public IEnumerator<KeyValuePair<string, FontNameInfo>> GetEnumerator()
		{
			GetAllFontNameInfo();
			return internalTable.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region Default Font

		private FontNameInfo Default
		{
			get
			{
				if(_Default == null) {
					var familyname = SystemFonts.MessageFontFamily.Source;
					var fni = GetFontNameInfo(familyname);

					if(fni != null)
						_Default = fni;
					else {
						GetAllFontNameInfo();
						_Default = internalTable.Values.First();
					}
				}
				return _Default;
			}
		}
		FontNameInfo? _Default = null;

		#endregion

		#region Singleton

		public static VerticalFontTable GetInstance()
		{
			if(_instance == null)
				_instance = new VerticalFontTable();

			return _instance;
		}
		private static VerticalFontTable? _instance = null;

		#endregion
	}
}
