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
			Directory.CreateDirectory(Destination.Path);
			var catalog = new Dictionary<string, string>();
			var last = default(string);
			foreach (var o in Backups.Old(Destination.Path).OrderBy(x => x))
			{
				var dst = Path.Combine(Destination.Path, o);
				var h = Hash.Hashes(dst);
				var p = Hash.Partial(dst);
				if (Directory.Exists(dst) && File.Exists(h) && !File.Exists(p))
					Hash.ReadHashes(h, (k, v) => catalog[k] = Path.Combine(Destination.Path, v));
				last = o;
			}
			foreach (var s in Backups.Old(source).OrderBy(x => x))
			{
				var src = Path.Combine(source, s);
				var dst = Path.Combine(Destination.Path, s);
				var h = Hash.Hashes(dst);
				var p = Hash.Partial(dst);
				if ((!Directory.Exists(dst) && !File.Exists(h) && !File.Exists(p)) ||
					(!File.Exists(h) && File.Exists(p)))
				{
					if (Directory.Exists(dst))
						Directory.Delete(dst, true);
					File.Delete(h);
					File.Delete(p);
					var check = default(Dictionary<string, string>);
					var f = Hash.Hashes(src);
					if (File.Exists(f))
					{
						check = new Dictionary<string, string>();
						Hash.ReadHashes(f, (k, v) => check[v] = k);
					}
					Console.WriteLine("create: " + s);
					Directory.CreateDirectory(dst);
					using (var hashes = Hash.CreateHashes(p))
					{
						Walk.Deep(
							src,
							null,
							null,
							null,
							dir =>
							{
								var sourceDir = Path.Combine(src, dir);
								try
								{
									Console.WriteLine("        " + PathEx.Suffixed(sourceDir));
									Directory.CreateDirectory(Path.Combine(dst, dir));
								}
								catch (Exception e)
								{
									Logger.Log(e.Message, "processing directory", sourceDir);
								}
							},
							file =>
							{
								var sourceFile = Path.Combine(src, file);
								try
								{
									Console.WriteLine("        " + sourceFile);
									var destinationFile = Path.Combine(dst, file);
									File.Delete(destinationFile);
									var link = false;
									var hash = Hash.FromFile(sourceFile);
									if (check != null)
									{
										string chk;
										var key = Path.Combine(s, file);
										if (check.TryGetValue(key, out chk))
										{
											if (hash != chk)
												Logger.Log("warning: corrupt file", Path.GetFullPath(sourceFile));
											check.Remove(key);
										}
										else
										{
											Logger.Log("warning: file without checksum", Path.GetFullPath(sourceFile));
										}
									}
									string cat;
									if (catalog.TryGetValue(hash, out cat) && File.Exists(cat) && Content.Compare(sourceFile, cat))
										link = HardLink.Create(cat, destinationFile);
									if (!link && last != null)
									{
										var tandem = Path.Combine(Destination.Path, last, file);
										if (tandem != cat && File.Exists(tandem) && Content.Compare(sourceFile, tandem))
											link = HardLink.Create(tandem, destinationFile);
									}
									if (!link)
										Content.Copy(sourceFile, destinationFile);
									Attributes.Copy(sourceFile, destinationFile);
									if (link)
									{
										Console.WriteLine("linked: " + sourceFile);
										Statistics.IncLink(new FileInfo(destinationFile).Length);
									}
									else
									{
										Console.WriteLine("copied: " + sourceFile);
										Statistics.IncCopy(new FileInfo(destinationFile).Length);
									}
									catalog[hash] = destinationFile;
									Hash.AppendHash(hashes, hash, Path.Combine(s, file));
								}
								catch (Exception e)
								{
									Logger.Log(e.Message, "processing file", sourceFile);
								}
							}
						);
					}
					Hash.Done(dst);
					if (check != null)
						foreach (var kvp in check)
							Logger.Log("warning: checksum without file", Path.GetFullPath(kvp.Key));
				}
				else
				{
					Logger.Log("error: inconsistent destination", s);
				}
				last = s;
			}
		}
	}
}
