using System;
using System.Diagnostics;
using System.Globalization;

namespace baco
{
	public static class Statistics
	{
		static Stopwatch stopwatch;
		static DateTime startUtc;
		static DateTime start;
		static string stamp;
		static long deleteCount;
		static long copyCount;
		static long copyBytes;
		static long linkCount;
		static long linkBytes;
		static long errorCount;

		public static void Create()
		{
			stopwatch = Stopwatch.StartNew();
			startUtc = DateTime.UtcNow;
			start = startUtc.ToLocalTime();
			stamp = Statistics.Start.ToString(Const.StampFormat, CultureInfo.InvariantCulture);
		}

		public static string Stamp { get { return stamp; } }

		public static TimeSpan Elapsed { get { return stopwatch.Elapsed; } }

		public static DateTime StartUtc { get { return startUtc; } }

		public static DateTime Start { get { return start; } }

		public static void IncDelete()
		{
			deleteCount++;
		}

		public static long DeleteCount { get { return deleteCount; } }

		public static void IncCopy(long bytes)
		{
			copyCount++;
			copyBytes += bytes;
		}

		public static long CopyCount { get { return copyCount; } }

		public static long CopyBytes { get { return copyBytes; } }

		public static void IncLink(long bytes)
		{
			linkCount++;
			linkBytes += bytes;
		}

		public static long LinkCount { get { return linkCount; } }

		public static long LinkBytes { get { return linkBytes; } }

		public static void IncError()
		{
			errorCount++;
		}

		public static long ErrorCount { get { return errorCount; } }
	}
}

