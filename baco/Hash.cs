using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace baco
{
	public class Hash : IDisposable
	{
		const string hashesExtension = "sha256.gz";
		const string partialExtension = "part";
		static readonly byte[] empty = new byte[0];
		static readonly int hashCharacters;
		HashAlgorithm alg;

		static HashAlgorithm CreateAlgorithm()
		{
			return SHA256.Create();
		}

		static Hash()
		{
			using (var alg = CreateAlgorithm())
				hashCharacters = alg.HashSize / 4;
		}

		public Hash()
		{
			alg = CreateAlgorithm();
		}

		public void Calculate(byte[] data, int len)
		{
			alg.TransformBlock(data, 0, len, null, 0);
		}

		public string Get()
		{
			alg.TransformFinalBlock (empty, 0, 0);
			return AsString(alg.Hash);
		}

		public void Dispose()
		{
			alg.Dispose();
		}


		/// <summary>
		/// Calculates hash/checksum from file.
		/// </summary>
		/// <returns>
		/// The hash string.
		/// </returns>
		/// <param name='path'>
		/// Path.
		/// </param>
		public static string FromFile(string path)
		{
			using (var stream = new BufferedStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Const.BufferSize))
			using (var alg = CreateAlgorithm())
				return AsString(alg.ComputeHash(stream));
		}

		static string AsString(byte[] hash)
		{
			var str = string.Join(null, Array.ConvertAll<byte, string>(hash, x => x.ToString("x2", CultureInfo.InvariantCulture)));
			return str;
		}

		/// <summary>
		/// Reads the catalog.
		/// </summary>
		/// <param name='hashes'>
		/// Path to hash/checksum file.
		/// </param>
		/// <param name='entry'>
		/// Entry handling action.
		/// </param>
		public static void ReadHashes(string hashes, Action<string, string> entry)
		{
			using (var streamReader = new StreamReader(new GZipStream(File.Open(hashes, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), CompressionMode.Decompress)))
			{
				var delims = new char[] { ' ', '*', '\t' };
				for (; ; )
				{
					var line = streamReader.ReadLine();
					if (line == null)
						break;
					if(line != "")
					{
						var parts = line.Split(delims, 2, StringSplitOptions.RemoveEmptyEntries);
						if (parts.Length != 2 || parts[0].Length != hashCharacters)
							throw new Exception(string.Format(CultureInfo.InvariantCulture, "Error while parsing line \"{0}\" in file \"{1}\".", line, hashes));
						entry(parts[0].ToLower(), parts[1]);
					}
				}
			}
		}

		/// <summary>
		/// Appends the hash.
		/// </summary>
		/// <param name='writer'>
		/// Writer.
		/// </param>
		/// <param name='hash'>
		/// Hash.
		/// </param>
		/// <param name='file'>
		/// File.
		/// </param>
		public static void AppendHash(TextWriter writer, string hash, string file)
		{
			writer.WriteLine(hash + " *" + file);
		}

		/// <summary>
		/// Creates a text writer for hash/checksum file.
		/// </summary>
		/// <returns>
		/// The hashes.
		/// </returns>
		/// <param name='hashes'>
		/// Hashes.
		/// </param>
		public static TextWriter CreateHashes(string hashes)
		{
			return new StreamWriter(new GZipStream(File.OpenWrite(hashes), CompressionMode.Compress));
		}

		/// <summary>
		/// Returns file name for hash/checksum file.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance hashes backup; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='backup'>
		/// Backup.
		/// </param>
		public static string Hashes(string backup)
		{
			return Path.ChangeExtension(backup, Hash.hashesExtension);
		}

		/// <summary>
		/// Returns file name for partial hash/checksum file.
		/// </summary>
		/// <param name='backup'>
		/// Backup.
		/// </param>
		public static string Partial(string backup)
		{
			return Path.ChangeExtension(backup, Hash.partialExtension);
		}

		/// <summary>
		/// Renames partial hash/checksum file after it is done.
		/// </summary>
		/// <param name='backup'>
		/// Backup.
		/// </param>
		public static void Done(string backup)
		{
			File.Move(Partial(backup), Hashes(backup));
		}

	}
}

