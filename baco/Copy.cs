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
			var catalog = new Dictionary<string, string>();
			var last = default(string);
			foreach (var s in Backups.Old(source).OrderBy(x => x))
			{
				var src = Path.Combine(source, s);
				var dst = Path.Combine(Destination.Path, s);
				var h = Hash.Hashes(dst);
				var p = Hash.Partial(dst);
				if(Directory.Exists(dst) && File.Exists(h) && !File.Exists(p))
					Hash.ReadHashes(h, (k, v) => catalog[k] = Path.Combine(Destination.Path, v));
				else
				{
					if(Directory.Exists(dst))
						Directory.Delete(dst, true);
					File.Delete(h);
					File.Delete(p);
					var check = new Dictionary<string, string>();
					var f = Hash.Hashes(src);
					if (File.Exists(f))
						Hash.ReadHashes(f, (k, v) => check[v] = k);
					using (var hashes = Hash.CreateHashes(p))
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
									Logger.Log(e.Message, "HandleDirectory()", dir);
								}
							},
							file =>
							{
								try
								{
									var sourceFile = Path.Combine(source, s, file);
									var destinationFile = Path.Combine(Destination.Path, s, file);
									File.Delete(destinationFile);
									var link = false;
									var hash = Hash.FromFile(sourceFile);
									string chk;
									if (check.TryGetValue(Path.Combine(s, file), out chk) && hash != chk)
										Logger.Log("warning: old file corrupt", Path.GetFullPath(sourceFile));
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
								catch (Exception e)
								{
									Logger.Log(e.Message, "HandleFile()", file);
								}
							}
						);
					}
					Hash.Done(dst);
				}
				last = s;
			}
		}
	}
}
