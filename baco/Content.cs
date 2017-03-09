using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
		/// Compare the specified files pathA and pathB. Additionally it returns file length and hash of file A.
		/// </summary>
		/// <param name="pathA">Path A.</param>
		/// <param name="pathB">Path B.</param>
		/// <param name="lengthA">Length A.</param>
		/// <param name="hashA">Hash A.</param>
		/// <returns>True if files are equal.</returns>
		public static bool Compare(string pathA, string pathB, out long lengthA, out string hashA)
		{
			try
			{
				lengthA = new FileInfo(pathA).Length;
				if (lengthA != new FileInfo(pathB).Length)
				{
					hashA = Hash.FromFile(pathA);
					return false;
				}
				var result = true;
				using (var streamA = File.Open(pathA, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var streamB = File.Open(pathB, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var hashAlgA = new Hash())
				{
					var bufferReadA = buffers[0];
					var bufferReadB = buffers[1];
					var bufferCompA = buffers[2];
					var bufferCompB = buffers[3];
					var len = 0;
					lengthA = 0;
					do
					{
						var readATask = streamA.ReadAsync(bufferReadA, 0, Const.BufferSize);
						var readBTask = default(Task<int>);
						if (result)
						{
							readBTask = streamB.ReadAsync(bufferReadB, 0, Const.BufferSize);
							for (var i = 0; i < len; ++i)
								if (bufferCompA[i] != bufferCompB[i])
								{
									result = false;
									break;
								}
						}
						hashAlgA.Calculate(bufferCompA, len);
						len = readATask.Result;
						if (result && len != readBTask.Result)
							result = false;
						lengthA += len;
						var bufferTemp = bufferCompA;
						bufferCompA = bufferReadA;
						bufferReadA = bufferTemp;
						bufferTemp = bufferCompB;
						bufferCompB = bufferReadB;
						bufferReadB = bufferTemp;
					} while (len != 0);
					hashA = hashAlgA.Get();
				}
				return result;
			}
			catch (Exception e)
			{
				throw new Exception(string.Format(CultureInfo.InvariantCulture, "Content.Compare ({0}, {1})", pathA, pathB), e);
			}
		}
	}
}

