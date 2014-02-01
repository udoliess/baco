using System;

using System.Linq;

using System.Globalization;
using System.IO;

namespace baco
{
	public static class Content
	{
		static byte[][] buffers = Enumerable. Repeat(0, 4).Select(x => new byte[Const.BufferSize]).ToArray();

		public static void Copy(string pathSrc, string pathDst)
		{
			try
			{
				var bufferRead = buffers[0];
				var bufferWrite = buffers[1];
				var len = 0;
				using (var streamSource = File.OpenRead(pathSrc))
				using (var streamDestination = File.OpenWrite(pathDst))
				{
					do
					{
						var resultWrite = streamDestination.BeginWrite(bufferWrite, 0, len, null, null);
						try
						{
							len = streamSource.Read(bufferRead, 0, Const.BufferSize);
						}
						finally
						{
							streamDestination.EndWrite(resultWrite);
						}
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
				var bufferReadA = buffers[0];
				var bufferReadB = buffers[1];
				var bufferCompA = buffers[2];
				var bufferCompB = buffers[3];
				var len = 0;
				using (var streamA = File.OpenRead(pathA))
				using (var streamB = File.OpenRead(pathB))
				{
					do
					{
						var cancel = false;
						var lenA = default(int);
						var resultA = streamA.BeginRead(bufferReadA, 0, Const.BufferSize, null, null);
						try
						{
							var resultB = streamB.BeginRead(bufferReadB, 0, Const.BufferSize, null, null);
							try
							{
								for (var i = 0; i < len; ++i)
									if (bufferCompA[i] != bufferCompB[i])
									{
										cancel = true;
										streamA.Close();
										streamB.Close();
										return false;
									}
							}
							finally
							{
								try
								{
									len = streamB.EndRead(resultB);
								}
								catch
								{
									if (!cancel)
										throw;
								}
							}
						}
						finally
						{
							try
							{
								lenA = streamA.EndRead(resultA);
							}
							catch
							{
								if (!cancel)
									throw;
							}
						}
						if (lenA != len)
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

