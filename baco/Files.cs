using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace baco
{
	public static class Files
	{
		public static IEnumerable<string> All(string path)
		{
			return new DirectoryInfo(path).EnumerateFiles().Select(x => x.Name);
		}

		public static IEnumerable<string> All(string path, Regex include, Regex exclude)
		{
			return All(path).Where(f => Filter.Where(Path.Combine(path, f), include, exclude));
		}

		public static IEnumerable<string> All(string path, string pattern)
		{
			return new DirectoryInfo(path).EnumerateFiles(pattern).Select(x => x.Name);
		}

		public static IEnumerable<string> All(string path, string pattern, Regex include, Regex exclude)
		{
			return All(path, pattern).Where(f => Filter.Where(Path.Combine(path, f), include, exclude));
		}

	}
}
