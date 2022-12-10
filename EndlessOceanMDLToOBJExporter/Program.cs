using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;
using OpenTK.Mathematics;
using Crews.Utility.TgaSharp;
using EndlessOceanFilesConverter;
using static EndlessOceanFilesConverter.Utils;

namespace EndlessOceanMDLToOBJExporter
{
    class Program
    {
        public static bool IsRod = false;
        public static bool DuplMesh = false;
        public static bool RTLParsed = false;
        public static bool DumpSingleOBJFile = true;
        public static byte x30MeshLevel = 0;
        public static ushort MeshTotalCount = 0;
        public static ushort x30CurrentChunk = 0;
        public static ushort DuplicatedMeshes = 0;
        public static ushort RTLRowCount = 0;
        public static ushort RTLColCount = 0;
        public static ushort NextHierarchyObject = 0;
        public static ushort x30IncreaserCounter = 0;
        public static int MDLCount = 0;
        public static int TDLCount = 0;
        public static int HITCount = 0;
        public static int RFCount = 0;
        public static int MDLCounter = 0;
        public static int TDLCounter = 0;
        public static int HITCounter = 0;
        public static int RFCounter = 0;
        public static int MeshProgressCount = 0;
        public static uint VDLOff = 0;
        public static uint OutputFormat = 0;
        public static uint PrevVtxCount = 0;
        public static uint PrevNormCount = 0;
        public static uint PrevUvCount = 0;
        public static float TransRow = 0;
        public static float TransCol = 0;
        public static float RTLChunkSize = 0;
        public static float RTLXChunkStart = 0;
        public static float RTLZChunkStart = 0;
        public static string MeshName = "";
        public static string NewPath = "";
        public static string FileTypes = "*.rtl,*.mdl,*.tdl,*.hit,*.pak,*.txs";
        public const string Version = "1.7.5";
        public const string SupportedFilesMsg = "To use this tool, drag and drop a file, multiple files, a folder, or multiple folders, containing one or more of the following supported file formats:\n.mdl -> .obj\n.hit -> .obj\n.tdl -> .tga\n.pak -> Dumps contents by creating a folder with the same name\n.txs -> Dumps contents by creating a folder with the same name\n.rtl -> To be passed along with the bNNrodMM.mdl files";
        public static Regex rxrod = new(@"^b(\d{2})rod(\d{2}).*"); //Regular expression for eo2 rods: bNNrodMM...
        public static Regex rxs01f = new(@"^s01f(\d{2})(\d{2}).*"); //Regular expression for eo1 s01f: s01fNNMM...
        public static Quaternion QuatIdentity = new(0, 0, 0, 1);
        public static Dictionary<ushort, ushort> x30CodesDict = new();
        public static Stopwatch stopWatch = new();
        public static List<List<ushort>> Rows = null;
        public static List<List<ushort>> Quadrants = null;
        public static List<string> ArgsToDelete = new();
        public static List<MDLStream.CHierarchyObject> HierarchyList;
        public static FileStream fsNew;

