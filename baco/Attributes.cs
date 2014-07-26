﻿using System;
using System.IO;

namespace baco
{
	public static class Attributes
	{
		public static void Copy(string source, string destination)
		{
			var copy = new Action(() =>
				{
					var sourceLastWriteTimeUtc = File.GetLastWriteTimeUtc(source);
					if (File.GetLastWriteTimeUtc(destination) != sourceLastWriteTimeUtc)
						File.SetLastWriteTimeUtc(destination, sourceLastWriteTimeUtc);
					var sourceCreationTimeUtc = File.GetCreationTimeUtc(source);
					if (File.GetCreationTimeUtc(destination) != sourceCreationTimeUtc)
						File.SetCreationTimeUtc(destination, sourceCreationTimeUtc);
				});
			try
			{
				copy();
			}
			catch
			{
				try
				{
					File.SetAttributes(destination, FileAttributes.Normal);
					copy();
				}
				catch
				{
				}
			}
		}
	}
}
