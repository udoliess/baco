using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace baco
{
	public static class Copy
	{
		public static void Do(string source)
		{
			// TODO: search for *.part files, delete these backups and restart from that point
			var catalog = new Dictionary<string, string>();
			var done = new Dictionary<string, object>();
			var old = Backups.Old(Destination.Path).OrderBy(x => x).ToList();
			var last = old.LastOrDefault();
			foreach (var o in old)
			{
				var h = Hash.Hashes(Path.Combine(Destination.Path, o));
				if (File.Exists(h))
					Hash.ReadHashes(
						h,
						(o != last) ?
						(Action<string, string>)((k, v) => catalog[k] = Path.Combine(Destination.Path, v)) :
						(k, v) =>
							{
								var p = Path.Combine(Destination.Path, v);
								catalog[k] = p;
								done[p] = null;
							}
					);
			}
			foreach (var s in Backups.Old(source).Where(x => string.CompareOrdinal(x, last) >= 0).OrderBy(x => x))
			{
				var src = Path.Combine(source, s);
				var dst = Path.Combine(Destination.Path, s);
				using (var hashes = Hash.CreateHashes(Hash.Partial(dst)))
				{
					Walk.Deep(
						src,
						null,
						null,
						null,
						dir =>
						{
							try
							{
								Directory.CreateDirectory(Path.Combine(Destination.Path, s, dir));
							}
							catch (Exception e)
							{
								Logger.Log(e, "HandleDirectory()", dir);
							}
						},
						file =>
						{
							try
							{
								var sourceFile = Path.Combine(source, s, file);
								var destinationFile = Path.Combine(Destination.Path, s, file);
								if (!done.ContainsKey(destinationFile))
								{
									File.Delete(destinationFile);
									var link = false;
									var hash = Hash.FromFile(sourceFile);
									string cat;
									if (catalog.TryGetValue(hash, out cat) && File.Exists(cat) && Content.Compare(sourceFile, cat))
										link = HardLink.Create(cat, destinationFile);
									if (!link && last != null)
									{
										var tandem = Path.Combine(Destination.Path, last, file);
										if (File.Exists(tandem) && Content.Compare(sourceFile, tandem))
											link = HardLink.Create(tandem, destinationFile);
									}
									if (!link)
										Content.Copy(sourceFile, destinationFile);
									Attributes.Copy(sourceFile, destinationFile);
									if (link)
									{
										Console.WriteLine("link: " + sourceFile);
										Statistics.IncLink(new FileInfo(destinationFile).Length);
									}
									else
									{
										Console.WriteLine("copy: " + sourceFile);
										Statistics.IncCopy(new FileInfo(destinationFile).Length);
									}
									catalog[hash] = destinationFile;
									Hash.AppendHash(hashes, hash, Path.Combine(s, file));
								}
							}
							catch (Exception e)
							{
								Logger.Log(e, "HandleFile()", file);
							}
						}
					);
				}
				Hash.Ready(dst);
				done.Clear();
				last = s;
			}
		}
	}
}
