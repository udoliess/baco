baco
====

What is it?
-----------
baco (backup copy) is a Mono / .NET program for easy backing up your files to a
volume which supports hard links (ext2, ext3, ext4, ReiserFS, NTFS, ...).
It runs on Unix/Linux and Windows (with Mono / .NET 4.0).
Every run of baco creates a snapshot of all your files in a new folder named
by current date and time. If there are unchanged files (compared by content),
they will not need new hard disk space. Rather baco generates hard links
(hard link: two or more files on the same volume share their data bytes).
Additional to the backup folder baco creates a compressed checksum file
(*.sha1.gz) for each single backup. Via those checksums baco can find and link
identical files. Those checksum files can also be used to verify integrity of
all backup files. To configure a set of files and folders, which should be in
the backup, you create a simple XML file.  
Run `baco.exe ?` or `mono baco.exe ?` for help.

This program comes with ABSOLUTELY NO WARRANTY.

Prerequisites
-------------
baco needs Mono or .NET Framework 4.0 (or higher version) to be installed.  
For Windows see <https://www.microsoft.com/netframework>.  
For Ubuntu the needed Mono components can be installed with following command:
`sudo apt-get install mono-runtime libmono-system-core4.0-cil`

License
-------
This program is free software: you can redistribute it and/or modify it under
the terms of the GNU General Public License as published by the Free Software
Foundation, either version 3 of the License, or any later version.

see: LICENSE.txt

---
<http://udol.de/baco>  
<https://github.com/udoliess/baco>  
Copyright (C) 2006...2014 Udo Liess

