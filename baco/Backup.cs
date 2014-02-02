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
							try
							{
								Directory.CreateDirectory(Path.Combine(Destination.Path, stamp, source.Alias, dir));
							} catch (Exception e)
							{
								Logger.Log(e.Message, "HandleDirectory()", dir);
							}
						},
						file =>
						{
							try
							{
								var sourceFile = Path.Combine(source.Directory, file);
								var destinationFile = Path.Combine(Destination.Path, stamp, source.Alias, file);
								var link = false;
								var hash = Hash.FromFile(sourceFile);
								string cat;
								if (catalog.TryGetValue(hash, out cat) && File.Exists(cat) && Content.Compare(sourceFile, cat))
									link = HardLink.Create(cat, destinationFile);
								if (!link && last != null)
								{
									var tandem = Path.Combine(Destination.Path, last, source.Alias, file);
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
								} else
								{
									Console.WriteLine("copy: " + sourceFile);
									Statistics.IncCopy(new FileInfo(destinationFile).Length);
								}
								catalog [hash] = destinationFile;
								Hash.AppendHash(hashes, hash, Path.Combine(stamp, source.Alias, file));
							} catch (Exception e)
							{
								Logger.Log(e.Message, "HandleFile()", file);
							}
						}
					);
				}
			}
			Hash.Done(backup);
		}
	}
}
