/*
* baco (BackupCopy), backup using copy and hard links.
* 
* Copyright (C) 2006..2008  Udo Liess  (udo.liess@gmx.net)
* 
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 2
* of the License, or any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with this program; if not, write to the Free Software
* Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
* http://www.fsf.org/licensing/licenses/gpl.txt
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using Mono.Unix.Native;
using System.Security.AccessControl;

namespace baco
{
    class baco
    {
        static int Main(string[] args) { return new baco().Run(args); }

        struct Source
        {
            public Source(string alias, string path, string exclude)
            {
                this.file = Path.GetFileName(path);
                this.directory = string.IsNullOrEmpty(this.file) ? path : Path.GetDirectoryName(path);
                this.alias = string.IsNullOrEmpty(alias) ? UnrootPath(directory) : UnrootPath(alias);
                this.exclude = string.IsNullOrEmpty(exclude) ? null : new Regex(exclude);
            }
            public string alias;
            public string directory;
            public string file;
            public Regex exclude;
        }
        class Reduce : IComparable<Reduce>
        {
            public Reduce(string age, string span)
            {
                this.age = TimeSpan.Parse(age);
                this.span = TimeSpan.Parse(span);
            }
            public Reduce(TimeSpan age)
            {
                this.age = age;
            }
            public TimeSpan age;
            public TimeSpan span;
            public int CompareTo(Reduce other)
            {
                return age.CompareTo(other.age);
            }
        }

        static string UnrootPath(string path)
        {
            if (Path.VolumeSeparatorChar != Path.DirectorySeparatorChar)
                path = path.Replace(Path.VolumeSeparatorChar, Path.DirectorySeparatorChar).Replace(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar, Path.DirectorySeparatorChar.ToString());
            if (path.StartsWith(Path.DirectorySeparatorChar.ToString()))
                path = path.Substring(1);
            return path;
        }

        int Run(string[] args)
        {
            DateTime start = DateTime.UtcNow;
            try
            {
                bool useDefaultConfig = File.Exists(defaultConfig) && new FileInfo(defaultConfig).Length > 0;
                Console.WriteLine(about);
                destination = Directory.GetCurrentDirectory();
                createHardLinkDelegate = Environment.OSVersion.Platform == PlatformID.Unix ? (CreateHardLinkDelegate)CreateHardLinkUnix : (CreateHardLinkDelegate)CreateHardLinkWin;
                copyFileAttributesDelegate = Environment.OSVersion.Platform == PlatformID.Unix ? (CopyAttributes)CopyAttributesUnix : (CopyAttributes)CopyFileAttributesWin;
                copyDirAttributesDelegate = Environment.OSVersion.Platform == PlatformID.Unix ? (CopyAttributes)CopyAttributesUnix : (CopyAttributes)CopyDirAttributesWin;
                bool readXmlDestination = true;
                switch (args.Length)
                {
                    default:
                        if (useDefaultConfig)
                            CreateBackup(defaultConfig, true);
                        else
                        {
                            Console.WriteLine(help);
                            return 2;
                        }
                        break;
                    case 1:
                        if (Directory.Exists(args[0]))
                            CopyBackup(args[0]);
                        else
                            CreateBackup(args[0], readXmlDestination);
                        break;
                    case 2:
                        destination = args[1];
                        readXmlDestination = false;
                        goto case 1;
                }
            }
            catch (Exception e) { Log(e); }
            Console.WriteLine();
            Console.WriteLine(string.Format("{0} backup(s) deleted.", deleteCount));
            Console.WriteLine(string.Format("{0} byte(s) in {1} file(s) copied.", copySize, copyCount));
            Console.WriteLine(string.Format("{0} byte(s) in {1} file(s) linked.", linkSize, linkCount));
            TimeSpan duration = DateTime.UtcNow.Subtract(start);
            Console.WriteLine(string.Format("{0} byte(s) in {1} file(s) backuped in {2} ({3} bytes/s).", copySize + linkSize, copyCount + linkCount, duration, (long)((copySize + linkSize) / duration.TotalSeconds)));
            if (exceptionCount == 0)
                Console.WriteLine("All OK.");
            else
                Console.WriteLine(string.Format("{0} error(s)! See log file for details.", exceptionCount));
            return exceptionCount > 0 ? 1 : 0;
        }

        void CreateBackup(string settingsFile, bool readXmlDestination)
        {
            List<Source> sources = new List<Source>();
            List<Reduce> reduces = new List<Reduce>();
            // read xml config
            XmlDocument doc = new XmlDocument();
            doc.Load(settingsFile);
            XmlNode rootNode = doc.SelectSingleNode("baco");
            foreach (XmlNode reduceNode in rootNode.SelectNodes("reduce"))
            {
                XmlNode ageNode = reduceNode.SelectSingleNode("age");
                XmlNode distanceNode = reduceNode.SelectSingleNode("span");
                reduces.Add(new Reduce(ageNode.InnerText, distanceNode.InnerText));
            }
            XmlNode destinationNode = rootNode.SelectSingleNode("destination");
            if (readXmlDestination && destinationNode != null)
                destination = destinationNode.InnerText;
            foreach (XmlNode sourceNode in rootNode.SelectNodes("source"))
            {
                XmlNode aliasNode = sourceNode.SelectSingleNode("alias");
                XmlNode excludeNode = sourceNode.SelectSingleNode("exclude");
                sources.Add(new Source(aliasNode != null ? aliasNode.InnerText : null, sourceNode.SelectSingleNode("path").InnerText, excludeNode != null ? excludeNode.InnerText : null));
            }
            // start
            Console.WriteLine();
            DateTime now = DateTime.Now;
            try { Directory.CreateDirectory(destination); }
            catch (Exception e) { Log(e, "CreateDirectory()", destination); }
            List<string> oldBackups;
            // reduce
            try
            {
                oldBackups = OldBackups(destination);
                reduces.Sort();
                string lastDir = null;
                DateTime lastDateTime = default(DateTime);
                foreach (string curDir in oldBackups)
                {
                    DateTime curDateTime = DateTime.ParseExact(Path.GetFileNameWithoutExtension(curDir), dateTimeFormat, null);
                    bool delete = false;
                    int entry = reduces.BinarySearch(new Reduce(now - curDateTime));
                    entry = entry < 0 ? ~entry - 1 : entry;
                    if (entry >= 0)
                    {
                        if (reduces[entry].span < TimeSpan.Zero)
                            delete = true;
                        else
                            if (lastDir != null)
                                if ((curDateTime - lastDateTime) < reduces[entry].span)
                                    delete = true;
                    }
                    if (delete)
                    {
                        try
                        {
                            SetFilesNormal(curDir);
                            Directory.Delete(curDir, true);
                            Console.WriteLine("delete: {0}", curDir);
                            ++deleteCount;
                        }
                        catch (Exception e) { Log(e); }
                    }
                    else
                    {
                        lastDir = curDir;
                        lastDateTime = curDateTime;
                    }
                }
            }
            catch (Exception e) { Log(e); }
            // prepare backup
            oldBackups = OldBackups(destination);
            string referenceBase = oldBackups.Count > 0 ? oldBackups[oldBackups.Count - 1] : null;
            string destinationBase = Path.Combine(destination, now.ToString(dateTimeFormat));
            if (Directory.Exists(destinationBase))
                throw new ApplicationException("There is already a backup with current time stamp: " + destinationBase);
            Console.WriteLine("destination: {0}", destinationBase);
            // process
            foreach (Source source in sources)
                try { Do(source.directory, source.file, referenceBase != null ? Path.Combine(referenceBase, source.alias) : null, Path.Combine(destinationBase, source.alias), source.exclude); }
                catch (Exception e) { Log(e); }
        }

        void SetFilesNormal(string path)
        {
            foreach (string dirName in Directory.GetDirectories(path))
                SetFilesNormal(dirName);
            foreach (string fileName in Directory.GetFiles(path))
                File.SetAttributes(fileName, FileAttributes.Normal);
        }

        void CopyBackup(string oldDestination)
        {
            List<string> alreadyCopiedBakups = OldBackups(destination);
            string referenceBase = alreadyCopiedBakups.Count > 0 ? alreadyCopiedBakups[alreadyCopiedBakups.Count - 1] : null;
            Directory.CreateDirectory(destination);
            foreach (string oldBackup in OldBackups(oldDestination))
            {
                try
                {
                    string destinationSubdir = Path.Combine(destination, Path.GetFileName(oldBackup));
                    Do(oldBackup, null, referenceBase, destinationSubdir, null);
                    referenceBase = destinationSubdir;
                }
                catch (Exception e) { Log(e); }
            }
        }

        List<string> OldBackups(string path)
        {
            string[] dirsAll = Directory.GetDirectories(path, "??????_????");
            List<string> dirsFiltered = new List<string>();
            Regex filter = new Regex("^[0-9]{2}[01][0-9][0-3][0-9]_[0-2][0-9][0-5][0-9]$");
            foreach (string dir in dirsAll)
                if (filter.IsMatch(Path.GetFileName(dir)))
                    dirsFiltered.Add(dir);
            dirsFiltered.Sort();
            return dirsFiltered;
        }

        void Do(string sourceDir, string sourceSearchPattern, string referenceDir, string destinationDir, Regex exclude)
        {
            try
            {
                if (!Skip(Path.Combine(sourceDir, " ").TrimEnd(), exclude))
                {
                    try { Directory.CreateDirectory(destinationDir); }
                    catch (Exception e) { Log(e, "CreateDirectory()", destinationDir); }
                    DirectoryInfo di = new DirectoryInfo(sourceDir);
                    FileSystemInfo[] infos = string.IsNullOrEmpty(sourceSearchPattern) ? di.GetFileSystemInfos() : di.GetFileSystemInfos(sourceSearchPattern);
                    foreach (FileSystemInfo info in infos)
                        try
                        {
                            string subReference = referenceDir != null ? Path.Combine(referenceDir, info.Name) : null;
                            string subDestination = Path.Combine(destinationDir, info.Name);
                            if ((info.Attributes & FileAttributes.Directory) != 0)
                            {
                                if (Path.GetFullPath(sourceDir) != Path.GetFullPath(destination))
                                    Do(info.FullName, null, subReference, subDestination, exclude);
                            }
                            else
                                if (!Skip(info.FullName, exclude))
                                    LinkOrCopy(info.FullName, subReference, subDestination);
                        }
                        catch (Exception e) { Log(e); }
                    copyDirAttributesDelegate(sourceDir, destinationDir);
                }
            }
            catch (Exception e) { Log(e); }
        }

        bool Skip(string name, Regex exclude)
        {
            return exclude != null ? exclude.IsMatch(name) : false;
        }

        void LinkOrCopy(string sourceFile, string referenceFile, string destinationFile)
        {
            bool done = false;
            if (FileExists(referenceFile))
                if (FilesEqualBinary(sourceFile, referenceFile))
                    if (createHardLinkDelegate(referenceFile, destinationFile))
                    {
                        Console.WriteLine("link: {0}", sourceFile);
                        ++linkCount;
                        linkSize += new FileInfo(sourceFile).Length;
                        done = true;
                    }
            if (!done)
            {
                CopyContent(sourceFile, destinationFile);
                Console.WriteLine("copy: {0}", sourceFile);
                ++copyCount;
                copySize += new FileInfo(sourceFile).Length;
            }
            copyFileAttributesDelegate(sourceFile, destinationFile);
        }

        void CopyContent(string source, string destination) // File.Copy() does not decrypt NTFS-encrypted files
        {
            try
            {
                byte[] bufferRead = new byte[bufferSize];
                byte[] bufferWrite = new byte[bufferSize];
                int len = 0;
                using (Stream streamSource = File.OpenRead(source),
                      streamDestination = File.OpenWrite(destination))
                {
                    do
                    {
                        IAsyncResult resultWrite = streamDestination.BeginWrite(bufferWrite, 0, len, null, null);
                        IAsyncResult resultRead = streamSource.BeginRead(bufferRead, 0, bufferSize, null, null);
                        streamDestination.EndWrite(resultWrite);
                        len = streamSource.EndRead(resultRead);
                        byte[] bufferTemp = bufferWrite;
                        bufferWrite = bufferRead;
                        bufferRead = bufferTemp;
                    } while (len != 0);
                }
            }
            catch (Exception e) { Log(e, "CopyContent()", source, destination); }
        }

        bool FileExists(string file)
        {
            try { return File.Exists(file); }
            catch (Exception e) { Log(e, "File.Exists()", file); }
            return false;
        }

        bool FilesEqualBinary(string fileA, string fileB)
        {
            try
            {
                if (new FileInfo(fileA).Length != new FileInfo(fileB).Length)
                    return false;
                byte[] bufferReadA = new byte[bufferSize];
                byte[] bufferReadB = new byte[bufferSize];
                byte[] bufferCompA = new byte[bufferSize];
                byte[] bufferCompB = new byte[bufferSize];
                int len = 0;
                using (Stream streamA = File.OpenRead(fileA),
                      streamB = File.OpenRead(fileB))
                {
                    do
                    {
                        IAsyncResult resultA = streamA.BeginRead(bufferReadA, 0, bufferSize, null, null);
                        IAsyncResult resultB = streamB.BeginRead(bufferReadB, 0, bufferSize, null, null);
                        for (int i = 0; i < len; ++i)
                            if (bufferCompA[i] != bufferCompB[i])
                                return false;
                        len = streamA.EndRead(resultA);
                        if (streamB.EndRead(resultB) != len)
                            return false;
                        byte[] bufferTemp = bufferCompA;
                        bufferCompA = bufferReadA;
                        bufferReadA = bufferTemp;
                        bufferTemp = bufferCompB;
                        bufferCompB = bufferReadB;
                        bufferReadB = bufferTemp;
                    } while (len != 0);
                }
                return true;
            }
            catch (Exception e) { Log(e, "FilesEqualBinary()", fileA, fileB); }
            return false;
        }

        void Log(Exception e, params string[] texts)
        {
            ++exceptionCount;
            StringBuilder sb = new StringBuilder(e.Message);
            foreach (string text in texts)
            {
                sb.Append(" ");
                sb.Append(text);
            }
            Console.Write("exception: ");
            Console.WriteLine(sb);
            try
            {
                Directory.CreateDirectory(destination);
                TextWriter tw = new StreamWriter(Path.Combine(destination, "baco.log"), true);
                tw.Write(DateTime.Now.ToString(@"yyyy-MM-dd HH:mm:ss "));
                tw.WriteLine(sb);
                tw.Close();
            }
            catch { }
        }

        [DllImport("kernel32")]
        static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

        bool CreateHardLinkWin(string existingFile, string newFile)
        {
            try { return CreateHardLink(newFile, existingFile, IntPtr.Zero); }
            catch (Exception e) { Log(e, "CreateHardLinkWin()", existingFile, newFile); }
            return false;
        }

        bool CreateHardLinkUnix(string existingFile, string newFile)
        {
            try { return Syscall.link(existingFile, newFile) == 0; }
            catch (Exception e) { Log(e, "CreateHardLinkUnix()", existingFile, newFile); }
            return false;
        }

        void CopyFileAttributesWin(string source, string destination)
        {
            try
            {
                FileAttributes sourceAttributes = File.GetAttributes(source) & fileAttributesMask;
                DateTime sourceCreationTimeUtc = File.GetCreationTimeUtc(source);
                DateTime sourceLastWriteTimeUtc = File.GetLastWriteTimeUtc(source);
                DateTime sourceLastAccessTimeUtc = File.GetLastAccessTimeUtc(source);
                if ((File.GetAttributes(destination) & fileAttributesMask) != sourceAttributes
                || File.GetCreationTimeUtc(destination) != sourceCreationTimeUtc
                || File.GetLastWriteTimeUtc(destination) != sourceLastWriteTimeUtc
                || File.GetLastAccessTimeUtc(destination) != sourceLastAccessTimeUtc)
                {
                    File.SetAttributes(destination, FileAttributes.Normal);
                    File.SetCreationTimeUtc(destination, sourceCreationTimeUtc);
                    File.SetLastWriteTimeUtc(destination, sourceLastWriteTimeUtc);
                    File.SetLastAccessTimeUtc(destination, sourceLastAccessTimeUtc);
                    File.SetAttributes(destination, sourceAttributes);
                }
            }
            catch (Exception e) { Log(e, "CopyFileAttributesWin()", source, destination); }
        }

        void CopyDirAttributesWin(string source, string destination)
        {
            try
            {
                Directory.SetAccessControl(destination, Directory.GetAccessControl(source));
                Directory.SetCreationTimeUtc(destination, Directory.GetCreationTimeUtc(source));
                Directory.SetLastWriteTimeUtc(destination, Directory.GetLastWriteTimeUtc(source));
                Directory.SetLastAccessTimeUtc(destination, Directory.GetLastAccessTimeUtc(source));
            }
            catch (Exception e) { Log(e, "CopyDirAttributesWin()", source, destination); }
        }

        void CopyAttributesUnix(string source, string destination)
        {
            try
            {
                Stat sourceStat;
                Syscall.stat(source, out sourceStat);
                Stat destinationStat;
                Syscall.stat(destination, out destinationStat);
                if (destinationStat.st_atime != sourceStat.st_atime
                || destinationStat.st_mtime != sourceStat.st_mtime
                || destinationStat.st_mode != sourceStat.st_mode
                || destinationStat.st_uid != sourceStat.st_uid
                || destinationStat.st_gid != sourceStat.st_gid)
                {
                    Utimbuf utimbuf = new Utimbuf();
                    utimbuf.actime = sourceStat.st_atime;
                    utimbuf.modtime = sourceStat.st_mtime;
                    Syscall.utime(destination, ref utimbuf);
                    Syscall.chmod(destination, sourceStat.st_mode);
                    Syscall.chown(destination, sourceStat.st_uid, sourceStat.st_gid);
                }
            }
            catch (Exception e) { Log(e, "CopyAttributesUnix()", source, destination); }
        }

        delegate bool CreateHardLinkDelegate(string existingFile, string newFile);
        delegate void CopyAttributes(string source, string destination);
        CreateHardLinkDelegate createHardLinkDelegate;
        CopyAttributes copyFileAttributesDelegate;
        CopyAttributes copyDirAttributesDelegate;
        const FileAttributes fileAttributesMask = FileAttributes.Archive | FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System;
        string destination = "";
        long deleteCount = 0;
        long copyCount = 0;
        long linkCount = 0;
        long exceptionCount = 0;
        long copySize = 0;
        long linkSize = 0;
        const string dateTimeFormat = "yyMMdd_HHmm";
        const string defaultConfig = "baco.xml";
        const int bufferSize = 0x10000;

        readonly string about = @"
baco (BackupCopy), backup using copy and hard links, version " + AssemblyName.GetAssemblyName(Assembly.GetCallingAssembly().Location).Version.ToString() + @"
Runs with Mono and .NET 2.0, under Linux/Unix and Windows.
Copyright (C) 2006...2008 Udo Liess, Dresden, Germany (udo.liess@gmx.net)
www.udol.de/baco

This program comes with ABSOLUTELY NO WARRANTY.
This program is free software; you can redistribute it and/or modify it under
the terms of the GNU General Public License as published by the Free Software
Foundation; either version 2 of the License, or any later version.
";

        const string help = @"
usage to show this help (there must not be a file ""baco.xml"" in current directory): baco.exe
usage to backup: baco.exe settings-file [destination]
usage to backup with settings in file ""baco.xml"": baco.exe
usage to copy old backups with hardlinks: baco.exe old-destination [new-destination]

priority of destination settings: command line parameter > settings file > current directory

settings-file:
	Source pathes are
		files (full qualified),
		folders (ending with / or \) or
		groups of files (with wildcards).
	To exclude files or folders use one regular expression string per source.
		See .NET help: ""Regular Expression Language Elements"" or <http://www.google.com/search?q=.NET+Regular+Expression+Language+Elements>.
	Use alias if the resulting destination path is too long. (The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters.)
	In order to delete some old backups you can setup several reducing levels.
		age: Age of backup as time span. Examples: ""3"" = 3 days, ""1.12:00"" = 1.5 days, ""2:30"" = 2.5 hours
		span: Minimal time span between backups. Special values: ""0"" = disables deleting for this age, ""-1"" = negative span means that all backups of this age will be deleted
	Destination in XML file is optional. It will be overwritten by optional second command line parameter.

example:

<baco>
	<source>
		<path>d:\Documents\*.jpg</path>
	</source>
	<source>
		<path>c:\Program Files\</path>
		<exclude>(?i)\.tmp$|\.temp$</exclude>
	</source>
	<source>
		<path>c:\very long path would be too long in destination - so use a shorter alias\</path>
		<alias>short\path</alias>
	</source>
	<reduce>
		<age>1</age>
		<span>1:00</span>
	</reduce>
	<reduce>
		<age>7</age>
		<span>12:00</span>
	</reduce>
	<reduce>
		<age>30</age>
		<span>1</span>
	</reduce>
	<reduce>
		<age>365</age>
		<span>7</span>
	</reduce>
	<reduce>
		<age>730</age>
		<span>30</span>
	</reduce>
	<destination>c:\BackupDestination\</destination>
</baco>
";

    }
}
