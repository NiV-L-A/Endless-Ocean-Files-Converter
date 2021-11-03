using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace EndlessOceanMDLToOBJExporter
{
    class Program
    {
        public static void GetIndexStripA(BinaryReader br, FileStream fs)
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

            TMP2 = fs.Position;

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
            if (vtxCountShort || indexCount == 0) //read vtx short
            {
                IndexPos = ReverseUInt16(br.ReadUInt16());
                IndexPos = (ushort)(IndexPos + 1);
            }
            else
            {
                IndexPos = br.ReadByte();//read vtx byte
                IndexPos = (byte)(IndexPos + 1);
            }
            return IndexPos;
        }

        public static int GetIndexNorm(BinaryReader br)
        {
            int IndexNorm = 0;
            if (normCountShort || indexCount == 0)
            {
                IndexNorm = ReverseUInt16(br.ReadUInt16()); //read norm short
                IndexNorm = (ushort)(IndexNorm + 1);
            }
            else
            {
                IndexNorm = br.ReadByte(); //read norm byte
                IndexNorm = (byte)(IndexNorm + 1);
            }
            return IndexNorm;
        }

        public static int GetIndexLight(BinaryReader br)
        {
            int IndexLight = 0;
            if (lightCountShort || RFPMD2 == true && lightOff > 0x00 || indexCount == 0 && lightOff != 0) //RFPMD2 special type
            {
                IndexLight = ReverseUInt16(br.ReadUInt16());
                IndexLight = (ushort)(IndexLight + 1);
            }
            else if (!lightCountShort && lightCount > 0x00)
            {
                IndexLight = br.ReadByte();
                IndexLight = (byte)(IndexLight + 1);
            }
            return IndexLight;
        }

        public static int GetIndexUV(BinaryReader br)
        {
            int IndexUV = 0;
            if (uvCountShort || indexCount == 0)
            {
                IndexUV = ReverseUInt16(br.ReadUInt16());
                IndexUV = (ushort)(IndexUV + 1);
            }
            else
            {
                IndexUV = br.ReadByte();
                IndexUV = (byte)(IndexUV + 1);
            }
            return IndexUV;
        }

        public static int GetIndexUnk01(BinaryReader br)
        {
            int IndexUnk01 = 0;
            if (unk01CountShort && indexCount != 0)
            {
                IndexUnk01 = ReverseUInt16(br.ReadUInt16());
                IndexUnk01 = (ushort)(IndexUnk01 + 1);
            }
            else if (!unk01CountShort && unk01Count >= 0x01 && indexCount != 0)
            {
                IndexUnk01 = br.ReadByte();
                IndexUnk01 = (byte)(IndexUnk01 + 1);
            }
            return IndexUnk01;
        }

        public static ushort GetCurrentChunk(bool RF2MD3, ushort ObjectListType, FileStream fs)
        {

            if (RF2MD3 && ObjectListType >= 0x12F)
                CurrentChunk = (ushort)((fs.Position - 0x08 - (VDLOff + 0x0C)) / 0x40);
            else
                CurrentChunk = (ushort)((fs.Position - 0x08 - VDLOff) / 0x40);

            return CurrentChunk;
        }
        public static Vector3 MeshTransform(byte flag, Vector3 Vec3VTX, int CorrectionCount)
        {
            if (flag == 0)
            {
                Vec3VTX.X *= DictXSizeCorrectionMain[0];
                Vec3VTX.Y *= DictYSizeCorrectionMain[0];
                Vec3VTX.Z *= DictZSizeCorrectionMain[0];
            }

            if (DuplMesh)
            {
                Vec3VTX = RotateMeshMainOnly(Quat, Vec3VTX);

                if (flag == 0)
                {
                    Vec3VTX = TranslateMeshMainOnly(Vec3VTX);
                    if (InsideMeshForEight)
                    {
                        Quat.X = Dictx31_XRotCorrectionMain[0];
                        Quat.Y = Dictx31_YRotCorrectionMain[0];
                        Quat.Z = Dictx31_ZRotCorrectionMain[0];
                        Quat.W = Dictx31_WRotCorrectionMain[0];
                        Vec3VTX = Vector3.Transform(Vec3VTX, Quat);
                        Vec3VTX.X += Dictx31_XPointCorrectionMain[0];
                        Vec3VTX.Y += Dictx31_YPointCorrectionMain[0];
                        Vec3VTX.Z += Dictx31_ZPointCorrectionMain[0];
                    }

                    for (da = 0; da < CorrectionCount; da++)
                    {
                        Quat.X = Dictx30_XRotCorrectionMain[PreviousCorrectionCount + da];
                        Quat.Y = Dictx30_YRotCorrectionMain[PreviousCorrectionCount + da];
                        Quat.Z = Dictx30_ZRotCorrectionMain[PreviousCorrectionCount + da];
                        Quat.W = Dictx30_WRotCorrectionMain[PreviousCorrectionCount + da];
                        Vec3VTX = Vector3.Transform(Vec3VTX, Quat);
                        Vec3VTX.X += Dictx30_XPointCorrectionMain[PreviousCorrectionCount + da];
                        Vec3VTX.Y += Dictx30_YPointCorrectionMain[PreviousCorrectionCount + da];
                        Vec3VTX.Z += Dictx30_ZPointCorrectionMain[PreviousCorrectionCount + da];
                    }
                }
                else/* if (flag == 1)*/
                {
                    if (InsideMeshForEight)
                    {
                        Quat.X = Dictx31_XRotCorrectionMain[0];
                        Quat.Y = Dictx31_YRotCorrectionMain[0];
                        Quat.Z = Dictx31_ZRotCorrectionMain[0];
                        Quat.W = Dictx31_WRotCorrectionMain[0];
                        Vec3VTX = Vector3.Transform(Vec3VTX, Quat);
                    }
                    for (da = 0; da < CorrectionCount; da++)
                    {
                        Quat.X = Dictx30_XRotCorrectionMain[PreviousCorrectionCount + da];
                        Quat.Y = Dictx30_YRotCorrectionMain[PreviousCorrectionCount + da];
                        Quat.Z = Dictx30_ZRotCorrectionMain[PreviousCorrectionCount + da];
                        Quat.W = Dictx30_WRotCorrectionMain[PreviousCorrectionCount + da];
                        Vec3VTX = Vector3.Transform(Vec3VTX, Quat);
                    }
                }
            }
            else
            {
                if (flag == 0)
                {
                    for (int da = 0; da < CorrectionCount; da++)
                    {
                        Quat.X = DictXRotCorrectionMain[da];
                        Quat.Y = DictYRotCorrectionMain[da];
                        Quat.Z = DictZRotCorrectionMain[da];
                        Quat.W = DictWRotCorrectionMain[da];
                        Vec3VTX = Vector3.Transform(Vec3VTX, Quat);
                        Vec3VTX.X += DictXPointCorrectionMain[da];
                        Vec3VTX.Y += DictYPointCorrectionMain[da];
                        Vec3VTX.Z += DictZPointCorrectionMain[da];
                    }
                }
                else/* if (flag == 1)*/
                {
                    for (int da = 0; da < CorrectionCount; da++)
                    {
                        Quat.X = DictXRotCorrectionMain[da];
                        Quat.Y = DictYRotCorrectionMain[da];
                        Quat.Z = DictZRotCorrectionMain[da];
                        Quat.W = DictWRotCorrectionMain[da];
                        Vec3VTX = Vector3.Transform(Vec3VTX, Quat);
                    }
                }
            }
            return Vec3VTX;
        }
        public static Vector3 RotateMeshMainOnly(Quaternion Quat, Vector3 Vec3VTX)
        {
            Quat.X = DictXRotCorrectionMain[0];
            Quat.Y = DictYRotCorrectionMain[0];
            Quat.Z = DictZRotCorrectionMain[0];
            Quat.W = DictWRotCorrectionMain[0];
            Vec3VTX = Vector3.Transform(Vec3VTX, Quat);
            return Vec3VTX;
        }
        public static Vector3 TranslateMeshMainOnly(Vector3 Vec3VTX)
        {
            Vec3VTX.X += DictXPointCorrectionMain[0];
            Vec3VTX.Y += DictYPointCorrectionMain[0];
            Vec3VTX.Z += DictZPointCorrectionMain[0];
            return Vec3VTX;
        }
        public static void GetTDLNames(FileStream fs, BinaryReader br, ushort Entries)
        {
            tdlNameArray = new string[TDLFilesRefCount];
            fs.Seek(0x10, SeekOrigin.Begin);
            ushort contatoreTDLNames = 0;
            for (ushort GetTDLNamesCount = 0; GetTDLNamesCount < Entries; GetTDLNamesCount++)
            {
                fs.Seek(0x1C, SeekOrigin.Current);
                byte indexExtensionType = br.ReadByte();
                if (indexExtensionType == 1)
                {
                    fs.Seek(-0x1D, SeekOrigin.Current);
                    tdlNameArray[contatoreTDLNames] = ReadNullTerminatedString(br);
                    tdlNameArray[contatoreTDLNames] = tdlNameArray[contatoreTDLNames].Replace(".tdl", ".png");
                    fs.Seek(0x1F - tdlNameArray[contatoreTDLNames].Length, SeekOrigin.Current);
                    contatoreTDLNames += 1;
                }
                else
                {
                    fs.Seek(0x03, SeekOrigin.Current);
                }
            }
        }
        public static void LogVtx(FileStream fs, FileStream fsNew, BinaryReader br, uint normOff, uint uvOff, string info)
        {
            x30_XCoordsArray = new float[vtxCount];
            x30_YCoordsArray = new float[vtxCount];
            x30_ZCoordsArray = new float[vtxCount];
            info = $"#vtxCount: {vtxCount} [0x{vtxCount:X4}]\n#POS: 0x{fs.Position:X8}\n";
            WriteText(fsNew, info);
            LogVeritces(fs, fsNew, br, normOff, uvOff, 0);

            if (DuplMesh)
            {
                x30_XCoordsArray = XCoordsArray;
                x30_YCoordsArray = YCoordsArray;
                x30_ZCoordsArray = ZCoordsArray;
            }

            MeshLevelCont = 1;
            ogPointArrayIndexCont += 3;
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
        public static void LogUVs(FileStream fs, FileStream fsNew, BinaryReader br, uint normOff, uint uvOff, uint MeshHDRStartOff, string info)
        {
            uvOff += MeshHDRStartOff;
            fs.Seek(uvOff, SeekOrigin.Begin);
            info = $"#uvCount: {uvCount} [0x{uvCount:X4}]\n#POS: 0x{fs.Position:X8}\n";
            WriteText(fsNew, info);
            XCoordsArray = new float[uvCount];
            YCoordsArray = new float[uvCount];

            LogVeritces(fs, fsNew, br, normOff, uvOff, 2);

            info = $"#MIN U/V: {XCoordsArray.Min()}, {YCoordsArray.Min()}\n";
            WriteText(fsNew, info);

            info = $"#MAX U/V: {XCoordsArray.Max()}, {YCoordsArray.Max()}\n";
            WriteText(fsNew, info);

            MeshPosX = (XCoordsArray.Max() + XCoordsArray.Min()) / 2;
            MeshPosY = (YCoordsArray.Max() + YCoordsArray.Min()) / 2;
            info = $"#(MAX + MIN)/2: {MeshPosX}, {MeshPosY}\n";
            WriteText(fsNew, info);

            if (DuplMesh)
                DuplMeshCopy = false;
        }
        public static void LogNormals(FileStream fs, FileStream fsNew, BinaryReader br, uint normOff, uint uvOff, uint MeshHDRStartOff, string info)
        {
            fs.Seek(normOff + MeshHDRStartOff, SeekOrigin.Begin); //go to normals offset
            info = $"#normCount: {normCount} [0x{normCount:X4}]\n#POS: 0x{fs.Position:X8}\n";
            WriteText(fsNew, info);

            XCoordsArray = new float[normCount];
            YCoordsArray = new float[normCount];
            ZCoordsArray = new float[normCount];

            x30_XNormCoordsArray = new float[normCount];
            x30_YNormCoordsArray = new float[normCount];
            x30_ZNormCoordsArray = new float[normCount];

            LogVeritces(fs, fsNew, br, normOff, uvOff, 1);

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
        public static void LogVeritces(FileStream fs, FileStream fsNew, BinaryReader br, uint normOff, uint uvOff, byte flag)
        {
            string vtxdata = "";
            if (flag == 1)
            {
                vtxCount = normCount;
            }
            else if (flag == 2)
            {
                vtxCount = uvCount;
            }

            if (DuplMesh)
            {
                if (!DuplMeshCopy || (flag == 1 || flag == 2))
                {
                    DuplMeshCopy = true;
                    CorrectionCount = Dictx30_MeshLevelForCorrectionCount[MeshLevelForCorrectionCount_Count];
                }
                else
                {
                    x30ArrayDuplPointIsolatedCount += 6;
                    DuplMeshCopy = false;
                }
            }
            else
            {
                CorrectionCount = DictXPointCorrectionMain.Count; //(MeshLevel)
            }

            for (int i = 0; i < vtxCount; i++) //Read and log vertices
            {
                if (flag != 2)
                {
                    Vec3VTX.X = ReadBigEndianFloat(br);
                    Vec3VTX.Y = ReadBigEndianFloat(br);
                    Vec3VTX.Z = ReadBigEndianFloat(br);

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

                    //Vec3VTX = MeshTransform(flag, Vec3VTX, CorrectionCount);

                }
                else
                {
                    Vec2UV.X = ReadBigEndianFloat(br);
                    Vec2UV.Y = ReadBigEndianFloat(br);

                    XCoordsArray[i] = Vec2UV.X;
                    YCoordsArray[i] = Vec2UV.Y;
                }

                if (flag == 0)
                {
                    vtxdata = $"v {Vec3VTX.X} {Vec3VTX.Y} {Vec3VTX.Z}\n";
                    WriteText(fsNew, vtxdata);
                }
                else if (flag == 1)
                {
                    vtxdata = $"vn {Vec3VTX.X} {Vec3VTX.Y} {Vec3VTX.Z}\n";
                    WriteText(fsNew, vtxdata);
                }
                else
                {
                    YCoordsArray[i] = 1.0F + (YCoordsArray[i] * -1.0F);
                    vtxdata = $"vt {XCoordsArray[i]} {YCoordsArray[i]}\n";
                    WriteText(fsNew, vtxdata);
                }

                if (ReplaceCommaWithDot)
                {
                    foreach (var c in charsToRemove)
                    {
                        vtxdata = vtxdata.Replace(c, ".");
                    }
                }

                /*Sometimes, the vertices and the normals alternate between each other.
                If this happens, then just skip 0x0C bytes (the 3 norm floats) */
                if ((flag != 2) && (normOff == 0x2C || normOff == 0x4C)) //EO1 + EO2
                {
                    fs.Seek(0x0C, SeekOrigin.Current);
                }
            }
        }
        /*
        public static bool IsQuaternionValid(float RotX, float RotY, float RotZ, float RotW)
        {
            double asdq = Math.Sqrt((RotX * RotX) + (RotY * RotY) + (RotZ * RotZ) + (RotW * RotW));
            if (Convert.ToInt32(asdq) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        */
        public static void ParseOBJListForDupl(FileStream fs, BinaryReader br)
        {
            long CurrPos = 0;
            fs.Seek(VDLOff, SeekOrigin.Begin);
            x30DuplCorrectionPointCont = 0;
            x30DuplRotCorrectionCont = 0;
            x30DuplMeshLevelCont = 1;

            if (ObjectListType >= 0x12F)
                CurrPos = VDLOff + 0x0F;
            else
                CurrPos = fs.Position;

            Dictx30_XDuplMeshIndex = new();
            Dictx30_MeshLevelForCorrectionCount = new();

            Dictx30_XPointCorrectionMain = new();
            Dictx30_YPointCorrectionMain = new();
            Dictx30_ZPointCorrectionMain = new();

            Dictx30_XRotCorrectionMain = new();
            Dictx30_YRotCorrectionMain = new();
            Dictx30_ZRotCorrectionMain = new();
            Dictx30_WRotCorrectionMain = new();

            Dictx30_XSizeCorrectionMain = new();
            Dictx30_YSizeCorrectionMain = new();
            Dictx30_ZSizeCorrectionMain = new();

            for (int i = 0; i < chunksCount; i++)
            {
                fs.Seek(CurrPos, SeekOrigin.Begin);
                DuplObjCode = (byte)(br.ReadByte() & 0xF0);
                if (DuplObjCode == 0x30)
                {
                    x30DuplMeshFound = true;
                    CurrPos = fs.Position + 0x3F;
                    DuplMeshLevel = br.ReadByte();
                    Dictx30_MeshLevelForCorrectionCount.Add(MeshLevelForCorrectionCount_Count, (ushort)(DuplMeshLevel + 1));
                    MeshLevelForCorrectionCount_Count += 1;
                    DuplTranspFlag = br.ReadByte();
                    Dictx30_XDuplMeshIndex.Add(x30DuplMeshIndexCont, ReverseUInt16(br.ReadUInt16()));

                    Dictx30_XPointCorrectionMain.Add(x30DuplCorrectionPointCont, ReadBigEndianFloat(br));
                    Dictx30_YPointCorrectionMain.Add(x30DuplCorrectionPointCont, ReadBigEndianFloat(br));
                    Dictx30_ZPointCorrectionMain.Add(x30DuplCorrectionPointCont, ReadBigEndianFloat(br));

                    Dictx30_XRotCorrectionMain.Add(x30DuplRotCorrectionCont, ReadBigEndianFloat(br));
                    Dictx30_YRotCorrectionMain.Add(x30DuplRotCorrectionCont, ReadBigEndianFloat(br));
                    Dictx30_ZRotCorrectionMain.Add(x30DuplRotCorrectionCont, ReadBigEndianFloat(br));
                    Dictx30_WRotCorrectionMain.Add(x30DuplRotCorrectionCont, ReadBigEndianFloat(br));

                    Dictx30_XSizeCorrectionMain.Add(x30DuplCorrectionPointCont, ReadBigEndianFloat(br));
                    Dictx30_YSizeCorrectionMain.Add(x30DuplCorrectionPointCont, ReadBigEndianFloat(br));
                    Dictx30_ZSizeCorrectionMain.Add(x30DuplCorrectionPointCont, ReadBigEndianFloat(br));

                    fs.Seek(-0x6D, SeekOrigin.Current);
                    x30DuplMeshIndexCont += 1;

                    do
                    {
                        byte DuplGmkObjCode = (byte)(br.ReadByte() & 0xF0);
                        byte DuplGmkMeshLevel = br.ReadByte();

                        if ((DuplGmkMeshLevel == DuplMeshLevel - x30DuplMeshLevelCont) && (DuplGmkObjCode == 0x10))
                        {
                            x30DuplMeshLevelCont += 1;
                            x30DuplCorrectionPointCont += 1;
                            x30DuplRotCorrectionCont += 1;
                            fs.Seek(0x03, SeekOrigin.Current);

                            Dictx30_XPointCorrectionMain.Add(x30DuplCorrectionPointCont, ReadBigEndianFloat(br));
                            Dictx30_YPointCorrectionMain.Add(x30DuplCorrectionPointCont, ReadBigEndianFloat(br));
                            Dictx30_ZPointCorrectionMain.Add(x30DuplCorrectionPointCont, ReadBigEndianFloat(br));

                            Dictx30_XRotCorrectionMain.Add(x30DuplRotCorrectionCont, ReadBigEndianFloat(br));
                            Dictx30_YRotCorrectionMain.Add(x30DuplRotCorrectionCont, ReadBigEndianFloat(br));
                            Dictx30_ZRotCorrectionMain.Add(x30DuplRotCorrectionCont, ReadBigEndianFloat(br));
                            Dictx30_WRotCorrectionMain.Add(x30DuplRotCorrectionCont, ReadBigEndianFloat(br));

                            Dictx30_XSizeCorrectionMain.Add(x30DuplCorrectionPointCont, ReadBigEndianFloat(br));
                            Dictx30_YSizeCorrectionMain.Add(x30DuplCorrectionPointCont, ReadBigEndianFloat(br));
                            Dictx30_ZSizeCorrectionMain.Add(x30DuplCorrectionPointCont, ReadBigEndianFloat(br));

                            if (DuplGmkMeshLevel == 0)
                            {
                                x30DuplCorrectionPointCont += 1;
                                x30DuplRotCorrectionCont += 1;
                                x30DuplMeshLevelCont = 1;
                                break;
                            }
                            else
                            {
                                fs.Seek(-0x6D, SeekOrigin.Current);
                            }
                        }
                        else
                        {
                            fs.Seek(-0x42, SeekOrigin.Current);
                        }
                    }
                    while (true);
                }
                /*
                else if (DuplObjCode == 0x50)
                {
                    fs.Seek(0x02, SeekOrigin.Current);
                    DictCloneMeshIndex.Add(ArrayCloneGlobalCont, ReverseUInt16(br.ReadUInt16()));

                    DictCloneCorrectionPoint.Add(CloneCorrectionPointCont, ReadBigEndianFloat(br));
                    DictCloneCorrectionPoint.Add((ushort)(CloneCorrectionPointCont + 1), ReadBigEndianFloat(br));
                    DictCloneCorrectionPoint.Add((ushort)(CloneCorrectionPointCont + 2), ReadBigEndianFloat(br));

                    DictCloneRotCorrection.Add(CloneRotCorrectionCont, ReadBigEndianFloat(br));
                    DictCloneRotCorrection.Add((ushort)(CloneRotCorrectionCont + 1), ReadBigEndianFloat(br));
                    DictCloneRotCorrection.Add((ushort)(CloneRotCorrectionCont + 2), ReadBigEndianFloat(br));
                    DictCloneRotCorrection.Add((ushort)(CloneRotCorrectionCont + 3), ReadBigEndianFloat(br));

                    DictCloneSizeCorrection.Add(CloneCorrectionPointCont, ReadBigEndianFloat(br));
                    DictCloneSizeCorrection.Add((ushort)(CloneCorrectionPointCont + 1), ReadBigEndianFloat(br));
                    DictCloneSizeCorrection.Add((ushort)(CloneCorrectionPointCont + 2), ReadBigEndianFloat(br));

                    DictCloneMeshName.Add(ArrayCloneGlobalCont, ReadNullTerminatedString(br));
                    CurrPos = (uint)fs.Seek(0x0F - DictCloneMeshName[ArrayCloneGlobalCont].Length + 0x03, SeekOrigin.Current);

                    CloneCorrectionPointCont += 3;
                    CloneRotCorrectionCont += 4;
                    ArrayCloneGlobalCont += 1;
                }
                */
                else
                {
                    CurrPos += 0x40;
                }
            }
        }

        public static void ParseOBJList(FileStream fs, BinaryReader br)
        {
            if ((MeshProgressCount == 1 && ObjectOff == 0) || ObjectOff == 0)
            {
                fs.Seek(VDLOff, SeekOrigin.Begin);
                if (RF2MD3 && ObjectListType >= 0x12F)
                {
                    chunksCount = ReverseUInt16(br.ReadUInt16());
                    fs.Seek(0x0A, SeekOrigin.Current);
                }
            }
            else if (MeshProgressCount != 1 && ObjectOff != 0)
            {
                fs.Seek(ObjectOff, SeekOrigin.Begin);
            }

            DictXPointCorrectionMain = new();
            DictYPointCorrectionMain = new();
            DictZPointCorrectionMain = new();

            DictXRotCorrectionMain = new();
            DictYRotCorrectionMain = new();
            DictZRotCorrectionMain = new();
            DictWRotCorrectionMain = new();

            DictXSizeCorrectionMain = new();
            DictYSizeCorrectionMain = new();
            DictZSizeCorrectionMain = new();

            for (int ObjCount = 0; ObjCount < chunksCount + 1; ObjCount++)
            {
                ReadFlag = br.ReadByte();
                fs.Seek(0x01, SeekOrigin.Current);
                ReadFlagForEight = br.ReadByte();
                ObjCode = (byte)(br.ReadByte() & 0xF0);
                MeshLevel = br.ReadByte();
                TransparencyFlag = br.ReadByte();
                MeshIndex = ReverseUInt16(br.ReadUInt16());
                int DictVec3PointCorrectionMainCount = 0;
                int DictVec3RotCorrectionMainCount = 0;

                if (MeshLevel <= MeshLevelForEight)
                {
                    MeshLevelForEight = 0;
                }

                if (ObjCode == 0x20 && MeshIndex == MeshProgressCount - 1)
                {
                    long tmpReal = fs.Position - 0x48;

                    CurrentChunk = GetCurrentChunk(RF2MD3, ObjectListType, fs);

                    DictXPointCorrectionMain.Add(DictVec3PointCorrectionMainCount, ReadBigEndianFloat(br));
                    DictYPointCorrectionMain.Add(DictVec3PointCorrectionMainCount, ReadBigEndianFloat(br));
                    DictZPointCorrectionMain.Add(DictVec3PointCorrectionMainCount, ReadBigEndianFloat(br));

                    DictXRotCorrectionMain.Add(DictVec3RotCorrectionMainCount, ReadBigEndianFloat(br));
                    DictYRotCorrectionMain.Add(DictVec3RotCorrectionMainCount, ReadBigEndianFloat(br));
                    DictZRotCorrectionMain.Add(DictVec3RotCorrectionMainCount, ReadBigEndianFloat(br));
                    DictWRotCorrectionMain.Add(DictVec3RotCorrectionMainCount, ReadBigEndianFloat(br));

                    DictXSizeCorrectionMain.Add(DictVec3PointCorrectionMainCount, ReadBigEndianFloat(br));
                    DictYSizeCorrectionMain.Add(DictVec3PointCorrectionMainCount, ReadBigEndianFloat(br));
                    DictZSizeCorrectionMain.Add(DictVec3PointCorrectionMainCount, ReadBigEndianFloat(br));

                    MeshName = ReadNullTerminatedString(br);
                    ObjectOff = (uint)fs.Seek(0x0F - MeshName.Length, SeekOrigin.Current);
                    if ( ObjectListType >= 0x12F && (Dictx30_XDuplMeshIndex[x30DuplMeshIndexCont] == CurrentChunk - 1 && Dictx30_XDuplMeshIndex[x30DuplMeshIndexCont] != 0 || MeshLevel > MeshLevelForEight && MeshLevelForEight != 0x00))
                    {
                        DuplMesh = true;
                        if (MeshLevel >= MeshLevelForEight + 2)
                            InsideMeshForEight = true;
                        else
                        {
                            if (InsideMeshForEight == true)
                            {
                                if (Dictx31_XPointCorrectionMain.ContainsKey(0))
                                {
                                    Dictx31_XPointCorrectionMain.Remove(0);
                                    Dictx31_YPointCorrectionMain.Remove(0);
                                    Dictx31_ZPointCorrectionMain.Remove(0);

                                    Dictx31_XRotCorrectionMain.Remove(0);
                                    Dictx31_YRotCorrectionMain.Remove(0);
                                    Dictx31_ZRotCorrectionMain.Remove(0);
                                    Dictx31_WRotCorrectionMain.Remove(0);

                                    Dictx31_XSizeCorrectionMain.Remove(0);
                                    Dictx31_YSizeCorrectionMain.Remove(0);
                                    Dictx31_ZSizeCorrectionMain.Remove(0);
                                }
                            }
                            InsideMeshForEight = false;
                            if (DuplMesh && !InsideMeshForEight && Dictx31_XPointCorrectionMain.ContainsKey(0))
                            {
                                Dictx31_XPointCorrectionMain.Remove(0);
                                Dictx31_YPointCorrectionMain.Remove(0);
                                Dictx31_ZPointCorrectionMain.Remove(0);

                                Dictx31_XRotCorrectionMain.Remove(0);
                                Dictx31_YRotCorrectionMain.Remove(0);
                                Dictx31_ZRotCorrectionMain.Remove(0);
                                Dictx31_WRotCorrectionMain.Remove(0);

                                Dictx31_XSizeCorrectionMain.Remove(0);
                                Dictx31_YSizeCorrectionMain.Remove(0);
                                Dictx31_ZSizeCorrectionMain.Remove(0);
                            }
                            if (MeshLevel == MeshLevelForEight + 1 && CurrentChunk > x30CurrentChunk + 1) //o > x30CurrentChunk
                            {
                                for (int asda = 0; asda < Dictx30_XDuplMeshIndex.Count; asda++)
                                {
                                    if (Dictx30_XDuplMeshIndex[(ushort)asda] == x30CurrentChunk)
                                    {
                                        for (int asd = 0; asd <= asda; asd++)
                                        {
                                            MeshLevelForCorrectionCount_Count = (ushort)asd;
                                        }
                                        PreviousCorrectionCount = (ushort)asda;
                                        break;
                                    }
                                }
                            }
                        }
                        if (InsideMeshForEight)
                        {
                            PreviousCorrectionCount = 0;
                            for (int asda = 0; asda < Dictx30_XDuplMeshIndex.Count; asda++)
                            {
                                if (Dictx30_XDuplMeshIndex[(ushort)asda] == x30CurrentChunk)
                                {
                                    for (int asd = 0; asd <= asda; asd++)
                                    {
                                        MeshLevelForCorrectionCount_Count = (ushort)asd;
                                        PreviousCorrectionCount += Dictx30_MeshLevelForCorrectionCount[(ushort)asd];
                                    }
                                    PreviousCorrectionCount -= Dictx30_MeshLevelForCorrectionCount[(ushort)asda];
                                    break;
                                }
                            }
                        }
                    }
                    else //Scan for the previous level
                    {
                        DuplMesh = false;
                        InsideMeshForEight = false;
                        fs.Seek(tmpReal, SeekOrigin.Begin);
                        for (int ObjInternal = 0; ObjInternal < CurrentChunk; ObjInternal++)
                        {
                            ObjReadFlag = ReverseUInt16(br.ReadUInt16());
                            byte ObjCodeTmp = br.ReadByte();
                            byte ObjCodeGmkTmp = (byte)(br.ReadByte() & 0xF0);
                            MeshLevelGmk = br.ReadByte();
                            fs.Seek(0x03, SeekOrigin.Current);
                            if (MeshLevelGmk == (MeshLevel - MeshLevelCont) && (ObjCodeTmp == 0x00 || ObjCodeTmp == 0x08 || ObjCodeTmp == 0xFF))
                            {
                                DictXPointCorrectionMain.Add(DictVec3PointCorrectionMainCount + 1, ReadBigEndianFloat(br));
                                DictYPointCorrectionMain.Add(DictVec3PointCorrectionMainCount + 1, ReadBigEndianFloat(br));
                                DictZPointCorrectionMain.Add(DictVec3PointCorrectionMainCount + 1, ReadBigEndianFloat(br));

                                DictXRotCorrectionMain.Add(DictVec3RotCorrectionMainCount + 1, ReadBigEndianFloat(br));
                                DictYRotCorrectionMain.Add(DictVec3RotCorrectionMainCount + 1, ReadBigEndianFloat(br));
                                DictZRotCorrectionMain.Add(DictVec3RotCorrectionMainCount + 1, ReadBigEndianFloat(br));
                                DictWRotCorrectionMain.Add(DictVec3RotCorrectionMainCount + 1, ReadBigEndianFloat(br));

                                DictXSizeCorrectionMain.Add(DictVec3PointCorrectionMainCount + 1, ReadBigEndianFloat(br));
                                DictYSizeCorrectionMain.Add(DictVec3PointCorrectionMainCount + 1, ReadBigEndianFloat(br));
                                DictZSizeCorrectionMain.Add(DictVec3PointCorrectionMainCount + 1, ReadBigEndianFloat(br));

                                MeshLevelCont += 1;
                                DictVec3PointCorrectionMainCount += 1;
                                DictVec3RotCorrectionMainCount += 1;

                                if (MeshLevelGmk == 0)
                                {
                                    break;
                                }
                                else
                                {
                                    fs.Seek(-0x70, SeekOrigin.Current);
                                }
                            }
                            else
                            {
                                fs.Seek(-0x48, SeekOrigin.Current);
                            }

                        }
                    }
                    break;
                }
                else if (ReadFlagForEight == 0x08) //Prepare for Duplication
                {
                    if (DuplMesh && DuplicatedMeshes > 0)
                    {
                        MeshLevelForCorrectionCount_Count += 1;
                    }

                    CurrentChunk = GetCurrentChunk(RF2MD3, ObjectListType, fs);

                    x30CurrentChunk = CurrentChunk;
                    InsideMeshForEight = false;
                    MeshLevelForEight = MeshLevel;
                    fs.Seek(0x38, SeekOrigin.Current);

                    if (Dictx31_XPointCorrectionMain.ContainsKey(0))
                    {
                        Dictx31_XPointCorrectionMain.Remove(0);
                        Dictx31_YPointCorrectionMain.Remove(0);
                        Dictx31_ZPointCorrectionMain.Remove(0);

                        Dictx31_XRotCorrectionMain.Remove(0);
                        Dictx31_YRotCorrectionMain.Remove(0);
                        Dictx31_ZRotCorrectionMain.Remove(0);
                        Dictx31_WRotCorrectionMain.Remove(0);

                        Dictx31_XSizeCorrectionMain.Remove(0);
                        Dictx31_YSizeCorrectionMain.Remove(0);
                        Dictx31_ZSizeCorrectionMain.Remove(0);
                    }
                    PreviousCorrectionCount = 0;
                    MeshLevelForCorrectionCount_Count = 0;
                    PreviousCorrectionCount = 0;

                    for (int asda = 0; asda < Dictx30_XDuplMeshIndex.Count; asda++)
                    {
                        if (Dictx30_XDuplMeshIndex[(ushort)asda] == x30CurrentChunk)
                        {
                            for (int asd = 0; asd < asda; asd++)
                            {
                                PreviousCorrectionCount += Dictx30_MeshLevelForCorrectionCount[(ushort)asd];
                            }
                            break;
                        }
                    }
                    x30DuplMeshIndexCont += 1;
                }
                else //nothing found
                {
                    fs.Seek(0x38, SeekOrigin.Current);
                }
            }
        }

        public static float ReadBigEndianFloat(BinaryReader br)
        {
            byte[] FloatArray = new byte[4] { br.ReadByte(), br.ReadByte(), br.ReadByte(), br.ReadByte() };
            Array.Reverse(FloatArray);
            float result = BitConverter.ToSingle(FloatArray);
            return result;
        }
        public static UInt16 ReverseUInt16(UInt16 value) //Big Endian Unsigned Short 2 Bytes
        {
            return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }
        public static UInt32 ReverseUInt32(UInt32 value) //Big Endian Unsigned Int 4 bytes
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        public static void WriteText(FileStream fsLog, string text)
        {
            byte[] textbytes = Encoding.UTF8.GetBytes(text);
            fsLog.Write(textbytes, 0, textbytes.Length);
            fsLog.Flush();
        }

        public static string ReadNullTerminatedString(BinaryReader stream)
        {
            string str = "";
            char ch;
            while ((ch = stream.ReadChar()) != 0)
                str += ch;
            return str;
        }

        public static void PrintCenter(string text)
        {
            Console.WriteLine(string.Format("{0," + ((Console.WindowWidth / 2) + (text.Length / 2)) + "}", text));
        }

        public static void PrintInfo()
        {
            PrintCenter("Endless Ocean 1 & 2 .mdl to .obj Exporter\n");
            PrintCenter("Author: NiV\n");
            PrintCenter("Special thanks to Hiroshi & MDB\n");
            PrintCenter("Version 1.6\n");;
            PrintCenter("If you have any issues, join this discord server and contact NiV-L-A:\n");
            PrintCenter("https://discord.gg/4hmcsmPMDG\n");
        }

        public static bool vtxCountShort = false;
        public static bool normCountShort = false;
        public static bool lightCountShort = false;
        public static bool uvCountShort = false;
        public static bool unk01CountShort = false;
        public static bool RF2MD2 = false;
        public static bool RF2MD3 = false;
        public static bool RFPMD2 = false;
        public static bool ReplaceCommaWithDot = false;
        public static bool x30DuplMeshFound = false;
        public static bool InsideMeshForEight = false;
        public static bool DuplMesh = false;
        public static bool DuplMeshCopy = false;
        public static byte indexCount = 0;
        public static byte unkIsOneOrTwo = 0;
        public static byte ObjCodeRefFlag = 0;
        public static byte MeshLevel = 0;
        public static byte TransparencyFlag = 0;
        public static byte MeshLevelGmk = 0;
        public static byte MeshLevelRef = 0;
        public static byte TransparencyFlagRef = 0;
        public static byte DuplObjCode = 0;
        public static byte DuplMeshLevel = 0;
        public static byte DuplTranspFlag = 0;
        public static byte ReadFlag = 0;
        public static byte ReadFlagForEight = 0;
        public static byte MeshLevelForEight = 0;
        public static byte MeshIDType3BF = 0;
        public static byte MeshIDType2 = 0;
        public static short MeshLevelCont = 1;
        public static short x30ArrayDuplPointIsolatedCount = 0;
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
        public static ushort MeshIndex = 0;
        public static ushort ObjCode = 0;
        public static ushort chunksCount = 0;
        public static ushort MeshLevelCont2 = 0;
        public static ushort ObjReadFlag = 0;
        public static ushort MeshTotalCount = 0;
        public static ushort x30DuplMeshIndexCont = 0;
        public static ushort x30DuplCorrectionPointCont = 0;
        public static ushort x30DuplRotCorrectionCont = 0;
        public static ushort x30DuplMeshLevelCont = 0;
        public static ushort x30CurrentChunk = 0;
        public static ushort MeshLevelForCorrectionCount_Count = 0;
        public static ushort CurrentChunk = 0;
        public static ushort ArrayCloneGlobalCont = 0;
        public static ushort CloneCorrectionPointCont = 0;
        public static ushort CloneRotCorrectionCont = 0;
        public static ushort DuplCount = 0;
        public static ushort ObjectListType = 0x12F;
        public static ushort DuplicatedMeshes = 0;
        public static int ogPointArrayFlagCont = 0;
        public static int ogPointArrayIndexCont = 0;
        public static int ended = 0;
        public static int MeshProgressCount = 0x00000000;
        public static int printinfo = 1;
        public static int printvtx = 1;
        public static int printnorm = 1;
        public static int printuv = 1;
        public static int printind = 1;
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
        public static int CorrectionCount = 0;
        public static int PreviousCorrectionCount = 0;
        public static int da = 0;
        public static int Backup_PreviousCorrectionCount = 0;
        public static int Texturecount = 0;
        public static uint narg = 0;
        public static uint VDLOff = 0x00;
        public static uint indOff = 0x00;
        public static uint vtxOff = 0;
        public static uint MatIndicesOff = 0;
        public static uint MatIndicesInfoOff = 0;
        public static uint ObjectOff = 0;
        public static uint StripWithEqualFaceCount = 0;
        public static uint lightOff = 0;
        public static uint x30IndexCount = 0;
        public static long cntCountAll = 0;
        public static long aCountAll = 0;
        public static long TMP2 = 0;
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
        public static string MeshName = "";
        public static string NewPath = "";
        public static ushort[] ArrayMeshIndexRef = null;
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
        public static float[] ArrayCloneCorrectionPoint = null;
        public static float[] ArrayCloneRotCorrection = null;
        public static float[] ArrayCloneSizeCorrection = null;
        public static float[] ArrayVec3PointCorrectionMain = null;
        public static float[] ArrayVec3RotCorrectionMain = null;
        public static float[] ArrayVec3SizeCorrectionMain = null;
        public static string[] charsToRemove = new string[] { "," };
        public static string[] ogPointMeshNameArray = null;
        public static string[] tdlNameArray = null;
        public static Quaternion Quat;
        public static Vector3 Vec3VTX;
        public static Vector2 Vec2UV;
        public static Dictionary<ushort, ushort> Dictx30_XDuplMeshIndex;
        public static Dictionary<ushort, ushort> Dictx30_MeshLevelForCorrectionCount;
        public static Dictionary<ushort, ushort> DictCloneMeshIndex = new();
        public static Dictionary<ushort, string> DictCloneMeshName = new();
        public static Dictionary<int, float> Dictx30_XPointCorrectionMain;
        public static Dictionary<int, float> Dictx30_YPointCorrectionMain;
        public static Dictionary<int, float> Dictx30_ZPointCorrectionMain;
        public static Dictionary<int, float> Dictx30_XRotCorrectionMain;
        public static Dictionary<int, float> Dictx30_YRotCorrectionMain;
        public static Dictionary<int, float> Dictx30_ZRotCorrectionMain;
        public static Dictionary<int, float> Dictx30_WRotCorrectionMain;
        public static Dictionary<int, float> Dictx30_XSizeCorrectionMain;
        public static Dictionary<int, float> Dictx30_YSizeCorrectionMain;
        public static Dictionary<int, float> Dictx30_ZSizeCorrectionMain;
        public static Dictionary<int, float> Dictx31_XPointCorrectionMain = new();
        public static Dictionary<int, float> Dictx31_YPointCorrectionMain = new();
        public static Dictionary<int, float> Dictx31_ZPointCorrectionMain = new();
        public static Dictionary<int, float> Dictx31_XRotCorrectionMain = new();
        public static Dictionary<int, float> Dictx31_YRotCorrectionMain = new();
        public static Dictionary<int, float> Dictx31_ZRotCorrectionMain = new();
        public static Dictionary<int, float> Dictx31_WRotCorrectionMain = new();
        public static Dictionary<int, float> Dictx31_XSizeCorrectionMain = new();
        public static Dictionary<int, float> Dictx31_YSizeCorrectionMain = new();
        public static Dictionary<int, float> Dictx31_ZSizeCorrectionMain = new();
        public static Dictionary<int, float> DictXPointCorrectionMain;
        public static Dictionary<int, float> DictYPointCorrectionMain;
        public static Dictionary<int, float> DictZPointCorrectionMain;
        public static Dictionary<int, float> DictXRotCorrectionMain;
        public static Dictionary<int, float> DictYRotCorrectionMain;
        public static Dictionary<int, float> DictZRotCorrectionMain;
        public static Dictionary<int, float> DictWRotCorrectionMain;
        public static Dictionary<int, float> DictXSizeCorrectionMain;
        public static Dictionary<int, float> DictYSizeCorrectionMain;
        public static Dictionary<int, float> DictZSizeCorrectionMain;
        public static Dictionary<ushort, float> DictCloneCorrectionPoint = new();
        public static Dictionary<ushort, float> DictCloneRotCorrection = new();
        public static Dictionary<ushort, float> DictCloneSizeCorrection = new();

        public static Stopwatch stopWatch = new();

        static void Main(string[] args)
        {
            Console.Title = "Endless Ocean 1 & 2 .mdl to .obj Exporter";
            if (args.Length == 0) //if no args are passed to the .exe
            {
                PrintInfo();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("To use this tool, drag and drop a .mdl file onto the .exe!");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Read();
                return;
            }
            else
            {
                int narg = 0;
                stopWatch.Start();
                foreach (string arg in args) //damn boi, at least one arg, let's see what the script can do
                {
                    MeshProgressCount = 0x00;
                    ObjectOff = 0;
                    narg += 1;
                    FileStream fs;
                    BinaryReader br;
                    string pathReal = Path.GetDirectoryName(arg) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(arg) + Path.GetExtension(arg);
                    if (Path.GetExtension(arg) == ".bmp")
                    {
                        foreach (string argq in args)
                        {
                            Console.WriteLine($"{Texturecount + 1} | {Path.GetFileNameWithoutExtension(argq) + Path.GetExtension(argq)}");
                            Texturecount += 1;
                            pathReal = Path.GetDirectoryName(argq) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(argq) + Path.GetExtension(argq);
                            Bitmap bmp1 = new(pathReal);
                            bmp1.Save(Path.GetDirectoryName(argq) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(argq) + ".png", ImageFormat.Png);
                        }
                        break;
                    }
                    try
                    {
                        fs = new FileStream(pathReal, FileMode.Open);
                        br = new BinaryReader(fs);
                    }
                    catch (IOException ex)
                    {
                        PrintInfo();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: {ex.Message}");
                        fs = null;
                        br = null;
                        Console.WriteLine("Press any key to close the window");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Read();
                        Environment.Exit(0);
                    }
                    catch (UnauthorizedAccessException uax)
                    {
                        PrintInfo();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: {uax.Message}");
                        fs = null;
                        br = null;
                        Console.WriteLine("Consider moving the .mdl file in another folder!");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("Press any key to close the window");
                        Console.Read();
                        Environment.Exit(0);
                    }

                    //***************************************************
                    //******* Magic detection and basic data read ******* JUST ONCE
                    //***************************************************

                    fs.Seek(0x00, SeekOrigin.Begin); //The magic is composed of a total of 6 bytes.
                    uint Magic = br.ReadUInt32(); //First 4, can be "RF2M" or "RFPM", RFPM is a special type used rarely.
                    ushort Magic2 = br.ReadUInt16(); //Last 2, can be "D2" or "D3" - prob indcating the version
                    if (Magic != 0x4D324652 && Magic != 0x4D504652) //RF2M or RFPM
                    {
                        PrintInfo();
                        fs = null;
                        br = null;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error: RF2M Magic is missing! Are you loading the correct file?");
                        Console.WriteLine($"Magic: 0x{ReverseUInt32(Magic):X8} - 0x{ReverseUInt16(Magic2):X8} | {Encoding.Default.GetString(BitConverter.GetBytes(Magic))} - {Encoding.Default.GetString(BitConverter.GetBytes(Magic2))}\n");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("Press any key to close the window");
                        Console.Read();
                        Environment.Exit(0);
                    }
                    //No need to assign the other bools to false. They will be false anyway.
                    if (Magic == 0x4D504652) //RFPM
                    {
                        RFPMD2 = true;
                    }
                    if (Magic != 0x4D504652 && Magic2 == 0x3244) //D2
                    {
                        RF2MD2 = true;
                        ObjectListType = 0x12E;
                    }
                    else if (Magic2 == 0x3344) //D3
                    {
                        RF2MD3 = true;
                        ObjectListType = 0x130;
                    }
                    else if (Magic2 != 0x3244 && Magic2 != 0x3344) //No type of Magic2 recognized, most likely wrong file as input
                    {
                        PrintInfo();
                        fs = null;
                        br = null;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error: D2/D3 Magic is missing! Are you loading the correct file?");
                        Console.WriteLine($"Magic: 0x{ReverseUInt32(Magic):X8} - 0x{ReverseUInt16(Magic2):X8} | {Encoding.Default.GetString(BitConverter.GetBytes(Magic))} - {Encoding.Default.GetString(BitConverter.GetBytes(Magic2))}\n");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine("Press any key to close the window");
                        Console.Read();
                        Environment.Exit(0);
                    }
                    ushort Entries = br.ReadUInt16();
                    ushort EntryList_SIZE = br.ReadUInt16();
                    fs.Seek(0x02, SeekOrigin.Current); //Skip DUMMY
                    uint HEAD_SIZE = br.ReadUInt32();
                    if (RFPMD2) //EO Special Format
                    {
                        fs.Seek(0x10, SeekOrigin.Current);
                    }
                    else //EO1 & EO2
                    {
                        fs.Seek(0x18, SeekOrigin.Current);
                    }
                    VDLOff = br.ReadUInt32(); //Offset of the .vdl

                    fs.Seek(EntryList_SIZE, SeekOrigin.Begin);
                    if (RF2MD2 || RFPMD2) //EO1
                    {
                        fs.Seek(0x04, SeekOrigin.Current);
                        chunksCount = br.ReadUInt16();
                        TDLFilesRefCount = br.ReadUInt16();
                        RF2MD2MatCount = br.ReadUInt16();
                    }
                    else if (RF2MD3) //EO2
                    {
                        fs.Seek(0x02, SeekOrigin.Current);
                        ObjectListType = br.ReadUInt16();
                        chunksCount = br.ReadUInt16();
                        TDLFilesRefCount = br.ReadUInt16();
                        fs.Seek(0x02, SeekOrigin.Current);
                        MatIndicesOffCount = br.ReadUInt16();
                        MatIndicesCount = br.ReadUInt16();
                    }

                    ushort MeshCount = br.ReadUInt16(); //Mesh count at +0x0E or both +0x0E and +0x10

                    if (RF2MD2 || RFPMD2)
                    {
                        fs.Seek(0x04, SeekOrigin.Current);
                        MatIndicesOff = br.ReadUInt32();
                    }
                    else if (RF2MD3)
                    {
                        fs.Seek(0x08, SeekOrigin.Current);
                        MatIndicesInfoOff = br.ReadUInt32();
                        MatIndicesOff = br.ReadUInt32();
                    }

                    MeshTotalCount += MeshCount;
                    long MESH_INFO_Offset = fs.Position;

                    //***********************************************************************************
                    //******* GET TDL NAMES FROM ENTRY LIST, CREATE & POPULATE MATERIAL.MTL FILE ********
                    //***********************************************************************************

                    GetTDLNames(fs, br, Entries);

                    string MatPath = $"{Path.GetDirectoryName(arg)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(arg)}.mtl";
                    File.Delete(MatPath); //delete file if it exists already
                    using FileStream fsMat = File.OpenWrite(MatPath);

                    for (ushort MatHeaderIndex = 0; MatHeaderIndex < TDLFilesRefCount; MatHeaderIndex++)
                    {
                        WriteText(fsMat, $"newmtl material{MatHeaderIndex}\nmap_Kd {tdlNameArray[MatHeaderIndex]}\n\n");
                    }

                    //**********************************************
                    //************** GET MATERIALS ID **************
                    //**********************************************

                    if (RF2MD3)
                    {
                        MatIndices = new ushort[MatIndicesCount];
                        fs.Seek(MatIndicesInfoOff, SeekOrigin.Begin);
                        for (int ContatoreMatIndicesCount = 0; ContatoreMatIndicesCount < MatIndicesOffCount; ContatoreMatIndicesCount++)
                        {
                            uint OffToMatIndex = ReverseUInt32(br.ReadUInt32());
                            byte FlagNumberOfInts = br.ReadByte();
                            fs.Seek(0x07, SeekOrigin.Current);
                            uint TMPOffMat = (uint)fs.Position;
                            fs.Seek(OffToMatIndex, SeekOrigin.Begin);
                            if (FlagNumberOfInts == 2) //work-around.
                                fs.Seek(0x04, SeekOrigin.Current);
                            MatIndices[ContatoreMatIndicesCount] = ReverseUInt16(br.ReadUInt16());
                            fs.Seek(TMPOffMat, SeekOrigin.Begin);
                        }
                    }
                    else
                    {
                        MatIndices = new ushort[RF2MD2MatCount];
                        fs.Seek(MatIndicesOff, SeekOrigin.Begin);
                        for (int ContatoreMatIndicesCount = 0; ContatoreMatIndicesCount < RF2MD2MatCount; ContatoreMatIndicesCount++)
                        {
                            MatIndices[ContatoreMatIndicesCount] = ReverseUInt16(br.ReadUInt16());
                            fs.Seek(0x0A, SeekOrigin.Current);
                        }
                    }

                    Console.WriteLine($"MeshCount: 0x{MeshCount:X8} ({MeshCount})\n");

                    ParseOBJListForDupl(fs, br);
                    x30DuplMeshIndexCont = 0;
                    x30DuplCorrectionPointCont = 0;
                    x30DuplRotCorrectionCont = 0;
                    MeshLevelForCorrectionCount_Count = 0;

                    if (!x30DuplMeshFound)
                        Dictx30_XDuplMeshIndex.Add(0, 0); //Work-around when no 0x30 codes are found.

                    ushort[] numbers = Dictx30_XDuplMeshIndex.Values.ToArray();
                    Dictionary<ushort, ushort> Dictx30_Countx30Codes = new();

                    foreach (var number in numbers)
                    {
                        if (Dictx30_Countx30Codes.ContainsKey(number))
                            Dictx30_Countx30Codes[number]++;
                        else
                            Dictx30_Countx30Codes.Add(number, 1);
                    }

                    //**********************************************
                    //************* GET MESH INFO DATA ************* FOR EACH MESH
                    //**********************************************
                    for (int m = 0; m < MeshCount; m++) //Get INFO mesh data. It will go here everytime it finishes parsing the indices.
                    {
                        if (DuplMesh)
                        {
                            int BackupTesta = 1;
                            if (InsideMeshForEight)
                            {
                                CurrentChunk = (ushort)(Dictx30_XDuplMeshIndex[x30DuplMeshIndexCont] + 1);
                            }
                            for (int testa = 1; testa < Dictx30_Countx30Codes[(ushort)(x30CurrentChunk)]; testa++)
                            {
                                if (x30DuplMeshIndexCont != 0xFFFF)
                                {
                                    MeshLevelForCorrectionCount_Count = 0;
                                    Console.WriteLine($"[Duplicating Mesh: {testa}/{Dictx30_Countx30Codes[(ushort)(x30CurrentChunk)] - 1}]");
                                    bool FoundFirst = false;
                                    Backup_PreviousCorrectionCount = PreviousCorrectionCount;
                                    PreviousCorrectionCount = 0;
                                    for (int asda = 0; asda < Dictx30_XDuplMeshIndex.Count; asda++)
                                    {
                                        if (FoundFirst)
                                        {
                                            if (Dictx30_XDuplMeshIndex[(ushort)asda] == x30CurrentChunk)
                                            {
                                                for (int asd = 0; asd < asda; asd++)
                                                {
                                                    PreviousCorrectionCount += Dictx30_MeshLevelForCorrectionCount[(ushort)asd];
                                                    MeshLevelForCorrectionCount_Count += 1;
                                                }
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (Dictx30_XDuplMeshIndex[(ushort)asda] == x30CurrentChunk && testa == BackupTesta)
                                            {
                                                FoundFirst = true;
                                            }
                                            else if (Dictx30_XDuplMeshIndex[(ushort)asda] == x30CurrentChunk && BackupTesta < testa)
                                            {
                                                BackupTesta += 1;
                                            }
                                        }
                                    }
                                }
                                CorrectionCount = Dictx30_MeshLevelForCorrectionCount[MeshLevelForCorrectionCount_Count];
                                BackupTesta = 1;
                                DuplicatedMeshes += 1;
                                string x30NewPath = "";
                                string line = "";
                                x30NewPath = $"{NewPath.Substring(0, NewPath.Length - 4)}_x30_{testa}.obj";
                                if (File.Exists(x30NewPath))
                                    File.Delete(x30NewPath);
                                File.Copy(NewPath, x30NewPath);
                                string[] Arrayx30File = File.ReadAllLines(x30NewPath);
                                uint x30vtxInternalCount = 0;
                                uint x30normInternalCount = 0;
                                using FileStream fsx30 = File.OpenWrite(x30NewPath);
                                for (int i = 0; i < Arrayx30File.Length; i++)
                                {
                                    line = Arrayx30File[i].Substring(0, 2);
                                    if (line == "v ")
                                    {
                                        Vec3VTX.X = x30_XCoordsArray[x30vtxInternalCount];
                                        Vec3VTX.Y = x30_YCoordsArray[x30vtxInternalCount];
                                        Vec3VTX.Z = x30_ZCoordsArray[x30vtxInternalCount];

                                        Vec3VTX = MeshTransform(0, Vec3VTX, CorrectionCount);

                                        string vtxdata = $"v {Vec3VTX.X} {Vec3VTX.Y} {Vec3VTX.Z}\n";
                                        WriteText(fsx30, vtxdata);
                                        x30vtxInternalCount += 1;
                                    }
                                    else if (line == "vn")
                                    {
                                        Vec3VTX.X = x30_XNormCoordsArray[x30normInternalCount];
                                        Vec3VTX.Y = x30_YNormCoordsArray[x30normInternalCount];
                                        Vec3VTX.Z = x30_ZNormCoordsArray[x30normInternalCount];

                                        Vec3VTX = MeshTransform(1, Vec3VTX, CorrectionCount);

                                        string vtxdata = $"vn {Vec3VTX.X} {Vec3VTX.Y} {Vec3VTX.Z}\n";
                                        WriteText(fsx30, vtxdata);
                                        x30normInternalCount += 1;
                                    }
                                    else //Everything else: comments, uvs, faces
                                    {
                                        WriteText(fsx30, Arrayx30File[i] + "\n");
                                    }
                                }
                            }
                            MeshLevelForCorrectionCount_Count = 0;
                            Console.Write("\n");
                        }
                        else
                        {
                            NewPath = "";
                        }
                        MeshProgressCount = m + 1;
                        Console.Title = $"Endless Ocean 1 & 2 .mdl to .obj Exporter - Arg: {narg}/{args.Length} - Mesh: {MeshProgressCount}/{MeshCount}";
                        fs.Seek(MESH_INFO_Offset + (m * 4), SeekOrigin.Begin);
                        ushort MeshIDType;
                        int contatore = 0;
                        int contatore2 = 0;
                        int contatoreoptimization = 0;
                        int contatoreoptimization2 = 0;
                        NewPath = $"{Path.GetDirectoryName(arg)}{Path.DirectorySeparatorChar}{Path.GetFileNameWithoutExtension(arg)}{Path.GetExtension(arg)}_{MeshProgressCount}.obj";
                        File.Delete(NewPath); //delete file if it exists already
                        using FileStream fsNew = File.OpenWrite(NewPath); //using because the Stream class implements the IDisposable class, so let's use the pattern dispose. With this, we don't have to .Close() at the end, it releases all bs automatically.
                        ended = 0; //this variable is used to force the indices data parsing process to move on to the next mesh
                        uint MeshStartOff = br.ReadUInt32(); //Start of the INFO for that particular mesh
                        long MeshTMP = fs.Position; //we will go back to this offset everytime we finish with the indices data
                        fs.Seek(MeshStartOff, SeekOrigin.Begin);
                        MeshIDType = (ushort)((int)ReverseUInt16(br.ReadUInt16()) & 0xF0); //0x10 or 0x50
                        MeshIDType2 = br.ReadByte();
                        MeshIDType3BF = br.ReadByte();
                        ushort unkTwoBytes = ReverseUInt16(br.ReadUInt16()); //get those 2 bytes, don't know what they are, but we will use them soon
                        ushort MeshNSection = ReverseUInt16(br.ReadUInt16()); //How many sections the indices data is composed of. 1 = 1 section, 2 = 2 sections. Always greater than 0.
                        ushort[] MatIndex = new ushort[MeshNSection];
                        fs.Seek(0x0C, SeekOrigin.Current); //skip unks

                        //Origin point, should always be: (Min Coord + Max Coord)/2
                        INFOOriginP1 = ReadBigEndianFloat(br);
                        INFOOriginP2 = ReadBigEndianFloat(br);
                        INFOOriginP3 = ReadBigEndianFloat(br);

                        //min vertices
                        INFOMinCoordP1 = ReadBigEndianFloat(br);
                        INFOMinCoordP2 = ReadBigEndianFloat(br);
                        INFOMinCoordP3 = ReadBigEndianFloat(br);

                        //max vertices
                        INFOMaxCoordP1 = ReadBigEndianFloat(br);
                        INFOMaxCoordP2 = ReadBigEndianFloat(br);
                        INFOMaxCoordP3 = ReadBigEndianFloat(br);

                        uint MeshHDRStartOff = ReverseUInt32(br.ReadUInt32()); //Offset that points to the beginning of the third (main) header of a mesh
                        uint MeshSize = ReverseUInt32(br.ReadUInt32()); //Size of the mesh
                        MeshHDRStartOff += VDLOff; //Add the offset to the .vdl offset. This will give us the actual offset in the .mdl file.
                        long MeshUntilOff = MeshHDRStartOff + MeshSize;
                        uint[] MeshArray = new uint[MeshNSection - 1]; //-1 because we don't need to know the first offset, since it's the beginning of the indices data, we will get the offset later in the code.
                        byte[] optimization = new byte[MeshNSection];
                        /* Optimization in .mdl is either triangles or tris (triangle strips/tristrip).
                         * There's a specific byte in the INFO for each *index section*, if it's 0x03, it's triangles (rare), if it's 0x04, it's tristrip (very common).
                         * We declared the MeshArray array of length MeshNSection (how many index sections that mesh is composed of) -1,
                         * -1 because the first offset is the beginning of the index section,
                         * which we skip because it is also present in the (third) Main header.
                         * The optimization array has length MeshNSection, because we need to know what optimization the mesh uses for the first index section too!
                         */

                        if (MeshIDType == 0x50) //skip bonesNameCount, unk, SecondHeaderOff/d codes
                        {
                            fs.Seek(0x08, SeekOrigin.Current);
                        }
                        else if (MeshIDType != 0x10) //should never happen
                        {
                            Console.WriteLine($"MeshIDType not 0x50 or 0x10: 0x{MeshIDType:X2}");
                            Console.ReadLine();
                        }

                        MatIndex[0] = ReverseUInt16(br.ReadUInt16());
                        fs.Seek(0x01, SeekOrigin.Current);
                        optimization[contatoreoptimization] = br.ReadByte();
                        fs.Seek(0x08, SeekOrigin.Current);
                        contatoreoptimization += 1;

                        for (int MeshOffTest = 1; MeshOffTest < MeshNSection; MeshOffTest++) //get other indices INFO
                        {
                            MatIndex[MeshOffTest] = ReverseUInt16(br.ReadUInt16());
                            fs.Seek(0x01, SeekOrigin.Current);
                            optimization[contatoreoptimization] = br.ReadByte(); //0x04 = Tris | 0x03 = Triangles
                            fs.Seek(0x04, SeekOrigin.Current);
                            MeshArray[contatore] = ReverseUInt32(br.ReadUInt32());
                            MeshArray[contatore] += MeshHDRStartOff;
                            contatoreoptimization += 1;
                            contatore += 1;
                        }

                        //*********************************************
                        //************ THIRD (MAIN) HEADER ************
                        //*********************************************

                        fs.Seek(MeshHDRStartOff, SeekOrigin.Begin); //go to main (third) Header of .vdl
                        try
                        {
                            vtxOff = ReverseUInt32(br.ReadUInt32()); //get the vertices Offset (always 0x20 or 0x40)
                        }
                        catch (EndOfStreamException eose) //we'll get'em next time
                        {
                            PrintInfo();
                            Console.WriteLine($"Error: {eose.Message}");
                            Console.WriteLine("Wrong Mesh Start Offset.");
                            Console.Write($"0x{fs.Position:X8}");
                            Console.WriteLine("Press any key to close the window.");
                            fs = null;
                            br = null;
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                        uint normOff = ReverseUInt32(br.ReadUInt32()); //get normals offset
                        lightOff = ReverseUInt32(br.ReadUInt32()); //get light offset
                        uint uvOff = ReverseUInt32(br.ReadUInt32()); //get uv offset

                        if (vtxOff == 0x20) //Endless Ocean 1 Format
                        {
                            indOff = ReverseUInt32(br.ReadUInt32());
                            vtxCount = ReverseUInt16(br.ReadUInt16());
                            normCount = ReverseUInt16(br.ReadUInt16());
                            lightCount = ReverseUInt16(br.ReadUInt16());
                            uvCount = ReverseUInt16(br.ReadUInt16());
                            ushort fakeIndexCount = ReverseUInt16(br.ReadUInt16());
                            ushort unkFlag = ReverseUInt16(br.ReadUInt16());
                            indOff = indOff + 0x20 + MeshHDRStartOff;
                        }
                        else if (vtxOff == 0x40) //Endless Ocean 2 Format
                        {
                            uint unk01Off = ReverseUInt32(br.ReadUInt32());
                            uint unk02Off = ReverseUInt32(br.ReadUInt32());
                            uint unk03Off = ReverseUInt32(br.ReadUInt32());
                            indOff = ReverseUInt32(br.ReadUInt32());
                            vtxCount = ReverseUInt16(br.ReadUInt16());
                            normCount = ReverseUInt16(br.ReadUInt16());
                            lightCount = ReverseUInt16(br.ReadUInt16());
                            uvCount = ReverseUInt16(br.ReadUInt16());
                            unk01Count = ReverseUInt16(br.ReadUInt16());
                            unk02Count = ReverseUInt16(br.ReadUInt16());
                            ushort unk03Count = ReverseUInt16(br.ReadUInt16());
                            ushort fakeIndexCount = ReverseUInt16(br.ReadUInt16());
                            fs.Seek(0x09, SeekOrigin.Current);
                            indexCount = br.ReadByte(); //Possible values: 0,3,4,5,6,7,8,9,10
                            byte isNormOff4C = br.ReadByte();
                            unkIsOneOrTwo = br.ReadByte();
                            indOff += 0x40 + MeshHDRStartOff;
                        }
                        vtxOff += MeshHDRStartOff;

                        WriteText(fsNew, $"mtllib {Path.GetFileNameWithoutExtension(arg)}.mtl\n");
                        string info = "";

                        //info = $"#vtxCount: {vtxCount} [0x{vtxCount:X4}]\n";
                        //WriteText(fsNew, info);
                        Console.Write($"Parsing Object List ");
                        ParseOBJList(fs, br);
                        info = $"({Path.GetFileNameWithoutExtension(arg)}{Path.GetExtension(arg)}_{MeshProgressCount}_{MeshName})\n";
                        Console.Write(info);

                        info = $"o {$"{Path.GetFileNameWithoutExtension(arg)}{Path.GetExtension(arg)}_{MeshProgressCount}_"}{MeshName}\n";
                        WriteText(fsNew, info);
                        XCoordsArray = new float[vtxCount];
                        YCoordsArray = new float[vtxCount];
                        ZCoordsArray = new float[vtxCount];
                        ogPointArrayFlagCont += 1;
                        info = $"#MeshIDType: 0x{MeshIDType:X2}\n#INFO_Origin X/Y/Z: {INFOOriginP1}, {INFOOriginP2}, {INFOOriginP3}\n#INFO_MIN X/Y/Z vtx: {INFOMinCoordP1}, {INFOMinCoordP2}, {INFOMinCoordP3}\n#INFO_MAX X/Y/Z vtx: {INFOMaxCoordP1}, {INFOMaxCoordP2}, {INFOMaxCoordP3}\n";
                        WriteText(fsNew, info);
                        //#Tristrip or Triangles
                        //#vtxCount: dec [0xhex]
                        //#MeshIDType: -
                        //#INFO_Origin X/Y/Z: -, -, -
                        //#INFO_MIN X/Y/Z vtx: -, -, -
                        //#INFO_MAX X/Y/Z vtx: -, -, -
                        //#CorrectionPointArrayIndex: -
                        //#INFO @: 0xhex
                        //#unkIsOneOrTwo: 1 or 2
                        info = $"#INFO @: 0x{MeshStartOff:X8}\n";
                        WriteText(fsNew, info);
                        info = $"#unkIsOneOrTwo: {unkIsOneOrTwo}\n";
                        WriteText(fsNew, info);

                        /* A general and global rule with .mdls is that,
                         * if the count of a specific section is greater than or equal to 0xFF,
                         * it means that the index section will be composed of a 2 bytes version of the index.
                         * There're a few exceptions here and there tho, especially for the light.
                         */
                        if (vtxCount >= 0xFF)
                            vtxCountShort = true;
                        else
                            vtxCountShort = false;
                        //*//
                        if (normCount >= 0xFF)
                            normCountShort = true;
                        else
                            normCountShort = false;
                        //*//
                        if (lightCount >= 0xFF || RF2MD2 == true && lightOff > 0x00) //If RF2MD2 and there's light data, light will always be 2 bytes for the indices.
                            lightCountShort = true;
                        else
                            lightCountShort = false;
                        //*//
                        if (uvCount >= 0xFF)
                            uvCountShort = true;
                        else
                            uvCountShort = false;
                        //*//
                        if (unk01Count >= 0xFF) //unknown data, it's indexed tho, so let's count it.
                            unk01CountShort = true;
                        else
                            unk01CountShort = false;

                        //*********************************************
                        //*************** VERTICES DATA ***************
                        //*********************************************

                        /*
                        Vec3.X = QuatMainRot.X;
                        Vec3.Y = QuatMainRot.Y;
                        Vec3.Z = QuatMainRot.Z;
                        Quat.W = QuatMainRot.W;
                        Quat.Xyz = Vec3;

                        if (!IsQuaternionValid(Vec3.X, Vec3.Y, Vec3.Z, Quat.W))
                        {
                            Console.Write("Quaternion invalid!");
                            Console.ReadLine();
                        }
                        */

                        fs.Seek(vtxOff, SeekOrigin.Begin);
                        if (DuplMesh && !InsideMeshForEight)
                        {
                            Dictx31_XPointCorrectionMain.Add(0, DictXPointCorrectionMain[0]);
                            Dictx31_YPointCorrectionMain.Add(0, DictYPointCorrectionMain[0]);
                            Dictx31_ZPointCorrectionMain.Add(0, DictZPointCorrectionMain[0]);

                            Dictx31_XRotCorrectionMain.Add(0, DictXRotCorrectionMain[0]);
                            Dictx31_YRotCorrectionMain.Add(0, DictYRotCorrectionMain[0]);
                            Dictx31_ZRotCorrectionMain.Add(0, DictZRotCorrectionMain[0]);
                            Dictx31_WRotCorrectionMain.Add(0, DictWRotCorrectionMain[0]);

                            Dictx31_XSizeCorrectionMain.Add(0, DictXSizeCorrectionMain[0]);
                            Dictx31_YSizeCorrectionMain.Add(0, DictYSizeCorrectionMain[0]);
                            Dictx31_ZSizeCorrectionMain.Add(0, DictZSizeCorrectionMain[0]);
                        }

                        Console.WriteLine($"\t[Vertices]: 0x{vtxCount:X4}");
                        LogVtx(fs, fsNew, br, normOff, uvOff, info);

                        //********************************************
                        //*************** NORMALS DATA ***************
                        //********************************************

                        Console.WriteLine($"\t[Normals]: 0x{normCount:X4}");
                        LogNormals(fs, fsNew, br, normOff, uvOff, MeshHDRStartOff, info);

                        //********************************************
                        //***************** UVS DATA *****************
                        //********************************************

                        Console.WriteLine($"\t[UVs]: 0x{uvCount:X4}");
                        LogUVs(fs, fsNew, br, normOff, uvOff, MeshHDRStartOff, info);

                        //********************************************
                        //*************** INDICES DATA ***************
                        //********************************************
                        Console.WriteLine($"\t[Indices]");

                        cntCountAll = 0;
                        aCountAll = 0;
                        /* a = unknown 2 bytes
                         * cnt = Sequence Count - It indicates each "fragment".
                         * EO2 only: You have to multiply it by the 0x39 byte in the Main Header to know the size of the current draw.
                         * The Indices Section, usually, is divided in multiple draw calls.
                         * Though there could even be 1 single draw call (very rare; it will always happens if optimization is triangles).
                         * Index Sections are usually, but not always, divided by 00s, these are here just for 0x10 alignment.
                         * Since normals are flipped, we have to reverse the parsing of the faces,
                         * so, instead of printing "f {pos}/{uv}/{norm} {pos2}/{uv2}/{norm2} {pos3}/{uv3}/{norm3}\n"
                         * We print                "f {pos3}/{uv3}/{norm3} {pos2}/{uv2}/{norm2} {pos}/{uv}/{norm}\n"
                         * pos -> pos3
                         * pos2 -> pos2
                         * pos3 -> pos
                         */

                        if (printind == 1) //The Epic Indices
                        {
                            try
                            {
                                fs.Seek(indOff, SeekOrigin.Begin);
                                if (vtxOff > 0x00) //not sure why I added this if, vtxOff is always > 0x00
                                {
                                    string info1 = $"usemtl material{MatIndices[MatIndex[contatoreoptimization2]]}\n";
                                    WriteText(fsNew, info1);
                                    info1 = $"#Index section: {contatoreoptimization2}\n#Mat Index: {MatIndex[contatoreoptimization2]}\n";
                                    WriteText(fsNew, info1);
                                    if (optimization[contatoreoptimization2] == 0x03)
                                        WriteText(fsNew, $"#Triangles\n");
                                    else
                                        WriteText(fsNew, $"#Tristrip\n");
                                    //WriteText(fsNew, $"usemtl material{MatIndex[0]}");
                                    if (printinfo == 1)
                                    {
                                        long TMP3 = fs.Position;
                                        info = $"#indexCount: {indexCount}\n#TMP: 0x{TMP3:X8}\n";
                                        WriteText(fsNew, info);
                                        //Console.WriteLine($"#indexCount: {indexCount}");
                                    }
                                    for (int j = 0; j < 1000000; j++)
                                    {
                                        long TMP3 = fs.Position;
                                        //info = $"#---TMP: 0x{TMP3:X8}---\n#---cntCountAll: 0x{cntCountAll:X8}---\n";
                                        //WriteText(fsNew, info);
                                        //Console.WriteLine(info);
                                        ushort a = ReverseUInt16(br.ReadUInt16());
                                        ushort cnt = ReverseUInt16(br.ReadUInt16());
                                        cntCountAll += cnt;
                                        aCountAll += a;
                                        TMP3 = fs.Position;

                                        if (cnt == 0x01 || cnt == 0x02)
                                        /*Sometimes, and don't even ask me why, the count is 0x01 or 0x02.
                                        * Which doesn't make sense, since you must need at least 3 indices to form a face.            */
                                        {
                                            for (int cntEqualsOneOrTwo = 0; cntEqualsOneOrTwo < 100000; cntEqualsOneOrTwo++)
                                            {
                                                // Let's assign indexcount, since we're going to use it to skip those indices
                                                if (RF2MD2 && lightOff > 0x00) //2 bytes(POS/NORM/LIGHT/UV) = 8 bytes
                                                {
                                                    indexCount = 8;
                                                }
                                                else if (RF2MD2 && lightOff == 0x00 || RF2MD3 && indexCount == 0 && lightOff == 0x00) //2 bytes(POS/NORM/UV)
                                                {
                                                    indexCount = 6;
                                                }

                                                fs.Seek(indexCount * cnt, SeekOrigin.Current); //here cnt must be 1 or 2

                                                if (RF2MD2 && lightOff >= 0x00 || (RF2MD3) && lightOff == 0x00 && indexCount == 0x06)
                                                {
                                                    //assign back to indexCount the value 0, we're going to use it
                                                    indexCount = 0;
                                                }
                                                // Let's see if next cnt equals to 0x01 or 0x02, if so, repeat the process.
                                                TMP3 = fs.Position;
                                                a = ReverseUInt16(br.ReadUInt16());
                                                cnt = ReverseUInt16(br.ReadUInt16());
                                                cntCountAll += cnt;
                                                aCountAll += a;
                                                if (cnt != 0x01 && cnt != 0x02) //else, just get out of the loop and move on
                                                {
                                                    //cntEqualsOneOrTwo = 500000;
                                                    info = $"#---TMP: 0x{TMP3:X8}---\n";
                                                    break;
                                                    //WriteText(fsNew, info);
                                                }
                                            }
                                        }

                                        if (TMP3 >= MeshUntilOff) //When a mesh has ended
                                        {
                                            Console.WriteLine("Mesh Ended\n");
                                            j = 5000000;
                                            ended = 1;
                                            fs.Seek(MeshTMP, SeekOrigin.Begin);
                                        }

                                        if (MeshNSection >= 2)
                                        {
                                            //For the love of my life, I don't remember why I added this if...
                                            if (contatore2 == contatore)
                                            {

                                            }
                                            else if (a == 0 && cnt == 0 && contatore2 != contatore || TMP3 >= MeshArray[contatore2])
                                            //if a == 0 and cnt == 0, then that's the alignment. New face section.
                                            {
                                                fs.Seek(MeshArray[contatore2], SeekOrigin.Begin);
                                                contatoreoptimization2 = contatore2 + 1;
                                                contatore2 += 1;
                                                a = ReverseUInt16(br.ReadUInt16());
                                                cnt = ReverseUInt16(br.ReadUInt16());
                                                cntCountAll += cnt;
                                                aCountAll += a;
                                                TMP3 = fs.Position;
                                                WriteText(fsNew, $"usemtl material{MatIndices[MatIndex[contatoreoptimization2]]}\n");
                                                WriteText(fsNew, $"#Index section: {contatoreoptimization2}\n#Mat Index:{MatIndex[contatoreoptimization2]}\n");
                                                if (optimization[contatoreoptimization2] == 0x03)
                                                {
                                                    WriteText(fsNew, $"#Triangles\n");
                                                }
                                                else
                                                {
                                                    WriteText(fsNew, $"#Tristrip\n");
                                                }
                                            }
                                        }
                                        else if (a == 0 && cnt == 0 && contatore2 == contatore && ended == 0)
                                        {
                                            //If there's only 1 index section, then go here
                                            Console.WriteLine("Mesh Ended\n");
                                            j = 5000000;
                                            ended = 1;
                                            fs.Seek(MeshTMP, SeekOrigin.Begin);
                                        }

                                        if (ended == 0) //get here when new face section
                                        {
                                            uint TMPFace = (uint)fs.Position;
                                            int id = 3;
                                            int id2 = 0;
                                            info = $"#---cnt: 0x{cnt:X8}---\n";
                                            //WriteText(fsNew, info);
                                            for (int q = 0; q < cnt; q++)
                                            {
                                                GetIndexStripA(br, fs);

                                                string data = "";
                                                if (pos == pos2 || pos == pos3 || pos2 == pos3)
                                                {
                                                    StripWithEqualFaceCount += 1;
                                                    data = $"#f {pos3}/{uv3}/{norm3} {pos2}/{uv2}/{norm2} {pos}/{uv}/{norm}\n";
                                                }
                                                else
                                                {
                                                    data = $"f {pos3}/{uv3}/{norm3} {pos2}/{uv2}/{norm2} {pos}/{uv}/{norm}\n";
                                                }

                                                WriteText(fsNew, data);

                                                id += 1;

                                                if (optimization[contatoreoptimization2] == 0x04) //if it's tristrip
                                                {
                                                    if (id >= cnt) //draw call ended, out
                                                    {
                                                        q = 10000000;
                                                    }
                                                    if (cnt == 4) //if cnt equals to 4, do one more, and then out.
                                                    {
                                                        id += id2;
                                                        id2 = 1;
                                                        GetIndexStripB(br);

                                                        string data2 = "";
                                                        if (pos4 == pos2 || pos4 == pos3 || pos2 == pos3)
                                                        {
                                                            StripWithEqualFaceCount += 1;
                                                            data2 = $"#f {pos2}/{uv2}/{norm2} {pos3}/{uv3}/{norm3} {pos4}/{uv4}/{norm4}\n";
                                                        }
                                                        else
                                                        {
                                                            data2 = $"f {pos2}/{uv2}/{norm2} {pos3}/{uv3}/{norm3} {pos4}/{uv4}/{norm4}\n";
                                                        }

                                                        WriteText(fsNew, data2);
                                                        //Console.Write(data2);

                                                        q = 10000000;
                                                    }
                                                    else if (id < cnt) //most hit if
                                                    {
                                                        id += id2;
                                                        id2 = 1;
                                                        GetIndexStripB(br);

                                                        string data3 = "";
                                                        if (pos4 == pos2 || pos4 == pos3 || pos2 == pos3)
                                                        {
                                                            StripWithEqualFaceCount += 1;
                                                            data3 = $"#f {pos2}/{uv2}/{norm2} {pos3}/{uv3}/{norm3} {pos4}/{uv4}/{norm4}\n";
                                                        }
                                                        else
                                                        {
                                                            data3 = $"f {pos2}/{uv2}/{norm2} {pos3}/{uv3}/{norm3} {pos4}/{uv4}/{norm4}\n";
                                                        }

                                                        WriteText(fsNew, data3);
                                                        //Console.Write(data3);

                                                        if (id == cnt) //now id equals to cnt, out
                                                        {
                                                            q = 10000000;
                                                        }
                                                        else //we haven't finished with this draw call yet
                                                        {
                                                            fs.Seek(TMP2, SeekOrigin.Begin);
                                                        }
                                                    }
                                                }
                                                else if (optimization[contatoreoptimization2] == 0x03) //if triangles
                                                {
                                                    if (id >= cnt)
                                                    {
                                                        q = 10000000; //out
                                                    }
                                                    id += 2; //else, just repeat the process
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (EndOfStreamException)
                            {
                                Console.WriteLine("#EndOfStreamException - Probably File Ended\n");
                            }
                        }
                    }
                }
                Console.WriteLine("File Ended\n");
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
    }
}