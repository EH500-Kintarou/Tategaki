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
	internal class FontUriTable
	{
		static readonly SortedDictionary<string, Uri> _AllVerticalFonts;
		static readonly SortedDictionary<string, Uri> _AllAdvancedVerticalFonts;
		static readonly Dictionary<CultureInfo, IDictionary<string, Uri>> _CultureVerticalFonts;
		static readonly Dictionary<CultureInfo, IDictionary<string, Uri>> _CultureAdvancedVerticalFonts;

		internal static IDictionary<string, Uri> AllVerticalFonts => _AllVerticalFonts;
		internal static IDictionary<string, Uri> AllAdvancedVerticalFonts => _AllAdvancedVerticalFonts;
		internal static IDictionary<CultureInfo, IDictionary<string, Uri>> CultureVerticalFonts => _CultureVerticalFonts;
		internal static IDictionary<CultureInfo, IDictionary<string, Uri>> CultureAdvancedVerticalFonts => _CultureAdvancedVerticalFonts;

		static FontUriTable()
		{
			_AllVerticalFonts = new();
			_AllAdvancedVerticalFonts = new();
			_CultureVerticalFonts = new();
			_CultureAdvancedVerticalFonts = new();

			string fontDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
			var uris = Directory.GetFiles(fontDir, "*.ttf").Concat(Directory.GetFiles(fontDir, "*.otf")).Select(p => new Uri(p))
				.Concat(Directory.GetFiles(fontDir, "*.ttc").SelectMany(p => {
					using(var fs = new FileStream(p, FileMode.Open, FileAccess.Read)) {
						return Enumerable.Range(0, TypefaceInfo.GetCollectionCount(fs)).Select(i => new UriBuilder("file", "", -1, p, "#" + i).Uri);
					}
				})
			);

			foreach(Uri uri in uris) {
				try {
					var gtf = new GlyphTypeface(uri);

					foreach(var (culture, name) in gtf.FamilyNames) {
						if(culture == null || name == null) continue;

						int num = uri.Fragment == "" ? 0 : int.Parse(uri.Fragment.Replace("#", ""));
						var info = new TypefaceInfo(gtf.GetFontStream(), num);

						if(info.GetVerticalGlyphConverter().Count != 0) {
							_AllVerticalFonts[name] = uri;

							IDictionary<string, Uri> culturedic;

							if(_CultureVerticalFonts.ContainsKey(culture))
								culturedic = _CultureVerticalFonts[culture];
							else {
								culturedic = new Dictionary<string, Uri>();
								_CultureVerticalFonts[culture] = culturedic;
							}
							culturedic[name] = uri;
						}

						if(info.GetAdvancedVerticalGlyphConverter().Count != 0) {
							_AllAdvancedVerticalFonts[name] = uri;

							IDictionary<string, Uri> culturedic;

							if(_CultureAdvancedVerticalFonts.ContainsKey(culture))
								culturedic = _CultureAdvancedVerticalFonts[culture];
							else {
								culturedic = new Dictionary<string, Uri>();
								_CultureAdvancedVerticalFonts[culture] = culturedic;
							}
							culturedic[name] = uri;
						}
					}
				}
				catch(NullReferenceException) { }
			}
		}
	}
}
