namespace Tategaki.Logic.Font.Tables.GsubGpos
{
	internal class EmptySubstitution : IGlyphSubstitution
	{
		private EmptySubstitution() { }

		internal static readonly EmptySubstitution Empty = new EmptySubstitution();
	}
}
