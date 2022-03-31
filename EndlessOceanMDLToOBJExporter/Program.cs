using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using OpenTK.Mathematics;

namespace EndlessOceanMDLToOBJExporter
{
    class Program
    {

        public static bool IsRod = false;
        //public static bool Ispmset = false;
        public static bool RF2MD2 = false;
        public static bool RF2MD3 = false;
        public static bool RFPMD2 = false;
        public static bool ReplaceCommaWithDot = false;
        public static bool DuplMesh = false;
        public static bool CreateFolder = false;
        public static bool RTLParsed = false;
        public static bool DuplicatingMesh = false;
        public static bool DumpSingleOBJFile = true;
        public static byte indexCount = 0;
        public static byte unkIsOneOrTwo = 0;
        public static byte MeshIDType3BF = 0;
        public static byte MeshIDType2 = 0;
        public static byte isNormOff2C4C = 0;
        public static byte FragmentSize = 0;
        public static byte GPU = 0;
        public static byte GPU2 = 0;
        public static byte x30MeshLevel = 0;
        public static ushort Entries = 0;
        public static ushort EntryList_SIZE = 0;
        public static ushort MeshCount = 0;
        public static ushort vtxCount = 0;
        public static ushort normCount = 0;
        public static ushort lightCount = 0;
        public static ushort uvCount = 0;
        public static ushort unk01Count = 0;
        public static ushort unk02Count = 0;
        public static ushort TDLFilesRefCount = 0;
        public static ushort MatIndicesCount = 0;
        public static ushort MatIndicesOffCount = 0;
        public static ushort RF2MD2MatCount = 0;
        public static ushort ObjectsCount = 0;
        public static ushort MeshTotalCount = 0;
        public static ushort x30CurrentChunk = 0;
        public static ushort ObjectListType = 0x12F;
        public static ushort DuplicatedMeshes = 0;
        public static ushort Magic2 = 0;
        public static ushort RTLRowCount = 0;
        public static ushort RTLColCount = 0;
        public static ushort NextHierarchyObject = 0;
        public static ushort x30IncreaserCounter = 0;
        public static int MeshProgressCount = 0;
        public static int TextureProgressCount = 0;
        public static int pos = 0;
        public static int pos2 = 0;
        public static int pos3 = 0;
        public static int pos4 = 0;
        public static int norm = 0;
        public static int norm2 = 0;
        public static int norm3 = 0;
        public static int norm4 = 0;
        public static int light = 0;
        public static int light2 = 0;
        public static int light3 = 0;
        public static int light4 = 0;
        public static int uv = 0;
        public static int uv2 = 0;
        public static int uv3 = 0;
        public static int uv4 = 0;
        public static int unk011 = 0;
        public static int unk012 = 0;
        public static int unk013 = 0;
        public static int unk014 = 0;
        public static int Texturecount = 0;
        public static int m = 0;
        public static uint HEAD_SIZE = 0;
        public static uint Magic = 0;
        public static uint VDLOff = 0;
        public static uint indOff = 0;
        public static uint vtxOff = 0;
        public static uint MatIndicesOff = 0;
        public static uint MatIndicesInfoOff = 0;
        public static uint StripWithEqualFaceCount = 0;
        public static uint lightOff = 0;
        public static uint OutputFormat = 0;
        public static uint unk01Off = 0;
        public static uint PrevvtxCount = 0;
        public static uint PrevnormCount = 0;
        public static uint PrevuvCount = 0;
        public static long TMP2 = 0;
        public static long MESH_INFO_Offset = 0;
        public static long x30FirstLine = 0;
        public static long x30LastLine = 0;
        public static float INFOOriginP1 = 0.0F;
        public static float INFOOriginP2 = 0.0F;
        public static float INFOOriginP3 = 0.0F;
        public static float MeshPosX = 0;
        public static float MeshPosY = 0;
        public static float MeshPosZ = 0;
        public static float INFOMinCoordP1 = 0F;
        public static float INFOMinCoordP2 = 0F;
        public static float INFOMinCoordP3 = 0F;
        public static float INFOMaxCoordP1 = 0F;
        public static float INFOMaxCoordP2 = 0F;
        public static float INFOMaxCoordP3 = 0F;
        public static float TransRow = 0;
        public static float TransCol = 0;
        public static float RTLChunkSize = 0;
        public static float RTLXChunkStart = 0;
        public static float RTLZChunkStart = 0;
        public static string MeshName = "";
        public static string NewPath = "";
        public static string pathReal = "";
        public static ushort[] MatIndices = null;
        public static float[] XCoordsArray = null;
        public static float[] YCoordsArray = null;
        public static float[] ZCoordsArray = null;
        public static float[] x30_XCoordsArray = null;
        public static float[] x30_YCoordsArray = null;
        public static float[] x30_ZCoordsArray = null;
        public static float[] x30_XNormCoordsArray = null;
        public static float[] x30_YNormCoordsArray = null;
        public static float[] x30_ZNormCoordsArray = null;
        public static string[] tdlNameArray = null;
        public static Quaternion QuatIdentity = new(0, 0, 0, 1);
        public static Vector3 Vec3VTX;
        public static Vector2 Vec2UV;
        public static Dictionary<ushort, ushort> Dictx30_Countx30Codes = new();
        //public static Dictionary<ushort, ushort> Dictx50_Countx50Codes = new();
        public static Stopwatch stopWatch = new();
        public static List<List<ushort>> Rows = null;
        public static List<List<ushort>> Quadrants = null;
        public static List<MDLStream.CHierarchyObject> HierarchyList;
        //public static List<MDLStream.CHierarchyObject> HierarchyListStage;
        public static FileStream fsNew;
        public static IndexStatus vtxIndex = IndexStatus.NONE;
        public static IndexStatus normIndex = IndexStatus.NONE;
        public static IndexStatus lightIndex = IndexStatus.NONE;
        public static IndexStatus uvIndex = IndexStatus.NONE;
        public static IndexStatus unk01Index = IndexStatus.NONE;

