using System.Collections.Generic;
using System.IO;
using EndlessOceanMDLToOBJExporter;
using OpenTK.Mathematics;
using static EndlessOceanFilesConverter.Utils;

namespace EndlessOceanFilesConverter
{
    class MDLStream
    {
        public Header_t Header;
        public MDLStream(EndianBinaryReader br)
        {
            Header = new(br);
        }

        public enum IndexStatus
        {
            NONE = 0,
            BYTE = 2,
            SHORT = 3
        }

        public class Mesh_t
        {
            public MeshHeader_t MeshHeader;
            public MeshData_t MeshData;
            public IndexStatus VtxIdx = IndexStatus.NONE;
            public IndexStatus NormIdx = IndexStatus.NONE;
            public IndexStatus LightIdx = IndexStatus.NONE;
            public IndexStatus UvIdx = IndexStatus.NONE;
            public IndexStatus Uv2Idx = IndexStatus.NONE;
            public byte FragmentSize;
            public long AbsAddr;
            public Mesh_t(EndianBinaryReader br, long off, string MagicRFTypeVersion)
            {
                AbsAddr = off;
                br.BaseStream.Seek(off, SeekOrigin.Begin);
                MeshHeader = new(br);
                MeshData = new(br, MeshHeader, off);
                FragmentSize = UnderstandIndexCount((byte)(MeshHeader.IndSizeFlags >> 8), (byte)MeshHeader.IndSizeFlags, MeshHeader, MagicRFTypeVersion, ref VtxIdx, ref NormIdx, ref LightIdx, ref UvIdx, ref Uv2Idx);
            }
        }
        public static byte UnderstandIndexCount(byte GPU, byte GPU2, MeshHeader_t MeshHeader, string MagicRFTypeVersion, ref IndexStatus VtxIdx, ref IndexStatus NormIdx, ref IndexStatus LightIdx, ref IndexStatus UvIdx, ref IndexStatus Uv2Idx)
        {
            /*
            Indices are always composed of 2 bytes when the MagicRFTypeVersion is 2. When MagicRFTypeVersion is 3, the indices can be a mix of 1 byte and/or 2 bytes.
            To understand when this happens, the .vdl header has 2 specific bytes (here called GPU and GPU2). We can divide the 8 bits in groups of 2 to understand the indices size.

            For example, GPU2 is 0xEF:
            VALUE:      1 1 | 1 0 | 1 1 | 1 1
            POSITION:   0 1 | 2 3 | 4 5 | 6 7

            0th bit = is uv data present (in 0xEF, 0th bit is set to 1. Yes, uv data is present).
            1st bit = is uv index 2 bytes (in 0xEF, 1st bit is set to 1. Yes, uv index is 2 bytes).
            2nd bit = is light data present (in 0xEF, 2nd bit is set to 1. Yes, light data is present).
            3rd bit = is light index 2 bytes (in 0xEF, 3rd bit is set to 0. No, light index is 1 byte).
            4th bit = is normals data present (in 0xEF, 4th bit is set to 1. Yes, normals data is present).
            5th bit = is normals index 2 bytes (in 0xEF, 5th bit is set to 1. Yes, normals index is 2 bytes).
            6th bit = is vertices data present (in 0xEF, 6th bit is set to 1. Yes, vertices data is present).
            7th bit = is vertices index 2 bytes (in 0xEF, 7th bit is set to 1. Yes, vertices index is 2 bytes).

            GPU is 0x2:
            VALUE:      0 0 | 0 0 | 0 0 | 1 0
            POSITION:   0 1 | 2 3 | 4 5 | 6 7

            -Unknown flags-
            6th bit = is uv2 data present (in 0x2, 6th bit is set to 1. Yes, uv2 data is present).
            7th bit = is uv2 index 2 bytes (in 0x2, 7th bit is set to 0. No, uv2 index is 1 byte).
                 
            Sometimes the GPU and GPU2 bytes are null, so we will have to calculate the indices size based on the vertices count. If the count is >= 0xFF, then the index count is composed of 2 bytes, else, 1 byte.
            */

            byte FragmentSize = 0;
            if (GPU2 != 0) //has GPU byte. Better, faster
            {
                if ((GPU2 & 0b00000011) == 0b00000011)
                {
                    VtxIdx = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else if ((GPU2 & 0b00000010) == 0b00000010)
                {
                    VtxIdx = IndexStatus.BYTE;
                    FragmentSize += 1;
                }

                if ((GPU2 & 0b00001100) == 0b00001100)
                {
                    NormIdx = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else if ((GPU2 & 0b00001000) == 0b00001000)
                {
                    NormIdx = IndexStatus.BYTE;
                    FragmentSize += 1;
                }

                if ((GPU2 & 0b00110000) == 0b00110000)
                {
                    LightIdx = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else if ((GPU2 & 0b00100000) == 0b00100000)
                {
                    LightIdx = IndexStatus.BYTE;
                    FragmentSize += 1;
                }

                if ((GPU2 & 0b11000000) == 0b11000000)
                {
                    UvIdx = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else if ((GPU2 & 0b10000000) == 0b10000000)
                {
                    UvIdx = IndexStatus.BYTE;
                    FragmentSize += 1;
                }

                if (GPU != 0)
                {
                    if ((GPU & 0b00000011) == 0b00000011)
                    {
                        Uv2Idx = IndexStatus.SHORT;
                        FragmentSize += 2;
                    }
                    else if ((GPU & 0b00000010) == 0b00000010)
                    {
                        Uv2Idx = IndexStatus.BYTE;
                        FragmentSize += 1;
                    }
                }
            }
            else //No GPU byte, we can understand how the indices are formed by the vertices counts
            {
                if (MeshHeader.VtxCount >= 0xFF || MagicRFTypeVersion == "2" || MeshHeader.IndStride == 0)
                {
                    VtxIdx = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else
                {
                    VtxIdx = IndexStatus.BYTE;
                    FragmentSize += 1;
                }

                if (MeshHeader.NormCount >= 0xFF || MagicRFTypeVersion == "2" || MeshHeader.IndStride == 0)
                {
                    NormIdx = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else
                {
                    NormIdx = IndexStatus.BYTE;
                    FragmentSize += 1;
                }

                if (MeshHeader.LightCount >= 0xFF || MagicRFTypeVersion == "2" && MeshHeader.LightOff > 0 || MeshHeader.IndStride == 0 && MeshHeader.LightOff > 0)
                {
                    LightIdx = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else if (MagicRFTypeVersion == "3" && MeshHeader.LightOff > 0)
                {
                    LightIdx = IndexStatus.BYTE;
                    FragmentSize += 1;
                }
                else
                {
                    LightIdx = IndexStatus.NONE;
                }

                if (MeshHeader.UvCount >= 0xFF || MagicRFTypeVersion == "2" || MeshHeader.IndStride == 0)
                {
                    UvIdx = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else
                {
                    UvIdx = IndexStatus.BYTE;
                    FragmentSize += 1;
                }

                if (MeshHeader.Uv2Off > 0 && MeshHeader.Uv2Count >= 0xFF)
                {
                    Uv2Idx = IndexStatus.SHORT;
                    FragmentSize += 2;
                }
                else if (MagicRFTypeVersion == "3" && MeshHeader.Uv2Off > 0)
                {
                    Uv2Idx = IndexStatus.BYTE;
                    FragmentSize += 1;
                }
                else
                {
                    Uv2Idx = IndexStatus.NONE;
                }
            }
            return FragmentSize;
        }

        public class MeshHeader_t
        {
            public uint VtxOff;
            public uint NormOff;
            public uint LightOff;
            public uint UvOff;
            public uint Uv2Off;
            public ushort VtxCount;
            public ushort NormCount;
            public ushort LightCount;
            public ushort UvCount;
            public ushort Uv2Count;
            public ushort IndSizeFlags;
            public byte IndStride;
            public byte IsStrideExtended;
            public byte UvMapsCount;
            public byte VtxStride;
            public byte NormStride;
            public byte LightStride;
            public byte UvStride;

            public MeshHeader_t(EndianBinaryReader br)
            {
                VtxOff = br.ReadUInt32();
                NormOff = br.ReadUInt32();
                LightOff = br.ReadUInt32();
                UvOff = br.ReadUInt32();

                if (VtxOff == 0x40)
                {
                    Uv2Off = br.ReadUInt32();
                    br.Skip(12);
                }
                else
                {
                    br.Skip(4);
                }

                VtxCount = br.ReadUInt16();
                NormCount = br.ReadUInt16();
                LightCount = br.ReadUInt16();
                UvCount = br.ReadUInt16();

                if (VtxOff == 0x40)
                {
                    Uv2Count = br.ReadUInt16();
                    br.Skip(8);
                    IndSizeFlags = br.ReadUInt16();
                    br.Skip(5);
                    IndStride = br.ReadByte();
                    IsStrideExtended = br.ReadByte();
                    UvMapsCount = br.ReadByte();
                    VtxStride = br.ReadByte();
                    NormStride = br.ReadByte();
                    LightStride = br.ReadByte();
                    UvStride = br.ReadByte();
                }
                else
                {
                    br.Skip(3);
                    IsStrideExtended = br.ReadByte();
                }
            }
        }

        public class MeshData_t
        {
            public List<Vector3> Vtx = new();
            public List<Vector3> Norm = new();
            public List<Vector2> Uv = new();
            //public List<Vector2> Uv2;
            public MeshData_t(EndianBinaryReader br, MeshHeader_t MeshHeader, long off)
            {
                br.BaseStream.Seek(off + MeshHeader.VtxOff, SeekOrigin.Begin);

                if (MeshHeader.IsStrideExtended == 1)
                {
                    for (int i = 0; i < MeshHeader.VtxCount; i++)
                    {
                        Vtx.Add(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                        Norm.Add(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                    }
                }
                else
                {
                    for (int i = 0; i < MeshHeader.VtxCount; i++)
                    {
                        Vtx.Add(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                    }
                    br.BaseStream.Seek(off + MeshHeader.NormOff, SeekOrigin.Begin);
                    for (int i = 0; i < MeshHeader.NormCount; i++)
                    {
                        Norm.Add(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                    }
                }

                br.BaseStream.Seek(off + MeshHeader.UvOff, SeekOrigin.Begin);
                for (int i = 0; i < MeshHeader.UvCount; i++)
                {
                    Uv.Add(new Vector2(br.ReadSingle(), br.ReadSingle()));
                }
                /*
                br.BaseStream.Seek(off + MeshHeader.Uv2Off, SeekOrigin.Begin);
                for (int i = 0; i < MeshHeader.Uv2Count; i++)
                {
                    Uv2.Add(new Vector2(br.ReadSingle(), br.ReadSingle()));
                }
                */
            }

            public MeshData_t DeepCopy()
            {
                MeshData_t temp = (MeshData_t)MemberwiseClone();
                temp.Vtx = new(Vtx);
                temp.Norm = new(Norm);
                temp.Uv = new(Uv);
                return temp;
            }
        }

        public class Header_t
        {
            public RFHeader_t RFHeader;
            public CountsOffs_t CountsOffs;
            public List<ushort> MatMD2;
            public List<MatInfo_t> MatInfo;
            public List<MatMD3_t> MatMD3;
            public List<MeshInfo_t> MeshInfo;
            public Header_t(EndianBinaryReader br)
            {
                RFHeader = new(br);
                CountsOffs = new(br, RFHeader.MagicRFTypeVersion);
                br._endianness = EndianBinaryReader.Endianness.Big;
                MeshInfo = new();

                if (RFHeader.MagicRFTypeVersion == "2")
                {
                    br.BaseStream.Seek(CountsOffs.MatOff, SeekOrigin.Begin);
                    MatMD2 = new();

                    for (int i = 0; i < CountsOffs.MatCount; i++)
                    {
                        MatMD2.Add(br.ReadUInt16());
                        br.BaseStream.Seek(0xA, SeekOrigin.Current);
                    }
                }
                else
                {
                    br.BaseStream.Seek(CountsOffs.MatsInfoOff, SeekOrigin.Begin);
                    MatInfo = new();
                    MatMD3 = new();

                    for (int i = 0; i < CountsOffs.MatInfoCount; i++)
                    {
                        MatInfo.Add(new MatInfo_t(br));
                        br.BaseStream.Seek(0x7, SeekOrigin.Current);
                    }

                    for (int i = 0; i < CountsOffs.MatInfoCount; i++)
                    {
                        br.BaseStream.Seek(MatInfo[i].Off, SeekOrigin.Begin);
                        MatMD3.Add(new MatMD3_t(br, MatInfo[i].TexturesCount));
                    }
                }

                for (int i = 0; i < CountsOffs.MeshCount; i++)
                {
                    MeshInfo.Add(new MeshInfo_t(br));
                }


            }
        }

        public class MeshInfo_t
        {
            public byte MeshType;
            public ushort IdxSectionsCount;
            /*
            public Vector3 Origin;
            public Vector3 AxisMin;
            public Vector3 AxisMax;
            */
            public uint MeshHeaderOff;
            public uint MeshSize;
            public List<InfoInd_t> InfoInd = new();

            public MeshInfo_t(EndianBinaryReader br)
            {
                br.Skip(1);
                MeshType = br.ReadByte();
                br.Skip(4);
                IdxSectionsCount = br.ReadUInt16();
                br.Skip(0x30);
                /*
                Origin = new(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                AxisMin = new(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                AxisMax = new(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                */
                MeshHeaderOff = br.ReadUInt32();
                MeshSize = br.ReadUInt32();
                
                if (MeshType == 0x50)
                {
                    br.Skip(8);
                }

                for (int i = 0; i < IdxSectionsCount; i++)
                {
                    InfoInd.Add(new InfoInd_t(br));
                }
            }
        }

        public class InfoInd_t
        {
            public ushort MatIdx;
            public byte Optim;
            public uint Off;

            public InfoInd_t(EndianBinaryReader br)
            {
                MatIdx = br.ReadUInt16();
                br.Skip(1);
                Optim = br.ReadByte();
                br.Skip(4);
                Off = br.ReadUInt32();
            }
        }

        public class MatInfo_t
        {
            public uint Off;
            public byte TexturesCount;
            public MatInfo_t(EndianBinaryReader br)
            {
                Off = br.ReadUInt32();
                TexturesCount = br.ReadByte();
            }
        }

        public class MatMD3_t
        {
            public ushort TextureIndex;
            public ushort TextureIndex2;

            public MatMD3_t(EndianBinaryReader br, byte TexturesCount)
            {
                TextureIndex = br.ReadUInt16();
                br.BaseStream.Seek(2, SeekOrigin.Current);
                if (TexturesCount == 2)
                {
                    TextureIndex2 = br.ReadUInt16();
                    br.BaseStream.Seek(2, SeekOrigin.Current);
                }
                
            }
        }

        public class CountsOffs_t
        {
            public ushort ObjListType;
            public ushort ObjectsCount;
            public ushort TDLFilesRefCount;
            public ushort MatCount;
            public ushort MeshCount;
            public ushort MeshWithBonesCount;
            public ushort MotFilesInMolFileCount;
            public uint MatOff;

            public ushort UnkVDLSectionCount;
            public ushort MatInfoCount;
            public uint UnkVDLSectionInfoOff;
            public uint MatsInfoOff;
            public uint MatsIndexOff;
            public List<uint> MeshInfoOffs = new();
            public CountsOffs_t(EndianBinaryReader br, string MagicRFTypeVersion)
            {
                if (MagicRFTypeVersion == "2")
                {
                    br.BaseStream.Seek(4, SeekOrigin.Current);
                    ObjectsCount = br.ReadUInt16();
                    TDLFilesRefCount = br.ReadUInt16();
                    MatCount = br.ReadUInt16();
                    MeshCount = br.ReadUInt16();
                    MeshWithBonesCount = br.ReadUInt16();
                    MotFilesInMolFileCount = br.ReadUInt16();
                    MatOff = br.ReadUInt32();
                    ObjListType = 0x12E;
                }
                else
                {
                    br.BaseStream.Seek(2, SeekOrigin.Current);
                    ObjListType = br.ReadUInt16();
                    ObjectsCount = br.ReadUInt16();
                    TDLFilesRefCount = br.ReadUInt16();
                    UnkVDLSectionCount = br.ReadUInt16();
                    MatInfoCount = br.ReadUInt16();
                    MatCount = br.ReadUInt16();
                    MeshCount = br.ReadUInt16();
                    MeshWithBonesCount = br.ReadUInt16();
                    MotFilesInMolFileCount = br.ReadUInt16();
                    UnkVDLSectionInfoOff = br.ReadUInt32();
                    MatsInfoOff = br.ReadUInt32();
                    MatsIndexOff = br.ReadUInt32();
                }

                for (int i = 0; i < MeshCount; i++)
                {
                    MeshInfoOffs.Add(br.ReadUInt32());
                }
                //BE
            }
        }
        public class CHierarchyObject
        {
            public int ID;
            public int PrevObjID;
            public byte Byte1;
            public byte Byte2;
            public byte Byte3;
            public byte Code;
            public byte Level;
            public byte TranspFlag;
            public ushort Index;
            public Vector3 Translation;
            public Quaternion Rotation;
            public Vector3 Scale;
            public string MeshName;


            public CHierarchyObject(EndianBinaryReader br, uint VDLOff)
            {
                ID = (ushort)((br.BaseStream.Position - VDLOff) / 0x40);
                Byte1 = br.ReadByte();
                Byte2 = br.ReadByte();
                Byte3 = br.ReadByte();
                Code = (byte)(br.ReadByte() & 0xF0);
                Level = br.ReadByte();
                TranspFlag = br.ReadByte();
                Index = br.ReadUInt16();
                Translation = (br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                Rotation.Xyz = (br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                Rotation.W = br.ReadSingle();
                Scale = (br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                MeshName = Program.ReadStrAdv(br, 0x10);
                if (Rotation.X == 0 && Rotation.Y == 0 && Rotation.Z == 0 && Rotation.W == 0)
                {
                    Rotation.W = 1F;
                }
            }
        }
    }
}
