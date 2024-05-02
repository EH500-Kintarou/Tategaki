using System.Collections.ObjectModel;

namespace Tategaki.Logic.Font.Tables.GsubGpos
{
	internal class GposTable : TableBase
	{
		internal GposTable(ReadOnlySpan<byte> data) : base(data)
		{
			var stss = new IReadOnlyList<IGlyphPositioning>[LookupRecords.Count];
			for(int i = 0; i < LookupRecords.Count; i++) {
				var record = LookupRecords[i];
				var sts = new IGlyphPositioning[record.SubtableOffsets.Count];

				for(int j = 0; j < record.SubtableOffsets.Count; j++) {
					var stdata = data.Slice((int)(record.SubtableOffsets[j] + record.SelfOffset));

					sts[j] = (GposLookupType)record.LookupType switch {
						GposLookupType.SingleAdjustment => new SingleAdjustment(stdata),
						// GposLookupType.PairAdjustment => new PairAdjustment(stdata),
						// GposLookupType.CursiveAttachment => new CursiveAttachment(stdata),
						// GposLookupType.MarkToBaseAttachment => new MarkToBaseAttachment(stdata),
						// GposLookupType.MarkToLigatureAttachment => new MarkToLigatureAttachment(stdata),
						// GposLookupType.MarkToMarkAttachment => new MarkToMarkAttachment(stdata),
						// GposLookupType.ContextPositioning => new ContextPositioning(stdata),
						// GposLookupType.ChainedContextPositioning => new ChainedContextPositioning(stdata),
						// GposLookupType.ExtensionPositioning => new ExtensionPositioning(stdata),
						_ => EmptyPositioning.Empty,
					};
				}
				stss[i] = new ReadOnlyCollection<IGlyphPositioning>(sts);
			}
			Subtables = new ReadOnlyCollection<IReadOnlyList<IGlyphPositioning>>(stss);
		}

		public IReadOnlyList<IReadOnlyList<IGlyphPositioning>> Subtables { get; }

		public IEnumerable<IGlyphPositioning> GetPositioning(string scriptTag, string langSysTag, string featureTag)
		{
			return GetLookupListIndices(scriptTag, langSysTag, featureTag)
				.SelectMany(p => Subtables[p]);
		}

		public IEnumerable<T> GetPositioning<T>(string scriptTag, string langSysTag, string featureTag) where T : IGlyphPositioning
		{
			return GetLookupListIndices(scriptTag, langSysTag, featureTag)
				.SelectMany(p => Subtables[p])
				.OfType<T>()
				.Where(t => t != null);
		}

		public IEnumerable<IGlyphPositioning> GetPositioning(string featureTag)
		{
			return GetLookupListIndices(featureTag)
				.SelectMany(p => Subtables[p]);
		}

		public IEnumerable<T> GetPositioning<T>(string featureTag) where T : IGlyphPositioning
		{
			return GetLookupListIndices(featureTag)
				.SelectMany(p => Subtables[p])
				.OfType<T>()
				.Where(t => t != null);
		}

	}
}
