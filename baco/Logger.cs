using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace baco
{
	public class Logger
	{
		public static void Log(params string[] texts)
		{
			Statistics.IncError();
			var msg = string.Join(" ", texts);
			Console.Error.WriteLine("error: " + msg);
			try
			{
				Directory.CreateDirectory(Destination.Path);
				using (TextWriter tw = new StreamWriter(Path.Combine(Destination.Path, Const.LogFile), true))
					tw.WriteLine(DateTime.UtcNow.ToString(Const.LogFormat, CultureInfo.InvariantCulture) + msg);
			}
			catch
			{
			}
		}
	}
}
