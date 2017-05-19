# RegistryNuker
A small C# console application for quickly travling the Registry to delete keys or values containing an unwanted string. 
Made because an application refused to installe because of hundreds of leftover registry keys from the previous installation.

The program will ask for the following options before starting:

1. Whether you want to search through CURRENT_USER or LOCAL_MACHINE
2. If the program should ask for confirmation before deleting each key, delete all immediately, or go read-only
3. The string to search for
4. If it should search in value names, value data, or both

