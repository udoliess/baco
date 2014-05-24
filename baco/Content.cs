using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace baco
{
	public static class Content
	{
		static byte[][] buffers = Enumerable. Repeat(0, 4).Select(x => new byte[Const.BufferSize]).ToArray();

		public static void Copy(string pathSrc, string pathDst)
		{
			try
			{
				using (var streamSource = File.OpenRead(pathSrc))
				using (var streamDestination = File.OpenWrite(pathDst))
				{
					var bufferRead = buffers[0];
					var bufferWrite = buffers[1];
					var len = 0;
					do
					{
						var taskWrite = streamDestination.WriteAsync(bufferWrite, 0, len);
						len = streamSource.Read(bufferRead, 0, Const.BufferSize);
						taskWrite.Wait();
						var bufferTemp = bufferWrite;
						bufferWrite = bufferRead;
						bufferRead = bufferTemp;
					} while (len != 0);
				}
			}
			catch (Exception e)
			{
				throw new Exception(string.Format(CultureInfo.InvariantCulture, "Content.Copy ({0}, {1})", pathSrc, pathDst), e);
			}
		}

		public static bool Compare(string pathA, string pathB)
		{
			try
			{
				if (new FileInfo(pathA).Length != new FileInfo(pathB).Length)
					return false;
				using (var streamA = File.OpenRead(pathA))
				using (var streamB = File.OpenRead(pathB))
				{
					var bufferReadA = buffers[0];
					var bufferReadB = buffers[1];
					var bufferCompA = buffers[2];
					var bufferCompB = buffers[3];
					var len = 0;
					do
					{
						var readATask = streamA.ReadAsync(bufferReadA, 0, Const.BufferSize);
						var readBTask = streamB.ReadAsync(bufferReadB, 0, Const.BufferSize);
						for (var i = 0; i < len; ++i)
							if (bufferCompA[i] != bufferCompB[i])
								return false;
						len = readATask.Result;
						if (len != readBTask.Result)
							return false;
						var bufferTemp = bufferCompA;
						bufferCompA = bufferReadA;
						bufferReadA = bufferTemp;
						bufferTemp = bufferCompB;
						bufferCompB = bufferReadB;
						bufferReadB = bufferTemp;
					} while (len != 0);
				}
				return true;
			}
			catch (Exception e)
			{
				throw new Exception(string.Format(CultureInfo.InvariantCulture, "Content.Compare ({0}, {1})", pathA, pathB), e);
			}
		}
	}
}

