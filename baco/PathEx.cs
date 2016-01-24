using System.Globalization;
using System.IO;

namespace baco
{
	public static class PathEx
	{
		/// <summary>
		/// Remove special characters from path string so it can be used as directory.
		/// </summary>
		/// <param name='path'>
		/// Path.
		/// </param>
		public static string Unroot(string path)
		{
			if (Path.VolumeSeparatorChar != Path.DirectorySeparatorChar)
				path = path.Replace(Path.VolumeSeparatorChar, Path.DirectorySeparatorChar).Replace(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture) + Path.DirectorySeparatorChar, Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture));
			if (path.StartsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
				path = path.Substring(1);
			return path;
		}

		/// <summary>
		/// Suffixed specified path with directory separator.
		/// </summary>
		/// <param name="path">Path.</param>
		public static string Suffixed(string path)
		{
			var s = Path.Combine(path, "*");
			return s.Substring (0, s.Length - 1);
		}
	}
}