        static void Main(string[] args)
        {
            Console.Title = "Endless Ocean Files Converter";
            if (args.Length == 0) //if no args are passed to the .exe
            {
                PrintInfo();
                PrintError("To use this tool, drag and drop a single or multiple .mdl, .tdl and/or .bmp files; or a folder containing .mdl, .tdl and/or .bmp files onto the .exe!");
                Console.Read();
                return;
            }
            else
            {
                //Which files have been passed? Was a folder passed? Multiple .mdl files? Also scans for .rtl
                string[] RTLargs = null;
                string[] MDLargs = null;
                string[] TDLargs = null;
                string[] BMPargs = null;
                List<string> ListAllFiles = new();
                Regex rxrod = new(@"^b(\d{2})rod(\d{2})$"); //Regular expression for eo2 rods: bNNrodNN
                Regex rxs01f = new(@"^s01f(\d{2})(\d{2})$"); //Regular expression for eo1 s01f: s01fNNMM
                //Regex rxpmset = new(@"^b(\d{2})pmset$"); //Regular expression for eo2 pmset: bNNpmset
                //Regex rxstage = new(@"^b(\d{2})stage$"); //Regular expression for eo2 stage: bNNstage

                foreach (string arg in args)
                {
                    if (Path.GetExtension(arg) == "") //folder
                    {
                        string[] AllFiles = Directory.GetFiles(arg);
                        List<string> RTLListFolder = FindExtInArray(AllFiles, ".rtl");
                        List<string> MDLListFolder = FindExtInArray(AllFiles, ".mdl");
                        List<string> TDLListFolder = FindExtInArray(AllFiles, ".tdl");
                        List<string> BMPListFolder = FindExtInArray(AllFiles, ".bmp");
                        RTLargs = RTLListFolder.ToArray();
                        MDLargs = MDLListFolder.ToArray();
                        TDLargs = TDLListFolder.ToArray();
                        BMPargs = BMPListFolder.ToArray();
                        ListAllFiles = ListAllFiles.Concat(MDLargs.Concat(TDLargs.Concat(BMPargs)).ToList()).ToList();
                        //ListAllFiles = ListAllFiles.Concat(MDLargs.Concat(TDLargs).ToList()).ToList();
                    }
                }

                if (ListAllFiles.Count == 0)
                {
                    List<string> RTLList = FindExtInArray(args, ".rtl");
                    List<string> MDLList = FindExtInArray(args, ".mdl");
                    List<string> TDLList = FindExtInArray(args, ".tdl");
                    List<string> BMPList = FindExtInArray(args, ".bmp");
                    RTLargs = RTLList.ToArray();
                    MDLargs = MDLList.ToArray();
                    TDLargs = TDLList.ToArray();
                    BMPargs = BMPList.ToArray();
                    ListAllFiles = ListAllFiles.Concat(MDLargs.Concat(TDLargs.Concat(BMPargs)).ToList()).ToList();
                    //ListAllFiles = ListAllFiles.Concat(MDLargs.Concat(TDLargs).ToList()).ToList();
                }

                if (RTLargs != null)
                {
                    if (RTLargs.Length > 0) //if .rtl found
                    {
                        ParseRTL(RTLargs);
                    }
                }

                /*
                for (int i = 0; i < ListAllFiles.Count(); i++)
                {
                    if (rxpmset.IsMatch(Path.GetFileNameWithoutExtension(ListAllFiles[i])))
                    {
                        string pmsetarg = ListAllFiles[i];
                        for (int j = i + 1; j < ListAllFiles.Count() - i; j++)
                        {
                            if(rxstage.IsMatch(Path.GetFileNameWithoutExtension(ListAllFiles[j])))
                            {
                                ListAllFiles[i] = ListAllFiles[j];
                                ListAllFiles[j] = pmsetarg;
                                break;
                            }
                        }
                        break;
                    }
                }
                */

                args = ListAllFiles.ToArray();

                if (args.Length == 0)
                {
                    PrintInfo();
                    PrintError("One or multiple arguments have been passed, but no correct file was found.\nTo use this tool, drag and drop a single or multiple .mdl, .tdl and/or .bmp files; or a folder containing .mdl, .tdl and/or .bmp files onto the .exe!");
                    Console.Read();
                    return;
                }

                int narg = 0;
                stopWatch.Start();

                foreach (string arg in args) //damn boi, at least one arg, let's see what the script can do
                {
                    pathReal = arg;

                    if (Path.GetExtension(arg) == ".mdl")
                    {
                        //Handle .mdl

                        MeshProgressCount = 0x00;
                        narg += 1;
                        NextHierarchyObject = 0;
                        HierarchyList = new();
                        FileStream fs = null;
                        BinaryReader br = null;
                        RF2MD2 = false;
                        RF2MD3 = false;
                        RFPMD2 = false;
                        try
                        {
                            fs = new FileStream(pathReal, FileMode.Open);
                            br = new BinaryReader(fs);
                        }
                        catch (IOException ex)
                        {
                            PrintInfo();
                            PrintError($"Error: {ex.Message}\nPress any key to close the window");
                            fs = null;
                            br = null;
                            Console.Read();
                            return;
                        }
                        catch (UnauthorizedAccessException uax)
                        {
                            PrintInfo();
                            PrintError($"Error: {uax.Message}\nConsider moving the .mdl file in another folder!");
                            fs = null;
                            br = null;
                            Console.WriteLine("Press any key to close the window");
                            Console.Read();
                            return;
                        }

                        //***********************************************************************************
                        //**************** Parsing of MDL's Header and VDL's Hierarchy List ***************** !JUST ONCE!
                        //***********************************************************************************

                        ParseHeader(br, arg);

                        if (RF2MD3 && ObjectListType >= 0x12F)
                        {
                            VDLOff += 0x0C;
                        }
                        fs.Seek(VDLOff, SeekOrigin.Begin);
                        GetHierarchyList(br);
                        if (RF2MD3 && ObjectListType >= 0x12F)
                        {
                            VDLOff -= 0x0C;
                        }

                        /*
                        if (rxstage.IsMatch(Path.GetFileNameWithoutExtension(arg)))
                        {
                            HierarchyListStage = HierarchyList;
                        }
                        */

                        Dictx30_Countx30Codes = Dictx30_Countx30Codes.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
                        //Dictx50_Countx50Codes = Dictx50_Countx50Codes.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);

                        if (RTLParsed) //Only if the .rtl was passed in the arguments
                        {
                            IsRod = false;
                            if (rxrod.IsMatch(Path.GetFileNameWithoutExtension(arg))) //"^b(\d{2})rod(\d{2})$" - bNNrodNN
                            {
                                IsRod = true;
                                bool TransFound = false;
                                ushort FileNameRodIndex = Convert.ToUInt16(Path.GetFileNameWithoutExtension(arg).Substring(6, 2));
                                for (int i = 0; i < RTLRowCount; i++)
                                {
                                    for (int j = 0; j < RTLColCount; j++)
                                    {
                                        if (Rows[i][j] == FileNameRodIndex)
                                        {
                                            TransRow = i * RTLChunkSize;
                                            TransCol = j * RTLChunkSize;
                                            TransFound = true;
                                            break;
                                        }
                                    }
                                    if (TransFound)
                                    {
                                        break;
                                    }
                                }
                            }
                            else if (rxs01f.IsMatch(Path.GetFileNameWithoutExtension(arg))) //"^s01f(\d{2})(\d{2})$" - s01fNNMM
                            {
                                IsRod = true;
                                ushort CurrQuadrant = (ushort)(Convert.ToUInt16(Path.GetFileNameWithoutExtension(arg).Substring(4, 2), 10) - 1);
                                ushort CurrID = Convert.ToUInt16(Path.GetFileNameWithoutExtension(arg).Substring(6, 2), 10);
                                ushort Modifier = Quadrants[CurrQuadrant][CurrID];

                                TransCol = 320f * (0.5f + (Modifier & 0xFF)) - 4000f;
                                TransRow = 320f * (0.5f + ((Modifier >> 0x8) & 0xFF)) - 3200f;
                            }
                        }

                        /*
                        if (rxpmset.IsMatch(Path.GetFileNameWithoutExtension(arg)))
                        {
                            Ispmset = true;
                        }
                        */

                        if(DumpSingleOBJFile)
                        {
                            NewPath = $"{Path.GetDirectoryName(arg)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(arg)}{Path.GetExtension(arg)}.obj";
                            File.Delete(NewPath);
                            fsNew = File.Open(NewPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

                            PrevvtxCount = 0;
                            PrevnormCount = 0;
                            PrevuvCount = 0;
                        }

                        Matrix4 TRSMat = Matrix4.CreateFromQuaternion(QuatIdentity);

                        //**********************************************
                        //************* GET MESH INFO DATA ************* FOR EACH MESH
                        //**********************************************
                        for (m = 0; m < MeshCount; m++) //Get INFO mesh data. It will go here everytime it finishes parsing the indices.
                        {
                            if (DuplMesh)
                            {
                                DuplicatingMesh = true;
                                DuplicateCurrentMesh(br, TRSMat);
                                DuplicatingMesh = false;
                                PrevvtxCount += (uint)(vtxCount * (Dictx30_Countx30Codes[x30CurrentChunk] - 1));
                                PrevnormCount += (uint)(normCount * (Dictx30_Countx30Codes[x30CurrentChunk] - 1));
                                PrevuvCount += (uint)(uvCount * (Dictx30_Countx30Codes[x30CurrentChunk] - 1));
                            }

                            MeshProgressCount = m + 1;
                            Console.Title = $"Endless Ocean Files Converter - Arg: {narg}/{args.Length} - Mesh: {MeshProgressCount}/{MeshCount}";
                            fs.Seek(MESH_INFO_Offset + (m * 4), SeekOrigin.Begin);
                            ushort MeshIDType;

                            if (CreateFolder)
                            {
                                NewPath = $"{Path.GetDirectoryName(arg)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(arg)}\\{Path.GetFileNameWithoutExtension(arg)}{Path.GetExtension(arg)}_{MeshProgressCount}.obj";
                                fsNew = File.OpenWrite(NewPath);
                            }
                            else if (!DumpSingleOBJFile)
                            {
                                NewPath = $"{Path.GetDirectoryName(arg)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(arg)}{Path.GetExtension(arg)}_{MeshProgressCount}.obj";
                                File.Delete(NewPath);
                                fsNew = File.OpenWrite(NewPath);
                            }

                            uint MeshStartOff = br.ReadUInt32(); //Start of the INFO for that particular mesh
                            long MeshTMP = fs.Position; //we will go back to this offset everytime we finish with the indices data
                            fs.Seek(MeshStartOff, SeekOrigin.Begin);
                            MeshIDType = (ushort)(ReadBEUInt16(br) & 0xF0); //0x10 or 0x50
                            MeshIDType2 = br.ReadByte();
                            MeshIDType3BF = br.ReadByte();
                            ushort unkTwoBytes = ReadBEUInt16(br);
                            ushort MeshNSection = ReadBEUInt16(br); //How many sections the indices data is composed of. 1 = 1 section, 2 = 2 sections. Always greater than 0.
                            ushort[] MatIndex = new ushort[MeshNSection];
                            fs.Seek(0x0C, SeekOrigin.Current); //skip unks

                            //Origin point, should always be: (Min Coord + Max Coord)/2
                            INFOOriginP1 = ReadBEFloat(br);
                            INFOOriginP2 = ReadBEFloat(br);
                            INFOOriginP3 = ReadBEFloat(br);

                            //min vertices
                            INFOMinCoordP1 = ReadBEFloat(br);
                            INFOMinCoordP2 = ReadBEFloat(br);
                            INFOMinCoordP3 = ReadBEFloat(br);

                            //max vertices
                            INFOMaxCoordP1 = ReadBEFloat(br);
                            INFOMaxCoordP2 = ReadBEFloat(br);
                            INFOMaxCoordP3 = ReadBEFloat(br);

                            uint MeshHDRStartOff = ReadBEUInt32(br); //Offset that points to the beginning of the third (main) header of a mesh
                            uint MeshSize = ReadBEUInt32(br); //Size of the mesh
                            MeshHDRStartOff += VDLOff; //Add the offset to the .vdl offset. This will give us the actual offset in the .mdl file.
                            long MeshUntilOff = MeshHDRStartOff + MeshSize;
                            uint[] MeshArray = new uint[MeshNSection];
                            byte[] optimization = new byte[MeshNSection];
                            /* Optimization in .mdl is either triangles or tris (triangle strips/tristrip).
                             * There's a specific byte in the INFO for each *index section*, if it's 0x03, it's triangles (rare), if it's 0x04, it's tristrip (very common).
                             * We declared the MeshArray array and the optimization array, both of length MeshNSection (how many index sections that mesh is composed of)
                             */

                            if (MeshIDType == 0x50) //skip bonesNameCount, unk, SecondHeaderOff
                            {
                                fs.Seek(0x08, SeekOrigin.Current);
                            }
                            else if (MeshIDType != 0x10) //should never happen
                            {
                                PrintError($"MeshIDType not 0x50 or 0x10: 0x{MeshIDType:X2}");
                                Console.ReadLine();
                            }

                            for (int MeshOffTest = 0; MeshOffTest < MeshNSection; MeshOffTest++) //get other indices INFO
                            {
                                MatIndex[MeshOffTest] = ReadBEUInt16(br);
                                fs.Seek(0x01, SeekOrigin.Current);
                                optimization[MeshOffTest] = br.ReadByte(); //0x04 = Tris | 0x03 = Triangles
                                fs.Seek(0x04, SeekOrigin.Current);
                                MeshArray[MeshOffTest] = ReadBEUInt32(br) + MeshHDRStartOff;
                            }

                            //*********************************************
                            //************ THIRD (MAIN) HEADER ************
                            //*********************************************

                            vtxIndex = IndexStatus.NONE;
                            normIndex = IndexStatus.NONE;
                            lightIndex = IndexStatus.NONE;
                            uvIndex = IndexStatus.NONE;
                            unk01Index = IndexStatus.NONE;
                            FragmentSize = 0;
                            GPU = 0;
                            GPU2 = 0;
                            indexCount = 0;

                            fs.Seek(MeshHDRStartOff, SeekOrigin.Begin); //go to main (third) Header of .vdl
                            try
                            {
                                vtxOff = ReadBEUInt32(br); //get the vertices Offset (always 0x20 or 0x40)
                            }
                            catch (EndOfStreamException eose) //we'll get'em next time
                            {
                                PrintInfo();
                                PrintError($"Error: {eose.Message}\nWrong Mesh Start Offset: 0x{fs.Position:X8}");
                                Console.WriteLine("Press any key to close the window.");
                                fs = null;
                                br = null;
                                Console.ReadKey();
                                return;
                            }
                            uint normOff = ReadBEUInt32(br); //get normals offset
                            lightOff = ReadBEUInt32(br); //get light offset
                            uint uvOff = ReadBEUInt32(br); //get uv offset

                            if (vtxOff == 0x20) //Endless Ocean 1 Format
                            {
                                indOff = ReadBEUInt32(br);
                                vtxCount = ReadBEUInt16(br);
                                normCount = ReadBEUInt16(br);
                                lightCount = ReadBEUInt16(br);
                                uvCount = ReadBEUInt16(br);
                                ushort fakeIndexCount = ReadBEUInt16(br);
                                ushort unkFlag = br.ReadByte();
                                isNormOff2C4C = br.ReadByte();
                                indOff = indOff + 0x20 + MeshHDRStartOff;
                            }
                            else if (vtxOff == 0x40) //Endless Ocean 2 Format
                            {
                                unk01Off = ReadBEUInt32(br);
                                uint unk02Off = ReadBEUInt32(br);
                                uint unk03Off = ReadBEUInt32(br);
                                indOff = ReadBEUInt32(br);
                                vtxCount = ReadBEUInt16(br);
                                normCount = ReadBEUInt16(br);
                                lightCount = ReadBEUInt16(br);
                                uvCount = ReadBEUInt16(br);
                                unk01Count = ReadBEUInt16(br);
                                unk02Count = ReadBEUInt16(br);
                                ushort unk03Count = ReadBEUInt16(br);
                                ushort fakeIndexCount = ReadBEUInt16(br);
                                fs.Seek(0x02, SeekOrigin.Current);
                                GPU = br.ReadByte();
                                GPU2 = br.ReadByte();
                                fs.Seek(0x05, SeekOrigin.Current);
                                indexCount = br.ReadByte(); //Possible values: 0,3,4,5,6,7,8,9,10
                                isNormOff2C4C = br.ReadByte();
                                unkIsOneOrTwo = br.ReadByte();
                                indOff += 0x40 + MeshHDRStartOff;
                            }

                            if ((DumpSingleOBJFile && m == 0) || (!DumpSingleOBJFile))
                            {
                                WriteText(fsNew, $"mtllib {Path.GetFileNameWithoutExtension(arg)}.mtl\n");
                            }
                            string info = "";

                            TRSMat = CreateTRSMatrixFromHierarchyList(br, (ushort)(MeshProgressCount - 1));

                            info = $"({Path.GetFileNameWithoutExtension(arg)}{Path.GetExtension(arg)}_{MeshProgressCount}_{MeshName})\n";
                            Console.Write(info);

                            if (CreateFolder)
                            {
                                info = $"o {MeshName}_{MeshProgressCount}\n";
                            }
                            else
                            {
                                x30FirstLine = fsNew.Position;
                                info = $"o {$"{Path.GetFileNameWithoutExtension(arg)}{Path.GetExtension(arg)}_{MeshProgressCount}_"}{MeshName}\n";
                            }
                            WriteText(fsNew, info);
                            XCoordsArray = new float[vtxCount];
                            YCoordsArray = new float[vtxCount];
                            ZCoordsArray = new float[vtxCount];
                            info = $"#MeshIDType: 0x{MeshIDType:X2}\n#INFO_Origin X/Y/Z: {INFOOriginP1}, {INFOOriginP2}, {INFOOriginP3}\n#INFO_MIN X/Y/Z vtx: {INFOMinCoordP1}, {INFOMinCoordP2}, {INFOMinCoordP3}\n#INFO_MAX X/Y/Z vtx: {INFOMaxCoordP1}, {INFOMaxCoordP2}, {INFOMaxCoordP3}\n";
                            WriteText(fsNew, info);
                            //mtllib MatFile.mtl
                            //o meshname
                            //#MeshIDType: 0x10 or 0x50
                            //#INFO_Origin X/Y/Z: X,Y,Z
                            //#INFO_MIN X/Y/Z vtx: X,Y,Z
                            //#INFO_MAX X/Y/Z vtx: X,Y,Z
                            //#INFO @: 0xOffset

                            info = $"#INFO @: 0x{MeshStartOff:X8}\n";
                            WriteText(fsNew, info);

                            /*
                             * Indices are always composed of 2 bytes when the magic is RFPMD2 or RF2MD2. For RF2MD3, the indices can be a mix of 1 byte and/or 2 bytes.
                             * To understand when this happens, the .vdl header has 2 specific bytes (here called GPU and GPU2). We can divide the 8 bits in groups of 2 to understand the indices size.
                             * 
                             * For example, GPU2 is 0xEF:
                             * 1 1 | 1 0 | 1 1 | 1 1
                             * 0th bit = is uv data present (in 0xEF, 0th bit is set to 1. Yes, uv data is present).
                             * 1st bit = is uv index 2 bytes (in 0xEF, 1st bit is set to 1. Yes, uv index is 2 bytes).
                             * 
                             * 2nd bit = is light data present (in 0xEF, 2nd bit is set to 1. Yes, light data is present).
                             * 3rd bit = is light index 2 bytes (in 0xEF, 3rd bit is set to 0. No, light data is 1 byte).
                             * 
                             * 4th bit = is normals data present (in 0xEF, 4th bit is set to 1. Yes, normals data is present).
                             * 5th bit = is normals index 2 bytes (in 0xEF, 5th bit is set to 1. Yes, normals index is 2 bytes).
                             * 
                             * 6th bit = is vertices data present (in 0xEF, 6th bit is set to 1. Yes, vertices data is present).
                             * 7th bit = is vertices index 2 bytes (in 0xEF, 7th bit is set to 1. Yes, vertices index is 2 bytes).
                             * 
                             * Sometimes the GPU and GPU2 bytes are null, so we will have to calculate the indices size based on the vertices count. If the count is >= 0xFF, then the index count is composed of 2 bytes, else, 1 byte.
                             */

                            UnderstandIndexCount();

                            //*********************************************
                            //*************** VERTICES DATA ***************
                            //*********************************************

                            Console.WriteLine($"\t[Vertices]: 0x{vtxCount:X4}");
                            vtxOff += MeshHDRStartOff;
                            br.BaseStream.Seek(vtxOff, SeekOrigin.Begin);
                            LogVtx(fsNew, br, TRSMat);

                            //********************************************
                            //*************** NORMALS DATA ***************
                            //********************************************

                            Console.WriteLine($"\t[Normals]: 0x{normCount:X4}");
                            normOff += MeshHDRStartOff;
                            br.BaseStream.Seek(normOff, SeekOrigin.Begin); //go to normals offset
                            LogNormals(fsNew, br, TRSMat);

                            //********************************************
                            //***************** UVS DATA *****************
                            //********************************************

                            Console.WriteLine($"\t[UVs]: 0x{uvCount:X4}");
                            uvOff += MeshHDRStartOff;
                            br.BaseStream.Seek(uvOff, SeekOrigin.Begin);
                            LogUVs(fsNew, br, TRSMat);

                            //********************************************
                            //*************** INDICES DATA ***************
                            //********************************************

                            Console.WriteLine($"\t[Indices]");

                            /* indunk = unknown 2 bytes
                             * cnt = Sequence Count - It indicates each "fragment".
                             * The Indices Section, usually, is divided in multiple draw calls.
                             * Though there could even be 1 single draw call (very rare; but it will always happens if optimization is triangles).
                             * Index Sections are usually, but not always, divided with null bytes for 0x10 alignment.
                             * Since normals are flipped, we have to reverse the output of the faces,
                             * so, instead of printing "f {pos}/{uv}/{norm} {pos2}/{uv2}/{norm2} {pos3}/{uv3}/{norm3}\n"
                             * We print                "f {pos3}/{uv3}/{norm3} {pos2}/{uv2}/{norm2} {pos}/{uv}/{norm}\n"
                             * pos -> pos3
                             * pos2 -> pos2
                             * pos3 -> pos
                             */

                            ParseIndices(br, fsNew, MeshNSection, MeshArray, optimization, MatIndex);

                            if (DumpSingleOBJFile)
                            {
                                fsNew.Flush();
                                PrevvtxCount += vtxCount;
                                PrevnormCount += normCount;
                                PrevuvCount += uvCount;
                            }
                            else
                            {
                                fsNew.Flush();
                                fsNew.Close();
                            }

                            Console.WriteLine("#Mesh Ended\n");

                            if (CreateFolder)
                            {
                                string NewFilePath = $"{Path.GetDirectoryName(arg)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(arg)}\\{MeshName}_{MeshProgressCount}.obj";

                                if (File.Exists(NewFilePath))
                                {
                                    File.Delete(NewFilePath);
                                }

                                File.Move(NewPath, NewFilePath);
                            }
                        }
                    }
                    else if (Path.GetExtension(arg) == ".bmp")
                    {
                        //Handle .bmp
                        TextureProgressCount += 1;
                        narg += 1;
                        Console.Title = $"Endless Ocean Files Converter - Arg: {narg}/{args.Length} - Texture: {TextureProgressCount}/{BMPargs.Length}";
                        Console.WriteLine($"{Texturecount + 1} | {Path.GetFileNameWithoutExtension(arg) + Path.GetExtension(arg)}");
                        Texturecount += 1;
                        ConvertBMPToPNG(arg);
                    }
                    else if (Path.GetExtension(arg) == ".tdl")
                    {
                        //Handle .tdl

                        TextureProgressCount += 1;
                        narg += 1;
                        Console.Title = $"Endless Ocean Files Converter - Arg: {narg}/{args.Length} - Texture: {TextureProgressCount}/{TDLargs.Length}";
                        Console.WriteLine($"{Texturecount + 1} | {Path.GetFileNameWithoutExtension(arg) + Path.GetExtension(arg)}");
                        Texturecount += 1;

                        using FileStream fsTDL = new(pathReal, FileMode.Open);
                        using BinaryReader brTDL = new(fsTDL);

                        //Read .tdl file. Header and Data
                        TDLStream TDLFile = new(brTDL);

                        //Create empty .bmp file
                        string BMPFilePath = $"{Path.GetDirectoryName(arg)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(arg)}.bmp";
                        using FileStream fsBMP = new(BMPFilePath, FileMode.OpenOrCreate);
                        using BinaryWriter bwBMP = new(fsBMP);
                        int Blocks = 0;

                        switch(TDLFile.Header.FileHeader.Format) //Possible formats: 10,5,8,1
                        {
                            case 10:
                                Blocks = TDLFile.Header.FileHeader.TotalWidth * TDLFile.Header.FileHeader.TotalHeight / 16;
                                TDLStream.TransformCMPRBlock(TDLFile.Data.CMPRBlock, Blocks, OutputFormat);

                                fsBMP.Seek(0, SeekOrigin.Begin);

                                TDLStream.WriteBMPHeader(bwBMP, TDLFile, OutputFormat);
                                TDLStream.WriteBMPCMPRBlock(bwBMP, TDLFile);
                                break;

                            case 5:
                                Blocks = TDLFile.Header.FileHeader.TotalWidth * TDLFile.Header.FileHeader.TotalHeight / 32;
                                TDLStream.TransformRGB5A3Block(TDLFile.Data.RGB5A3Block, Blocks, OutputFormat);
                                TDLStream.TransformRGB5A3Palette(TDLFile.Data.Palette, TDLFile.Header.FileHeader.PaletteSize / 2);

                                fsBMP.Seek(0, SeekOrigin.Begin);

                                TDLStream.WriteBMPHeader(bwBMP, TDLFile, OutputFormat);
                                TDLStream.WriteBMPRGB5A3Block(bwBMP, TDLFile);
                                break;
                        }

                        bwBMP.Dispose();
                        fsBMP.Dispose();
                        bwBMP.Close();
                        fsBMP.Close();

                        //ConvertBMPToPNG(BMPFilePath);
                    }

                    if (Path.GetExtension(arg) == ".mdl" && DumpSingleOBJFile)
                    {
                        fsNew.Flush();
                        fsNew.Close();
                    }

                    Console.WriteLine("File Ended\n");
                }
            }
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = $"Elapsed time: {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            Console.WriteLine(elapsedTime);
            if (DuplicatedMeshes > 0)
            {
                Console.WriteLine($"Duplicated Meshes: {DuplicatedMeshes} [0x{DuplicatedMeshes:X4}]");
                Console.WriteLine($"Meshes: {MeshTotalCount} [0x{MeshTotalCount:X4}]");
                MeshTotalCount += DuplicatedMeshes;
            }

            if (Texturecount > 0)
                Console.WriteLine($"Total Textures: {Texturecount} [0x{Texturecount:X4}]");

            Console.WriteLine($"Total Meshes: {MeshTotalCount} [0x{MeshTotalCount:X4}]");

            if (StripWithEqualFaceCount != 0)
                Console.WriteLine($"Face declarations with at least 2 equal face indices: {StripWithEqualFaceCount} [0x{StripWithEqualFaceCount:X4}] (Dumped but commented with #)");

            Console.WriteLine("Please, press any key to exit");
            Console.ReadKey();
        }

        public static void GetIndexStripA(BinaryReader br)
        {
            pos = GetIndexPos(br);
            norm = GetIndexNorm(br);
            light = GetIndexLight(br);
            uv = GetIndexUV(br);
            unk011 = GetIndexUnk01(br);

            pos2 = GetIndexPos(br);
            norm2 = GetIndexNorm(br);
            light2 = GetIndexLight(br);
            uv2 = GetIndexUV(br);
            unk012 = GetIndexUnk01(br);

            TMP2 = br.BaseStream.Position;

            pos3 = GetIndexPos(br);
            norm3 = GetIndexNorm(br);
            light3 = GetIndexLight(br);
            uv3 = GetIndexUV(br);
            unk013 = GetIndexUnk01(br);
        }

        public static void GetIndexStripB(BinaryReader br)
        {
            pos4 = GetIndexPos(br);
            norm4 = GetIndexNorm(br);
            light4 = GetIndexLight(br);
            uv4 = GetIndexUV(br);
            unk014 = GetIndexUnk01(br);
        }

        public static int GetIndexPos(BinaryReader br)
        {
            int IndexPos = 0;
            if (vtxIndex == IndexStatus.SHORT) //read vtx short
            {
                IndexPos = ReadBEUInt16(br);
                IndexPos = (ushort)(IndexPos + 1);
            }
            else
            {
                IndexPos = br.ReadByte();//read vtx byte
                IndexPos = (byte)(IndexPos + 1);
            }

            if (DumpSingleOBJFile)
            {
                IndexPos += (int)PrevvtxCount;
            }

            return IndexPos;
        }

        public static int GetIndexNorm(BinaryReader br)
        {
            int IndexNorm = 0;
            if (normIndex == IndexStatus.SHORT)
            {
                IndexNorm = ReadBEUInt16(br); //read norm short
                IndexNorm = (ushort)(IndexNorm + 1);
            }
            else
            {
                IndexNorm = br.ReadByte(); //read norm byte
                IndexNorm = (byte)(IndexNorm + 1);
            }

            if (DumpSingleOBJFile)
            {
                IndexNorm += (int)PrevnormCount;
            }

            return IndexNorm;
        }

        public static int GetIndexLight(BinaryReader br)
        {
            int IndexLight = 0;
            if (lightIndex == IndexStatus.SHORT)
            {
                IndexLight = ReadBEUInt16(br);
                IndexLight = (ushort)(IndexLight + 1);
            }
            else if (lightIndex == IndexStatus.BYTE)
            {
                IndexLight = br.ReadByte();
                IndexLight = (byte)(IndexLight + 1);
            }

            return IndexLight;
        }

        public static int GetIndexUV(BinaryReader br)
        {
            int IndexUV = 0;
            if (uvIndex == IndexStatus.SHORT)
            {
                IndexUV = ReadBEUInt16(br);
                IndexUV = (ushort)(IndexUV + 1);
            }
            else
            {
                IndexUV = br.ReadByte();
                IndexUV = (byte)(IndexUV + 1);
            }

            if (DumpSingleOBJFile)
            {
                IndexUV += (int)PrevuvCount;
            }

            return IndexUV;
        }

        public static int GetIndexUnk01(BinaryReader br)
        {
            int IndexUnk01 = 0;
            if (unk01Index == IndexStatus.SHORT)
            {
                IndexUnk01 = ReadBEUInt16(br);
                IndexUnk01 = (ushort)(IndexUnk01 + 1);
            }
            else if (unk01Index == IndexStatus.BYTE)
            {
                IndexUnk01 = br.ReadByte();
                IndexUnk01 = (byte)(IndexUnk01 + 1);
            }
            return IndexUnk01;
        }

        public static void LogVtx(FileStream fsNew, BinaryReader br, Matrix4 TRSMat)
        {
            x30_XCoordsArray = new float[vtxCount];
            x30_YCoordsArray = new float[vtxCount];
            x30_ZCoordsArray = new float[vtxCount];
            string info = $"#vtxCount: {vtxCount} [0x{vtxCount:X4}]\n#POS: 0x{br.BaseStream.Position:X8}\n";
            WriteText(fsNew, info);
            LogVertices(fsNew, br, 0, TRSMat);

            if (DuplMesh)
            {
                x30_XCoordsArray = XCoordsArray;
                x30_YCoordsArray = YCoordsArray;
                x30_ZCoordsArray = ZCoordsArray;
            }

            info = $"#MIN X/Y/Z: {XCoordsArray.Min()}, {YCoordsArray.Min()}, {ZCoordsArray.Min()}\n";
            WriteText(fsNew, info);

            info = $"#MAX X/Y/Z: {XCoordsArray.Max()}, {YCoordsArray.Max()}, {ZCoordsArray.Max()}\n";
            WriteText(fsNew, info);

            MeshPosX = (XCoordsArray.Max() + XCoordsArray.Min()) / 2;
            MeshPosY = (YCoordsArray.Max() + YCoordsArray.Min()) / 2;
            MeshPosZ = (ZCoordsArray.Max() + ZCoordsArray.Min()) / 2;
            info = $"#(MAX + MIN)/2: {MeshPosX}, {MeshPosY}, {MeshPosZ}\n";
            WriteText(fsNew, info);
        }

        public static void LogUVs(FileStream fsNew, BinaryReader br, Matrix4 TRSMat)
        {
            string info = $"#uvCount: {uvCount} [0x{uvCount:X4}]\n#POS: 0x{br.BaseStream.Position:X8}\n";
            WriteText(fsNew, info);
            XCoordsArray = new float[uvCount];
            YCoordsArray = new float[uvCount];

            LogVertices(fsNew, br, 2, TRSMat);

            info = $"#MIN U/V: {XCoordsArray.Min()}, {YCoordsArray.Min()}\n";
            WriteText(fsNew, info);

            info = $"#MAX U/V: {XCoordsArray.Max()}, {YCoordsArray.Max()}\n";
            WriteText(fsNew, info);

            MeshPosX = (XCoordsArray.Max() + XCoordsArray.Min()) / 2;
            MeshPosY = (YCoordsArray.Max() + YCoordsArray.Min()) / 2;
            info = $"#(MAX + MIN)/2: {MeshPosX}, {MeshPosY}\n";
            WriteText(fsNew, info);
        }

        public static void LogNormals(FileStream fsNew, BinaryReader br, Matrix4 TRSMat)
        {
            string info = $"#normCount: {normCount} [0x{normCount:X4}]\n#POS: 0x{br.BaseStream.Position:X8}\n";
            WriteText(fsNew, info);

            XCoordsArray = new float[normCount];
            YCoordsArray = new float[normCount];
            ZCoordsArray = new float[normCount];

            x30_XNormCoordsArray = new float[normCount];
            x30_YNormCoordsArray = new float[normCount];
            x30_ZNormCoordsArray = new float[normCount];

            TRSMat = TRSMat.ClearTranslation();
            TRSMat = TRSMat.ClearScale();

            LogVertices(fsNew, br, 1, TRSMat);

            info = $"#MIN X/Y/Z: {XCoordsArray.Min()}, {YCoordsArray.Min()}, {ZCoordsArray.Min()}\n";
            WriteText(fsNew, info);

            info = $"#MAX X/Y/Z: {XCoordsArray.Max()}, {YCoordsArray.Max()}, {ZCoordsArray.Max()}\n";
            WriteText(fsNew, info);

            MeshPosX = (XCoordsArray.Max() + XCoordsArray.Min()) / 2;
            MeshPosY = (YCoordsArray.Max() + YCoordsArray.Min()) / 2;
            MeshPosZ = (ZCoordsArray.Max() + ZCoordsArray.Min()) / 2;
            info = $"#(MAX + MIN)/2: {MeshPosX}, {MeshPosY}, {MeshPosZ}\n";
            WriteText(fsNew, info);
        }

        public static void LogVertices(FileStream fsNew, BinaryReader br, byte flag, Matrix4 TRSMat)
        {
            string vtxdata = "";
            uint VerticesCount = 0;
            if (flag == 0)
            {
                VerticesCount = vtxCount;
            }
            else if (flag == 1)
            {
                VerticesCount = normCount;
            }
            else
            {
                VerticesCount = uvCount;
            }

            for (int i = 0; i < VerticesCount; i++) //Read and log vertices
            {
                if (flag != 2)
                {
                    Vec3VTX.X = ReadBEFloat(br);
                    Vec3VTX.Y = ReadBEFloat(br);
                    Vec3VTX.Z = ReadBEFloat(br);

                    if (flag == 0)
                    {
                        XCoordsArray[i] = Vec3VTX.X;
                        YCoordsArray[i] = Vec3VTX.Y;
                        ZCoordsArray[i] = Vec3VTX.Z;
                    }
                    else
                    {
                        x30_XNormCoordsArray[i] = Vec3VTX.X;
                        x30_YNormCoordsArray[i] = Vec3VTX.Y;
                        x30_ZNormCoordsArray[i] = Vec3VTX.Z;
                    }

                    Vec3VTX = Vector3.TransformPosition(Vec3VTX, TRSMat);

                    /*Sometimes, the vertices and the normals alternate between each other. Usually when there's bones data.
                     * If this happens, then just skip 0x0C bytes (the 3 norm floats) */
                    if (isNormOff2C4C == 1)
                    {
                        br.BaseStream.Seek(0x0C, SeekOrigin.Current);
                    }
                }
                else
                {
                    Vec2UV.X = ReadBEFloat(br);
                    Vec2UV.Y = ReadBEFloat(br);

                    XCoordsArray[i] = Vec2UV.X;
                    YCoordsArray[i] = Vec2UV.Y;
                }

                if (flag == 0)
                {
                    if (IsRod)
                    {
                        Vec3VTX.X += RTLXChunkStart;
                        Vec3VTX.Z += RTLZChunkStart;

                        Vec3VTX.X += TransCol;
                        Vec3VTX.Z += TransRow;
                    }
                    vtxdata = $"v {Vec3VTX.X} {Vec3VTX.Y} {Vec3VTX.Z}\n";
                }
                else if (flag == 1)
                {
                    vtxdata = $"vn {Vec3VTX.X} {Vec3VTX.Y} {Vec3VTX.Z}\n";
                }
                else
                {
                    YCoordsArray[i] = 1.0F + (YCoordsArray[i] * -1.0F); //Mirror and translate. Important.
                    vtxdata = $"vt {XCoordsArray[i]} {YCoordsArray[i]}\n";
                }

                if (ReplaceCommaWithDot)
                {
                    vtxdata = vtxdata.Replace(",", ".");
                }

                WriteText(fsNew, vtxdata);
            }
        }

        public static float ReadBEFloat(BinaryReader br) //Big Endian Float
        {
            byte[] FloatArray = new byte[4] { br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte() };
            Array.Reverse(FloatArray);
            float result = BitConverter.ToSingle(FloatArray);
            return result;
        }

        public static ushort ReadBEUInt16(BinaryReader br) //Big Endian Unsigned Short 2 Bytes
        {
            ushort value = br.ReadUInt16();
            return (ushort)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }

        public static uint ReadBEUInt32(BinaryReader br) //Big Endian Unsigned Int 4 bytes
        {
            uint value = br.ReadUInt32();
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        public static ushort ReverseUInt16(ushort value)
        {
            return (ushort)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }

        public static uint ReverseUInt32(uint value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        public static byte[] ReverseIntToByteArray(int Value)
        {
            byte[] ArrayValue = BitConverter.GetBytes(Value);
            Array.Reverse(ArrayValue);
            return ArrayValue;
        }

        public static byte[] ReverseShortToByteArray(short Value)
        {
            byte[] ArrayValue = BitConverter.GetBytes(Value);
            Array.Reverse(ArrayValue);
            return ArrayValue;
        }

        public static void WriteText(FileStream fsLog, string text)
        {
            byte[] textbytes = Encoding.UTF8.GetBytes(text);
            fsLog.Write(textbytes, 0, textbytes.Length);
            //fsLog.Flush();
        }

        public static string ReadNullTerminatedString(BinaryReader stream, uint MaxLength)
        {
            string str = "";
            char ch;
            while ((ch = (char)stream.PeekChar()) != 0 && (str.Length < MaxLength))
            {
                ch = stream.ReadChar();
                str += ch;
            }
            stream.BaseStream.Seek(MaxLength - str.Length, SeekOrigin.Current);
            return str;
        }

        public static string[] ReadSlashTerminatedString(string value)
        {
            string[] str = new string[9];
            ushort count = 0;
            string[] Arr;
            string[] NewArr;
            Arr = value.Split(" ");
            Arr = Arr.Skip(1).ToArray();
            foreach (string arga in Arr)
            {
                NewArr = arga.Split("/");
                str[count] = (Convert.ToInt32(NewArr[0]) + vtxCount).ToString();
                str[count + 2] = (Convert.ToInt32(NewArr[2]) + normCount).ToString();
                str[count + 1] = (Convert.ToInt32(NewArr[1]) + uvCount).ToString();
                count += 3;
            }
            return str;
        }

        public static void PrintCenter(string text)
        {
            Console.WriteLine(string.Format("{0," + ((Console.WindowWidth / 2) + (text.Length / 2)) + "}", text));
        }

        public static void PrintInfo()
        {
            PrintCenter("Endless Ocean Files Converter\n");
            PrintCenter("Author: NiV, MDB\n");
            PrintCenter("Special thanks to Hiroshi\n");
            PrintCenter("Version 1.7.1\n"); ;
            PrintCenter("If you have any issues, join this discord server and contact NiV-L-A:\n");
            PrintCenter("https://discord.gg/4hmcsmPMDG\n");
        }

        public static ushort StringToUInt16(string text)
        {
            return BitConverter.ToUInt16(Encoding.ASCII.GetBytes(text));
        }

        public static uint StringToUInt32(string text)
        {
            return BitConverter.ToUInt32(Encoding.ASCII.GetBytes(text));
        }

        public static void GetMagic(BinaryReader br)
        {
            //******************************
            //******* Magic detection*******
            //******************************

            br.BaseStream.Seek(0x00, SeekOrigin.Begin); //The magic is composed of a total of 6 bytes.
            Magic = br.ReadUInt32(); //First 4, can be "RF2M" or "RFPM". RFPM is a special type used rarely.
            Magic2 = br.ReadUInt16(); //Last 2, can be "D2" or "D3" - prob indicating the version

            if (Magic == StringToUInt32("RFPM")) //RFPM
            {
                RFPMD2 = true;
            }
            if (!RFPMD2)
            {
                if (Magic2 == StringToUInt16("D2")) //D2
                {
                    RF2MD2 = true;
                    ObjectListType = 0x12E;
                }
                else if (Magic2 == StringToUInt16("D3")) //D3
                {
                    RF2MD3 = true;
                    ObjectListType = 0x130;
                }
            }
            if (Magic != StringToUInt32("RFPM") && Magic != StringToUInt32("RF2M") && Magic2 != StringToUInt16("D2") && Magic2 != StringToUInt16("D3")) //No type of Magic2 recognized, most likely wrong file as input
            {
                PrintInfo();
                br = null;
                PrintError($"Error: Magic is missing! Are you loading the correct file?\nMagic: 0x{ReverseUInt32(Magic):X8} - 0x{ReverseUInt16(Magic2):X4} | {Encoding.Default.GetString(BitConverter.GetBytes(Magic))} - {Encoding.Default.GetString(BitConverter.GetBytes(Magic2))}\nFile path: {pathReal}\n");
                Console.WriteLine("Press any key to close the window");
                Console.Read();
                return;
            }
        }

        public static void GetOffsetsAndCounts(BinaryReader br)
        {
            //**************************************
            //******* Get Offsets and Counts *******
            //**************************************
            Entries = br.ReadUInt16();
            EntryList_SIZE = br.ReadUInt16();
            br.BaseStream.Seek(0x02, SeekOrigin.Current); //Skip DUMMY
            HEAD_SIZE = br.ReadUInt32();
            if (RFPMD2) //EO Special Format
            {
                br.BaseStream.Seek(0x10, SeekOrigin.Current);
            }
            else //EO1 & EO2
            {
                br.BaseStream.Seek(0x18, SeekOrigin.Current);
            }
            VDLOff = br.ReadUInt32(); //Offset of the .vdl

            br.BaseStream.Seek(EntryList_SIZE, SeekOrigin.Begin);
            if (RF2MD2 || RFPMD2) //EO1
            {
                br.BaseStream.Seek(0x04, SeekOrigin.Current);
                ObjectsCount = br.ReadUInt16();
                TDLFilesRefCount = br.ReadUInt16();
                RF2MD2MatCount = br.ReadUInt16();
            }
            else if (RF2MD3) //EO2
            {
                br.BaseStream.Seek(0x02, SeekOrigin.Current);
                ObjectListType = br.ReadUInt16();
                ObjectsCount = br.ReadUInt16();
                TDLFilesRefCount = br.ReadUInt16();
                br.BaseStream.Seek(0x02, SeekOrigin.Current);
                MatIndicesOffCount = br.ReadUInt16();
                MatIndicesCount = br.ReadUInt16();
            }

            MeshCount = br.ReadUInt16(); //Mesh count at +0x0E or both +0x0E and +0x10

            if (RF2MD2 || RFPMD2)
            {
                br.BaseStream.Seek(0x04, SeekOrigin.Current);
                MatIndicesOff = br.ReadUInt32();
            }
            else if (RF2MD3)
            {
                br.BaseStream.Seek(0x08, SeekOrigin.Current);
                MatIndicesInfoOff = br.ReadUInt32();
                MatIndicesOff = br.ReadUInt32();
            }

            MeshTotalCount += MeshCount;
            MESH_INFO_Offset = br.BaseStream.Position;
        }

        public static void GetTDLNames(BinaryReader br, ushort Entries)
        {

            //***********************************************************
            //************** GET TDL NAMES FROM ENTRY LIST **************
            //***********************************************************
            tdlNameArray = new string[TDLFilesRefCount];
            br.BaseStream.Seek(0x10, SeekOrigin.Begin);
            ushort contatoreTDLNames = 0;
            for (ushort GetTDLNamesCount = 0; GetTDLNamesCount < Entries; GetTDLNamesCount++)
            {
                br.BaseStream.Seek(0x1C, SeekOrigin.Current);
                byte indexExtensionType = br.ReadByte();
                if (indexExtensionType == ((byte)RFExtensionType.TDL))
                {
                    br.BaseStream.Seek(-0x1D, SeekOrigin.Current);
                    tdlNameArray[contatoreTDLNames] = ReadNullTerminatedString(br, 0x14);
                    tdlNameArray[contatoreTDLNames] = tdlNameArray[contatoreTDLNames].Replace(".tdl", ".png");
                    br.BaseStream.Seek(0x0c, SeekOrigin.Current);
                    //br.BaseStream.Seek(0x1F - tdlNameArray[contatoreTDLNames].Length, SeekOrigin.Current);
                    contatoreTDLNames += 1;
                }
                else
                {
                    br.BaseStream.Seek(0x03, SeekOrigin.Current);
                }
            }
        }

        public static void CreateAndPopulateMTLFile(string arg)
        {
            //***********************************************************************************
            //******* CREATE & POPULATE MATERIAL.MTL FILE ********
            //***********************************************************************************

            string MatPath = "";
            if (CreateFolder)
            {
                MatPath = $"{Path.GetDirectoryName(arg)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(arg)}\\{Path.GetFileNameWithoutExtension(arg)}.mtl";
            }
            else
            {
                MatPath = $"{Path.GetDirectoryName(arg)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(arg)}.mtl";
            }
            File.Delete(MatPath); //delete file if it exists already
            using FileStream fsMat = File.OpenWrite(MatPath);
            {
                for (ushort MatHeaderIndex = 0; MatHeaderIndex < TDLFilesRefCount; MatHeaderIndex++)
                {
                    WriteText(fsMat, $"newmtl material{MatHeaderIndex}\nmap_Kd {tdlNameArray[MatHeaderIndex]}\n\n");
                }
                fsMat.Flush();
            }
        }

        public static void GetMaterialsID(BinaryReader br)
        {
            //**********************************************
            //************** GET MATERIALS ID **************
            //**********************************************

            if (RF2MD3)
            {
                MatIndices = new ushort[MatIndicesCount];
                br.BaseStream.Seek(MatIndicesInfoOff, SeekOrigin.Begin);
                for (int ContatoreMatIndicesCount = 0; ContatoreMatIndicesCount < MatIndicesOffCount; ContatoreMatIndicesCount++)
                {
                    uint OffToMatIndex = ReadBEUInt32(br);
                    byte FlagNumberOfInts = br.ReadByte();
                    br.BaseStream.Seek(0x07, SeekOrigin.Current);
                    uint TMPOffMat = (uint)br.BaseStream.Position;
                    br.BaseStream.Seek(OffToMatIndex, SeekOrigin.Begin);
                    /*
                    if (FlagNumberOfInts == 2)//work-around.
                    {
                        fs.Seek(0x04, SeekOrigin.Current);
                    }
                    */
                    MatIndices[ContatoreMatIndicesCount] = ReadBEUInt16(br);
                    br.BaseStream.Seek(TMPOffMat, SeekOrigin.Begin);
                }
            }
            else
            {
                MatIndices = new ushort[RF2MD2MatCount];
                br.BaseStream.Seek(MatIndicesOff, SeekOrigin.Begin);
                for (int ContatoreMatIndicesCount = 0; ContatoreMatIndicesCount < RF2MD2MatCount; ContatoreMatIndicesCount++)
                {
                    MatIndices[ContatoreMatIndicesCount] = ReadBEUInt16(br);
                    br.BaseStream.Seek(0x0A, SeekOrigin.Current);
                }
            }
        }

        public static void ParseHeader(BinaryReader br, string arg)
        {
            GetMagic(br);
            if (CreateFolder)
            {
                Directory.CreateDirectory($"{Path.GetDirectoryName(arg)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(arg)}");
            }
            GetOffsetsAndCounts(br);
            GetTDLNames(br, Entries);
            CreateAndPopulateMTLFile(arg);
            GetMaterialsID(br);
        }

        public static void DuplicateCurrentMesh(BinaryReader br, Matrix4 TRSMat)
        {
            NextHierarchyObject -= 1;
            Matrix4 TRSMatOg = CreateTRSMatrixFromHierarchyList(br, (ushort)(MeshProgressCount - 1));
            x30LastLine = fsNew.Position;
            StreamReader reader = new(fsNew);

            for (int testa = 1; testa < Dictx30_Countx30Codes[x30CurrentChunk]; testa++)
            {
                Console.WriteLine($"[Duplicating Mesh: {testa}/{Dictx30_Countx30Codes[x30CurrentChunk] - 1}]");
                TRSMat = Getx30App(x30CurrentChunk, (ushort)testa, TRSMatOg);
                DuplicatedMeshes += 1;

                if(DumpSingleOBJFile)
                {
                    long x30TMP = x30FirstLine;
                    fsNew.Position = x30TMP;
                    uint x30vtxInternalCount = 0;
                    uint x30normInternalCount = 0;
                    string line = "";
                    string AllLines = reader.ReadToEnd();
                    string[] result = AllLines.Split("\n");
                    result = result.Take(result.Count() - 1).ToArray();

                    x30FirstLine = fsNew.Position;

                    for (int i = 0; i < result.Length; i++)
                    {
                        line = result[i].Substring(0, 2);
                        if (line == "v ")
                        {
                            Vec3VTX.X = x30_XCoordsArray[x30vtxInternalCount];
                            Vec3VTX.Y = x30_YCoordsArray[x30vtxInternalCount];
                            Vec3VTX.Z = x30_ZCoordsArray[x30vtxInternalCount];

                            Vec3VTX = Vector3.TransformPosition(Vec3VTX, TRSMat);

                            string vtxdata = $"v {Vec3VTX.X} {Vec3VTX.Y} {Vec3VTX.Z}\n";
                            WriteText(fsNew, vtxdata);
                            x30vtxInternalCount += 1;
                        }
                        else if (line == "vn")
                        {
                            if (x30normInternalCount == 0)
                            {
                                TRSMat = TRSMat.ClearTranslation();
                                TRSMat = TRSMat.ClearScale();
                            }
                            Vec3VTX.X = x30_XNormCoordsArray[x30normInternalCount];
                            Vec3VTX.Y = x30_YNormCoordsArray[x30normInternalCount];
                            Vec3VTX.Z = x30_ZNormCoordsArray[x30normInternalCount];

                            Vec3VTX = Vector3.TransformPosition(Vec3VTX, TRSMat);

                            string vtxdata = $"vn {Vec3VTX.X} {Vec3VTX.Y} {Vec3VTX.Z}\n";
                            WriteText(fsNew, vtxdata);
                            x30normInternalCount += 1;
                        }
                        else if (line == "f ") //faces
                        {
                            string[] str = ReadSlashTerminatedString(result[i]);
                            string text = $"f {str[0]}/{str[1]}/{str[2]} {str[3]}/{str[4]}/{str[5]} {str[6]}/{str[7]}/{str[8]}";
                            WriteText(fsNew, text + "\n");
                        }
                        else //Everything else: comments, uvs, faces
                        {
                            WriteText(fsNew, result[i] + "\n");
                        }
                    }

                    x30LastLine = fsNew.Position;

                }
                else
                {
                    string x30NewPath = "";
                    string line = "";
                    x30NewPath = $"{NewPath.Substring(0, NewPath.Length - 4)}_x30_{testa}.obj";
                    if (File.Exists(x30NewPath))
                    {
                        File.Delete(x30NewPath);
                    }
                    File.Copy(NewPath, x30NewPath);
                    string[] Arrayx30File = File.ReadAllLines(x30NewPath);
                    uint x30vtxInternalCount = 0;
                    uint x30normInternalCount = 0;
                    using FileStream fsx30 = File.OpenWrite(x30NewPath);
                    {
                        for (int i = 0; i < Arrayx30File.Length; i++)
                        {
                            line = Arrayx30File[i].Substring(0, 2);
                            if (line == "v ")
                            {
                                Vec3VTX.X = x30_XCoordsArray[x30vtxInternalCount];
                                Vec3VTX.Y = x30_YCoordsArray[x30vtxInternalCount];
                                Vec3VTX.Z = x30_ZCoordsArray[x30vtxInternalCount];

                                Vec3VTX = Vector3.TransformPosition(Vec3VTX, TRSMat);

                                string vtxdata = $"v {Vec3VTX.X} {Vec3VTX.Y} {Vec3VTX.Z}\n";
                                WriteText(fsx30, vtxdata);
                                x30vtxInternalCount += 1;
                            }
                            else if (line == "vn")
                            {
                                if (x30normInternalCount == 0)
                                {
                                    TRSMat = TRSMat.ClearTranslation();
                                    TRSMat = TRSMat.ClearScale();
                                }
                                Vec3VTX.X = x30_XNormCoordsArray[x30normInternalCount];
                                Vec3VTX.Y = x30_YNormCoordsArray[x30normInternalCount];
                                Vec3VTX.Z = x30_ZNormCoordsArray[x30normInternalCount];

                                Vec3VTX = Vector3.TransformPosition(Vec3VTX, TRSMat);

                                string vtxdata = $"vn {Vec3VTX.X} {Vec3VTX.Y} {Vec3VTX.Z}\n";
                                WriteText(fsx30, vtxdata);
                                x30normInternalCount += 1;
                            }
                            else //Everything else: comments, uvs, faces
                            {
                                WriteText(fsx30, Arrayx30File[i] + "\n");
                            }
                        }
                        fsx30.Flush();
                    }
                }
            }
            Console.Write("\n");
        }

        public static void PrintError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void ConvertBMPToPNG(string BMPFilePath)
        {
            Bitmap bmp1 = new(BMPFilePath);
            bmp1.Save(BMPFilePath.Substring(0, BMPFilePath.Length-4) + ".png", ImageFormat.Png);
        }

        public static void ParseRTL(string[] RTLargs)
        {
            if (Path.GetFileNameWithoutExtension(RTLargs[0]) != "s01f")
            {
                using FileStream fsRTL = new(RTLargs[0], FileMode.Open);
                using BinaryReader brRTL = new(fsRTL);

                fsRTL.Seek(0x08, SeekOrigin.Begin);
                RTLColCount = ReadBEUInt16(brRTL);
                RTLRowCount = ReadBEUInt16(brRTL);
                RTLChunkSize = ReadBEFloat(brRTL);
                RTLXChunkStart = ReadBEFloat(brRTL);
                RTLZChunkStart = ReadBEFloat(brRTL);
                fsRTL.Seek(0x24, SeekOrigin.Begin);

                uint Just = 0;
                Rows = new List<List<ushort>>();
                List<ushort> QW = new();

                for (int i = 0; i < RTLRowCount; i++)
                {
                    List<ushort> Row = new();
                    for (int j = 0; j < RTLColCount; j++)
                    {
                        ushort RTLCurrIndex = ReadBEUInt16(brRTL);
                        Row.Add(RTLCurrIndex);
                        QW.Add(RTLCurrIndex);
                        Just++;
                    }
                    Rows.Add(Row);
                }

                fsRTL.Dispose();
                brRTL.Dispose();
                fsRTL.Close();
                brRTL.Close();
                RTLParsed = true;
            }
            else if (Path.GetFileNameWithoutExtension(RTLargs[0]) == "s01f")
            {
                using FileStream fsRTL = new(RTLargs[0], FileMode.Open);
                using BinaryReader brRTL = new(fsRTL);

                ushort[] ArrayLen = new ushort[6];
                fsRTL.Seek(0x10, SeekOrigin.Begin);

                for (int j = 0; j < 6; j++)
                {
                    ArrayLen[j] = ReverseUInt16(brRTL.ReadUInt16());
                }

                Quadrants = new List<List<ushort>>();
                fsRTL.Seek(0x24, SeekOrigin.Begin);

                int i = 0;
                while (i < 6)
                {
                    List<ushort> Quad = new();
                    ushort Len = ArrayLen[i];
                    for (int k = 0; k < Len; k++)
                    {
                        ushort RTLCurrIndex = ReadBEUInt16(brRTL);
                        Quad.Add(RTLCurrIndex);
                    }
                    Quadrants.Add(Quad);
                    i++;
                }

                fsRTL.Dispose();
                brRTL.Dispose();
                fsRTL.Close();
                brRTL.Close();
                RTLParsed = true;
            }
        }

        public static List<string> FindExtInArray(string[] array, string value)
        {
            List<string> EXTList = new();
            foreach (string arg in array)
            {
                if (Path.GetExtension(arg) == value)
                {
                    EXTList.Add(arg);
                }
            }
            return EXTList;
        }

        public static void ParseIndices(BinaryReader br, FileStream fsNew, ushort MeshNSection, uint[] MeshArray, byte[] optimization, ushort[] MatIndex)
        {
            ushort indunk = 0;
            ushort cnt = 0;
            for (int IndexSection = 0; IndexSection < MeshNSection; IndexSection++)
            {
                ushort NStrips = 0;
                br.BaseStream.Seek(MeshArray[IndexSection], SeekOrigin.Begin);
                string info1 = $"usemtl material{MatIndices[MatIndex[IndexSection]]}\n#POS: 0x{br.BaseStream.Position:X8}\n";
                WriteText(fsNew, info1);

                if (optimization[IndexSection] == 3) //triangles. Only 1 draw call. cnt / 3 = Number of strips.
                {
                    do
                    {
                        indunk = ReadBEUInt16(br);
                        cnt = ReadBEUInt16(br);
                        NStrips = (ushort)(cnt / 3);
                        for (int i = 0; i < NStrips; i++)
                        {
                            GetIndexStripA(br);

                            string data = "";
                            if (pos == pos2 || pos == pos3 || pos2 == pos3)
                            {
                                StripWithEqualFaceCount += 1;
                                data = $"#f {pos3}/{uv3}/{norm3} {pos2}/{uv2}/{norm2} {pos}/{uv}/{norm}\n";
                            }
                            else
                            {
                                //data = $"f {pos}/{uv}/{norm} {pos2}/{uv2}/{norm2} {pos3}/{uv3}/{norm3}\n"; //importing
                                data = $"f {pos3}/{uv3}/{norm3} {pos2}/{uv2}/{norm2} {pos}/{uv}/{norm}\n"; //exporting
                            }

                            WriteText(fsNew, data);
                        }
                    } while ((indunk & 0x1) != 0x0);

                }
                else/* if (optimization[IndexSection] == 4)*/ //tristrip
                {
                    do
                    {
                        indunk = ReadBEUInt16(br);
                        cnt = ReadBEUInt16(br);

                        while (cnt < 3)
                        {
                            br.BaseStream.Position = br.BaseStream.Position + (cnt * FragmentSize);
                            indunk = ReadBEUInt16(br);
                            cnt = ReadBEUInt16(br);
                        }

                        NStrips = (ushort)(cnt - 2);
                        for (int i = 0; i < NStrips; i++)
                        {
                            if (i != 0)
                            {
                                br.BaseStream.Seek(TMP2, SeekOrigin.Begin);
                            }
                            GetIndexStripA(br);

                            string data = "";
                            if (pos == pos2 || pos == pos3 || pos2 == pos3)
                            {
                                StripWithEqualFaceCount += 1;
                                data = $"#f {pos3}/{uv3}/{norm3} {pos2}/{uv2}/{norm2} {pos}/{uv}/{norm}\n";
                            }
                            else
                            {
                                //data = $"f {pos}/{uv}/{norm} {pos2}/{uv2}/{norm2} {pos3}/{uv3}/{norm3}\n"; //importing
                                data = $"f {pos3}/{uv3}/{norm3} {pos2}/{uv2}/{norm2} {pos}/{uv}/{norm}\n"; //exporting
                            }

                            WriteText(fsNew, data);

                            if (i != NStrips - 1)
                            {
                                GetIndexStripB(br);
                                i += 1;

                                string data2 = "";
                                if (pos4 == pos2 || pos4 == pos3 || pos2 == pos3)
                                {
                                    StripWithEqualFaceCount += 1;
                                    data2 = $"#f {pos2}/{uv2}/{norm2} {pos3}/{uv3}/{norm3} {pos4}/{uv4}/{norm4}\n";
                                }
                                else
                                {
                                    //data2 = $"f {pos4}/{uv4}/{norm4} {pos3}/{uv3}/{norm3} {pos2}/{uv2}/{norm2}\n"; //importing
                                    data2 = $"f {pos2}/{uv2}/{norm2} {pos3}/{uv3}/{norm3} {pos4}/{uv4}/{norm4}\n"; //exporting
                                }

                                WriteText(fsNew, data2);
                            }
                        }
                    } while ((indunk & 0x1) != 0x0);
                }
            }
        }

        public static void UnderstandIndexCount()
        {
            if (GPU2 != 0) //has GPU byte. Better, faster
            {
                if ((GPU2 & 0b00000011) == 0b00000011)
                {
                    vtxIndex = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else if ((GPU2 & 0b00000010) == 0b00000010)
                {
                    vtxIndex = IndexStatus.BYTE;
                    FragmentSize += 1;
                }

                if ((GPU2 & 0b00001100) == 0b00001100)
                {
                    normIndex = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else if ((GPU2 & 0b00001000) == 0b00001000)
                {
                    normIndex = IndexStatus.BYTE;
                    FragmentSize += 1;
                }

                if ((GPU2 & 0b00110000) == 0b00110000)
                {
                    lightIndex = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else if ((GPU2 & 0b00100000) == 0b00100000)
                {
                    lightIndex = IndexStatus.BYTE;
                    FragmentSize += 1;
                }

                if ((GPU2 & 0b11000000) == 0b11000000)
                {
                    uvIndex = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else if ((GPU2 & 0b10000000) == 0b10000000)
                {
                    uvIndex = IndexStatus.BYTE;
                    FragmentSize += 1;
                }

                if (GPU != 0)
                {
                    if ((GPU & 0b00000011) == 0b00000011)
                    {
                        unk01Index = IndexStatus.SHORT;
                        FragmentSize += 2;
                    }
                    else if ((GPU & 0b00000010) == 0b00000010)
                    {
                        unk01Index = IndexStatus.BYTE;
                        FragmentSize += 1;
                    }
                }
            }
            else //No GPU byte, we can understand how the indices are formed by the vertices counts
            {
                if (vtxCount >= 0xFF || RF2MD2 || RFPMD2 || indexCount == 0)
                {
                    vtxIndex = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else
                {
                    vtxIndex = IndexStatus.BYTE;
                    FragmentSize += 1;
                }

                if (normCount >= 0xFF || RF2MD2 || RFPMD2 || indexCount == 0)
                {
                    normIndex = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else
                {
                    normIndex = IndexStatus.BYTE;
                    FragmentSize += 1;
                }

                if (lightCount >= 0xFF || (RF2MD2 || RFPMD2) && lightOff > 0x00 || indexCount == 0 && lightOff > 0) //If RF2MD2 and there's light data, light will always be 2 bytes for the indices.
                {
                    lightIndex = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else if (RF2MD3 && lightOff > 0)
                {
                    lightIndex = IndexStatus.BYTE;
                    FragmentSize += 1;
                }
                else
                {
                    lightIndex = IndexStatus.NONE;
                }

                if (uvCount >= 0xFF || RF2MD2 || RFPMD2 || indexCount == 0)
                {
                    uvIndex = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else
                {
                    uvIndex = IndexStatus.BYTE;
                    FragmentSize += 1;
                }

                if (unk01Off > 0 && unk01Count >= 0xFF) //unknown data, it's indexed tho, so let's count it.
                {
                    unk01Index = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else if (RF2MD3 && unk01Off > 0)
                {
                    unk01Index = IndexStatus.BYTE;
                    FragmentSize += 1;
                }
                else
                {
                    unk01Index = IndexStatus.NONE;
                }
            }
        }

        public static void GetHierarchyList(BinaryReader br)
        {
            for (int i = 0; i < ObjectsCount; i++)
            {
                MDLStream.CHierarchyObject HierarchyObject = new(br, VDLOff);
                HierarchyList.Add(HierarchyObject);
                if (HierarchyList[i].ObjCode == 0x30)
                {
                    if (Dictx30_Countx30Codes.ContainsKey(HierarchyList[i].MeshIndex))
                    {
                        Dictx30_Countx30Codes[HierarchyList[i].MeshIndex]++;
                    }
                    else
                    {
                        Dictx30_Countx30Codes.Add(HierarchyList[i].MeshIndex, 1);
                    }
                }
                /*
                else if (HierarchyList[i].ObjCode == 0x50)
                {
                    if (Dictx50_Countx50Codes.ContainsKey(HierarchyList[i].MeshIndex))
                    {
                        Dictx50_Countx50Codes[HierarchyList[i].MeshIndex]++;
                    }
                    else
                    {
                        Dictx50_Countx50Codes.Add(HierarchyList[i].MeshIndex, 1);
                    }
                }
                */
            }
        }

        public static Matrix4 CreateTRSMatrixFromHierarchyList(BinaryReader br, ushort MeshIndexTarget)
        {
            Matrix4 TRSMat = Matrix4.CreateFromQuaternion(QuatIdentity);

            for (int i = NextHierarchyObject; i < ObjectsCount; i++) //i = 0
            {
                if (DuplMesh)
                {
                    if (HierarchyList[i].Level <= x30MeshLevel)
                    {
                        DuplMesh = false;
                        x30IncreaserCounter += 1;
                    }
                }

                if (HierarchyList[i].MeshIndex == MeshIndexTarget && HierarchyList[i].ObjCode == 0x20)
                {
                    TRSMat *= Matrix4.CreateScale(HierarchyList[i].Scale);
                    TRSMat *= Matrix4.CreateFromQuaternion(HierarchyList[i].Rotation);
                    TRSMat *= Matrix4.CreateTranslation(HierarchyList[i].Translation);
                    MeshName = HierarchyList[i].MeshName;
                    uint LevelDecreaser = 1;
                    NextHierarchyObject = (ushort)(i + 1);

                    if (DuplMesh)
                    {
                        if (HierarchyList[i].NObject == Dictx30_Countx30Codes.ElementAt(x30IncreaserCounter).Key + 1) //level+1
                        {
                            return TRSMat;
                        }
                        else if (HierarchyList[i].Level == x30MeshLevel + 1 && !DuplicatingMesh) //level+1 but not the first
                        {
                            TRSMat = Getx30App(Dictx30_Countx30Codes.ElementAt(x30IncreaserCounter).Key, 0, TRSMat);
                            return TRSMat;
                        }
                        else if (HierarchyList[i].Level == x30MeshLevel + 1 && DuplicatingMesh)
                        {
                            return TRSMat;
                        }
                        else //level+1+x
                        {
                            for (int j = HierarchyList[i].NObject - 1; j < HierarchyList[i].NObject; j--)
                            {
                                if (HierarchyList[j].Level == HierarchyList[i].Level - LevelDecreaser)
                                {
                                    TRSMat *= Matrix4.CreateScale(HierarchyList[j].Scale);
                                    TRSMat *= Matrix4.CreateFromQuaternion(HierarchyList[j].Rotation);
                                    TRSMat *= Matrix4.CreateTranslation(HierarchyList[j].Translation);
                                    LevelDecreaser += 1;

                                    if (HierarchyList[j].Level == x30MeshLevel + 1)
                                    {
                                        if(!DuplicatingMesh)
                                        {
                                            TRSMat = Getx30App(Dictx30_Countx30Codes.ElementAt(x30IncreaserCounter).Key, 0, TRSMat);
                                        }

                                        return TRSMat;
                                    }
                                }
                            }
                        }
                    }

                    if (HierarchyList[i].Level != 0)
                    {
                        for (int j = HierarchyList[i].NObject - 1; j < HierarchyList[i].NObject; j--)
                        {
                            if (HierarchyList[j].Level == HierarchyList[i].Level - LevelDecreaser)
                            {
                                TRSMat *= Matrix4.CreateScale(HierarchyList[j].Scale);
                                TRSMat *= Matrix4.CreateFromQuaternion(HierarchyList[j].Rotation);
                                TRSMat *= Matrix4.CreateTranslation(HierarchyList[j].Translation);
                                LevelDecreaser += 1;

                                if (HierarchyList[j].Level == 0)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    break;
                }
                else if (Dictx30_Countx30Codes.ContainsKey((ushort)HierarchyList[i].NObject))
                {
                    if (HierarchyList[i].NObject == Dictx30_Countx30Codes.ElementAt(x30IncreaserCounter).Key)
                    {
                        x30CurrentChunk = (ushort)HierarchyList[i].NObject;
                        NextHierarchyObject = (ushort)(i + 1);
                        x30MeshLevel = HierarchyList[i].Level;
                        DuplMesh = true; //false se level è <=
                        TRSMat = CreateTRSMatrixFromHierarchyList(br, MeshIndexTarget);
                        TRSMat = Getx30App(Dictx30_Countx30Codes.ElementAt(x30IncreaserCounter).Key, 0, TRSMat);
                        return TRSMat;

                    }
                }
            }
            return TRSMat;
        }

        public static Matrix4 Getx30App(ushort ObjectTarget, ushort Increaser, Matrix4 TRSMat)
        {
            ushort InternalIncreaser = 0;
            for (int i = 0; i < ObjectsCount; i++)
            {
                if (HierarchyList[i].ObjCode == 0x30 && HierarchyList[i].MeshIndex == ObjectTarget)
                {
                    if(InternalIncreaser < Increaser)
                    {
                        InternalIncreaser++;
                        continue;
                    }
                    TRSMat *= Matrix4.CreateScale(HierarchyList[i].Scale);
                    TRSMat *= Matrix4.CreateFromQuaternion(HierarchyList[i].Rotation);
                    TRSMat *= Matrix4.CreateTranslation(HierarchyList[i].Translation);

                    uint LevelDecreaser = 1;

                    if (HierarchyList[i].Level != 0)
                    {
                        for (int j = HierarchyList[i].NObject - 1; j < HierarchyList[i].NObject; j--)
                        {
                            if (HierarchyList[j].Level == HierarchyList[i].Level - LevelDecreaser)
                            {
                                TRSMat *= Matrix4.CreateScale(HierarchyList[j].Scale);
                                TRSMat *= Matrix4.CreateFromQuaternion(HierarchyList[j].Rotation);
                                TRSMat *= Matrix4.CreateTranslation(HierarchyList[j].Translation);
                                LevelDecreaser += 1;

                                if (HierarchyList[j].Level == 0)
                                {
                                    return TRSMat;
                                }
                            }
                        }
                    }
                    break;
                }
            }
            return TRSMat;
        }

        public enum IndexStatus
        {
            NONE = 0,
            BYTE = 2,
            SHORT = 3
        }

        public enum RFExtensionType
        {
            VDL = 0,
            TDL = 1,
            TXS = 2,
            MOT = 6,
            MOL = 7
        }
    }
}