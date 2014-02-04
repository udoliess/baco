using System.Reflection;

namespace baco
{
	public static class Info
	{

		public static string About { get { return about; } }

		public static string Help { get { return help; } }

		readonly static string about = @"
baco (BackupCopy), backup using copy and hard links, version " + AssemblyName.GetAssemblyName(Assembly.GetCallingAssembly().Location).Version.ToString() + @"
Runs with Mono / .NET 4.0, under Linux/Unix and Windows.
Copyright (C) 2006...2014 Udo Liess, Dresden, Germany (udo.liess@gmx.net)
www.udol.de/baco

This program comes with ABSOLUTELY NO WARRANTY.

This program is free software: you can redistribute it and/or modify it under
the terms of the GNU General Public License as published by the Free Software
Foundation, either version 3 of the License, or any later version.
";

		const string help = @"
usage to show this help: baco.exe ?
usage to backup: baco.exe settings-file [destination]
usage to backup with settings in default file ""baco.xml"" in current directory: baco.exe
usage to copy old backups with hardlinks: baco.exe old-destination [new-destination]

priority of destination settings: command line parameter > settings file > current directory

settings-file:
	Source pathes are
		files (full qualified),
		folders (ending with / or \) or
		groups of files (with wildcards).
	To include certain files or folders use one regular expression string per source.
	To exclude certain files or folders use one regular expression string per source.
		The include regular expression is processed first.
		See .NET help: ""Regular Expression Language Elements"" or <http://www.google.com/search?q=.NET+Regular+Expression+Language+Elements>.
	Use alias if the resulting destination path is too long. (The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters.)
	In order to delete some old backups you can setup several reducing levels.
		age: Age of backup as time span. Examples: ""3"" = 3 days, ""1.12:00"" = 1.5 days, ""2:30"" = 2.5 hours
		span: Minimal time span between backups. Special values: ""0"" = disables deleting for this age, ""-1"" = negative span means that all backups of this age will be deleted
	Destination in XML file is optional. It will be overwritten by optional second command line parameter. If none is given, current directory is used as destination.

settings-file example:
<baco>
	<source>
		<!-- copy all .jpg pictures in directory Pictures but no files in subdirectories -->
		<path>d:\Pictures\*.jpg</path>
	</source>
	<source>
		<!-- copy all files in Documents and below (recursive) but skip files ending with "".tmp"", "".TmP"", "".temp"", "".teMp"", ... -->
		<path>c:\My Documents\</path>
		<exclude>(?i)\.tmp$|\.temp$</exclude>
	</source>
	<source>
		<!-- copy all files from this folder an below but store them by another name in backup -->
		<path>c:\very long path would be too long in destination - so use a shorter alias\</path>
		<alias>short\path</alias>
	</source>
	<reduce>
		<!-- keep only hourly backups if they are older than 1 day -->
		<age>1</age>
		<span>1:00</span>
	</reduce>
	<reduce>
		<age>7</age>
		<span>12:00</span>
	</reduce>
	<reduce>
		<!-- keep only daily backups if they are older than 30 days -->
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

Coping of backups to new location can take very long but you can break this process and start it again later with same parameters - it will smoothly continue.

linux command example to create checksum file: find 140101_0000/ -type f -exec sha1sum -b {} + | gzip -c > 140101_0000.sha1.gz
linux command example to check files by checksum file: gzip -dc 140101_0000.sha1.gz | sha1sum -c --quiet --strict
linux command example to check files by all checksum files: gzip -dc ??????_????.sha1.gz | sha1sum -c --quiet --strict
";

	}
}

