using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace baco
{
	public class Source
	{
		public Source(string alias, string source, string include, string exclude, IEnumerable<string> ignores)
		{
			Pattern = Path.GetFileName(source);
			Directory = Path.GetFullPath(Path.GetDirectoryName(source));
			Alias = PathEx.Unroot(alias ?? Directory);
			Include = string.IsNullOrEmpty(include) ? null : new Regex(include);
			var ign = string.Join("|", ignores.Select(x => "(^" + Regex.Escape(x).Replace(@"\*", ".*").Replace(@"\?", ".") + "$)"));
			var ex = string.Join("|", new string[] { !string.IsNullOrEmpty(exclude) ? "(" + exclude + ")" : null, !string.IsNullOrEmpty(ign) ? "(?i:" + ign + ")" : null }.Where(x => !string.IsNullOrEmpty(x)));
			Exclude = string.IsNullOrEmpty(ex) ? null : new Regex(ex);
		}

		public string Directory { get; private set; }

		public string Pattern { get; private set; }

		public Regex Include { get; private set; }

		public Regex Exclude { get; private set; }

		public string Alias { get; private set; }
	}

}

