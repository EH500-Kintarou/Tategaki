using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.GsubGpos
{
	internal abstract class TableBase
	{
		internal TableBase(ReadOnlySpan<byte> data)
		{
			MajorVersion = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
			MinorVersion = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
			ScriptListOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));
			FeatureListOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6, 2));
			LookupListOffset = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(8, 2));

			LangSysRecords = LoadLangSysRecords(data.Slice(ScriptListOffset));
			FeatureRecords = LoadFeatureRecords(data.Slice(FeatureListOffset));
			LookupRecords = LoadLookupRecords(data.Slice(LookupListOffset), LookupListOffset);
		}

		private static IReadOnlyList<LangSysRecord> LoadLangSysRecords(ReadOnlySpan<byte> sldata)
		{
			var ret = new List<LangSysRecord>();

			var scriptCount = BinaryPrimitives.ReadUInt16BigEndian(sldata.Slice(0, 2));
			for(int i = 0; i < scriptCount; i++) {
				var scriptTag = Encoding.UTF8.GetString(sldata.Slice(2 + i * 6, 4).ToArray());
				var scriptOffset = BinaryPrimitives.ReadUInt16BigEndian(sldata.Slice(6 + i * 6, 2));

				var sdata = sldata.Slice(scriptOffset);

				var defaultLangSys = BinaryPrimitives.ReadUInt16BigEndian(sdata.Slice(0, 2));
				var langSysCount = BinaryPrimitives.ReadUInt16BigEndian(sdata.Slice(2, 2));

				if(defaultLangSys != 0)
					ret.Add(new LangSysRecord(scriptTag, "DFLT", sdata.Slice(defaultLangSys)));

				for(int j = 0; j < langSysCount; j++) {
					var langSysTag = Encoding.UTF8.GetString(sdata.Slice(4 + j * 6, 4).ToArray());
					var langSysOffset = BinaryPrimitives.ReadUInt16BigEndian(sdata.Slice(8 + j * 6, 2));

					ret.Add(new LangSysRecord(scriptTag, langSysTag, sdata.Slice(langSysOffset)));
				}
			}

			return ret.AsReadOnly();
		}

		private static IReadOnlyList<FeatureRecord> LoadFeatureRecords(ReadOnlySpan<byte> fldata)
		{
			var featureCount = BinaryPrimitives.ReadUInt16BigEndian(fldata.Slice(0, 2));

			var ret = new FeatureRecord[featureCount];
			for(int i = 0; i < featureCount; i++) {
				var featureTag = Encoding.UTF8.GetString(fldata.Slice(2 + i * 6, 4).ToArray());
				var featureOffset = BinaryPrimitives.ReadUInt16BigEndian(fldata.Slice(6 + i * 6, 2));

				ret[i] = new FeatureRecord(featureTag, fldata.Slice(featureOffset));
			}

			return new ReadOnlyCollection<FeatureRecord>(ret);
		}

		private static IReadOnlyList<LookupRecord> LoadLookupRecords(ReadOnlySpan<byte> lldata, uint offset)
		{
			var lookupCount = BinaryPrimitives.ReadUInt16BigEndian(lldata.Slice(0, 2));

			var ret = new LookupRecord[lookupCount];
			for(int i = 0; i < lookupCount; i++) {
				var lookupOffset = BinaryPrimitives.ReadUInt16BigEndian(lldata.Slice(2 + i * 2, 2));

				ret[i] = new LookupRecord(lldata.Slice(lookupOffset), offset + lookupOffset);
			}

			return new ReadOnlyCollection<LookupRecord>(ret);
		}

		public ushort MajorVersion { get; }

		public ushort MinorVersion { get; }

		public ushort ScriptListOffset { get; }

		public ushort FeatureListOffset { get; }

		public ushort LookupListOffset { get; }

		public IReadOnlyList<LangSysRecord> LangSysRecords { get; }
		public IReadOnlyList<FeatureRecord> FeatureRecords { get; }
		public IReadOnlyList<LookupRecord> LookupRecords { get; }

		public IEnumerable<(string scriptTag, string langSysTag, string featureTag)> GetAllTags()
		{
			foreach(var lang in LangSysRecords) {
				foreach(var index in lang.FeatureIndices)
					yield return (lang.ScriptTag, lang.LangSysTag, FeatureRecords[index].FeatureTag);
			}
		}

		public IEnumerable<ushort> GetLookupListIndices(string scriptTag, string langSysTag, string featureTag)
		{
			return LangSysRecords
				.Where(p => p.ScriptTag == scriptTag && p.LangSysTag == langSysTag)
				.SelectMany(p => p.FeatureIndices)
				.Select(p => FeatureRecords[p])
				.Where(p => p.FeatureTag == featureTag)
				.SelectMany(p => p.LookupListIndices)
				.Distinct()
				.OrderBy(p => p);
		}

		public IEnumerable<ushort> GetLookupListIndices(string featureTag)
		{
			return FeatureRecords
				.Where(p => p.FeatureTag == featureTag)
				.SelectMany(p => p.LookupListIndices)
				.Distinct()
				.OrderBy(p => p);
		}

		internal static List<ushort> ReadCoverage(ReadOnlySpan<byte> data)
		{
			var ret = new List<ushort>();
			var coverageFormat = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));

			switch(coverageFormat) {
				case 1:
					var glyphCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
					for(int i = 0; i < glyphCount; i++)
						ret.Add(BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4 + i * 2, 2)));
					break;
				case 2:
					var rangeCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
					for(int i = 0; i < rangeCount; i++) {
						var startGlyphID = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4 + i * 6, 2));
						var endGlyphID = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6 + i * 6, 2));
						var startCoverageIndex = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(8 + i * 6, 2));

						for(ushort id = startGlyphID; id <= endGlyphID; id++)
							ret.Add(id);
					}
					break;
			}
			return ret;
		}
	}
}
