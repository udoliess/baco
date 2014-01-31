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
				path = path.Replace(Path.VolumeSeparatorChar, Path.DirectorySeparatorChar).Replace(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar, Path.DirectorySeparatorChar.ToString());
			if (path.StartsWith(Path.DirectorySeparatorChar.ToString()))
				path = path.Substring(1);
			return path;
		}
	}
}

