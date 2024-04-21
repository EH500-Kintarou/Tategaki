using System;
using System.Buffers.Binary;
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

				for(int j = 0; j < record.SubtableOffsets.Count; j++)
					sts[j] = LoadSubstitution(data.Slice((int)(record.SubtableOffsets[j] + record.SelfOffset)), (GsubLookupType)record.LookupType);

				stss[i] = new ReadOnlyCollection<IGlyphSubstitution>(sts);
			}
			Subtables = new ReadOnlyCollection<IReadOnlyList<IGlyphSubstitution>>(stss);
		}

		IGlyphSubstitution LoadExtensionSubstitution(ReadOnlySpan<byte> data)
		{
			var substFormat = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
			var extensionLookupType = (GsubLookupType)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
			var extensionOffset = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(4, 4));

			if(extensionLookupType == GsubLookupType.ExtensionSubstitution)
				return EmptySubstitution.Empty; // 無限ループ防止
			else
				return LoadSubstitution(data.Slice((int)extensionOffset), extensionLookupType);
		}

		IGlyphSubstitution LoadSubstitution(ReadOnlySpan<byte> stdata, GsubLookupType lookup)
		{
			return lookup switch {
				GsubLookupType.SingleSubstitution => new SingleSubstitution(stdata),
				// GsubLookupType.MultipleSubstitution => new MultipleSubstitution(stdata),
				// GsubLookupType.AlternateSubstitution => new AlternateSubstitution(stdata),
				// GsubLookupType.LigatureSubstitution => new LigatureSubstitution(stdata),
				// GsubLookupType.ContexualSubstitution => new ContexualSubstitution(stdata),
				// GsubLookupType.ChainingContexualSubstitution => new ChainingContexualSubstitution(stdata),
				GsubLookupType.ExtensionSubstitution => LoadExtensionSubstitution(stdata),
				// GsubLookupType.ReverseChainingContexualSingleSubstitution => new ReverseChainingContexualSingleSubstitution(stdata),
				_ => EmptySubstitution.Empty,
			};
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
