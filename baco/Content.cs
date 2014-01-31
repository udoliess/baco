using System;
using System.Globalization;
using System.IO;

namespace baco
{
	public static class Content
	{
		public static void Copy(string pathSrc, string pathDst)
		{
			try
			{
				var bufferRead = new byte[Const.BufferSize];
				var bufferWrite = new byte[Const.BufferSize];
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
				var bufferReadA = new byte[Const.BufferSize];
				var bufferReadB = new byte[Const.BufferSize];
				var bufferCompA = new byte[Const.BufferSize];
				var bufferCompB = new byte[Const.BufferSize];
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

