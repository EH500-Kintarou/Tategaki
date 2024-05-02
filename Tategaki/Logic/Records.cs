namespace Tategaki.Logic
{
	internal record struct GlyphAdjustment(double XPlacement, double YPlacement, double XAdvance, double YAdvance);
	internal record struct GlyphMetrics(double XMin, double YMin, double XMax, double YMax);
	internal record struct DecorationMetrics(double OverlinePos, double StrikethroughPos, double BaselinePos, double UnderlinePos);
}
