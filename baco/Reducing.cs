using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace baco
{
	public static class Reducing
	{
		public static void Do(List<Reduce> reduces)
		{
			var lastDir = default(string);
			var lastDateTime = default(DateTime);
			foreach (string curDir in Backups.Old(Destination.Path).OrderBy(x => x).WithoutLast(1))
			{
				var curDateTime = DateTime.ParseExact(Path.GetFileNameWithoutExtension(curDir), Const.StampFormat, CultureInfo.InvariantCulture);
				bool delete = false;
				int entry = reduces.BinarySearch(new Reduce(Statistics.Start - curDateTime));
				entry = entry < 0 ? ~entry - 1 : entry;
				if (entry >= 0)
				{
					if (reduces[entry].Span < TimeSpan.Zero)
						delete = true;
					else
						if (lastDir != null)
							if ((curDateTime - lastDateTime) < reduces[entry].Span)
								delete = true;
				}
				if (delete)
				{
					Console.WriteLine("delete: {0}", curDir);
					var dir = Path.Combine(Destination.Path, curDir);
					try
					{
						try
						{
							Directory.Delete(dir, true);
						}
						catch
						{
							Delete(new DirectoryInfo(dir));
						}
					}
					catch (Exception e)
					{
						Logger.Log(e.Message, "deleting directory", dir);
					}
					var file = Hash.Hashes(dir);
					try
					{
						try
						{
							File.Delete(file);
						}
						catch
						{
							Delete(new FileInfo(file));
						}
					}
					catch (Exception e)
					{
						Logger.Log(e.Message, "deleting file", file);
					}
					Statistics.IncDelete();
				}
				else
				{
					lastDir = curDir;
					lastDateTime = curDateTime;
				}
			}
		}

		static void Delete(FileSystemInfo fileSystemInfo)
		{
			if ((fileSystemInfo.Attributes & FileAttributes.ReparsePoint) == 0)
			{
				var di = fileSystemInfo as DirectoryInfo;
				if (di != null)
					foreach (var fsi in di.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly))
						Delete(fsi);
			}
			fileSystemInfo.Attributes = FileAttributes.Normal;
			fileSystemInfo.Delete();
		}
	}
}

