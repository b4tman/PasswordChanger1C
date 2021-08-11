using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PasswordChanger1C
{
    public static class AccessFunctions
    {
        public struct StorageTable
        {
            public int Number;
            public List<int> DataBlocks;
        }

        public struct TableFields
        {
            public string Name;
            public int Length;
            public int Precision;
            public int Size;
            public int Offset;
            public string Type;
            public int CouldBeNull;
        }

        public struct PageParams
        {
            public string TableName;
            public string Sign;
            public int PageType;
            public long Length;
            public int version1;
            public int version2;
            public int version;
            public List<int> PagesNum;
            public List<StorageTable> StorageTables;
            public List<TableFields> Fields;
            public int RowSize;
            public List<Dictionary<string, object>> Records;
            public int BlockData;
            public int BlockBlob;
            public int PageSize;
            public string TableDefinition;
            public string DatabaseVersion;
            public byte[] BinaryData;
        }

        public static PageParams ReadInfoBase(string FileName, string TableNameUsers)
        {
            using var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(fs);

            var bytesBlock = new byte[24];
            reader.Read(bytesBlock, 0, 24);
            string Sign = Encoding.UTF8.GetString(bytesBlock, 0, 8);

            if ("1CDBMSV8" != Sign) throw new Exception("wrong infobase file format");

            string V1 = bytesBlock[8].ToString();
            string V2 = bytesBlock[9].ToString();
            string V3 = bytesBlock[10].ToString();
            string DatabaseVersion = $"{V1}.{V2}.{V3}";
            //int DBSize = BitConverter.ToInt32(bytesBlock, 12);
            int PageSize = BitConverter.ToInt32(bytesBlock, 20);
            if (PageSize == 0)
            {
                PageSize = 4096;
            }

            reader.BaseStream.Seek(PageSize, SeekOrigin.Begin);
            if (DatabaseVersion.StartsWith("8.3"))
            {
                var Param = DatabaseAccess838.ReadInfoBase(reader, TableNameUsers, PageSize);
                Param.DatabaseVersion = DatabaseVersion;
                return Param;
            }
            else
            {
                var Param = DatabaseAccess8214.ReadInfoBase(reader, TableNameUsers);
                Param.DatabaseVersion = DatabaseVersion;
                return Param;
            }
            return default;
        }

        public static void WritePasswordIntoInfoBaseRepo(string FileName, PageParams PageHeader, byte[] UserID, string NewPass, int Offset)
        {
            byte[] bytesBlock;
            PageParams DataPage;
            int TotalBlocks;
            int i;

            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var reader = new BinaryReader(fs);

                var bytesBlock1 = new byte[4096];
                reader.BaseStream.Seek(PageHeader.BlockData * 4096, SeekOrigin.Begin);
                reader.Read(bytesBlock1, 0, 4096);
                DataPage = DatabaseAccess8214.ReadPage(reader, bytesBlock1);
                TotalBlocks = DataPage.StorageTables.Sum(ST => ST.DataBlocks.Count);
                bytesBlock = new byte[(4096 * TotalBlocks)];
                i = 0;
                foreach (var ST in DataPage.StorageTables)
                {
                    foreach (var DB in ST.DataBlocks)
                    {
                        var TempBlock = new byte[4096];
                        reader.BaseStream.Seek(DB * 4096, SeekOrigin.Begin);
                        reader.Read(TempBlock, 0, 4096);
                        foreach (var ElemByte in TempBlock)
                        {
                            bytesBlock[i] = ElemByte;
                            i++;
                        }
                    }
                }
            }

            //string Test = Encoding.Unicode.GetString(bytesBlock, Offset, 64);
            var Pass = Encoding.Unicode.GetBytes(NewPass);
            i = 0;
            for (i = 0; i < Pass.Length; i++)
                bytesBlock[i + Offset] = Pass[i];

            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var writer = new BinaryWriter(fs);
                i = 0;
                foreach (var ST in DataPage.StorageTables)
                {
                    foreach (var DB in ST.DataBlocks)
                    {
                        var TempBlock = new byte[4096];
                        for (int j = 0; j <= 4095; j++)
                        {
                            TempBlock[j] = bytesBlock[i];
                            i++;
                        }

                        writer.Seek(DB * 4096, SeekOrigin.Begin);
                        writer.Write(TempBlock);
                    }
                }
            }
        }

        public static void WritePasswordIntoInfoBaseIB(string FileName, PageParams PageHeader, byte[] UserID, byte[] OldData, byte[] NewData, int DataPos, int DataSize)
        {
            if (PageHeader.DatabaseVersion.StartsWith("8.3"))
            {
                DatabaseAccess838.WritePasswordIntoInfoBaseIB(FileName, PageHeader, UserID, OldData, NewData, DataPos, DataSize);
                return;
            }

            int i;
            int TotalBlocks;
            byte[] bytesBlock;
            int PageSize = PageHeader.PageSize;
            var bytesBlock1 = new byte[PageSize];
            PageParams DataPage = default;

            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var reader = new BinaryReader(fs);
                reader.BaseStream.Seek(PageHeader.BlockBlob * PageSize, SeekOrigin.Begin);
                reader.Read(bytesBlock1, 0, PageSize);
                DataPage = DatabaseAccess8214.ReadPage(reader, bytesBlock1);
                TotalBlocks = DataPage.StorageTables.Sum(ST => ST.DataBlocks.Count);
                bytesBlock = new byte[(PageSize * TotalBlocks)];
                i = 0;
                foreach (var ST in DataPage.StorageTables)
                {
                    foreach (var DB in ST.DataBlocks)
                    {
                        var TempBlock = new byte[PageSize];
                        reader.BaseStream.Seek(DB * PageSize, SeekOrigin.Begin);
                        reader.Read(TempBlock, 0, PageSize);
                        foreach (var ElemByte in TempBlock)
                        {
                            bytesBlock[i] = ElemByte;
                            i++;
                        }
                    }
                }
            }

            int NextBlock = DataPos;
            int Pos = DataPos * 256;
            int ii = 0;
            while (NextBlock > 0)
            {
                NextBlock = BitConverter.ToInt32(bytesBlock, Pos);
                short BlockSize = BitConverter.ToInt16(bytesBlock, Pos + 4);
                for (int j = 0; j < BlockSize; j++)
                {
                    bytesBlock[Pos + 6 + j] = NewData[ii];
                    ii++;
                }

                Pos = NextBlock * 256;
            }

            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var writer = new BinaryWriter(fs);
                ii = 0;
                foreach (var ST in DataPage.StorageTables)
                {
                    foreach (var DB in ST.DataBlocks)
                    {
                        var TempBlock = new byte[PageSize];
                        for (int j = 0; j < PageSize; j++)
                        {
                            TempBlock[j] = bytesBlock[ii];
                            ii++;
                        }

                        writer.Seek(DB * PageSize, SeekOrigin.Begin);
                        writer.Write(TempBlock);
                    }
                }
            }
        }
    }
}