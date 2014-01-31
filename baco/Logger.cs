using System;
using System.IO;
using System.Text;

namespace baco
{
	public class Logger
	{
		public static void Log(Exception e, params string[] texts)
		{
			Statistics.IncException();
			StringBuilder sb = new StringBuilder(e.Message);
			foreach (string text in texts)
			{
				sb.Append(" ");
				sb.Append(text);
			}
			Console.Error.Write("exception: ");
			Console.Error.WriteLine(sb);
			try
			{
				Directory.CreateDirectory(Destination.Path);
				using (TextWriter tw = new StreamWriter(Path.Combine(Destination.Path, Const.LogFile), true))
				{
					tw.Write(DateTime.Now.ToString(Const.LogFormat));
					tw.WriteLine(sb);
				}
			}
			catch
			{
			}
		}
	}
}
