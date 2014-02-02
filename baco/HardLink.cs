using Mono.Unix.Native;
using System;
using System.Runtime.InteropServices;

namespace baco
{
	public static class HardLink
	{
		static HardLink()
		{
			Create = OS.Unix ? (CreateDelegate)CreateUnix : CreateWin;
		}

		public delegate bool CreateDelegate(string existingFile, string newFile);

		public static readonly CreateDelegate Create;

		[DllImport("kernel32")]
		static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

		static bool CreateWin(string existingFile, string newFile)
		{
			try
			{
				return CreateHardLink(newFile, existingFile, IntPtr.Zero);
			}
			catch (Exception e)
			{
				Logger.Log(e.Message, "CreateHardLink()", existingFile, newFile);
			}
			return false;
		}

		static bool CreateUnix(string existingFile, string newFile)
		{
			try
			{
				return Syscall.link(existingFile, newFile) == 0;
			}
			catch (Exception e)
			{
				Logger.Log(e.Message, "Syscall.link()", existingFile, newFile);
			}
			return false;
		}
	}
}
