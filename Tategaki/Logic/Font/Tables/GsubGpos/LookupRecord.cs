using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.GsubGpos
{
	internal class LookupRecord
	{
		internal LookupRecord(ReadOnlySpan<byte> data, uint offset)
		{
			LookupType = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2));
			LookupFlag = (LookupFlag)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(2, 2));
			SubTableCount = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4, 2));

			var offsets = new ushort[SubTableCount];
			for(int i = 0; i < SubTableCount; i++)
				offsets[i] = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6 + i * 2, 2));
			SubtableOffsets = new ReadOnlyCollection<ushort>(offsets);

			if(LookupFlag.HasFlag(LookupFlag.UseMarkFilteringSet))
				MarkFilteringSet = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(6 + SubTableCount * 2, 2));

			SelfOffset = offset;
		}

		public ushort LookupType { get; }
		public LookupFlag LookupFlag { get; }
		public ushort SubTableCount { get; }
		public IReadOnlyList<ushort> SubtableOffsets { get; }
		public ushort MarkFilteringSet { get; }
		public uint SelfOffset { get; }

		public override string ToString()
		{
			return $"(LookupType = {LookupType}, {LookupFlag}, SubtableOffsets = {{{string.Join(", ", SubtableOffsets)}}}, MarkFilteringSet = {MarkFilteringSet})";
		}
	}

	[Flags]
	public enum LookupFlag : ushort
	{
		NoFlag = 0x0000,
		RightToLeft = 0x0001,
		IgnoreBaseGlyphs = 0x0002,
		IgnoreLigatures = 0x0004,
		IgnoreMarks = 0x0008,
		UseMarkFilteringSet = 0x0010,
		Reserved = 0x00E0,
		MarkAttachmentType = 0xFF00,
	}

	public enum GsubLookupType : ushort
	{
		SingleSubstitution = 1,
		MultipleSubstitution = 2,
		AlternateSubstitution = 3,
		LigatureSubstitution = 4,
		ContexualSubstitution = 5,
		ChainingContexualSubstitution = 6,
		ExtensionSubstitution = 7,
		ReverseChainingContexualSingleSubstitution = 8,
	}

	public enum GposLookupType : ushort
	{
		SingleAdjustment = 1,
		PairAdjustment = 2,
		CursiveAttachment = 3,
		MarkToBaseAttachment = 4,
		MarkToLigatureAttachment = 5,
		MarkToMarkAttachment = 6,
		ContextPositioning = 7,
		ChainedContextPositioning = 8,
		ExtensionPositioning = 9,
	}
}
