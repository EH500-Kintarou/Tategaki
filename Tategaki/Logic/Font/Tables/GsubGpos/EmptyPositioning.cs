using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.GsubGpos
{
	internal class EmptyPositioning : IGlyphPositioning
	{
		private EmptyPositioning() { }

		internal static readonly EmptyPositioning Empty = new EmptyPositioning();
	}
}
