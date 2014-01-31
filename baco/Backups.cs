using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace baco
{
	public static class Backups
	{

		static Regex filter = new Regex("^[0-9]{2}[01][0-9][0-3][0-9]_[0-2][0-9][0-5][0-9]$");

		/// <summary>
		/// Old backups in the specified path. Only timestamp strings are returned.
		/// </summary>
		/// <param name='path'>
		/// Path.
		/// </param>
		public static IEnumerable<string> Old(string path)
		{
			return Directory.EnumerateDirectories(path, "??????_????").Select(x => Path.GetFileName(x)).Where(x => filter.IsMatch(x));
		}
	}
}