        static void Main(string[] args)
        {
            Console.Title = $"Endless Ocean Files Converter v{Version}";
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            PrintInfo();
            if (args.Length == 0) //if no args are passed to the .exe
            {
                PrintError($"No arguments passed.");
                Console.WriteLine($"{SupportedFilesMsg}\n\nPress any key to close this window.");
                Console.Read();
                return;
            }
            else
            {
                args = ParseInput(args).ToArray();
                if (args.Length == 0)
                {
                    PrintError($"One or multiple arguments have been passed, but no correct file was found.");
                    Console.WriteLine($"{SupportedFilesMsg}\n\nPress any key to close this window.");
                    Console.Read();
                    return;
                }
                int narg = 0;
                stopWatch.Start();

                for (int argi = 0; argi < args.Length; argi++)
                {
                    string arg = args[argi];
                    string ArgExt = Path.GetExtension(arg);
                    FileStream fs = null;
                    EndianBinaryReader br = null;

                    try
                    {
                        fs = new FileStream(arg, FileMode.Open);
                        br = new EndianBinaryReader(fs, EndianBinaryReader.Endianness.Little);
                    }
                    catch (IOException ex)
                    {
                        PrintError($"{ex.Message}");
                        fs = null;
                        br = null;
                        Console.WriteLine("Press any key to close the window");
                        Console.Read();
                        return;
                    }
                    catch (UnauthorizedAccessException uax)
                    {
                        PrintError($"{uax.Message}\nConsider moving the file in another folder!");
                        fs = null;
                        br = null;
                        Console.WriteLine("Press any key to close the window");
                        Console.Read();
                        return;
                    }

                    if (ArgExt == ".mdl")
                    {
                        //Handle .mdl
                        MDLCounter += 1;
                        MeshProgressCount = 0x00;
                        narg += 1;
                        NextHierarchyObject = 0;
                        x30IncreaserCounter = 0;
                        HierarchyList = new();
                        x30CodesDict = new();
                        DuplMesh = false;
                        MDLStream MDLStream = new(br);
                        ushort MeshCount = MDLStream.Header.CountsOffs.MeshCount;
                        if (MeshCount == 0)
                        {
                            PrintWarning($"No meshes found for {arg}. File not parsed.\n");
                            continue;
                        }

                        CreateAndPopulateMTLFile(MDLStream.Header.RFHeader, arg);
                        args = ExtractFilesAndUpdateArgs(br, MDLStream.Header.RFHeader, args, arg);
                        br._endianness = EndianBinaryReader.Endianness.Big;
                        if (VDLOff == 0)
                        {
                            PrintWarning($".vdl not present for {arg}. File not parsed.\n");
                            continue;
                        }
                        br.BaseStream.Seek(VDLOff, SeekOrigin.Begin);
                        if (MDLStream.Header.CountsOffs.ObjListType >= 0x12F)
                        {
                            br.Skip(12);
                        }

                        GetHierarchyList(br, MDLStream.Header.CountsOffs.ObjectsCount);
                        x30CodesDict = x30CodesDict.OrderBy(obj => obj.Key).ToDictionary(obj => obj.Key, obj => obj.Value);
                        if (RTLParsed) //Only if the .rtl was passed in the arguments
                        {
                            IsRod = false;
                            if (rxrod.IsMatch(Path.GetFileNameWithoutExtension(arg))) //"^b(\d{2})rod(\d{2}).*" - bNNrodMM...
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
                            else if (rxs01f.IsMatch(Path.GetFileNameWithoutExtension(arg))) //"^s01f(\d{2})(\d{2}).*" - s01fNNMM...
                            {
                                GetTransColRow(arg);
                            }
                        }
                        if(DumpSingleOBJFile)
                        {
                            NewPath = $"{Path.GetDirectoryName(arg)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(arg)}{ArgExt}.obj";
                            File.Delete(NewPath);
                            fsNew = File.Open(NewPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

                            PrevVtxCount = 0;
                            PrevNormCount = 0;
                            PrevUvCount = 0;
                        }

                        Matrix4 TRSMat = Matrix4.CreateFromQuaternion(QuatIdentity);
                        WriteText(fsNew, $"mtllib {Path.GetFileNameWithoutExtension(arg)}.mtl\n");
                        MeshTotalCount += MeshCount;
                        for (int i = 0; i < MeshCount; i++)
                        {
                            MeshProgressCount = i + 1;
                            Console.Title = $"Endless Ocean Files Converter - Arg: {narg}/{args.Length} - .mdl ({MDLCounter}/{MDLCount}) - Mesh: {MeshProgressCount}/{MeshCount}";
                            MDLStream.Mesh_t Mesh = new(br, MDLStream.Header.MeshInfo[i].MeshHeaderOff + VDLOff, MDLStream.Header.RFHeader.MagicRFTypeVersion);
                            TRSMat = GetTRSMatFromMeshIdx(MDLStream.Header.CountsOffs.ObjectsCount, (ushort)i);
                            if (DuplMesh)
                            {
                                Matrix4 TRSMatBase = TRSMat;
                                MDLStream.MeshData_t MeshDataBase = Mesh.MeshData.DeepCopy();
                                ushort DuplMeshesCount = x30CodesDict[x30CurrentChunk];
                                ushort DuplMeshesCounter = 0;
                                for (int j = 0; j < MDLStream.Header.CountsOffs.ObjectsCount; j++) //Keep it j = 0.
                                {
                                    if (HierarchyList[j].Code == 0x30 && HierarchyList[j].Index == x30CurrentChunk)
                                    {
                                        DuplMeshesCounter++;
                                        TRSMat = TRSMatBase;
                                        Mesh.MeshData = MeshDataBase.DeepCopy();
                                        TRSMat *= GetTRSMatFromID((ushort)j);
                                        WriteText(fsNew, $"o {$"{Path.GetFileNameWithoutExtension(arg)}{ArgExt}_{MeshProgressCount}_"}{MeshName}\n");
                                        WriteText(fsNew, $"#INFO @: 0x{MDLStream.Header.CountsOffs.MeshInfoOffs[i]:X8}\n#Mesh @: 0x{Mesh.AbsAddr:X8}\n");
                                        TransformBuffs(Mesh.MeshData, TRSMat);
                                        Mesh.MeshData.Vtx.ForEach(item => WriteText(fsNew, $"v {item.X} {item.Y} {item.Z}\n"));
                                        Mesh.MeshData.Norm.ForEach(item => WriteText(fsNew, $"vn {item.X} {item.Y} {item.Z}\n"));
                                        Mesh.MeshData.Uv.ForEach(item => WriteText(fsNew, $"vt {item.X} {item.Y}\n"));
                                        ParseIdx(br, fsNew, MDLStream.Header.MeshInfo[i], MDLStream.Header.MatMD3, MDLStream.Header.MatMD2, Mesh.FragmentSize, Mesh.VtxIdx, Mesh.NormIdx, Mesh.LightIdx, Mesh.UvIdx, Mesh.Uv2Idx);
                                        if (DumpSingleOBJFile)
                                        {
                                            PrevVtxCount += Mesh.MeshHeader.VtxCount;
                                            PrevNormCount += Mesh.MeshHeader.NormCount;
                                            PrevUvCount += Mesh.MeshHeader.UvCount;
                                        }
                                    }
                                    else if (DuplMeshesCounter == DuplMeshesCount)
                                        break;
                                }
                                DuplicatedMeshes += DuplMeshesCount;
                                continue;
                            }
                            WriteText(fsNew, $"o {$"{Path.GetFileNameWithoutExtension(arg)}{ArgExt}_{MeshProgressCount}_"}{MeshName}\n");
                            WriteText(fsNew, $"#INFO @: 0x{MDLStream.Header.CountsOffs.MeshInfoOffs[i]:X8}\n#Mesh @: 0x{Mesh.AbsAddr:X8}\n");
                            TransformBuffs(Mesh.MeshData, TRSMat);
                            Mesh.MeshData.Vtx.ForEach(item => WriteText(fsNew, $"v {item.X} {item.Y} {item.Z}\n"));
                            Mesh.MeshData.Norm.ForEach(item => WriteText(fsNew, $"vn {item.X} {item.Y} {item.Z}\n"));
                            Mesh.MeshData.Uv.ForEach(item => WriteText(fsNew, $"vt {item.X} {item.Y}\n"));
                            ParseIdx(br, fsNew, MDLStream.Header.MeshInfo[i], MDLStream.Header.MatMD3, MDLStream.Header.MatMD2, Mesh.FragmentSize, Mesh.VtxIdx, Mesh.NormIdx, Mesh.LightIdx, Mesh.UvIdx, Mesh.Uv2Idx);
                            if (DumpSingleOBJFile)
                            {
                                PrevVtxCount += Mesh.MeshHeader.VtxCount;
                                PrevNormCount += Mesh.MeshHeader.NormCount;
                                PrevUvCount += Mesh.MeshHeader.UvCount;
                            }
                        }
                    }
                    else if (ArgExt == ".tdl")
                    {
                        //Handle .tdl
                        //Ideal is directly convert tdl to tga. For now, it's tdl -> (MemoryStream)bmp -> tga
                        TDLCounter += 1;
                        narg += 1;
                        int Blocks = 0;
                        NewPath = arg + ".tga";
                        Console.Title = $"Endless Ocean Files Converter - Arg: {narg}/{args.Length} - .tdl ({TDLCounter}/{TDLCount})";
                        br._endianness = EndianBinaryReader.Endianness.Big;
                        TDLStream TDLFile = new(br);
                        using MemoryStream msBMP = new();
                        using BinaryWriter bwBMP = new(msBMP);
                        switch (TDLFile.Header.FileHeader.Format) //Possible formats: 10,5,8,1
                        {
                            case 10:
                                Blocks = TDLFile.Header.FileHeader.TotalWidth * TDLFile.Header.FileHeader.TotalHeight / 16;
                                TDLStream.TransformCMPRBlock(TDLFile.Data.CMPRBlock, Blocks, OutputFormat);
                                msBMP.Seek(0, SeekOrigin.Begin);
                                TDLStream.WriteBMPHeader(bwBMP, TDLFile, OutputFormat);
                                TDLStream.WriteBMPCMPRBlock(bwBMP, TDLFile);
                                break;
                            case 5:
                                Blocks = TDLFile.Header.FileHeader.TotalWidth * TDLFile.Header.FileHeader.TotalHeight / 32;
                                TDLStream.TransformRGB5A3Block(TDLFile.Data.RGB5A3Block, Blocks);
                                TDLStream.TransformRGB5A3Palette(TDLFile.Data.Palette, TDLFile.Header.FileHeader.PaletteSize / 2);
                                msBMP.Seek(0, SeekOrigin.Begin);
                                TDLStream.WriteBMPHeader(bwBMP, TDLFile, OutputFormat);
                                TDLStream.WriteBMPRGB5A3Block(bwBMP, TDLFile);
                                break;
                            case 8:
                            case 1:
                                PrintWarning($"Unsupported .tdl format for {arg}. File not parsed.\n");
                                continue;
                        }

                        System.Drawing.Bitmap bmp1 = new(msBMP);
                        TGA TGAFile = (TGA)bmp1;
                        TGAFile.Save(NewPath);
                        msBMP.Dispose();
                        msBMP.Close();
                        fs.Dispose();
                        fs.Close();
                        if (ArgsToDelete.Any(arg.Contains))
                        {
                            int e = ArgsToDelete.IndexOf(arg);
                            if (e != -1)
                                File.Delete(ArgsToDelete[e]);
                        }
                    }
                    else if (ArgExt == ".hit")
                    {
                        //Handle .hit
                        HITCounter += 1;
                        narg += 1;
                        Console.Title = $"Endless Ocean Files Converter - Arg: {narg}/{args.Length} - .hit ({HITCounter}/{HITCount})";
                        NewPath = $"{arg}.obj";
                        File.Delete(NewPath);
                        fsNew = File.Open(NewPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                        br._endianness = EndianBinaryReader.Endianness.Big;
                        HITStream HITFile = new(br);
                        if (RTLParsed) //Only if the .rtl was passed in the arguments
                        {
                            IsRod = false;
                            if (rxs01f.IsMatch(Path.GetFileNameWithoutExtension(arg))) //"^s01f(\d{2})(\d{2}).*" - s01fNNMM
                            {
                                GetTransColRow(arg);
                            }
                        }
                        int m = 0;
                        uint index = 1;
                        for (int i = 0; i < HITFile.Header.ColCount; i++)
                        {
                            int p = 0;

                            HITStream.TransformVertices(HITFile.ColData.HITData[i].Vertices, HITFile.FBHData.FBHList[m].Translation, HITFile.FBHData.FBHList[m].Rotation, IsRod, TransCol, TransRow);

                            if (i == HITFile.FBHData.FBHList[m].ID + HITFile.FBHData.FBHList[m].NextColsCount - 1)
                            {
                                m++;
                            }

                            WriteText(fsNew, $"o {Path.GetFileNameWithoutExtension(arg)}{ArgExt}_{i}_{HITFile.ColData.HITData[i].ColName}\n");

                            for (int k = 0; k < HITFile.ColData.HITData[i].ColInfo.PolyCount; k++)
                            {
                                uint VTXCount = HITFile.ColData.HITData[i].PolyInfo[k].VTXCount;

                                //WriteText(fsNew, $"#{VTXCount}\n");

                                for (int j = 0; j < VTXCount; j++)
                                {
                                    WriteText(fsNew, $"v {HITFile.ColData.HITData[i].Vertices[j + p].X} {HITFile.ColData.HITData[i].Vertices[j + p].Y} {HITFile.ColData.HITData[i].Vertices[j + p].Z}\n");
                                }

                                p += (int)VTXCount;
                                VTXCount -= 2;

                                for (int j = 0; j < VTXCount; j++)
                                {
                                    WriteText(fsNew, $"f {2 + j + index} {1 + j + index} {index /*n*/}\n"); //lines (l) declarations render in a weird way. Let's keep it to faces (f) for now.
                                }

                                WriteText(fsNew, "\n");

                                //n += VTXCount + 2;
                                index += VTXCount + 2;
                            }
                        }
                    }
                    else if (ArgExt == ".pak" || ArgExt == ".txs")
                    {
                        //Handle generic RF Archive
                        RFCounter += 1;
                        narg += 1;
                        Console.Title = $"Endless Ocean Files Converter - Arg: {narg}/{args.Length} - .pak/.txs ({RFCounter}/{RFCount})";
                        RFHeader_t RFHeader = new(br);
                        NewPath = $"{Path.GetDirectoryName(arg)}\\{Path.GetFileNameWithoutExtension(arg)}";
                        if (!Directory.Exists(NewPath))
                            Directory.CreateDirectory(NewPath);

                        for (int i = 0; i < RFHeader.FileCount; i++)
                        {
                            if (RFHeader.Files[i].IsInFile == 1)
                                ExtractFileFromRF(br, RFHeader.Files[i], NewPath);
                        }
                    }

                    if ((ArgExt == ".mdl" && DumpSingleOBJFile) || ArgExt == ".hit")
                    {
                        fsNew.Flush();
                        fsNew.Close();
                    }
                    Console.WriteLine($"Converted \"{arg}\" to \"{NewPath}\"\n");
                }
                Console.Title = $"Endless Ocean Files Converter - Arg: {narg}/{args.Length}";
            }
            MeshTotalCount += DuplicatedMeshes;
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = $"Elapsed time: {ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            Console.WriteLine(elapsedTime);
            if (DuplicatedMeshes > 0)
                Console.WriteLine($"Duplicated Meshes: {DuplicatedMeshes} [0x{DuplicatedMeshes:X8}]");
            
            if (MDLCounter > 0)
                Console.WriteLine($"Total .mdl: {MDLCounter} [0x{MDLCounter:X8}]");

            if (TDLCounter > 0)
                Console.WriteLine($"Total .tdl: {TDLCounter} [0x{TDLCounter:X8}]");

            if (HITCounter > 0)
                Console.WriteLine($"Total .hit: {HITCounter} [0x{HITCounter:X8}]");

            if (RFCounter > 0)
                Console.WriteLine($"Total .pak/.txs: {RFCounter} [0x{RFCounter:X8}]");

            if (MeshProgressCount > 0)
                Console.WriteLine($"Total Meshes: {MeshTotalCount} [0x{MeshTotalCount:X8}]");

            //if (StripWithEqualFaceCount > 0)
                //Console.WriteLine($"Face declarations with at least 2 equal face indices: {StripWithEqualFaceCount} [0x{StripWithEqualFaceCount:X4}] (Dumped but commented with #)");

            Console.WriteLine("Please, press any key to exit");
            Console.ReadKey();
        }

        public static void PrintCenter(string text)
        {
            Console.WriteLine(string.Format("{0," + ((Console.WindowWidth / 2) + (text.Length / 2)) + "}", text));
        }

        public static void PrintInfo()
        {
            PrintCenter("Endless Ocean Files Converter\n");
            PrintCenter("Authors: NiV, MDB\n");
            PrintCenter("Special thanks to Hiroshi\n");
            PrintCenter($"Version {Version}\n"); ;
            PrintCenter("If you have any issues, join this discord server and contact NiV-L-A:\n");
            PrintCenter("https://discord.gg/4hmcsmPMDG\n");
        }

        public static void PrintError(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ERROR:\n" + text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static void PrintWarning(string text)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("WARNING:\n" + text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static string ReadStrAdv(EndianBinaryReader br, uint MaxLength)
        {
            //Returns a string from the current position until either a 0x00 byte is encountered or the length of the string is >= than MaxLength.
            //In either case, advance the current position by MaxLength.
            string str = "";
            char ch;
            while ((ch = (char)br.PeekChar()) != 0 && (str.Length < MaxLength))
            {
                ch = br.ReadChar();
                str += ch;
            }
            br.BaseStream.Seek(MaxLength - str.Length, SeekOrigin.Current);
            return str;
        }

        public static byte[] ReverseIntToByteArray(int Value)
        {
            byte[] ArrayValue = BitConverter.GetBytes(Value);
            Array.Reverse(ArrayValue);
            return ArrayValue;
        }

        public static void WriteText(FileStream fs, string v)
        {
            byte[] textbytes = Encoding.UTF8.GetBytes(v);
            fs.Write(textbytes, 0, textbytes.Length);
            //fsLog.Flush();
        }

        public static List<string> ParseInput(string[] args)
        {
            //Parses the arguments passed to the exe. Also parses .rtl. "*.rtl,*.mdl,*.tdl,*.hit,*.pak,*.txs"
            List<string> ListAllFiles = new();

            if (args[0].StartsWith("-")) //if first argument is -extension, then only count that filetype
            {
                FileTypes = "*." + args[0].Substring(1, 3);
                List<string> q = args.ToList();
                q.RemoveAt(0);
                args = q.ToArray();
            }

            foreach (string arg in args)
            {
                if (Path.GetExtension(arg) == "") //folder
                {
                    try
                    {
                        string[] AllFilesExt = Directory.GetFiles(arg, "*.*", SearchOption.AllDirectories).Where(s => FileTypes.Contains(Path.GetExtension(s))).ToArray();
                        ListAllFiles.AddRange(AllFilesExt);
                    }
                    catch (DirectoryNotFoundException dnfe)
                    {
                        PrintError(dnfe.Message);
                        Console.WriteLine($"{SupportedFilesMsg}\n\nPress any key to close this window.");
                        Console.Read();
                        Environment.Exit(0); //Brutal. Is there a better way?
                    }
                }
                else //file(s)
                {
                    if (FileTypes.Contains(Path.GetExtension(arg)))
                    {
                        ListAllFiles.Add(arg);
                    }
                }
            }

            ListAllFiles = ListAllFiles.Distinct().ToList(); //remove dups
            string[] RTLargs = ListAllFiles.Where(s => s.EndsWith(".rtl")).ToArray();

            if (RTLargs != null)
            {
                if (RTLargs.Length > 0) //if .rtl found
                {
                    ParseRTL(RTLargs);
                    ListAllFiles.Remove(RTLargs[0]);
                }
            }

            MDLCount = ListAllFiles.Where(s => s.EndsWith(".mdl")).ToArray().Length;
            TDLCount = ListAllFiles.Where(s => s.EndsWith(".tdl")).ToArray().Length;
            HITCount = ListAllFiles.Where(s => s.EndsWith(".hit")).ToArray().Length;
            RFCount = ListAllFiles.Where(s => s.EndsWith(".pak")).ToArray().Length + ListAllFiles.Where(s => s.EndsWith(".txs")).ToArray().Length;
            return ListAllFiles;
        }

        public static void ExtractFileFromRF(EndianBinaryReader br, RFFile_t FileX, string arg)
        {
            if (FileX.IsInFile == 1)
            {
                long temp = br.BaseStream.Position;
                string NewP = $"{arg}\\{FileX.FileName}";
                if (FileX.FileType == 1 && !NewP.EndsWith(".tdl"))
                {
                    NewP += ".tdl";
                }
                //NewPath = $"{Path.GetDirectoryName(arg)}\\{FileX.FileName}";
                br.BaseStream.Seek(FileX.FileOff, SeekOrigin.Begin);
                byte[] arrayFile = br.ReadBytes((int)FileX.FileSize);
                File.WriteAllBytes(NewP, arrayFile);
                br.BaseStream.Seek(temp, SeekOrigin.Begin);
            }
        }

        public static void CreateAndPopulateMTLFile(RFHeader_t Header, string arg)
        {
            string MatPath = $"{Path.GetDirectoryName(arg)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(arg)}.mtl";
            File.Delete(MatPath); //delete file if it exists already
            using FileStream fs = File.OpenWrite(MatPath);
            {
                ushort idx = 0;
                for (int i = 0; i < Header.FileCount; i++)
                {
                    if (Header.Files[i].FileType == 1)
                    {
                        string txt = $"newmtl material{idx}\nmap_Kd ";
                        if (!Header.Files[i].FileName.EndsWith(".tdl"))
                            txt += $"{Header.Files[i].FileName + ".tdl.tga"}\n\n";
                        else
                            txt += $"{Header.Files[i].FileName + ".tga"}\n\n";
                        
                        WriteText(fs, txt);
                        idx++;
                    }
                }
                fs.Flush();
            }
        }

        public static void ParseRTL(string[] RTLargs)
        {
            using FileStream fs = new(RTLargs[0], FileMode.Open);
            using EndianBinaryReader br = new(fs, EndianBinaryReader.Endianness.Big);

            if (Path.GetFileNameWithoutExtension(RTLargs[0]) != "s01f") //EO2 bNNstage.rtl files
            {
                br.BaseStream.Seek(0x08, SeekOrigin.Begin);
                RTLColCount = br.ReadUInt16();
                RTLRowCount = br.ReadUInt16();
                RTLChunkSize = br.ReadSingle();
                RTLXChunkStart = br.ReadSingle();
                RTLZChunkStart = br.ReadSingle();
                br.BaseStream.Seek(0x24, SeekOrigin.Begin);

                uint Just = 0;
                Rows = new List<List<ushort>>();
                List<ushort> QW = new();

                for (int i = 0; i < RTLRowCount; i++)
                {
                    List<ushort> Row = new();
                    for (int j = 0; j < RTLColCount; j++)
                    {
                        ushort RTLCurrIndex = br.ReadUInt16();
                        Row.Add(RTLCurrIndex);
                        QW.Add(RTLCurrIndex);
                        Just++;
                    }
                    Rows.Add(Row);
                }
                RTLParsed = true;
            }
            else if (Path.GetFileNameWithoutExtension(RTLargs[0]) == "s01f")
            {
                ushort[] ArrayLen = new ushort[6];
                br.BaseStream.Seek(0x10, SeekOrigin.Begin);

                for (int j = 0; j < 6; j++)
                {
                    ArrayLen[j] = br.ReadUInt16();
                }
                Quadrants = new List<List<ushort>>();
                br.BaseStream.Seek(0x24, SeekOrigin.Begin);
                int i = 0;
                while (i < 6)
                {
                    List<ushort> Quad = new();
                    ushort Len = ArrayLen[i];
                    for (int k = 0; k < Len; k++)
                    {
                        ushort RTLCurrIndex = br.ReadUInt16();
                        Quad.Add(RTLCurrIndex);
                    }
                    Quadrants.Add(Quad);
                    i++;
                }
                RTLParsed = true;
            }
        }

        public static void GetTransColRow(string arg)
        {
            IsRod = true;
            ushort Quadrant = (ushort)(Convert.ToUInt16(Path.GetFileNameWithoutExtension(arg).Substring(4, 2), 10) - 1);
            ushort ID = Convert.ToUInt16(Path.GetFileNameWithoutExtension(arg).Substring(6, 2), 10);
            ushort Modifier = Quadrants[Quadrant][ID];
            TransCol = 320f * (0.5f + (Modifier & 0xFF)) - 4000f;
            TransRow = 320f * (0.5f + ((Modifier >> 0x8) & 0xFF)) - 3200f;
        }

        public static string[] ExtractFilesAndUpdateArgs(EndianBinaryReader br, RFHeader_t RFHeader, string[] args, string arg)
        {
            for (int i = 0; i < RFHeader.FileCount; i++)
            {
                if (RFHeader.Files[i].IsInFile == 1)
                {
                    switch (RFHeader.Files[i].FileType)
                    {
                        case 0: //vdl
                            VDLOff = RFHeader.Files[i].FileOff;
                            break;
                        case 1: //tdl
                            string FinalPath = $"{Path.GetDirectoryName(arg)}\\{RFHeader.Files[i].FileName}";
                            if (!FinalPath.EndsWith(".tdl"))
                            {
                                FinalPath += ".tdl";
                            }
                            if (!args.Contains(FinalPath))
                            {
                                ExtractFileFromRF(br, RFHeader.Files[i], $"{Path.GetDirectoryName(arg)}");
                                List<string> x = args.ToList();
                                x.Add(FinalPath);
                                args = x.ToArray();
                                ArgsToDelete.Add(FinalPath);
                                TDLCount++;
                            }
                            break;
                        case 2: //txs
                            br._endianness = EndianBinaryReader.Endianness.Little;
                            br.BaseStream.Seek(RFHeader.Files[i].FileOff, SeekOrigin.Begin);
                            RFHeader_t RFHeader2 = new(br);
                            for (int j = 0; j < RFHeader2.FileCount; j++)
                            {
                                if (RFHeader2.Files[j].IsInFile == 1)
                                {
                                    string FPath = $"{Path.GetDirectoryName(arg)}\\{RFHeader2.Files[j].FileName}";
                                    RFHeader2.Files[j].FileOff += RFHeader2.Files[i].FileOff;
                                    ExtractFileFromRF(br, RFHeader2.Files[j], $"{Path.GetDirectoryName(arg)}");
                                    List<string> x = args.ToList();
                                    x.Add(FPath);
                                    args = x.ToArray();
                                    ArgsToDelete.Add(FPath);
                                    TDLCount++;
                                }
                            }
                            break;
                    }
                }
            }
            return args;
        }

        public static ushort GetIdx(EndianBinaryReader br, MDLStream.IndexStatus Idx)
        {
            if (Idx == MDLStream.IndexStatus.BYTE)
                return br.ReadByte();
            else if (Idx == MDLStream.IndexStatus.SHORT)
                return br.ReadUInt16();
            else
                return 0xFFFF;
        }

        public static void ParseIdx(EndianBinaryReader br, FileStream fsNew, MDLStream.MeshInfo_t MeshInfo, List<MDLStream.MatMD3_t> MatMD3, List<ushort> MatMD2, byte FragmentSize, MDLStream.IndexStatus VtxIdx, MDLStream.IndexStatus NormIdx, MDLStream.IndexStatus LightIdx, MDLStream.IndexStatus UvIdx, MDLStream.IndexStatus Uv2Idx)
        {
            //The Indices Section, usually, is divided in multiple draw calls.
            //Though there could even be 1 single draw call (very rare; but it will always happens if the optimization is triangles).
            //Since normals are flipped, we have to reverse the output of the faces,
            //so, instead of printing "f {pos}/{uv}/{norm} {pos2}/{uv2}/{norm2} {pos3}/{uv3}/{norm3}\n"
            //We print                "f {pos3}/{uv3}/{norm3} {pos2}/{uv2}/{norm2} {pos}/{uv}/{norm}\n"

            ushort MeshNSection = MeshInfo.IdxSectionsCount;
            ushort flag;
            ushort cnt;
            for (int i = 0; i < MeshNSection; i++)
            {
                br.BaseStream.Seek(VDLOff + MeshInfo.MeshHeaderOff + MeshInfo.InfoInd[i].Off, SeekOrigin.Begin);
                if (MatMD2 == null) //MD3
                    WriteText(fsNew, $"usemtl material{MatMD3[MeshInfo.InfoInd[i].MatIdx].TextureIndex}\n");
                else //MD2
                    WriteText(fsNew, $"usemtl material{MatMD2[MeshInfo.InfoInd[i].MatIdx]}\n");
                
                if (MeshInfo.InfoInd[i].Optim == 4) //tristrip, more common
                {
                    do
                    {
                        long StartIdxPos = br.BaseStream.Position;
                        flag = br.ReadUInt16();
                        cnt = br.ReadUInt16();
                        if (cnt < 3)
                        {
                            br.BaseStream.Position = br.BaseStream.Position + (cnt * FragmentSize);
                            continue;
                        }
                        ushort NStrips = (ushort)(cnt - 2);
                        for (int j = 0; j < NStrips; j++)
                        {
                            ushort pos1 = GetIdx(br, VtxIdx);
                            ushort norm1 = GetIdx(br, NormIdx);
                            ushort light1 = GetIdx(br, LightIdx);
                            ushort uv11 = GetIdx(br, UvIdx);
                            ushort uv21 = GetIdx(br, Uv2Idx);
                            ushort pos2 = GetIdx(br, VtxIdx);
                            ushort norm2 = GetIdx(br, NormIdx);
                            ushort light2 = GetIdx(br, LightIdx);
                            ushort uv12 = GetIdx(br, UvIdx);
                            ushort uv22 = GetIdx(br, Uv2Idx);
                            long temp = br.BaseStream.Position;
                            ushort pos3 = GetIdx(br, VtxIdx);
                            ushort norm3 = GetIdx(br, NormIdx);
                            ushort light3 = GetIdx(br, LightIdx);
                            ushort uv13 = GetIdx(br, UvIdx);
                            ushort uv23 = GetIdx(br, Uv2Idx);

                            WriteText(fsNew, $"f {pos3 + 1 + PrevVtxCount}/{uv13 + 1 + PrevUvCount}/{norm3 + 1 + PrevNormCount} {pos2 + 1 + PrevVtxCount}/{uv12 + 1 + PrevUvCount}/{norm2 + 1 + PrevNormCount} {pos1 + 1 + PrevVtxCount}/{uv11 + 1 + PrevUvCount}/{norm1 + 1 + PrevNormCount}\n");
                            j += 1;

                            if (j < NStrips)
                            {
                                ushort pos4 = GetIdx(br, VtxIdx);
                                ushort norm4 = GetIdx(br, NormIdx);
                                ushort light4 = GetIdx(br, LightIdx);
                                ushort uv14 = GetIdx(br, UvIdx);
                                ushort uv24 = GetIdx(br, Uv2Idx);

                                WriteText(fsNew, $"f {pos2 + 1 + PrevVtxCount}/{uv12 + 1 + PrevUvCount}/{norm2 + 1 + PrevNormCount} {pos3 + 1 + PrevVtxCount}/{uv13 + 1 + PrevUvCount}/{norm3 + 1 + PrevNormCount} {pos4 + 1 + PrevVtxCount}/{uv14 + 1 + PrevUvCount}/{norm4 + 1 + PrevNormCount}\n");
                            }
                            br.BaseStream.Seek(temp, SeekOrigin.Begin);
                        }
                        br.BaseStream.Seek(StartIdxPos + 4 + FragmentSize * cnt, SeekOrigin.Begin);
                    } while ((flag & 0x1) != 0);
                }
                else //optim = 3, triangles, only 1 draw call
                {
                    flag = br.ReadUInt16();
                    cnt = br.ReadUInt16();
                    int NStrips = cnt / 3;
                    for (int j = 0; j < NStrips; j++)
                    {
                        ushort pos1 = GetIdx(br, VtxIdx);
                        ushort norm1 = GetIdx(br, NormIdx);
                        ushort light1 = GetIdx(br, LightIdx);
                        ushort uv11 = GetIdx(br, UvIdx);
                        ushort uv21 = GetIdx(br, Uv2Idx);
                        ushort pos2 = GetIdx(br, VtxIdx);
                        ushort norm2 = GetIdx(br, NormIdx);
                        ushort light2 = GetIdx(br, LightIdx);
                        ushort uv12 = GetIdx(br, UvIdx);
                        ushort uv22 = GetIdx(br, Uv2Idx);
                        ushort pos3 = GetIdx(br, VtxIdx);
                        ushort norm3 = GetIdx(br, NormIdx);
                        ushort light3 = GetIdx(br, LightIdx);
                        ushort uv13 = GetIdx(br, UvIdx);
                        ushort uv23 = GetIdx(br, Uv2Idx);

                        WriteText(fsNew, $"f {pos3 + 1 + PrevVtxCount}/{uv13 + 1 + PrevUvCount}/{norm3 + 1 + PrevNormCount} {pos2 + 1 + PrevVtxCount}/{uv12 + 1 + PrevUvCount}/{norm2 + 1 + PrevNormCount} {pos1 + 1 + PrevVtxCount}/{uv11 + 1 + PrevUvCount}/{norm1 + 1 + PrevNormCount}\n");
                    }
                }
            }
        }

        public static void GetHierarchyList(EndianBinaryReader br, ushort ObjectsCount)
        {
            for (int i = 0; i < ObjectsCount; i++)
            {
                MDLStream.CHierarchyObject HierarchyObject = new(br, VDLOff);
                HierarchyList.Add(HierarchyObject);
                if (HierarchyList[i].Code == 0x30)
                {
                    if (x30CodesDict.ContainsKey(HierarchyList[i].Index))
                        x30CodesDict[HierarchyList[i].Index]++;
                    else
                        x30CodesDict.Add(HierarchyList[i].Index, 1);
                }
            }

            for (int i = 0; i < ObjectsCount; i++)
            {
                if (HierarchyList[i].Level > 0)
                {
                    byte CurrLevel = HierarchyList[i].Level;
                    for (int j = i - 1; j > 0; j--)
                    {
                        if (HierarchyList[j].Level == CurrLevel - 1)
                        {
                            HierarchyList[i].PrevObjID = HierarchyList[j].ID;
                            break;
                        }
                    }
                }
                else
                    HierarchyList[i].PrevObjID = -1;
            }
        }

        public static Matrix4 GetTRSMatFromID(ushort Index)
        {
            Matrix4 TRSMat = Matrix4.CreateFromQuaternion(QuatIdentity);

            TRSMat = MulTRSMatObjIdx(TRSMat, Index);
            //TRSMat *= Matrix4.CreateScale(HierarchyList[Index].Scale);
            //TRSMat *= Matrix4.CreateFromQuaternion(HierarchyList[Index].Rotation);
            //TRSMat *= Matrix4.CreateTranslation(HierarchyList[Index].Translation);

            if (HierarchyList[Index].Level != 0)
            {
                int q = HierarchyList[Index].PrevObjID;
                for (int i = 0; i < HierarchyList[Index].Level; i++)
                {
                    TRSMat = MulTRSMatObjIdx(TRSMat, q);
                    //TRSMat *= Matrix4.CreateScale(HierarchyList[q].Scale);
                    //TRSMat *= Matrix4.CreateFromQuaternion(HierarchyList[q].Rotation);
                    //TRSMat *= Matrix4.CreateTranslation(HierarchyList[q].Translation);
                    q = HierarchyList[q].PrevObjID;
                }
            }
            return TRSMat;
        }

        public static Matrix4 GetTRSMatFromMeshIdx(ushort ObjectsCount, ushort MeshIdx)
        {
            Matrix4 TRSMat = Matrix4.CreateFromQuaternion(QuatIdentity);
            for (int i = NextHierarchyObject; i < ObjectsCount; i++)
            {
                if (DuplMesh && HierarchyList[i].Level <= x30MeshLevel) //if outside of the dupl collection
                {
                    DuplMesh = false;
                    x30IncreaserCounter++;
                }
                if (HierarchyList[i].Index == MeshIdx && HierarchyList[i].Code == 0x20) //if is mesh and matches index
                {
                    TRSMat = MulTRSMatObjIdx(TRSMat, i);
                    //TRSMat *= Matrix4.CreateScale(HierarchyList[i].Scale);
                    //TRSMat *= Matrix4.CreateFromQuaternion(HierarchyList[i].Rotation);
                    //TRSMat *= Matrix4.CreateTranslation(HierarchyList[i].Translation);
                    MeshName = HierarchyList[i].MeshName;
                    NextHierarchyObject = (ushort)(i + 1);

                    if (DuplMesh) //if needs dupl, break when encountering the start of the collection
                    {
                        int q = HierarchyList[i].PrevObjID;
                        for (int j = 0; j < HierarchyList[i].Level; j++)
                        {
                            if (q == x30CodesDict.ElementAt(x30IncreaserCounter).Key)
                                break;
                            TRSMat = MulTRSMatObjIdx(TRSMat, q);
                            //TRSMat *= Matrix4.CreateScale(HierarchyList[q].Scale);
                            //TRSMat *= Matrix4.CreateFromQuaternion(HierarchyList[q].Rotation);
                            //TRSMat *= Matrix4.CreateTranslation(HierarchyList[q].Translation);
                            q = HierarchyList[q].PrevObjID;
                        }
                        return TRSMat;
                    }

                    if (HierarchyList[i].Level > 0) //if object has a previous level
                    {
                        int q = HierarchyList[i].PrevObjID;
                        for (int j = 0; j < HierarchyList[i].Level; j++)
                        {
                            TRSMat = MulTRSMatObjIdx(TRSMat, q);
                            //TRSMat *= Matrix4.CreateScale(HierarchyList[q].Scale);
                            //TRSMat *= Matrix4.CreateFromQuaternion(HierarchyList[q].Rotation);
                            //TRSMat *= Matrix4.CreateTranslation(HierarchyList[q].Translation);
                            q = HierarchyList[q].PrevObjID;
                        }
                        break;
                    }
                }
                else if (x30CodesDict.ContainsKey((ushort)HierarchyList[i].ID)) //Entering dupl collection
                {
                    if (HierarchyList[i].ID == x30CodesDict.ElementAt(x30IncreaserCounter).Key) //Confirmation, this collection needs dupl
                    {
                        x30CurrentChunk = (ushort)HierarchyList[i].ID;
                        NextHierarchyObject = (ushort)(i + 1);
                        x30MeshLevel = HierarchyList[i].Level;
                        DuplMesh = true;
                        TRSMat = GetTRSMatFromMeshIdx(ObjectsCount, MeshIdx); //get Mesh's TRSMat
                        return TRSMat;
                    }
                }
            }
            return TRSMat;
        }

        public static Matrix4 MulTRSMatObjIdx(Matrix4 TRSMat, int i)
        {
            TRSMat *= Matrix4.CreateScale(HierarchyList[i].Scale);
            TRSMat *= Matrix4.CreateFromQuaternion(HierarchyList[i].Rotation);
            TRSMat *= Matrix4.CreateTranslation(HierarchyList[i].Translation);
            return TRSMat;
        }

        public static void TransformBuffs(MDLStream.MeshData_t MeshData, Matrix4 TRSMat)
        {
            //Vertex correction
            MeshData.Vtx = TransformVtxBuff(MeshData.Vtx, TRSMat);
            TRSMat = TRSMat.ClearTranslation();
            TRSMat = TRSMat.ClearScale();
            MeshData.Norm = TransformBaseBuff(MeshData.Norm, TRSMat);
            MeshData.Uv = TransformUvBuff(MeshData.Uv);
        }

        public static List<Vector3> TransformVtxBuff(List<Vector3> Buffer, Matrix4 TRSMat)
        {
            Buffer = TransformBaseBuff(Buffer, TRSMat);
            if (IsRod)
            {
                Buffer = Buffer.Select(item => Vector3.Add(item, new Vector3(RTLXChunkStart, 0, RTLZChunkStart))).ToList();
                Buffer = Buffer.Select(item => Vector3.Add(item, new Vector3(TransCol, 0, TransRow))).ToList();
            }
            return Buffer;
        }

        public static List<Vector3> TransformBaseBuff(List<Vector3> Buffer, Matrix4 TRSMat)
        {
            return Buffer.Select(item => Vector3.TransformPosition(item, TRSMat)).ToList();
        }

        public static List<Vector2> TransformUvBuff(List<Vector2> Buffer)
        {
            return Buffer.Select(item => new Vector2(item.X, 1.0f + (item.Y * -1.0f))).ToList();
        }
    }
}