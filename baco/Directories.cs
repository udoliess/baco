using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace baco
{
	public static class Directories
	{
		public static IEnumerable<string> All(string path)
		{
			return new DirectoryInfo(path).EnumerateDirectories().Select(x => x.Name);
		}

		public static IEnumerable<string> All(string path, Regex include, Regex exclude)
		{
			return All(path).Where(d =>
				{
					var p = Path.Combine(path, d, "*");
					return Filter.Where(p.Substring(0, p.Length - 1), include, exclude);
				});
		}
	}
}
