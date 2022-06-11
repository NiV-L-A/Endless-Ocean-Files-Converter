using EndlessOceanMDLToOBJExporter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EndlessOceanFilesConverter
{
    public class TDLStream
    {
        public CHeader Header;
        public CData Data;

        public TDLStream(BinaryReader brTDL)
        {
            this.Header = new(brTDL);
            brTDL.BaseStream.Position = Header.FileHeader.PixelDataStart;
            this.Data = new(brTDL, Header);
        }

        public static void TransformCMPRBlock(CCMPRBlock Data, int Blocks, uint OutputFormat)
        {
            for (int InternalBlocks = 0; InternalBlocks < Blocks; InternalBlocks++)
            {
                var BGRA1 = Data.BGRA[InternalBlocks][0];
                uint B1 = GetBFromBGRA(BGRA1);
                uint G1 = GetGFromBGRA(BGRA1);
                uint R1 = GetRFromBGRA(BGRA1);

                var BGRA2 = Data.BGRA[InternalBlocks][1];
                uint B2 = GetBFromBGRA(BGRA2);
                uint G2 = GetGFromBGRA(BGRA2);
                uint R2 = GetRFromBGRA(BGRA2);

                uint B3;
                uint G3;
                uint R3;
                uint B4;
                uint G4;
                uint R4;
                uint BGRA3 = 0;
                uint BGRA4 = 0;

                if (BGRA1 > BGRA2)
                {
                    B3 = (uint)Math.Floor((2 * B1 + B2) / 3.0 + 0.5);
                    G3 = (uint)Math.Floor((2 * G1 + G2) / 3.0 + 0.5);
                    R3 = (uint)Math.Floor((2 * R1 + R2) / 3.0 + 0.5);
                    B4 = (uint)Math.Floor((2 * B2 + B1) / 3.0 + 0.5);
                    G4 = (uint)Math.Floor((2 * G2 + G1) / 3.0 + 0.5);
                    R4 = (uint)Math.Floor((2 * R2 + R1) / 3.0 + 0.5);

                    if (OutputFormat == 0)
                    {
                        BGRA1 = CreateBGRA(B1, G1, R1, OutputFormat);
                        BGRA2 = CreateBGRA(B2, G2, R2, OutputFormat);
                        BGRA3 = CreateBGRA(B3, G3, R3, OutputFormat);
                        BGRA4 = CreateBGRA(B4, G4, R4, OutputFormat);
                    }
                    else if (OutputFormat == 1)
                    {
                        B3 = B3 & 0b11111000;
                        G3 = G3 & 0b11111100;
                        R3 = R3 & 0b11111000;
                        B4 = B4 & 0b11111000;
                        G4 = G4 & 0b11111100;
                        R4 = R4 & 0b11111000;

                        BGRA1 = CreateBGRA(B1, G1, R1, OutputFormat);
                        BGRA2 = CreateBGRA(B2, G2, R2, OutputFormat);
                        BGRA3 = CreateBGRA(B3, G3, R3, OutputFormat);
                        BGRA4 = CreateBGRA(B4, G4, R4, OutputFormat);
                    }
                }
                else
                {
                    B3 = (uint)Math.Floor((B1 + B2) / 2.0 + 0.5);
                    G3 = (uint)Math.Floor((G1 + G2) / 2.0 + 0.5);
                    R3 = (uint)Math.Floor((R1 + R2) / 2.0 + 0.5);

                    if (OutputFormat == 0)
                    {
                        BGRA1 = CreateBGRA(B1, G1, R1, OutputFormat);
                        BGRA2 = CreateBGRA(B2, G2, R2, OutputFormat);
                        BGRA3 = CreateBGRA(B3, G3, R3, OutputFormat);
                        BGRA4 = 0;
                    }
                    else if (OutputFormat == 1)
                    {
                        B3 = B3 & 0b11111000;
                        G3 = G3 & 0b11111100;
                        R3 = R3 & 0b11111000;

                        BGRA1 = CreateBGRA(B1, G1, R1, OutputFormat);
                        BGRA2 = CreateBGRA(B2, G2, R2, OutputFormat);
                        BGRA3 = CreateBGRA(B3, G3, R3, OutputFormat);
                        BGRA4 = 0;
                    }
                }

                Data.BGRA[InternalBlocks][0] = BGRA1;
                Data.BGRA[InternalBlocks][1] = BGRA2;
                Data.BGRA[InternalBlocks][2] = BGRA3;
                Data.BGRA[InternalBlocks][3] = BGRA4;

                uint ColorIndices = Data.Indices[InternalBlocks][0];

                for (int IndexNum = 0; IndexNum < 16; IndexNum++)
                {
                    uint IndexTemp = (ColorIndices >> (32 - (IndexNum + 1) * 2)) & 0b00000000000000000000000000000011;
                    Data.Indices[InternalBlocks][IndexNum] = IndexTemp;
                }
            }
        }

        public static void TransformRGB5A3Block(CRGB5A3Block Data, int Blocks, uint OutputFormat)
        {
            for (int CurrentBlock = 0; CurrentBlock < Blocks; CurrentBlock++)
            {
                for (int IndexInt = 0; IndexInt < 8; IndexInt++)
                {
                    uint ColorIndex = Data.ColorIndices[CurrentBlock][IndexInt];

                    for (int IndexNum = 1; IndexNum <= 4; IndexNum++)
                    {
                        var IndexTemp = (ColorIndex >> (32 - IndexNum * 8)) & 0b00000000000000000000000011111111;
                        Data.Indices[CurrentBlock][IndexInt * 4 + IndexNum - 1] = IndexTemp;
                    }
                }
            }
        }

        public static void TransformRGB5A3Palette(CPalette Palette, int Blocks)
        {
            uint B = 0;
            uint G = 0;
            uint R = 0;
            uint A = 0;
            for (int i = 0; i < Blocks; i++)
            {
                ushort ARGB = (ushort)Palette.ARGB[i];
                uint Alpha = (uint)(ARGB & 0b1000000000000000) >> 15;

                if (Alpha == 0)
                {
                    B = (uint)Math.Floor((ARGB & 0b1111) * 1.0 / (0b1111 * 1.0) * 255 + 0.5);
                    G = (uint)Math.Floor(((ARGB & 0b11110000) >> 4) * 1.0 / (0b1111 * 1.0) * 255 + 0.5);
                    R = (uint)Math.Floor(((ARGB & 0b111100000000) >> 8) * 1.0 / (0b1111 * 1.0) * 255 + 0.5);
                    A = (uint)Math.Floor(((ARGB & 0b111000000000000) >> 12) * 1.0 / (0b111 * 1.0) * 255 + 0.5);
                }
                else if (Alpha == 1)
                {
                    B = (uint)Math.Floor((ARGB & 0b11111) * 1.0 / (0b11111 * 1.0) * 255 + 0.5);
                    G = (uint)Math.Floor(((ARGB & 0b1111100000) >> 5) * 1.0 / (0b11111 * 1.0) * 255 + 0.5);
                    R = (uint)Math.Floor(((ARGB & 0b111110000000000) >> 10) * 1.0 / (0b11111 * 1.0) * 255 + 0.5);
                    A = 255;
                }

                uint BGRA = (B << 24) + (G << 16) + (R << 8) + A;
                Palette.ARGB[i] = BGRA;
            }
        }

        public static uint CreateBGRA(uint B, uint G, uint R, uint OutputFormat)
        {
            uint BGRA = 0;
            if (OutputFormat == 0)
            {
                BGRA = (B << 24) + (G << 16) + (R << 8) + 0xFF;
            }
            else if (OutputFormat == 1)
            {
                BGRA = (B << 24) + (G << 19) + (R << 13) + 0xFF00;
            }

            return BGRA;
        }

        public static uint GetBFromBGRA(uint BGRA)
        {
            uint B = (uint)Math.Floor((BGRA & 0b11111) / (1.0 * 0b11111) * 255 + 0.5);
            return B;
        }

        public static uint GetGFromBGRA(uint BGRA)
        {
            uint G = (uint)Math.Floor(((BGRA & 0b11111100000) >> 5) / (1.0 * 0b111111) * 255 + 0.5);
            return G;
        }

        public static uint GetRFromBGRA(uint BGRA)
        {
            uint R = (uint)Math.Floor(((BGRA & 0b1111100000000000) >> 11) / (1.0 * 0b11111) * 255 + 0.5);
            return R;
        }

        public static void WriteBMPCMPRBlock(BinaryWriter bwBMP, TDLStream TDLFile)
        {
            ushort LocalTotalWidth = TDLFile.Header.FileHeader.TotalWidth;
            ushort LocalTotalHeight = TDLFile.Header.FileHeader.TotalHeight;

            for (int RowNum = 0; RowNum < LocalTotalHeight; RowNum++)
            {
                bwBMP.BaseStream.Seek(122 + (LocalTotalHeight - RowNum - 1) * LocalTotalWidth * 4, SeekOrigin.Begin);

                for (int ColumnNum = 0; ColumnNum < LocalTotalWidth; ColumnNum++)
                {
                    uint CurrentBlock = (uint)(ColumnNum / 8 * 2 + ColumnNum / 4 + RowNum / 4 * 2 + RowNum / 8 * (LocalTotalWidth / 4) * 2 - RowNum / 8 * 4);
                    uint CurrentIndex = (uint)(RowNum % 4 * 4 + (ColumnNum % 4));
                    uint CurrentRGB = TDLFile.Data.CMPRBlock.Indices[(int)CurrentBlock][CurrentIndex];

                    bwBMP.Write(Program.ReverseIntToByteArray((int)TDLFile.Data.CMPRBlock.BGRA[(int)CurrentBlock][CurrentRGB]));
                }
            }

            bwBMP.BaseStream.Seek(0x2, SeekOrigin.Begin);
            bwBMP.Write((short)bwBMP.BaseStream.Length);
        }

        public static void WriteBMPRGB5A3Block(BinaryWriter bwBMP, TDLStream TDLFile)
        {
            ushort LocalTotalWidth = TDLFile.Header.FileHeader.TotalWidth;
            ushort LocalTotalHeight = TDLFile.Header.FileHeader.TotalHeight;

            for (int RowNum = 0; RowNum < LocalTotalHeight; RowNum++)
            {
                bwBMP.BaseStream.Seek(122 + (LocalTotalHeight - RowNum - 1) * LocalTotalWidth * 4, SeekOrigin.Begin);

                for (int ColumnNum = 0; ColumnNum < LocalTotalWidth; ColumnNum++)
                {
                    uint CurrentBlock = (uint)(RowNum / 4 * (LocalTotalWidth / 8) + ColumnNum / 8);
                    uint CurrentIndex = (uint)((RowNum % 4) * 8 + (ColumnNum % 8));
                    var CurrentPalette = TDLFile.Data.RGB5A3Block.Indices[(int)CurrentBlock][CurrentIndex];

                    bwBMP.Write(Program.ReverseIntToByteArray((int)TDLFile.Data.Palette.ARGB[(int)CurrentPalette]));
                }
            }

            bwBMP.BaseStream.Seek(0x2, SeekOrigin.Begin);
            bwBMP.Write((short)bwBMP.BaseStream.Length);
        }

        public static void WriteBMPHeader(BinaryWriter bwBMP, TDLStream TDLFile, uint OutputFormat)
        {
            bwBMP.Write("BM".ToArray());
            bwBMP.BaseStream.Seek(0x8, SeekOrigin.Current);
            bwBMP.Write(122);
            bwBMP.Write(108);
            bwBMP.Write((int)TDLFile.Header.FileHeader.TotalWidth);
            bwBMP.Write((int)TDLFile.Header.FileHeader.TotalHeight);
            bwBMP.Write((short)1);

            if (OutputFormat == 0)
            {
                bwBMP.Write((short)32);
            }

            bwBMP.Write(3);
            bwBMP.Write(0);
            bwBMP.BaseStream.Seek(0x10, SeekOrigin.Current);

            bwBMP.Write(Program.ReverseIntToByteArray(0x0000FF00));
            bwBMP.Write(Program.ReverseIntToByteArray(0x00FF0000));
            bwBMP.Write(0xFF);
            bwBMP.Write(Program.ReverseIntToByteArray(0x000000FF));
            bwBMP.BaseStream.Seek(0x34, SeekOrigin.Current);
        }
    }

    public class CHeader
    {
        public CFileHeader FileHeader;
        public List<CTextureHeader> TextureHeaders = new();

        public CHeader(BinaryReader brTDL)
        {
            this.FileHeader = new(brTDL);

            for (int i = 0; i < FileHeader.TextureCount; i++)
            {
                CTextureHeader TextureHeader;
                TextureHeader = new(brTDL);
                this.TextureHeaders.Add(TextureHeader);
            }
        }
    }

    public class CFileHeader
    {
        public string Magic;
        public uint unk01;
        public ushort TotalWidth;
        public ushort TotalHeight;
        public ushort TextureCount;
        public ushort MipmapCount;
        public byte Format;
        public byte unk02;
        public ushort PaletteSize;
        public uint PixelDataStart;
        public uint PaletteStart;

        public CFileHeader(BinaryReader brTDL)
        {
            this.Magic = Encoding.ASCII.GetString(BitConverter.GetBytes(brTDL.ReadUInt32()));
            this.unk01 = Program.ReadBEUInt32(brTDL);
            this.TotalWidth = Program.ReadBEUInt16(brTDL);
            this.TotalHeight = Program.ReadBEUInt16(brTDL);
            this.TextureCount = Program.ReadBEUInt16(brTDL);
            this.MipmapCount = Program.ReadBEUInt16(brTDL);
            this.Format = brTDL.ReadByte();
            this.unk02 = brTDL.ReadByte();
            this.PaletteSize = Program.ReadBEUInt16(brTDL);
            this.PixelDataStart = Program.ReadBEUInt32(brTDL);
            this.PaletteStart = Program.ReadBEUInt32(brTDL);
        }
    }

    public class CTextureHeader
    {
        public uint unk01;
        public ushort TextureWidth;
        public ushort TextureHeight;
        public ushort OffsetWidth;
        public ushort OffsetHeight;

        public CTextureHeader(BinaryReader brTDL)
        {
            this.unk01 = Program.ReadBEUInt32(brTDL);
            this.TextureWidth = Program.ReadBEUInt16(brTDL);
            this.TextureHeight = Program.ReadBEUInt16(brTDL);
            this.OffsetWidth = Program.ReadBEUInt16(brTDL);
            this.OffsetHeight = Program.ReadBEUInt16(brTDL);
        }
    }

    public class CData
    {
        public CCMPRBlock CMPRBlock;
        public CRGB5A3Block RGB5A3Block;
        public CPalette Palette;

        public CData(BinaryReader brTDL, CHeader Header)
        {
            if (Header.FileHeader.Format == 10)
            {
                this.CMPRBlock = new(brTDL, Header.FileHeader.TotalHeight * Header.FileHeader.TotalWidth / 16);
            }
            else if (Header.FileHeader.Format == 5)
            {
                this.RGB5A3Block = new(brTDL, Header.FileHeader.TotalHeight * Header.FileHeader.TotalWidth / 32);
                brTDL.BaseStream.Seek(Header.FileHeader.PaletteStart, SeekOrigin.Begin);
                this.Palette = new(brTDL, (Header.FileHeader.PaletteSize / 2));
            }
        }
    }

    public class CCMPRBlock
    {
        public List<uint[]> BGRA = new();
        public List<uint[]> Indices = new();

        public CCMPRBlock(BinaryReader brTDL, int Blocks)
        {
            for (int CountBlocks = 0; CountBlocks < Blocks; CountBlocks++)
            {
                uint[] BGRATemp = new uint[4] { Program.ReadBEUInt16(brTDL), Program.ReadBEUInt16(brTDL), 0, 0 };
                BGRA.Add(BGRATemp);
                uint[] Index = new uint[16] { Program.ReadBEUInt32(brTDL), 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
                Indices.Add(Index);
            }
        }
    }

    public class CRGB5A3Block
    {
        public List<uint[]> ColorIndices = new();
        public List<uint[]> Indices = new();

        public CRGB5A3Block(BinaryReader brTDL, int Blocks)
        {
            for (int CountBlocks = 0; CountBlocks < Blocks; CountBlocks++)
            {
                uint[] Index = new uint[8] { Program.ReadBEUInt32(brTDL), Program.ReadBEUInt32(brTDL), Program.ReadBEUInt32(brTDL), Program.ReadBEUInt32(brTDL), Program.ReadBEUInt32(brTDL), Program.ReadBEUInt32(brTDL), Program.ReadBEUInt32(brTDL), Program.ReadBEUInt32(brTDL)};
                ColorIndices.Add(Index);
                uint[] TIndex = new uint[32];
                Indices.Add(TIndex);
            }
        }
    }

    public class CPalette
    {
        public List<uint> ARGB = new();

        public CPalette(BinaryReader brTDL, int Blocks)
        {
            for (int CountBlocks = 0; CountBlocks < Blocks; CountBlocks++)
            {
                uint Num = Program.ReadBEUInt16(brTDL);
                ARGB.Add(Num);
            }
        }
    }
}
