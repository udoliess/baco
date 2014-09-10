using System;
using System.IO;

namespace baco
{
	public static class Attributes
	{
		public static void Copy(string source, string destination)
		{
			var s = new FileInfo(source);
			var d = new FileInfo(destination);
			var copy = new Action(() =>
				{
					if (d.CreationTimeUtc != s.CreationTimeUtc)
						d.CreationTimeUtc = s.CreationTimeUtc;
					if (d.LastWriteTimeUtc != s.LastWriteTimeUtc)
						d.LastWriteTimeUtc = s.LastWriteTimeUtc;
				});
			try
			{
				copy();
			}
			catch
			{
				try
				{
					d.Attributes = FileAttributes.Normal;
					copy();
				}
				catch
				{
				}
			}
		}
	}
}
