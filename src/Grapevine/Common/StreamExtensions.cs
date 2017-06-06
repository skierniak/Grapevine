using System.IO;
using System.Text;

namespace Grapevine.Common
{
    public static class StreamExtensions
    {
        /// <summary>
        /// Returns a byte array representation of the current stream using the specified encoding
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="encoding"></param>
        /// <returns>byte[]</returns>
        internal static byte[] GetTextBytes(this Stream stream, Encoding encoding)
        {
            using (var reader = new StreamReader(stream))
            {
                return encoding.GetBytes(reader.ReadToEnd());
            }
        }

        /// <summary>
        /// Returns a byte array representation of the current stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        internal static byte[] GetBinaryBytes(this Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                return reader.ReadBytes((int)stream.Length);
            }
        }
    }
}
