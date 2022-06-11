using System;
using System.IO;
using EndlessOceanMDLToOBJExporter;
using OpenTK.Mathematics;


namespace EndlessOceanFilesConverter
{
    class MDLStream
    {
        public class CHierarchyObject
        {
            public byte Byte1;
            public byte Byte2;
            public byte Byte3;
            public byte ObjCode;

            public byte Level;
            public byte TransparencyFlag;
            public ushort MeshIndex;

            public Vector3 Translation;
            public Quaternion Rotation;
            public Vector3 Scale;

            public string MeshName;

            public int NObject;

            public CHierarchyObject(BinaryReader br, uint VDLOff)
            {
                this.Byte1 = br.ReadByte();
                this.Byte2 = br.ReadByte();
                this.Byte3 = br.ReadByte();
                this.ObjCode = (byte)(br.ReadByte() & 0xF0);

                this.Level = br.ReadByte();
                this.TransparencyFlag = br.ReadByte();
                this.MeshIndex = Program.ReadBEUInt16(br);

                this.Translation = (Program.ReadBEFloat(br), Program.ReadBEFloat(br), Program.ReadBEFloat(br));
                this.Rotation.Xyz = (Program.ReadBEFloat(br), Program.ReadBEFloat(br), Program.ReadBEFloat(br));
                this.Rotation.W = Program.ReadBEFloat(br);
                this.Scale = (Program.ReadBEFloat(br), Program.ReadBEFloat(br), Program.ReadBEFloat(br));

                if (Rotation.X == 0 && Rotation.Y == 0 && Rotation.Z == 0 && Rotation.W == 0)
                {
                    this.Rotation.W = 1F;
                }

                this.MeshName = Program.ReadNullTerminatedString(br, 0x10);

                this.NObject = (ushort)((br.BaseStream.Position - 0x40 - VDLOff) / 0x40);
            }
        }
    }
}
