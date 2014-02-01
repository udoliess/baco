using System.IO;
using System.Text.RegularExpressions;

namespace baco
{
	public class Source
	{
		public Source(string alias, string source, string include, string exclude)
		{
			Pattern = Path.GetFileName(source);
			Directory = Path.GetFullPath(Path.GetDirectoryName(source));
			Alias = PathEx.Unroot(alias ?? Directory);
			Include = string.IsNullOrEmpty(include) ? null : new Regex(include);
			Exclude = string.IsNullOrEmpty(exclude) ? null : new Regex(exclude);
		}

		public string Directory { get; private set; }

		public string Pattern { get; private set; }

		public Regex Include { get; private set; }

		public Regex Exclude { get; private set; }

		public string Alias { get; private set; }
	}

}

