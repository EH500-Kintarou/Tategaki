using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.Metrix
{
	internal struct LongHorMetric
	{
		public LongHorMetric(short advanceWidth, short leftSideBearing)
		{
			AdvanceWidth = advanceWidth;
			LeftSideBearing = leftSideBearing;
		}

		public short AdvanceWidth { get; }
		public short LeftSideBearing { get; }
	}
}
