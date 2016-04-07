using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace baco
{
	public static class Content
	{
		static byte[][] buffers = Enumerable.Repeat(0, 4).Select(x => new byte[Const.BufferSize]).ToArray();

		public static void Copy(string pathSrc, string pathDst, out long length, out string hash)
		{
			try
			{
				length = 0;
				using (var streamSource = File.Open(pathSrc, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var streamDestination = File.OpenWrite(pathDst))
				using (var hashAlg = new Hash())
				{
					var bufferRead = buffers[0];
					var bufferWrite = buffers[1];
					var len = 0;
					do
					{
						var taskWrite = streamDestination.WriteAsync(bufferWrite, 0, len);
						var taskRead = streamSource.ReadAsync(bufferRead, 0, Const.BufferSize);
						hashAlg.Calculate(bufferWrite, len);
						length += len;
						taskWrite.Wait();
						len = taskRead.Result;
						var bufferTemp = bufferWrite;
						bufferWrite = bufferRead;
						bufferRead = bufferTemp;
					} while (len != 0);
					hash = hashAlg.Get();
				}
			}
			catch (Exception e)
			{
				throw new Exception(string.Format(CultureInfo.InvariantCulture, "Content.Copy ({0}, {1})", pathSrc, pathDst), e);
			}
		}

		/// <summary>
		/// Compare the specified files pathA and pathB. If files are equal it returns also file length and hashB.
		/// </summary>
		/// <param name="pathA">Path a.</param>
		/// <param name="pathB">Path b.</param>
		/// <param name="length">Length.</param>
		/// <param name="hashB">Hash b.</param>
		/// <returns>True if files are equal.</returns>
		public static bool Compare(string pathA, string pathB, out long length, out string hashB)
		{
			try
			{
				hashB = null;
				length = 0;
				if (new FileInfo(pathA).Length != new FileInfo(pathB).Length)
					return false;
				using (var streamA = File.Open(pathA, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var streamB = File.Open(pathB, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var hashAlgB = new Hash())
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
						hashAlgB.Calculate(bufferCompB, len);
						len = readATask.Result;
						if (len != readBTask.Result)
							return false;
						length += len;
						var bufferTemp = bufferCompA;
						bufferCompA = bufferReadA;
						bufferReadA = bufferTemp;
						bufferTemp = bufferCompB;
						bufferCompB = bufferReadB;
						bufferReadB = bufferTemp;
					} while (len != 0);
					hashB = hashAlgB.Get();
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

