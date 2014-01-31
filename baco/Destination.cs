using System.IO;

namespace baco
{
	public static class Destination
	{
		static string path;
		static bool @set;

		static Destination()
		{
			path = Directory.GetCurrentDirectory();
		}

		public static string Path
		{
			set
			{
				path = System.IO.Path.GetFullPath(value);
				@set = true;
			}
			get
			{
				return path;
			}
		}

		public static bool Set { get { return @set; } }
	}
}

