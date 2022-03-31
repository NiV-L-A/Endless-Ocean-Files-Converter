## INFO ##
Endless Ocean Files Converter
Author: NiV, MDB
Special thanks to Hiroshi
Current version: 1.7.1
*If you have any issues, join this discord server and contact NiV-L-A: https://discord.gg/4hmcsmPMDG - Endless Ocean Wiki Server

## Changes ##
- Better Parsing of the Hierarchy List:
	- Instantiate the class MDLStream.CHierarchyObject to understand how the list is made.
	- Better parsing of x30 Codes (duplicate meshes).
- Converts .tdl image files to .bmp files (supports file format 0x05 and 0xA).
	- Implemented the TDLStream.cs file with its classes to parse the .tdl file.
- Better parsing of the indices (understood by looking at the asm and pseudo-decompilation from the game).
- Parsing of the .rtl file (has to be passed with the other arguments!) to correctly place the meshes from the b01/b02/b03 .mdl files.
- Parsing of the Endless Ocean 1 main map (stage/sea/s01fNNMM.mdl) require a special .rtl file to correctly place the meshes. You can download it from here:
	- https://mega.nz/file/19wSHbhD#X4Z5SU3hq38YoeAc7KhbWn2XE7ijmvgwcnk8RJGgE_E 
	- A similiar structure is present in the main executable of the game (main.dol), but that would require to pass the file as one of the arguments too.
- The code now dumps, per default, 1 .obj per .mdl. This can be omitted (dumping 1 .obj per mesh) by manually modifying the variable "DumpSingleOBJFile".
- Better understanding of how the indices in the RF2MD3 format are composed by using the GPU and GPU2 byte (if available).

## Description ##
Straight-forward C# code that converts a .mdl from the Endless Ocean serie into an .obj. Per default, one .obj equals to one mdl.
It also supports converting a .tdl file to .bmp, and a .bmp to a .png.

## How to run ##
To use this tool, drag and drop a single or multiple .mdl, .tdl and/or .bmp files; or a folder containing .mdl, .tdl and/or .bmp files onto the .exe!

## TO DO: ##
- 0x50 Codes (duplicate meshes outside of the .mdl, that references other .mdl files: deep hole, weddell sea, north coast of canada, etc...).
- Fixing some stuff with the materials.
- Needs to add transparency to the meshes/textures that require it (there's a flag for it so no big deal, but .obj doesn't support transparency as far as I looked around).
- Discover how the character's heads get in their right position.
