using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace baco
{
	public class Backup
	{
		static public void Do(IEnumerable<Source> sources)
		{
			var catalog = new Dictionary<string, string>();
			var last = default(string);
			var stamp = Statistics.Start.ToString(Const.StampFormat, CultureInfo.InvariantCulture);
			foreach (var l in Backups.Old(Destination.Path).OrderBy(x => x))
			{
				if (stamp == l)
					throw new Exception(string.Format(CultureInfo.InvariantCulture, "backup {0} already exits", stamp));
				var h = Hash.Hashes(Path.Combine(Destination.Path, l));
				if (File.Exists(h))
					Hash.ReadHashes(h, (k, v) => catalog[k] = Path.Combine(Destination.Path, v));
				last = l;
			}
			Console.WriteLine("create: " + stamp);
			var backup = Path.Combine(Destination.Path, stamp);
			Directory.CreateDirectory(backup);
			using (var hashes = Hash.CreateHashes(Hash.Partial(backup)))
			{
				foreach (var source in sources)
				{
					Walk.Deep(
						source.Directory,
						source.Pattern,
						source.Include,
						source.Exclude,
						dir =>
						{
							var sourceDir = Path.Combine(source.Directory, dir);
							try
							{
								Console.WriteLine("        " + PathEx.Suffixed(sourceDir));
								Directory.CreateDirectory(Path.Combine(Destination.Path, stamp, source.Alias, dir));
							}
							catch (Exception e)
							{
								Logger.Log(e.Message, "processing directory", sourceDir);
							}
						},
						file =>
						{
							var sourceFile = Path.Combine(source.Directory, file);
							try
							{
								Console.WriteLine("        " + sourceFile);
								var destinationFile = Path.Combine(Destination.Path, stamp, source.Alias, file);
								var link = false;
								var hash = Hash.FromFile(sourceFile);
								string cat;
								long length = 0;
								if (catalog.TryGetValue(hash, out cat))
								{
									if (!File.Exists(cat))
										Logger.Log("checksum without file", cat);
									else
									{
										string catHash;
										if (Content.Compare(cat, sourceFile, out length, out catHash))
											if (hash == catHash)
												link = HardLink.Create(cat, destinationFile);
											else
												Logger.Log("inconsistent data", cat);
										else
											if (hash == catHash)
												Console.WriteLine($"hash collision {sourceFile} {cat}");
											else
												Logger.Log("corrupt file", cat);
									}
								}
								if (!link && last != null)
								{
									var tandem = Path.Combine(Destination.Path, last, source.Alias, file);
									string tandemHash;
									if (tandem != cat && File.Exists(tandem) && Content.Compare(tandem, sourceFile, out length, out tandemHash) && tandemHash == hash)
										link = HardLink.Create(tandem, destinationFile);
								}
								if (!link)
									Content.Copy(sourceFile, destinationFile, out length, out hash);
								Attributes.Copy(sourceFile, destinationFile);
								if (link)
								{
									Console.WriteLine("linked: " + sourceFile);
									Statistics.IncLink(length);
								}
								else
								{
									Console.WriteLine("copied: " + sourceFile);
									Statistics.IncCopy(length);
								}
								catalog [hash] = destinationFile;
								Hash.AppendHash(hashes, hash, Path.Combine(stamp, source.Alias, file));
							}
							catch (Exception e)
							{
								Logger.Log(e.Message, "processing file", sourceFile);
							}
						}
					);
				}
			}
			Hash.Done(backup);
		}
	}
}
