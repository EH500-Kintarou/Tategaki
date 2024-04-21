using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic.Font.Tables.Metrix
{
	internal struct LongVerMetric
	{
		public LongVerMetric(short advanceHeight, short topSideBearing)
		{
			AdvanceHeight = advanceHeight;
			TopSideBearing = topSideBearing;
		}

		public short AdvanceHeight { get; }
		public short TopSideBearing { get; }
	}
}
