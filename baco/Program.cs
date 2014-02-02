using System;
using System.Globalization;
using System.IO;

namespace baco
{
	public class Program
	{
		static bool help;

		public static int Main(string[] args)
		{
			Statistics.Create();
			var useDefaultSettings = File.Exists(Const.DefaultSettings) && new FileInfo(Const.DefaultSettings).Length > 0;
			Console.WriteLine(Info.About);
			try
			{
				switch (args.Length)
				{
					case 0:
						if (useDefaultSettings)
							BackupReduce(Const.DefaultSettings);
						else
							Help();
						break;

					case 1:
						if (Directory.Exists(args[0]))
							Copy(args[0]);
						else
							BackupReduce(args[0]);
						break;

					case 2:
						Destination.Path = args[1];
						if (Directory.Exists(args[0]))
							Copy(args[0]);
						else
							BackupReduce(args[0]);
						break;

					default:
						Help();
						break;
				}
			}
			catch (Exception e)
			{
				Logger.Log(e.Message);
			}

			if (!help)
			{
				Console.WriteLine();
				WriteDestination();
				Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:N0} backup(s) deleted.", Statistics.DeleteCount));
				Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:N0} byte(s) in {1:N0} file(s) copied.", Statistics.CopyBytes, Statistics.CopyCount));
				Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:N0} byte(s) in {1:N0} file(s) linked.", Statistics.LinkBytes, Statistics.LinkCount));
				var elapsed = Statistics.Elapsed;
				Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:N0} byte(s) in {1:N0} file(s) backuped in {2} ({3:N0} bytes/s).", Statistics.CopyBytes + Statistics.LinkBytes, Statistics.CopyCount + Statistics.LinkCount, TimeSpan.FromSeconds(Math.Ceiling(elapsed.TotalSeconds)), (Statistics.CopyBytes + Statistics.LinkBytes) / elapsed.TotalSeconds));
				if (Statistics.ErrorCount == 0)
					Console.WriteLine("All OK.");
				else
					Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:N0} error(s)! See file baco.log for details.", Statistics.ErrorCount));
			}

			Console.WriteLine();
			return Statistics.ErrorCount > 0 ? 1 : 0;
		}

		static void Help()
		{
			Console.WriteLine(Info.Help);
			help = true;
		}

		static void BackupReduce(string path)
		{
			var settings = new Settings(path);
			WriteDestination();
			Reducing.Do(settings.Reduces);
			Backup.Do(settings.Sources);
			Reducing.Do(settings.Reduces);
		}

		static void Copy(string source)
		{
			WriteDestination();
			baco.Copy.Do(source);
		}

		static void WriteDestination()
		{
			Console.WriteLine("destination: " + Destination.Path);
		}
	}
}
