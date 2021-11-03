## INFO ##
aEndless Ocean 1 & 2 .mdl to .obj Exporter
Author: NiV
Special thanks to Hiroshi & MDB
Current version: 1.6
*If you have any issues, join this discord server and contact NiV-L-A: https://discord.gg/4hmcsmPMDG - Endless Ocean Wiki Server

## Changes ##
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

## Description ##
Straight-forward C# code that converts a .mdl from the Endless Ocean serie into multiple .obj. One .obj equals to one mesh.

## How to run ##
Download the executable, drag and drop the .mdl onto the .exe. It supports multiple .mdl as the input argument, don't abuse it tho, it will make it mad.

## TO DO: ##
- 0x50 Codes (duplicate meshes outside of the .mdl, that references other .mdl files: deep hole, weddell sea, north coast of canada, etc...).
- Fixing some stuff with the materials.
- Needs to add transparency to the meshes/textures that require it (there's a flag for it so no big deal, but .obj doesn't support transparency as far as I looked around).
- EO2 main stages (b01,02,03): "decoration" meshes gets put at the right position, but the main "ground" is wrongly placed because it might communicate with the .rtl file.
- Discover how the character's heads get in their right position.