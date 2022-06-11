## INFO ##
- Endless Ocean Files Converter
- Author: NiV, MDB
- Special thanks to Hiroshi
- Current version: 1.7.2
- *If you have any issues, join this discord server and contact NiV-L-A: https://discord.gg/4hmcsmPMDG - Endless Ocean Wiki Server

## Changes ##
- Parsing of .hit files (collision hitbox), converts them to .obj.
	- Instantiate the class HITStream to parse the .hit file.
- Parsing of .pak and .txs files. Generic RF Archives. Extract files from the archive.
	- Possibility of renaming a .mdl to a .pak. The code will treat it as a RF Archive and extract its contents.
		- Example: renaming b00stage.mdl to b00stage.pak. This action will dump the .tdl and .vdl files contained inside b00stage.mdl.
- Changed console output when converting files.

## Description ##
- Straight-forward C# code that converts multiple file formats from the Endless Ocean serie into common ones.
- Supported file formats:
	- .mdl -> .obj
	- .hit -> .obj
	- .tdl -> .bmp
	- .bmp -> .png
	- .pak -> Dumps contents by creating a folder with the same name
	- .txs -> Dumps contents by creating a folder with the same name
	- .rtl -> To be passed along with the bNNrod.mdl files

## How to run ##
- To use this tool, drag and drop a single or multiple .mdl, .tdl and/or .bmp files; or a folder containing .mdl, .tdl and/or .bmp files onto the .exe!
- NOTE: You need .NET Desktop Runtime 5.0 to run this program.
	- https://dotnet.microsoft.com/en-us/download/dotnet/5.0
- Parsing of the Endless Ocean 1 main map (stage/sea/s01fNNMM.mdl) requires a special .rtl file to correctly place the meshes. You can download it from here:
	- https://mega.nz/file/19wSHbhD#X4Z5SU3hq38YoeAc7KhbWn2XE7ijmvgwcnk8RJGgE_E 

## TO DO: ##
- 0x50 Codes (duplicate meshes outside of the .mdl, that references other .mdl files: deep hole, weddell sea, north coast of canada, etc...).
- Correct parsing for bNNpmset.mdl.
- Fixing some stuff with the materials (EO2 only).
- Needs to add transparency to the meshes/textures that require it (there's a flag for it so no big deal, but .obj doesn't support transparency as far as I looked around).
- Discover how the character's heads get in their right position.
- Better code to convert .tdl (textures) files.
- The converter only supports .tdl files that have the 0xA and 0x5 format. Other 2 possible formats are 0x8 and 0x1.