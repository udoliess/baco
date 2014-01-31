using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;

namespace baco
{
	public static class Hash
	{
		public static string FileExtension = "sha1";

		static HashAlgorithm CreateAlgorithm()
		{
			return SHA1.Create();
		}

		static readonly int hashCharacters;

		static Hash()
		{
			using (var alg = CreateAlgorithm())
				hashCharacters = alg.HashSize / 4;
		}

		public static string FromFile(string path)
		{
			using (var fileStream = File.OpenRead(path))
			using (var bufferedStream = new BufferedStream(fileStream, Const.BufferSize))
			using (var alg = CreateAlgorithm())
				return AsString(alg.ComputeHash(bufferedStream));
		}

		public static string AsString(byte[] hash)
		{
			var str = string.Join(null, Array.ConvertAll<byte, string>(hash, x => x.ToString("x2")));
			return str;
		}

		/// <summary>
		/// Reads the catalog.
		/// </summary>
		/// <param name='hashes'>
		/// Path to catalog file.
		/// </param>
		/// <param name='entry'>
		/// Entry handling action.
		/// </param>
		public static void ReadHashes(string hashes, Action<string, string> entry)
		{
			using (var fileStream = File.OpenRead(hashes))
			using (var bufferedStream = new BufferedStream(fileStream, Const.BufferSize))
			using (var streamReader = new StreamReader(bufferedStream))
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
		/// <param name='hashes'>
		/// Path to catalog file.
		/// </param>
		/// <param name='hash'>
		/// Hash value.
		/// </param>
		/// <param name='file'>
		/// Filename.
		/// </param>
		public static void AppendHash(string hashes, string hash, string file)
		{
			using (var writer = new StreamWriter(new FileStream(hashes, FileMode.Append)))
				writer.WriteLine(hash + " *" + file);
		}
	}
}

