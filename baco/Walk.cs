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
			var p = Path.Combine(path, relative);
			foreach (var d in Directories.All(p, include, exclude))
			{
				var pr = Path.Combine(relative, d);
				handleDir(pr);
				Go(path, pr, include, exclude, handleDir, handleFile);
			}
			foreach (var f in Files.All(p, include, exclude))
				handleFile(Path.Combine(relative, f));
		}

		static void Go(string directory, string pattern, Regex include, Regex exclude, Action<string> handleFile)
		{
			foreach (var f in Files.All(directory, pattern, include, exclude))
				handleFile(f);
		}
	}
}
