namespace Tategaki.Logic.Font.Tables.GsubGpos
{
	internal class EmptyPositioning : IGlyphPositioning
	{
		private EmptyPositioning() { }

		internal static readonly EmptyPositioning Empty = new EmptyPositioning();
	}
}
