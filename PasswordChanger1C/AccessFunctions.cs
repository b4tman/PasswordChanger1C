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

    
    public static partial class AccessFunctions
    {
        private const string InfobaseFile_Sign = "1CDBMSV8";
        public const string InfoBaseRepo_EmptyPassword = "d41d8cd98f00b204e9800998ecf8427e";
        public const int PageSize82 = 4096; // PageSize for 8.2.14 infobase file

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

        public static PageParams ReadInfoBase(in string FileName, in string TargetTableName)
        {
            using var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new InfobaseBinaryReader(fs);
            const int HeaderSize = 24;
            var HeaderBlock = reader.ReadBytes(HeaderSize);

            string Sign = "";

            try
            {
                Sign = Encoding.ASCII.GetString(HeaderBlock, 0, 8);
            }
            catch { }
            if (InfobaseFile_Sign != Sign)
                throw new WrongFileFormatException($"wrong infobase file format, expected \"{InfobaseFile_Sign}\", got \"{Sign}\"");

            string V1 = HeaderBlock[8].ToString();
            string V2 = HeaderBlock[9].ToString();
            string V3 = HeaderBlock[10].ToString();
            string DatabaseVersion = $"{V1}.{V2}.{V3}";
            //int DBSize = BitConverter.ToInt32(HeaderBlock, 12);
            int PageSize = BitConverter.ToInt32(HeaderBlock, 20);
            if (PageSize == 0)
            {
                PageSize = PageSize82;
            }

            reader.PageSize = PageSize;

            PageParams Param;
            if ("8.3.8" == DatabaseVersion)
            {
                Param = DatabaseAccess838.ReadInfoBase(reader, TargetTableName);
            }
            else if ("8.2.14" == DatabaseVersion)
            {
                Param = DatabaseAccess8214.ReadInfoBase(reader, TargetTableName);
            } else
            {
                throw new NotSupportedException($"Infobase file version \"{DatabaseVersion}\" not supported");
            }
            Param.DatabaseVersion = DatabaseVersion;

            return Param;
        }

        public static void WritePasswordIntoInfoBaseRepo(in string FileName, in PageParams PageHeader, int Offset, in string NewPass = null)
        {
            byte[] TargetDataBuffer;
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
                using var reader = new InfobaseBinaryReader(fs, PageHeader.PageSize);
                var DataPageBuffer = reader.ReadPage(PageHeader.BlockData);

                DataPage = DatabaseAccess8214.ReadPage(reader, DataPageBuffer);
                TotalBlocks = DataPage.StorageTables.Sum(ST => (long)ST.DataBlocks.Count);
                TargetDataBuffer = new byte[PageHeader.PageSize * TotalBlocks];
                i = 0;
                foreach (var ST in DataPage.StorageTables)
                {
                    foreach (var DB in ST.DataBlocks)
                    {
                        var PageBuffer = reader.ReadPage(DB);
                        PageBuffer.CopyTo(TargetDataBuffer.AsMemory(i));
                        i += PageBuffer.Length;
                    }
                }
            }

            Pass.CopyTo(TargetDataBuffer.AsMemory(Offset));

            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var writer = new BinaryWriter(fs);
                i = 0;
                foreach (var ST in DataPage.StorageTables)
                {
                    foreach (var DB in ST.DataBlocks)
                    {
                        var TempBlock = new byte[PageHeader.PageSize];
                        TargetDataBuffer.AsMemory(i, TempBlock.Length).CopyTo(TempBlock.AsMemory());
                        i += TempBlock.Length;

                        writer.BaseStream.Seek(DB * (long)PageHeader.PageSize, SeekOrigin.Begin);
                        writer.Write(TempBlock);
                    }
                }
            }
        }

        public static void WritePasswordIntoInfoBaseIB(in string FileName, in PageParams PageHeader, in byte[] OldData, in byte[] NewData, int DataPos, int DataSize)
        {
            if (PageHeader.DatabaseVersion.StartsWith("8.3"))
            {
                DatabaseAccess838.WritePasswordIntoInfoBaseIB(FileName, PageHeader, OldData, NewData, DataPos, DataSize);
                return;
            }

            int i;
            long TotalBlocks;
            byte[] TargetDataBuffer;
            int PageSize = PageHeader.PageSize;
            PageParams DataPage = default;

            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var reader = new InfobaseBinaryReader(fs, PageHeader.PageSize);
                var DataPageBuffer = reader.ReadPage(PageHeader.BlockBlob);                
                DataPage = DatabaseAccess8214.ReadPage(reader, DataPageBuffer);
                TotalBlocks = DataPage.StorageTables.Sum(ST => (long)ST.DataBlocks.Count);
                TargetDataBuffer = new byte[PageSize * TotalBlocks];
                i = 0;
                foreach (var ST in DataPage.StorageTables)
                {
                    foreach (var DB in ST.DataBlocks)
                    {
                        var PageBuffer = reader.ReadPage(DB);
                        PageBuffer.CopyTo(TargetDataBuffer.AsMemory(i));
                        i += PageBuffer.Length;
                    }
                }
            }

            int NextBlock = DataPos;
            int Pos = (int)DataPos * 256;
            int ii = 0;
            while (NextBlock > 0)
            {
                NextBlock = BitConverter.ToInt32(TargetDataBuffer, Pos);
                short BlockSize = BitConverter.ToInt16(TargetDataBuffer, Pos + 4);

                NewData.AsMemory(ii, BlockSize).CopyTo(TargetDataBuffer.AsMemory(Pos + 6));
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
                        TargetDataBuffer.AsMemory(ii, TempBlock.Length).CopyTo(TempBlock.AsMemory());
                        ii += TempBlock.Length;

                        writer.BaseStream.Seek(DB * (long)PageSize, SeekOrigin.Current);
                        writer.Write(TempBlock);
                    }
                }
            }
        }
    }
}