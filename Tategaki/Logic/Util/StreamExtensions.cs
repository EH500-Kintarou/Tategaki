using System.IO;

namespace Tategaki.Logic.Util
{
    internal static class StreamExtensions
    {
#if !NET7_0_OR_GREATER
		public static void ReadExactly(this Stream stream, byte[] buffer, int offset, int count)
		{
			var read = stream.Read(buffer, offset, count);
			if(read < count)
				throw new EndOfStreamException($"The end of the stream is reached before reading {nameof(count)} number of bytes.");
		}
#endif
	}
}
