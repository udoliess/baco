using System;
using System.IO;
using System.Reflection;

namespace baco
{
	public static class Info
	{

		public static void WriteAbout()
		{
			WriteLines(about);
		}

		public static void WriteHelp()
		{
			WriteLines(help);
		}

		static void WriteLines(string text)
		{
			using (var reader = new StringReader(text))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
					Console.WriteLine(line);
			}
		}

		readonly static string about = @"
baco (BackupCopy), backup using copy and hard links, version " + AssemblyName.GetAssemblyName(Assembly.GetCallingAssembly().Location).Version.ToString() + @"
Runs with Mono / .NET 4.5, under Linux/Unix and Windows.
Copyright (C) 2006...2014 Udo Liess, Dresden, Germany (udo.liess@gmx.net)
<udol.de/baco>
<github.com/udoliess/baco>
usage to show help: baco.exe ?

This program comes with ABSOLUTELY NO WARRANTY.

This program is free software: you can redistribute it and/or modify it under
the terms of the GNU General Public License as published by the Free Software
Foundation, either version 3 of the License, or any later version.

";

		const string help = @"usage to show this help: baco.exe ?
usage to backup: baco.exe settings-file [destination]
usage to backup with settings in default file ""baco.xml"" in current directory: baco.exe
usage to copy old backups with hardlinks: baco.exe old-destination [new-destination]

priority of destination settings: command-line-parameter > settings-file > current-directory

settings-file:
	Source pathes are
		files (full qualified),
		folders (ending with / or \) or
		groups of files (with wildcards).
	To include certain files or folders use one regular expression string per source.
	Alternatively one or more ""take"" entries with wildcards (""*"" and ""?"") can be used. On Windows they are case insensitive, on Linux/Unix they are case sensitive.
	To exclude certain files or folders use one regular expression string per source.
	Alternatively one or more ""omit"" entries with wildcards (""*"" and ""?"") can be used. On Windows they are case insensitive, on Linux/Unix they are case sensitive.
		The include and take expressions are processed first, then the exclude and omit expressions.
		For regular expressions see .NET help: ""Regular Expression Language Elements"" or <google.com/search?q=.NET+Regular+Expression+Language+Elements>.
	Use alias if the resulting destination path is too long. (The fully qualified file name must be less than 260 characters, and the directory name must be less than 248 characters.)
	In order to delete some old backups you can setup several reducing levels.
		age: Age of backup as time span. Examples: ""3"" = 3 days, ""1.12:00"" = 1.5 days, ""2:30"" = 2.5 hours
		span: Minimal time span between backups. Special values: ""0"" = disables deleting for this age, ""-1"" = negative span means that all backups of this age will be deleted.
	Destination in XML file is optional. It will be overwritten by optional second command line parameter. If none is given, current directory is used as destination.

settings-file example:
<baco>

	<!-- Following 'source' entries are illustrative examples for Windows: -->
	<!-- ================================================================= -->

	<!-- Copy all .jpg pictures in directory 'Pictures' but no files in subdirectories. -->
	<source>
		<path>d:\Pictures\*.jpg</path>
	</source>

	<!-- Copy all files in 'Documents' and below (recursive) but skip files ending with '.tmp', '.TmP', '.temp', '.teMp', ... -->
	<source>
		<path>c:\Documents\</path>
		<omit>*.tmp</omit>
		<omit>*.temp</omit>
	</source>

	<!-- Copy all files in 'Texts' and below (recursive). Define the same exclude rules as above with one regular expression. -->
	<source>
		<path>c:\Texts\</path>
		<exclude>(?i)\.te?mp$</exclude>
	</source>

	<!-- Copy all files from this folder and below but backup them by another name. -->
	<source>
		<path>c:\very long path would be too long in destination - so use a shorter alias\</path>
		<alias>short\path</alias>
	</source>


	<!-- Following 'source' entries are real life examples for Windows 7: -->
	<!-- ================================================================ -->

	<source>
		<path>c:\baco\</path>
	</source>
	<source>
		<path>c:\Users\user\Documents\</path>
		<omit>c:\Users\user\Documents\My Music\</omit>
		<omit>c:\Users\user\Documents\My Pictures\</omit>
		<omit>c:\Users\user\Documents\My Videos\</omit>
	</source>
	<source>
		<path>c:\Users\user\Music\</path>
	</source>
	<source>
		<path>c:\Users\user\Pictures\</path>
	</source>
	<source>
		<path>c:\Users\user\Videos\</path>
	</source>
	<source>
		<path>c:\Users\user\AppData\Roaming\Mozilla\Firefox\</path>
		<take>*\</take>
		<take>*\places.sqlite</take>
	</source>
	<source>
		<path>c:\Users\user\AppData\Roaming\Thunderbird\</path>
	</source>


	<!-- Following 'source' entries are real life examples for Linux: -->
	<!-- ============================================================ -->

	<!-- Backup the user directory but ignore hidden files and folders (starting with '.'), 'lost+found' directory, 'Desktop' and 'Downloads'. -->
	<source>
		<path>/home/user/</path>
		<omit>/home/user/.*</omit>
		<omit>/home/user/lost+found/</omit>
		<omit>/home/user/Desktop/</omit>
		<omit>/home/user/Downloads/</omit>
	</source>

	<!-- Backup the Firefox bookmarks database file. Only the directories below '.mozilla' and the files named 'places.sqlite' are taken, other files are skipped. -->
	<source>
		<path>/home/user/.mozilla/</path>
		<take>*/</take>
		<take>*/places.sqlite</take>
	</source>

	<source>
		<path>/home/user/.thunderbird/</path>
	</source>

	<source>
		<path>/home/user/.gnupg/</path>
	</source>


	<!-- The following 'reduce' entries define rules for automatic deletion of old backups. -->
	<!-- ================================================================================== -->

	<!-- Backups will be deleted when they are older than 1 day and time differences between two backups are less than 1 hour. -->
	<reduce>
		<age>1</age>
		<span>1:00</span>
	</reduce>

	<reduce>
		<age>7</age>
		<span>12:00</span>
	</reduce>

	<!-- Thin out backups to daily backups when they are older then 30 days. -->
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


	<!-- Additional settings: -->
	<!-- ==================== -->

	<!-- Setting a destination directory here is optional. You can overwrite this setting in command line. -->
	<destination>e:\BackupDestination\</destination>

</baco>

Copying of all backups from an old backup destination to a new backup destination can take very long. You can break this process and start it with same parameters later again - it will smoothly continue.

linux command example to create checksum file: find 140101_0000/ -type f -exec sha1sum -b {} + | gzip -c > 140101_0000.sha1.gz
linux command example to check files by checksum file: gzip -dc 140101_0000.sha1.gz | sha1sum -c --quiet --strict
linux command example to check files by all checksum files: gzip -dc ??????_????.sha1.gz | sha1sum -c --quiet --strict
";

	}
}

