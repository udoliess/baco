using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace baco
{
	public class Source
	{
		public Source(string alias, string source, string include, IEnumerable<string> takes, string exclude, IEnumerable<string> omits)
		{
			Pattern = Path.GetFileName(source);
			Directory = Path.GetFullPath(Path.Combine(Path.GetPathRoot(source) ?? "", Path.GetDirectoryName(source) ?? ""));
			Alias = PathEx.Unroot(alias ?? Directory);
			Include = Combine(include, takes);
			Exclude = Combine(exclude, omits);
		}

		static Regex Combine(string direct, IEnumerable<string> wildcards)
		{
			var parts = string.Join("|", wildcards.Select(x => "(^" + Regex.Escape(x).Replace(@"\*", ".*").Replace(@"\?", ".") + "$)"));
			var all = string.Join("|", new string[] { !string.IsNullOrEmpty(direct) ? "(" + direct + ")" : null, !string.IsNullOrEmpty(parts) ? (OS.Unix ? "(" : "(?i:") + parts + ")" : null }.Where(x => !string.IsNullOrEmpty(x)));
			return string.IsNullOrEmpty(all) ? null : new Regex(all);
		}

		public string Directory { get; private set; }

		public string Pattern { get; private set; }

		public Regex Include { get; private set; }

		public Regex Exclude { get; private set; }

		public string Alias { get; private set; }
	}

}

