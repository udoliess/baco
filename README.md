baco
====

What is it?
-----------
baco (backup copy) is a Mono / .NET program for easy backing up your files to a volume that supports hard links (ext2, ext3, ext4, ReiserFS, NTFS, ...). It runs on Unix/Linux and Windows (with Mono / .NET 4.0). Every run of baco produces a snapshot of all your files in a new folder named by current date and time. But if there are unchanged files (compared by content), these files will not need new hard disk space. Instead baco produces hard links. (Hard link: two or more file entries on the same volume share their data bytes.) Beneath the backup folder baco creates a compressed checksum file (*.sha1.gz) for each backup. With help of these checksums baco can find and link identically files. You can use these checksum files to verify integrity of all backup files.
To configure the list of files and folders you want to have backed up, you edit a simple XML file. Run "baco.exe ?" or "mono baco.exe ?" for help.

This program comes with ABSOLUTELY NO WARRANTY.

License
-------
This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or any later version.

see: LICENSE.txt

---
Copyright (C) 2006..2014 Udo Liess

