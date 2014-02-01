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
			foreach (var fsi in new DirectoryInfo(Path.Combine(path, relative)).EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly))
			{
				if ((fsi.Attributes & FileAttributes.Directory) == 0)
					if (Filter.Where(fsi.FullName, include, exclude))
						handleFile(Path.Combine(relative, fsi.Name));
				else
				{
					var s = Path.Combine(fsi.FullName, "*");
					if (Filter.Where(s.Substring(0, s.Length - 1), include, exclude))
					{
						var pr = Path.Combine(relative, fsi.Name);
						handleDir(pr);
						Go(path, pr, include, exclude, handleDir, handleFile);
					}
				}
			}
		}

		static void Go(string directory, string pattern, Regex include, Regex exclude, Action<string> handleFile)
		{
			foreach (var fsi in new DirectoryInfo(directory).EnumerateFileSystemInfos(pattern, SearchOption.TopDirectoryOnly))
				if ((fsi.Attributes & FileAttributes.Directory) == 0)
					if (Filter.Where(fsi.FullName, include, exclude))
						handleFile(fsi.Name);
		}
	}
}
