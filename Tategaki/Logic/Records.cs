using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tategaki.Logic
{
	internal record struct GlyphAdjustment(double XPlacement, double YPlacement, double XAdvance, double YAdvance);
	internal record struct GlyphMetrics(double XMin, double YMin, double XMax, double YMax);
}
