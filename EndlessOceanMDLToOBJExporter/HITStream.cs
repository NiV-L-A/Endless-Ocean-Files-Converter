using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EndlessOceanMDLToOBJExporter;
using OpenTK.Mathematics;

namespace EndlessOceanFilesConverter
{
    public class HITStream
    {
        public CHITFileHeader Header;
        public CHITColData ColData;
        public CFBH.CFBHHDR FBHData;

        public HITStream(BinaryReader brHIT)
        {
            Header = new(brHIT);

            ColData = new(brHIT, Header);

            FBHData = new(brHIT, Header);
        }

        public class CHITFileHeader
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

            public CHITFileHeader(BinaryReader brHIT)
            {
                this.unk01 = Program.ReadBEUInt16(brHIT);
                this.HITVersion = Program.ReadBEUInt16(brHIT);
                this.Magic = Encoding.ASCII.GetString(BitConverter.GetBytes(brHIT.ReadUInt32()));
                this.ColCount = Program.ReadBEUInt32(brHIT);
                this.OffColInfo = Program.ReadBEUInt32(brHIT);
                this.OffPolyInfo = Program.ReadBEUInt32(brHIT);
                this.OffVTXBuffer = Program.ReadBEUInt32(brHIT);
                this.unk02 = Program.ReadBEUInt32(brHIT);
                this.OffFBH = Program.ReadBEUInt32(brHIT);
            }
        }

        public class CHITColData
        {
            public List<CHITData> HITData = new();

            public CHITColData(BinaryReader brHIT, CHITFileHeader Header)
            {
                for (int i = 0; i < Header.ColCount; i++)
                {
                    HITData.Add(new(brHIT, Header, i));
                }
            }
        }


        public class CHITData
        {
            public string ColName;
            public CHITColInfo ColInfo;
            public List<CHITPolyInfo> PolyInfo = new();
            public List<Vector3> Vertices = new();

            public CHITData(BinaryReader brHIT, CHITFileHeader Header, int i)
            {
                ColName = Program.ReadNullTerminatedString(brHIT, (uint)((Header.HITVersion == 0x10) ? 0x20 : 0x10));
                brHIT.BaseStream.Seek(Header.OffColInfo + (i * 0x40), SeekOrigin.Begin);
                ColInfo = new(brHIT);
                brHIT.BaseStream.Seek(Header.OffPolyInfo + (ColInfo.PolyCountSum * 0x20), SeekOrigin.Begin);

                for (int j = 0; j < ColInfo.PolyCount; j++)
                {
                    PolyInfo.Add(new(brHIT));
                    brHIT.BaseStream.Seek(Header.OffVTXBuffer + PolyInfo[j].VTXBufferOff * 0x10, SeekOrigin.Begin);

                    for (int k = 0; k < PolyInfo[j].VTXCount; k++)
                    {
                        Vertices.Add((Program.ReadBEFloat(brHIT), Program.ReadBEFloat(brHIT), Program.ReadBEFloat(brHIT)));
                        brHIT.BaseStream.Seek(0x4, SeekOrigin.Current);
                    }

                    brHIT.BaseStream.Seek(Header.OffPolyInfo + (ColInfo.PolyCountSum * 0x20) + ((j + 1) * 0x20), SeekOrigin.Begin);
                }

                brHIT.BaseStream.Seek(0x20 + ((i + 1) * ((Header.HITVersion == 0x10) ? 0x20 : 0x10)), SeekOrigin.Begin);
            }
        }

        public class CHITPolyInfo
        {
            public uint VTXCount;
            public uint VTXBufferOff;
            public uint unk02;
            public uint unk03;
            public Vector3 Floats;

            public CHITPolyInfo(BinaryReader brHIT)
            {
                this.VTXCount = Program.ReadBEUInt32(brHIT);
                this.VTXBufferOff = Program.ReadBEUInt32(brHIT);
                this.unk02 = Program.ReadBEUInt32(brHIT);
                this.unk03 = Program.ReadBEUInt32(brHIT);
                this.Floats = (Program.ReadBEFloat(brHIT), Program.ReadBEFloat(brHIT), Program.ReadBEFloat(brHIT));
            }
        }

