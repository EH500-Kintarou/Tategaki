using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.GsubGpos
{
	internal class GsubTable : TableBase
	{
		internal GsubTable(ReadOnlySpan<byte> data) : base(data)
		{
			var stss = new IReadOnlyList<IGlyphSubstitution>[LookupRecords.Count];
			for(int i = 0; i < LookupRecords.Count; i++) {
				var record = LookupRecords[i];
				var sts = new IGlyphSubstitution[record.SubtableOffsets.Count];

				for(int j = 0; j < record.SubtableOffsets.Count; j++) {
					var stdata = data.Slice((int)(record.SubtableOffsets[j] + record.SelfOffset));

					sts[j] = (GsubLookupType)record.LookupType switch {
						GsubLookupType.SingleSubstitution => new SingleSubstitution(stdata),
						// GsubLookupType.MultipleSubstitution => new MultipleSubstitution(stdata),
						// GsubLookupType.AlternateSubstitution => new AlternateSubstitution(stdata),
						// GsubLookupType.LigatureSubstitution => new LigatureSubstitution(stdata),
						// GsubLookupType.ContexualSubstitution => new ContexualSubstitution(stdata),
						// GsubLookupType.ChainingContexualSubstitution => new ChainingContexualSubstitution(stdata),
						// GsubLookupType.ExtensionSubstitution => new ExtensionSubstitution(stdata),
						// GsubLookupType.ReverseChainingContexualSingleSubstitution => new ReverseChainingContexualSingleSubstitution(stdata),
						_ => EmptySubstitution.Empty,
					};
				}
				stss[i] = new ReadOnlyCollection<IGlyphSubstitution>(sts);
			}
			Subtables = new ReadOnlyCollection<IReadOnlyList<IGlyphSubstitution>>(stss);
		}

		public IReadOnlyList<IReadOnlyList<IGlyphSubstitution>> Subtables { get; }

		public IEnumerable<IGlyphSubstitution> GetSubstitution(string scriptTag, string langSysTag, string featureTag)
		{
			return GetLookupListIndices(scriptTag, langSysTag, featureTag)
				.SelectMany(p => Subtables[p]);
		}

		public IEnumerable<T> GetSubstitution<T>(string scriptTag, string langSysTag, string featureTag) where T : IGlyphSubstitution
		{
			return GetLookupListIndices(scriptTag, langSysTag, featureTag)
				.SelectMany(p => Subtables[p])
				.OfType<T>()
				.Where(t => t != null);
		}

		public IEnumerable<IGlyphSubstitution> GetSubstitution(string featureTag)
		{
			return GetLookupListIndices(featureTag)
				.SelectMany(p => Subtables[p]);
		}

		public IEnumerable<T> GetSubstitution<T>(string featureTag) where T : IGlyphSubstitution
		{
			return GetLookupListIndices(featureTag)
				.SelectMany(p => Subtables[p])
				.OfType<T>()
				.Where(t => t != null);
		}
	}
}
