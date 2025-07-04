# This repository is deprecated.<br>Please, use [EndlessOceanFilesConverter](https://github.com/NiV-L-A/EndlessOceanFilesConverter) instead.
--------
## INFO ##
- Endless Ocean Files Converter
- Authors: NiV, MDB
- Special thanks to Hiroshi
- Current version: 1.7.6
- *If you have any issues, join this discord server and contact NiV-L-A: https://discord.gg/4hmcsmPMDG - Endless Ocean Wiki Server

## Description ##
- C# app that converts multiple file formats from the Endless Ocean serie into common ones.
- Supported file formats:
	- .mdl -> .obj + .mtl
	- .hit -> .obj
	- .tdl -> .tga
	- .pak -> Dumps contents by creating a folder with the same name
	- .txs -> Dumps contents by creating a folder with the same name
	- .rtl -> To be passed along with the bNNrodMM.mdl or s01fNNMM.mdl files

## How to run / Troubleshooting ##
- To use this tool, drag and drop a file, multiple files, a folder, or multiple folders, containing one or more of the supported file formats, onto the .exe!
- IMPORTANT: You need .NET Desktop Runtime 7.0 to run this program.
	- https://dotnet.microsoft.com/en-us/download/dotnet/7.0
- NOTE: If you have used QuickBMS to extract the contents of the game, make sure the flag 'AUTO_PARSING_RF2' in the .bms script is set to 0 (you need .mdl files, not .mdl folders!).
- Parsing of the Endless Ocean 1 main map (stage/sea/s01fNNMM.mdl) requires a custom .rtl file to correctly place the meshes. It is included in the .7z, but you can download it from here:
	- https://mega.nz/file/19wSHbhD#X4Z5SU3hq38YoeAc7KhbWn2XE7ijmvgwcnk8RJGgE_E
- The first argument passed to the app *can* be "-ABC", where ABC is the extension of one of the supported file types (see above for a list).
	- When this is done, the app will *only* detect files with the ABC extension. So, it is recommended modifying the shortcut destination path of the app, and adding, for example, "-tdl", to only parse .tdl files.
	
## TO DO: ##
- 0x50 Codes (duplicate meshes outside of the .mdl, that references other .mdl files: deep hole, weddell sea, north coast of canada, etc...).
- Correct parsing for bNNpmset.mdl files.
- Implement multitexturing support for EO2.
- Add transparency to the meshes/textures that require it (there's a flag for it so no big deal, but .obj doesn't support transparency as far as I looked around).
- Discover how the character's heads get in their right position.
- Convert .tdl directly to .tga.
- The converter only supports .tdl files that have the 10 and 5 format. Other possible formats are 8, 1, 3 and 9.
- Better code to handle .hit files.

## Changelog ##
### v1.7.6 - 04/03/2023 ###
- Added LICENSE file.
- Fixed a bug in which you could press any key before the end message.
- Fixed a bug for a mesh name in EO1/stage/zoom/s03/s03z0121.mdl.
- Fixed a bug in which the app would throw an Unauthorized Access Exception in case the user passed a ".mdl folder" as an argument.
	- If this happens, it tells the user to disable the 'AUTO_PARSING_RF2' flag in the .bms script (see the 'How to run' section).
- If the .hit file does not contain any collision data, the app prints a warning and continues with the next file.
	- It does not create the .obj file.
- Modified .hit contents (new line for every collision instead of every polygon).
- Modified "Converted X to Y" message format.
- Changed end message:
	- Elapsed time
	- Total .mdl
	- Total .tdl
	- Total .hit
	- Total .pak/.txs
	- Total files
- Polished code.
- Removed outdated comments.

### v1.7.5 - 10/12/2022 ###
- Removed the string termination character from the regular expressions for rods.
	- For example: "b02rod57.mdl" and "b02rod57 - Copy.mdl" will be treated the same way.
- If a .mdl file contains .tdl files, the app will dump and convert them to .tga.
- If a .mdl file contains .txs files, which contain .tdl files, the app will dump the .tdl files contained in the .txs files and convert them to .tga.
- Changed the naming for .tga files to match with how the other file types are treated.
	- For example: "jaws.tdl" will get converted to "jaws.tdl.tga" (previously it got converted to "jaws.tga").
	- If a file name is truncated (because of the RF archive limitations), the app will append ".tdl" when dumping.
		- For example: "s00stagetm00.td" will get converted to "s00stagetm00.td.tdl.tga".
