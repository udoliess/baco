using System;
using System.IO;
using System.Text.RegularExpressions;

namespace baco
{
	public static class Walk
	{
		public static void Deep(string directory, string pattern, Regex include, Regex exclude, Action<string> handleDir, Action<string> handleFile)
		{
			handleDir(""); // handle "root"
			if (string.IsNullOrEmpty(pattern))
				Go(directory, "", include, exclude, handleDir, handleFile);
			else
				Go(directory, pattern, include, exclude, handleFile);
		}

		static void Go(string path, string relative, Regex include, Regex exclude, Action<string> handleDir, Action<string> handleFile)
		{
			try
			{
				foreach (var fsi in new DirectoryInfo(Path.Combine(path, relative)).EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly))
				{
					if ((fsi.Attributes & FileAttributes.Directory) == 0)
					{
						if (Filter.Where(fsi.FullName, include, exclude))
							handleFile(Path.Combine(relative, fsi.Name));
					} else
					{
						if (Filter.Where(PathEx.Suffixed(fsi.FullName), include, exclude))
						{
							var pr = Path.Combine(relative, fsi.Name);
							handleDir(pr);
							Go(path, pr, include, exclude, handleDir, handleFile);
						}
					}
				}
			}
			catch (Exception e)
			{
				Logger.Log(e.Message, "processing path", Path.Combine(path, relative));
			}
		}

		static void Go(string directory, string pattern, Regex include, Regex exclude, Action<string> handleFile)
		{
			try
			{
				foreach (var fsi in new DirectoryInfo(directory).EnumerateFileSystemInfos(pattern, SearchOption.TopDirectoryOnly))
					if ((fsi.Attributes & FileAttributes.Directory) == 0)
						if (Filter.Where(fsi.FullName, include, exclude))
							handleFile(fsi.Name);
			}
			catch (Exception e)
			{
				Logger.Log(e.Message, "processing path", Path.Combine(directory, pattern));
			}
		}
	}
}
