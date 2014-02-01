using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace baco
{
	public static class Backups
	{

		static Regex filter = new Regex(@"^\d{2}(0[1-9]|1[012])(0[1-9]|[12]\d|3[01])_([01]\d|2[0-3])[0-5]\d$");

		/// <summary>
		/// Old backups in the specified path. Only timestamp strings are returned.
		/// </summary>
		/// <param name='path'>
		/// Path.
		/// </param>
		public static IEnumerable<string> Old(string path)
		{
			return new DirectoryInfo(path).
				EnumerateFileSystemInfos("??????_????", SearchOption.TopDirectoryOnly).
				Where(fsi => (fsi.Attributes & FileAttributes.Directory) == 0).
				Select(fsi => fsi.Name).
				Where(n => filter.IsMatch(n));
		}
	}
}