- Better code to handle .mdl files.
	- Implemented multiple MDLStream related classes.
	- Better code to handle duplicated meshes.
	- Better code to get the final TRS Matrix from the Hierarchy list.
	- Better code to parse the indices sections.
- Implemented the EndianBinaryReader class to be able to swap endianness on the fly.
- Removed some information from the .mdl.obj output.
- Removed redundant code.
- Changed README.md structure.

### v1.7.4 - 20/11/2022 ###
- If a folder is passed as an argument, the app will now detect the files contained in its sub-directories.
	- For example, if "C:\Users\Roberto\Desktop\Test\qwe" is passed, the app will *also* parse the files in "C:\Users\Roberto\Desktop\Test\qwe\rty".
- The first argument passed to the app *can* be "-ABC", where ABC is the extension of one of the supported file types (see above for a list).
	- When this is done, the app will only detect files with the ABC extension. So, it is recommended modifying the shortcut destination path of the app, and adding, for example, "-tdl", to only parse .tdl files.
- The app now automatically removes duplicates.
	- For example, if "C:\Users\Roberto\Desktop\Test\qwe" and "C:\Users\Roberto\Desktop\Test\qwe\d123.mdl" are passed, there will be only one appearance of "C:\Users\Roberto\Desktop\Test\qwe\d123.mdl".
- Fixed a bug in which multiple folders failed to parse.
- Better parsing of the arguments given by the user.

### v1.7.3 - 13/11/2022 ###
- Fixed a bug regarding the duplication mesh system.
- .tdl files now get converted to .tga.
	- Removed conversion from .tdl to .bmp.
	- Removed conversion from .bmp to .png.
- The .mtl files now look for .tga and not .png files.
- Added the Crews.Utility.TgaSharp package to handle .tga files.
- EO1's custom .rtl to parse the stage/sea/s01fNNMM.mdl files is now included in the .7z.
- Added the filename to the meshes produced by parsing a .hit file.
	- Example: From "0_Hitc0000H00Wal" to "b29stage.hit_0_Hitc0000H00Wal".
- More precise print messages.
- Updated README.md
	- Introduced the Changelog section.

### v1.7.2 - 11/06/2022 ###
- Parsing of .hit files (collision hitbox), converts them to .obj.
	- Instantiate the class HITStream to parse the .hit file.
- Parsing of .pak and .txs files. Generic RF Archives. Extract files from the archive.
	- Possibility of renaming a .mdl to a .pak. The code will treat it as a RF Archive and extract its contents.
	- Example: renaming b00stage.mdl to b00stage.pak. This action will dump the .tdl and .vdl files contained inside b00stage.mdl.
- Changed console output when converting files.

### v1.7.1 - 31/03/2022 ###
- Fixed typo: "Instantiate the class *MDL*Stream.CHierarchyObject to understand how the list is made.".

### v1.7 - 30/03/2022 ###
- Better Parsing of the Hierarchy List:
	- Instantiate the class TDLStream.CHierarchyObject to understand how the list is made.
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

### v1.6 - 03/11/2021 ###
- Parses the Object List:
	- Meshes gets their proprer name.
	- Meshes gets their proprer transformation applied.
	- Parses the Object List for x30 Codes (duplicate meshes).
- Almost full support of materials.
	- Creates and populates .mtl file.
	- Gets texture names from the Entry List, then remaps them into specific IDs given by the .mdl.
- The console output does not show the vertices, faces declarations and other lines.
	- Instead, now it shows the ID, the name, the vertices count, the normals count the uvs count of the mesh and if it's duplicating the current mesh.
- V (Y) coordinates for the UVs are now correctly placed by using this epic formula: VCoord = 1.0F + (VCoord  * -1.0F).
- Converts .bmp image files into .png.
- When parsing the indices, triangle strips with at least 2 equal faces are ignored and commented (#) in the .obj file.
- Utilizes the OpenTK library to transform vertices.
- When all the meshes from all arguments are finished converting, console output shows:
	- The elapsed time.
	- How many duplicated meshes it dumped.
	- How many bmp textures it converted.
	- How many triangle strips with at least 2 equal faces it found.
	- How many meshes it converted.
