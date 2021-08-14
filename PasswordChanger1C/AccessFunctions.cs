using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PasswordChanger1C
{
    /// <summary>
    ///  Error when password length for Repo file is not 64 unicode bytes
    /// </summary>
    public class RepoPasswordLengthException : Exception
    {
        public RepoPasswordLengthException(string message) : base(message)
        {
        }
    }

    /// <summary>
    ///  Error when file format is not match 1C DBMS
    /// </summary>
    public class WrongFileFormatException : Exception
    {
        public WrongFileFormatException(string message) : base(message)
        {
        }
    }

    public static class AccessFunctions
    {
        private const string InfobaseFile_Sign = "1CDBMSV8";
        public const string InfoBaseRepo_EmptyPassword = "d41d8cd98f00b204e9800998ecf8427e";
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

        public static PageParams ReadInfoBase(in string FileName, in string TableNameUsers)
        {
            using var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(fs);
            var bytesBlock = new byte[24];
            reader.Read(bytesBlock, 0, 24);
            string Sign = "";

            try
            {
                Sign = Encoding.ASCII.GetString(bytesBlock, 0, 8);
            }
            catch { }
            if (InfobaseFile_Sign != Sign)
                throw new WrongFileFormatException($"wrong infobase file format, expected \"{InfobaseFile_Sign}\", got \"{Sign}\"");

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

            PageParams Param;
            if ("8.3.8" == DatabaseVersion)
            {
                Param = DatabaseAccess838.ReadInfoBase(reader, TableNameUsers, PageSize);
            }
            else if ("8.2.14" == DatabaseVersion)
            {
                Param = DatabaseAccess8214.ReadInfoBase(reader, TableNameUsers);
            } else
            {
                throw new NotSupportedException($"Infobase file version \"{DatabaseVersion}\" not supported");
            }
            Param.DatabaseVersion = DatabaseVersion;

            return Param;
        }

        public static void WritePasswordIntoInfoBaseRepo(in string FileName, in PageParams PageHeader, in int Offset, in string NewPass = null)
        {
            byte[] bytesBlock;
            PageParams DataPage;
            long TotalBlocks;
            int i;
            string PassStr = string.IsNullOrEmpty(NewPass) ? InfoBaseRepo_EmptyPassword : NewPass;
            var Pass = Encoding.Unicode.GetBytes(PassStr);
            if (Pass.Length != 64) throw new RepoPasswordLengthException($"password length should be 64 unicode bytes, but got {Pass.Length} bytes");

            if ("8.3.8" == PageHeader.DatabaseVersion)
            {
                DatabaseAccess838.WritePasswordIntoInfoBaseRepo(FileName, PageHeader, Offset, NewPass);
                return;
            }
            if ("8.2.14" != PageHeader.DatabaseVersion)
            {
                throw new NotSupportedException($"Repo infobase file version \"{PageHeader.DatabaseVersion}\" not supported");
            }

            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var reader = new BinaryReader(fs);

                var bytesBlock1 = new byte[PageHeader.PageSize];
                reader.BaseStream.Seek(PageHeader.BlockData * (long)PageHeader.PageSize, SeekOrigin.Begin);
                reader.Read(bytesBlock1, 0, PageHeader.PageSize);
                DataPage = DatabaseAccess8214.ReadPage(reader, bytesBlock1);
                TotalBlocks = DataPage.StorageTables.Sum(ST => (long)ST.DataBlocks.Count);
                bytesBlock = new byte[PageHeader.PageSize * TotalBlocks];
                i = 0;
                foreach (var ST in DataPage.StorageTables)
                {
                    foreach (var DB in ST.DataBlocks)
                    {
                        var TempBlock = new byte[PageHeader.PageSize];
                        reader.BaseStream.Seek(DB * (long)PageHeader.PageSize, SeekOrigin.Begin);
                        reader.Read(TempBlock, 0, PageHeader.PageSize);
                        TempBlock.AsMemory().CopyTo(bytesBlock.AsMemory(i));
                        i += TempBlock.Length;
                    }
                }
            }

            Pass.AsMemory().CopyTo(bytesBlock.AsMemory(Offset));

            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var writer = new BinaryWriter(fs);
                i = 0;
                foreach (var ST in DataPage.StorageTables)
                {
                    foreach (var DB in ST.DataBlocks)
                    {
                        var TempBlock = new byte[PageHeader.PageSize];
                        bytesBlock.AsMemory(i, TempBlock.Length).CopyTo(TempBlock.AsMemory());
                        i += TempBlock.Length;

                        writer.BaseStream.Seek(DB * (long)PageHeader.PageSize, SeekOrigin.Begin);
                        writer.Write(TempBlock);
                    }
                }
            }
        }

        public static void WritePasswordIntoInfoBaseIB(in string FileName, in PageParams PageHeader, in byte[] OldData, in byte[] NewData, in int DataPos, in int DataSize)
        {
            if (PageHeader.DatabaseVersion.StartsWith("8.3"))
            {
                DatabaseAccess838.WritePasswordIntoInfoBaseIB(FileName, PageHeader, OldData, NewData, DataPos, DataSize);
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
                ii = 0;
                foreach (var ST in DataPage.StorageTables)
                {
                    foreach (var DB in ST.DataBlocks)
                    {
                        var TempBlock = new byte[PageSize];
                        bytesBlock.AsMemory(ii, TempBlock.Length).CopyTo(TempBlock.AsMemory());
                        ii += TempBlock.Length;

                        writer.BaseStream.Seek(DB * (long)PageSize, SeekOrigin.Current);
                        writer.Write(TempBlock);
                    }
                }
            }
        }
    }
}