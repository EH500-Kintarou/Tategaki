using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.GsubGpos
{
	internal class EmptySubstitution : IGlyphSubstitution
	{
		private EmptySubstitution() { }

		internal static readonly EmptySubstitution Empty = new EmptySubstitution();
	}
}
