using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace baco
{
	public class Backup
	{
		string stamp;
		string last;
		Dictionary<string, string> catalog = new Dictionary<string, string>();
		string hashes;

		static public void Do(IEnumerable<Source> sources)
		{
			var backup = new Backup();
			foreach (var s in sources)
				backup.Do(s);
		}

		Backup()
		{
			foreach (var l in Backups.Old(Destination.Path).OrderBy(x => x))
			{
				var h = Path.ChangeExtension(Path.Combine(Destination.Path, l), Hash.FileExtension);
				if (File.Exists(h))
					Hash.ReadHashes(h, (k, v) => catalog[k] = Path.Combine(Destination.Path, v));
				last = l;
			}
			stamp = Statistics.Start.ToString(Const.StampFormat, CultureInfo.InvariantCulture);
			if (stamp == last)
				throw new Exception(string.Format(CultureInfo.InvariantCulture, "backup {0} already exits", last));
			Console.WriteLine("create: " + stamp);
			hashes = Path.ChangeExtension(Path.Combine(Destination.Path, stamp), Hash.FileExtension);
		}

		void Do(Source source)
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
						}
						else
						{
							Console.WriteLine("copy: " + sourceFile);
							Statistics.IncCopy(new FileInfo(destinationFile).Length);
						}
						catalog[hash] = destinationFile;
						Hash.AppendHash(hashes, hash, Path.Combine(stamp, source.Alias, file));
					}
					catch (Exception e)
					{
						Logger.Log(e, "HandleFile()", file);
					}
				}
			);
		}
	}
}

