using System.Collections.Generic;
using System.IO;
using EndlessOceanMDLToOBJExporter;
using OpenTK.Mathematics;
using static EndlessOceanFilesConverter.Utils;

namespace EndlessOceanFilesConverter
{
    class HITStream
    {
        public Header_t Header;
        public ColData_t ColData;
        public FBH_t.FBHData_t FBHData;

        public HITStream(EndianBinaryReader br)
        {
            Header = new(br);
            ColData = new(br, Header);
            FBHData = new(br, Header);
        }

        public class Header_t
        {
            public ushort unk01;
            public ushort HITVersion;
            public string Magic;
            public uint ColCount;
            public uint OffColInfo;
            public uint OffPolyInfo;
            public uint OffVTXBuffer;
            public uint unk02;
            public uint OffFBH;

            public Header_t(EndianBinaryReader br)
            {
                unk01 = br.ReadUInt16();
                HITVersion = br.ReadUInt16();
                Magic = Program.ReadStrAdv(br, 4);
                ColCount = br.ReadUInt32();
                OffColInfo = br.ReadUInt32();
                OffPolyInfo = br.ReadUInt32();
                OffVTXBuffer = br.ReadUInt32();
                unk02 = br.ReadUInt32();
                OffFBH = br.ReadUInt32();
            }
        }

        public class ColData_t
        {
            public List<HITData_t> HITData = new();

            public ColData_t(EndianBinaryReader br, Header_t Header)
            {
                for (int i = 0; i < Header.ColCount; i++)
                {
                    HITData.Add(new(br, Header, i));
                }
            }
        }


        public class HITData_t
        {
            public string ColName;
            public ColInfo_t ColInfo;
            public List<PolyInfo_t> PolyInfo = new();
            public List<Vector3> Vertices = new();

            public HITData_t(EndianBinaryReader br, Header_t Header, int i)
            {
                ColName = Program.ReadStrAdv(br, (uint)((Header.HITVersion == 0x10) ? 0x20 : 0x10));
                br.BaseStream.Seek(Header.OffColInfo + (i * 0x40), SeekOrigin.Begin);
                ColInfo = new(br);
                br.BaseStream.Seek(Header.OffPolyInfo + (ColInfo.PolyCountSum * 0x20), SeekOrigin.Begin);

                for (int j = 0; j < ColInfo.PolyCount; j++)
                {
                    PolyInfo.Add(new(br));
                    br.BaseStream.Seek(Header.OffVTXBuffer + PolyInfo[j].VTXBufferOff * 0x10, SeekOrigin.Begin);

                    for (int k = 0; k < PolyInfo[j].VTXCount; k++)
                    {
                        Vertices.Add((br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
                        br.BaseStream.Seek(0x4, SeekOrigin.Current);
                    }

                    br.BaseStream.Seek(Header.OffPolyInfo + (ColInfo.PolyCountSum * 0x20) + ((j + 1) * 0x20), SeekOrigin.Begin);
                }

                br.BaseStream.Seek(0x20 + ((i + 1) * ((Header.HITVersion == 0x10) ? 0x20 : 0x10)), SeekOrigin.Begin);
            }
        }

        public class PolyInfo_t
        {
            public uint VTXCount;
            public uint VTXBufferOff;
            public uint unk02;
            public uint unk03;
            public Vector3 Floats;

            public PolyInfo_t(EndianBinaryReader br)
            {
                VTXCount = br.ReadUInt32();
                VTXBufferOff = br.ReadUInt32();
                unk02 = br.ReadUInt32();
                unk03 = br.ReadUInt32();
                Floats = (br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            }
        }

        public class ColInfo_t
        {
            public uint OneOrTwo;
            public uint PolyCount;
            public uint PolyCountSum;
            public float unk01;
            public Vector3 Origin_XYZ;
            public Vector3 Origin_XYZCopy;
            public Vector3 Scale_XYZ;

            public ColInfo_t(EndianBinaryReader br)
            {
                OneOrTwo = br.ReadUInt32();
                PolyCount = br.ReadUInt32();
                PolyCountSum = br.ReadUInt32();
                unk01 = br.ReadSingle();
                Origin_XYZ = (br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                br.BaseStream.Seek(0x4, SeekOrigin.Current);
                Origin_XYZCopy = (br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                br.BaseStream.Seek(0x4, SeekOrigin.Current);
                Scale_XYZ = (br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            }
        }

        public class FBH_t
        {
            public class FBHData_t
            {
                public List<Trans_t> FBHList = new();

                public FBHData_t(EndianBinaryReader br, Header_t Header)
                {
                    br.BaseStream.Seek(Header.OffFBH + ((Header.HITVersion == 0x10) ? 0x8 : 0x6), SeekOrigin.Begin);

                    ushort TransCount = br.ReadUInt16();

                    br.BaseStream.Seek(((Header.HITVersion == 0x10) ? 0xE : 0x8), SeekOrigin.Current);

                    for (int i = 0; i < TransCount; i++)
                    {
                        Trans_t FBHTrans = new(br, (byte)Header.HITVersion);
                        FBHList.Add(FBHTrans);
                    }
                }
            }

            public class Trans_t
            {
                public ushort ID;
                public ushort NextColsCount;
                public byte Flag;

                public Vector3 Translation;
                public Quaternion Rotation;

                public Trans_t(EndianBinaryReader br, byte version)
                {
                    if (version == 0x10)
                    {
                        ID = br.ReadUInt16();
                        NextColsCount = br.ReadByte();
                        Flag = br.ReadByte();
                        Translation = (br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                        Rotation.Xyz = (br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                        Rotation.W = br.ReadSingle();
                    }
                    else
                    {
                        Translation = (br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                        Rotation.W = 1f;
                        ID = br.ReadUInt16();
                        Flag = br.ReadByte();
                        NextColsCount = br.ReadByte();
                    }
                }
            }
        }

        public static void TransformVertices(List<Vector3> Vec3, Vector3 TransCorrection, Quaternion RotCorrection, bool IsRod, float TransCol, float TransRow)
        {
            Matrix4 TRSMat = Matrix4.Identity;
            TRSMat *= Matrix4.CreateFromQuaternion(RotCorrection);
            TRSMat *= Matrix4.CreateTranslation(new(TransCorrection));
            for (int i = 0; i < Vec3.Count; i++)
            {
                Vec3[i] = Vector3.TransformPosition(Vec3[i], TRSMat);
            }

            if (IsRod)
            {
                for (int i = 0; i < Vec3.Count; i++)
                {
                    Vec3[i] = (Vec3[i].X + TransCol, Vec3[i].Y, Vec3[i].Z + TransRow);
                }
            }
        }
    }
}
