using System;
using System.IO;
using System.IO.Compression;

namespace UberDespatch
{
	public class Decoder
	{
		public Decoder()
		{
		}

		public static byte[] base64_decode(string encodedData)
		{
		    byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
		    return encodedDataAsBytes;
		}

		public static byte[] Decompress(byte[] data)
		{
		    using (var compressedStream = new MemoryStream(data))
		    using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
		    using (var resultStream = new MemoryStream())
		    {
		        var buffer = new byte[4096];
		        int read;

		        while ((read = zipStream.Read(buffer, 0, buffer.Length)) > 0)
		        {
		            resultStream.Write(buffer, 0, read);
		        }

		        return resultStream.ToArray();
		    }
		}
	}
}

