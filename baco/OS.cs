using System;

namespace baco
{
	public static class OS
	{
		public static readonly bool Unix;

		static OS()
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Unix:
				case PlatformID.MacOSX:
					Unix = true;
					break;
				case PlatformID.Win32S:
				case PlatformID.Win32Windows:
				case PlatformID.Win32NT:
				case PlatformID.WinCE:
				case PlatformID.Xbox:
					Unix = false;
					break;
			}
		}
	}
}
