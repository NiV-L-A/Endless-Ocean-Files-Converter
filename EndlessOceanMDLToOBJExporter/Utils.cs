using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EndlessOceanMDLToOBJExporter;

namespace EndlessOceanFilesConverter
{
    class Utils
    {
        public class EndianBinaryReader : BinaryReader
        {
            public enum Endianness
            {
                Little,
                Big,
            }

            public Endianness _endianness = Endianness.Little;

            public EndianBinaryReader(Stream input) : base(input)
            {
            }

            public EndianBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
            {
            }

            public EndianBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(
                input, encoding, leaveOpen)
            {
            }

            public EndianBinaryReader(Stream input, Endianness endianness) : base(input)
            {
                _endianness = endianness;
            }

            public EndianBinaryReader(Stream input, Encoding encoding, Endianness endianness) :
                base(input, encoding)
            {
                _endianness = endianness;
            }

            public EndianBinaryReader(Stream input, Encoding encoding, bool leaveOpen,
                Endianness endianness) : base(input, encoding, leaveOpen)
            {
                _endianness = endianness;
            }

            public void Skip(long v)
            {
                this.BaseStream.Seek(v, SeekOrigin.Current);
            }

            public override short ReadInt16() => ReadInt16(_endianness);

            public short ReadInt16(Endianness endianness) => endianness == Endianness.Little
                ? BinaryPrimitives.ReadInt16LittleEndian(ReadBytes(sizeof(short)))
                : BinaryPrimitives.ReadInt16BigEndian(ReadBytes(sizeof(short)));

            public override ushort ReadUInt16() => ReadUInt16(_endianness);

            public ushort ReadUInt16(Endianness endianness) => endianness == Endianness.Little
                ? BinaryPrimitives.ReadUInt16LittleEndian(ReadBytes(sizeof(ushort)))
                : BinaryPrimitives.ReadUInt16BigEndian(ReadBytes(sizeof(ushort)));

            public override int ReadInt32() => ReadInt32(_endianness);

            public int ReadInt32(Endianness endianness) => endianness == Endianness.Little
                ? BinaryPrimitives.ReadInt32LittleEndian(ReadBytes(sizeof(int)))
                : BinaryPrimitives.ReadInt32BigEndian(ReadBytes(sizeof(int)));

            public override uint ReadUInt32() => ReadUInt32(_endianness);

            public uint ReadUInt32(Endianness endianness) => endianness == Endianness.Little
                ? BinaryPrimitives.ReadUInt32LittleEndian(ReadBytes(sizeof(uint)))
                : BinaryPrimitives.ReadUInt32BigEndian(ReadBytes(sizeof(uint)));

            public override long ReadInt64() => ReadInt64(_endianness);

            public long ReadInt64(Endianness endianness) => endianness == Endianness.Little
                ? BinaryPrimitives.ReadInt64LittleEndian(ReadBytes(sizeof(long)))
                : BinaryPrimitives.ReadInt64BigEndian(ReadBytes(sizeof(long)));

            public override ulong ReadUInt64() => ReadUInt64(_endianness);

            public ulong ReadUInt64(Endianness endianness) => endianness == Endianness.Little
                ? BinaryPrimitives.ReadUInt64LittleEndian(ReadBytes(sizeof(ulong)))
                : BinaryPrimitives.ReadUInt64BigEndian(ReadBytes(sizeof(ulong)));

            public override float ReadSingle() => ReadSingle(_endianness);

            public float ReadSingle(Endianness endianness) => endianness == Endianness.Little ? BinaryPrimitives.ReadSingleLittleEndian(ReadBytes(sizeof(float))) : BinaryPrimitives.ReadSingleBigEndian(ReadBytes(sizeof(float)));
        }

        public class RFHeader_t
        {
            public string MagicRF;
            public string MagicRFVersion;
            public string MagicRFType;
            public string MagicRFTypeVersion;
            public ushort FileCount;
            public ushort FileListSize;
            public ushort Flag;
            public uint HeaderSize;
            public List<RFFile_t> Files;

            public RFHeader_t(EndianBinaryReader br)
            {
                MagicRF = Program.ReadStrAdv(br, 2);
                MagicRFVersion = Program.ReadStrAdv(br, 1);
                MagicRFType = Program.ReadStrAdv(br, 2);
                MagicRFTypeVersion = Program.ReadStrAdv(br, 1);
                FileCount = br.ReadUInt16();
                FileListSize = br.ReadUInt16();
                Flag = br.ReadUInt16();
                HeaderSize = br.ReadUInt32();
                Files = new();

                for (int i = 0; i < FileCount; i++)
                {
                    RFFile_t RFFile = new(br, MagicRFVersion);
                    Files.Add(RFFile);
                }

                if (Files.GroupBy(n => n.FileName).Any(c => c.Count() > 1)) //if there're duplicates
                {
                    for (int i = 0; i < FileCount; i++)
                    {
                        string name = Files[i].FileName;
                        int idx = 0;

                        for (int j = i + 1; j < FileCount - i; j++)
                        {
                            if (name == Files[j].FileName)
                            {
                                Files[i].FileName += idx.ToString();
                            }
                        }
                    }
                }
            }
        }

        public class RFFile_t
        {
            public string FileName;
            public uint FileSize;
            public uint FileOff;
            public byte FileType;
            public byte unk1;
            public byte IsInFile;
            public byte unk2;

            public RFFile_t(EndianBinaryReader br, string MagicRFVersion)
            {
                if (MagicRFVersion != "P") //RF2
                {
                    FileName = Program.ReadStrAdv(br, 0x14);
                    FileSize = br.ReadUInt32();
                    FileOff = br.ReadUInt32();
                    FileType = br.ReadByte();
                    unk1 = br.ReadByte();
                    IsInFile = br.ReadByte();
                    unk2 = br.ReadByte();
                }
                else //RFP
                {
                    FileName = Program.ReadStrAdv(br, 0x10);
                    FileOff = br.ReadUInt32();
                    FileSize = br.ReadUInt32();
                    br.BaseStream.Seek(0x4, SeekOrigin.Current);
                    FileType = br.ReadByte();
                    unk1 = br.ReadByte();
                    IsInFile = br.ReadByte();
                    unk2 = br.ReadByte();
                }
            }
        }

        public enum RFExtensionType
        {
            VDL = 0,
            TDL = 1,
            TXS = 2,
            MDL = 5,
            MOT = 6,
            MOL = 7
        }
    }
}
