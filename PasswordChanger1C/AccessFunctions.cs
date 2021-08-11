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
            public long Number;
            public List<long> DataBlocks;
        }

        public struct TableFields
        {
            public string Name;
            public int Length;
            public int Precision;
            public int Size;
            public long Offset;
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
            public List<long> PagesNum;
            public List<StorageTable> StorageTables;
            public List<TableFields> Fields;
            public long RowSize;
            public List<Dictionary<string, object>> Records;
            public long BlockData;
            public long BlockBlob;
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
            long TotalBlocks;
            int i;

            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var reader = new BinaryReader(fs);

                var bytesBlock1 = new byte[4096];
                reader.BaseStream.Seek(PageHeader.BlockData * 4096, SeekOrigin.Begin);
                reader.Read(bytesBlock1, 0, 4096);
                DataPage = DatabaseAccess8214.ReadPage(reader, bytesBlock1);
                TotalBlocks = DataPage.StorageTables.Sum(ST => (long)ST.DataBlocks.Count);
                bytesBlock = new byte[4096 * TotalBlocks];
                i = 0;
                foreach (var ST in DataPage.StorageTables)
                {
                    foreach (var DB in ST.DataBlocks)
                    {
                        var TempBlock = new byte[4096];
                        reader.BaseStream.Seek(DB * 4096, SeekOrigin.Begin);
                        reader.Read(TempBlock, 0, 4096);
                        TempBlock.AsMemory().CopyTo(bytesBlock.AsMemory(i));
                        i += TempBlock.Length;
                    }
                }
            }

            //string Test = Encoding.Unicode.GetString(bytesBlock, Offset, 64);
            var Pass = Encoding.Unicode.GetBytes(NewPass);
            Pass.AsMemory().CopyTo(bytesBlock.AsMemory(Offset));

            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var writer = new BinaryWriter(fs);
                long LastPos = 0;
                i = 0;
                foreach (var ST in DataPage.StorageTables)
                {
                    foreach (var DB in ST.DataBlocks)
                    {
                        var TempBlock = new byte[4096];
                        bytesBlock.AsMemory(i, TempBlock.Length).CopyTo(TempBlock.AsMemory());
                        i += TempBlock.Length;

                        long CurPos = DB * 4096;
                        int RelativePos = (int)(CurPos - LastPos);
                        writer.Seek(RelativePos, SeekOrigin.Current);
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
            long TotalBlocks;
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
                TotalBlocks = DataPage.StorageTables.Sum(ST => (long)ST.DataBlocks.Count);
                bytesBlock = new byte[PageSize * TotalBlocks];
                i = 0;
                foreach (var ST in DataPage.StorageTables)
                {
                    foreach (var DB in ST.DataBlocks)
                    {
                        var TempBlock = new byte[PageSize];
                        reader.BaseStream.Seek(DB * PageSize, SeekOrigin.Begin);
                        reader.Read(TempBlock, 0, PageSize);
                        TempBlock.AsMemory().CopyTo(bytesBlock.AsMemory(i));
                        i += TempBlock.Length;
                    }
                }
            }

            int NextBlock = DataPos;
            int Pos = (int)DataPos * 256;
            int ii = 0;
            while (NextBlock > 0)
            {
                NextBlock = BitConverter.ToInt32(bytesBlock, Pos);
                short BlockSize = BitConverter.ToInt16(bytesBlock, Pos + 4);

                NewData.AsMemory(ii, BlockSize).CopyTo(bytesBlock.AsMemory(Pos + 6));
                ii += BlockSize;

                Pos = NextBlock * 256;
            }

            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var writer = new BinaryWriter(fs);
                long LastPos = 0;
                ii = 0;
                foreach (var ST in DataPage.StorageTables)
                {
                    foreach (var DB in ST.DataBlocks)
                    {
                        var TempBlock = new byte[PageSize];
                        bytesBlock.AsMemory(ii, TempBlock.Length).CopyTo(TempBlock.AsMemory());
                        ii += TempBlock.Length;

                        long CurPos = DB * PageSize;
                        int RelativePos = (int)(CurPos - LastPos);
                        writer.Seek(RelativePos, SeekOrigin.Current);
                        writer.Write(TempBlock);
                    }
                }
            }
        }
    }
}