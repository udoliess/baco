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
				if (!File.Exists(p))
				{
					if (File.Exists(h))
						Hash.ReadHashes(h, (k, v) => catalog[k] = Path.Combine(Destination.Path, v));
					last = o;
				}
			}
			foreach (var s in Backups.Old(source).OrderBy(x => x))
			{
				var src = Path.Combine(source, s);
				var dst = Path.Combine(Destination.Path, s);
				var h = Hash.Hashes(dst);
				var p = Hash.Partial(dst);
				if (!Directory.Exists(dst) || File.Exists(p))
				{
					if (!File.Exists(h))
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
										var link = false;
										var hash = Hash.FromFile(sourceFile);
										if (check != null)
										{
											string chk;
											var key = Path.Combine(s, file);
											if (check.TryGetValue(key, out chk))
											{
												if (hash != chk)
													Logger.Log("corrupt file", Path.GetFullPath(sourceFile));
												check.Remove(key);
											}
											else
											{
												Logger.Log("file without checksum", Path.GetFullPath(sourceFile));
											}
										}
										string cat;
										long length = 0;
										if (catalog.TryGetValue(hash, out cat))
										{
											if (!File.Exists(cat))
												Logger.Log("checksum without file", cat);
											else
											{
												string catHash;
												var equal = Content.Compare(sourceFile, cat, out length, out catHash);
												if (catHash != hash)
													Logger.Log("corrupt file", cat);
												else
													if (equal)
														link = HardLink.Create(cat, destinationFile);
											}
										}
										if (!link && last != null)
										{
											var tandem = Path.Combine(Destination.Path, last, file);
											string tandemHash;
											if (tandem != cat && File.Exists(tandem) && Content.Compare(sourceFile, tandem, out length, out tandemHash) && tandemHash == hash)
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
								Logger.Log("checksum without file", Path.GetFullPath(kvp.Key));
						last = s;
					}
					else
					{
						Logger.Log("error: inconsistent destination", s);
					}
				}
				else
				{
					last = s;
				}
			}
		}
	}
}