        public class CHITColInfo
        {
            public uint OneOrTwo;
            public uint PolyCount;
            public uint PolyCountSum;
            public float unk01;
            public Vector3 Origin_XYZ;
            public Vector3 Origin_XYZCopy;
            public Vector3 Scale_XYZ;

            public CHITColInfo(BinaryReader brHIT)
            {
                this.OneOrTwo = Program.ReadBEUInt32(brHIT);
                this.PolyCount = Program.ReadBEUInt32(brHIT);
                this.PolyCountSum = Program.ReadBEUInt32(brHIT);
                this.unk01 = Program.ReadBEFloat(brHIT);
                this.Origin_XYZ = (Program.ReadBEFloat(brHIT), Program.ReadBEFloat(brHIT), Program.ReadBEFloat(brHIT));
                brHIT.BaseStream.Seek(0x4, SeekOrigin.Current);
                this.Origin_XYZCopy = (Program.ReadBEFloat(brHIT), Program.ReadBEFloat(brHIT), Program.ReadBEFloat(brHIT));
                brHIT.BaseStream.Seek(0x4, SeekOrigin.Current);
                this.Scale_XYZ = (Program.ReadBEFloat(brHIT), Program.ReadBEFloat(brHIT), Program.ReadBEFloat(brHIT));
            }
        }

        public class CFBH
        {
            public class CFBHHDR
            {
                public List<CTrans> FBHList = new();

                public CFBHHDR(BinaryReader brHIT, CHITFileHeader Header)
                {
                    brHIT.BaseStream.Seek(Header.OffFBH + ((Header.HITVersion == 0x10) ? 0x8 : 0x6), SeekOrigin.Begin);

                    ushort TransCount = Program.ReadBEUInt16(brHIT);

                    brHIT.BaseStream.Seek(((Header.HITVersion == 0x10) ? 0xE : 0x8), SeekOrigin.Current);

                    for (int i = 0; i < TransCount; i++)
                    {
                        CTrans FBHTrans = new(brHIT, (byte)Header.HITVersion);
                        FBHList.Add(FBHTrans);
                    }
                }
            }

            public class CTrans
            {
                public ushort ID;
                public ushort NextColsCount;
                public byte Flag;

                public Vector3 Translation;
                public Quaternion Rotation;

                public CTrans(BinaryReader br, byte version)
                {
                    if (version == 0x10)
                    {
                        this.ID = Program.ReadBEUInt16(br);
                        this.NextColsCount = br.ReadByte();
                        this.Flag = br.ReadByte();

                        this.Translation = (Program.ReadBEFloat(br), Program.ReadBEFloat(br), Program.ReadBEFloat(br));
                        this.Rotation.Xyz = (Program.ReadBEFloat(br), Program.ReadBEFloat(br), Program.ReadBEFloat(br));
                        this.Rotation.W = Program.ReadBEFloat(br);
                    }
                    else
                    {
                        this.Translation = (Program.ReadBEFloat(br), Program.ReadBEFloat(br), Program.ReadBEFloat(br));
                        this.Rotation.W = 1f;
                        this.ID = Program.ReadBEUInt16(br);
                        this.Flag = br.ReadByte();
                        this.NextColsCount = br.ReadByte();
                    }
                }
            }
        }

        public static void TransformVertices(List<Vector3> Vec3, Vector3 TransCorrection, Quaternion RotCorrection, bool IsRod, float TransCol, float TransRow)
        {
            Matrix4 TRSMat = Matrix4.CreateFromQuaternion(Program.QuatIdentity);
            TRSMat *= Matrix4.CreateFromQuaternion(RotCorrection);
            TRSMat *= Matrix4.CreateTranslation(new(TransCorrection));
            for (int i = 0; i < Vec3.Count(); i++)
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
