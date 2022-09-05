using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PasswordChanger1C
{
    using static AccessFunctions;

    public class FileModifiedException : Exception
    {
        public FileModifiedException(string message) : base(message)
        {
        }

        public FileModifiedException() : base("Информация в БД была изменена другим процессом! Прочитайте список пользователей заново.")
        {
        }
    }

    public class DataLengthNotMatchException : Exception
    {
        public DataLengthNotMatchException(string message) : base(message)
        {
        }

        public DataLengthNotMatchException(long ExpectedSize, long ActualSize) : base(
            $"Новый байтовый массив должен совпадать по размерам со старым массивом (т.к. мы только заменяем хэши одинаковой длины).{Environment.NewLine}" +
            $"Ожидали {ExpectedSize} байт, получили: {ActualSize}.{Environment.NewLine}" +
            "Сообщите пожалуйста об этой ошибке!")
        {
        }
    }

    internal static partial class DatabaseAccess838
    {        

        internal class FieldReader838 : FieldReaderBase
        {
            public FieldReader838(InfobaseBinaryReader reader, PageParams PageHeader, byte[] data) : base(reader, PageHeader, data)
            {
            }

            protected override byte[] Value_BlobData
            {
                get
                {
                    // двоичные данные неограниченной длины
                    // в рамках хранилища 8.3.6 их быть не должно

                    byte[] result = null;

                    if (BlobDataSize > 0)
                    {
                        var BlobPageBuffer = reader.ReadPage(PageHeader.BlockBlob);
                        var BlobPage = ReadObjectPageDefinition(BlobPageBuffer, PageHeader.PageSize);
                        BlobPage.BinaryData = ReadAllStoragePagesForObject(reader, BlobPage);
                        int[] argDataPositions = null;
                        result = GetCleanDataFromBlob(BlobDataPos, BlobDataSize, BlobPage.BinaryData, DataPositions: ref argDataPositions);
                    }
                    return result;
                }
            }
        }

        public static AccessFunctions.PageParams ReadInfoBase(InfobaseBinaryReader reader, in string TargetTableName)
        {
            // корневой блок
            var RootPageBuffer = reader.ReadPage(2);
            var TargetPage = FindTableDefinition(reader, RootPageBuffer, TargetTableName);

            if (TargetPage.TableName is not null) // if table found
            {
                ReadAllRecordsFromStoragePages(ref TargetPage, reader);
            }
            return TargetPage;
        }

        private static AccessFunctions.PageParams FindTableDefinition(InfobaseBinaryReader reader, byte[] Bytes, in string TargetTableName)
        {
            int PageSize = reader.PageSize;

            string TargetTable = $"\"{TargetTableName.ToUpper()}\"";
            var Page = ReadObjectPageDefinition(Bytes, PageSize);
            Page.BinaryData = ReadAllStoragePagesForObject(reader, Page);
            Page.PageSize = PageSize;
            int PagesCountTableStructure = Page.PagesNum.Count;
            var BytesTableStructure = Page.BinaryData;
            int[] argDataPositions = null;
            var BytesTableStructureBlockNumbers = GetCleanDataFromBlob(1, PagesCountTableStructure * PageSize, Page.BinaryData, DataPositions: ref argDataPositions);
            int TotalBlocks = BitConverter.ToInt32(BytesTableStructureBlockNumbers, 32);
            var PagesWithTableSchema = new List<int>();
            for (int j = 1; j <= TotalBlocks; j++)
            {
                int BlockNumber = BitConverter.ToInt32(BytesTableStructureBlockNumbers, 32 + j * 4);
                PagesWithTableSchema.Add(BlockNumber);
            }

            foreach (var TablePageNumber in PagesWithTableSchema)
            {
                int Position = TablePageNumber * 256;
                int NextBlock = BitConverter.ToInt32(BytesTableStructure, Position);
                short StringLen = BitConverter.ToInt16(BytesTableStructure, Position + 4);
                var StrDefinition = new StringBuilder();
                StrDefinition.Append(Encoding.UTF8.GetString(BytesTableStructure, Position + 6, StringLen));
                while (NextBlock > 0)
                {
                    Position = NextBlock * 256;
                    NextBlock = BitConverter.ToInt32(BytesTableStructure, Position);
                    StringLen = BitConverter.ToInt16(BytesTableStructure, Position + 4);
                    StrDefinition.Append(Encoding.UTF8.GetString(BytesTableStructure, Position + 6, StringLen));
                }

                var TableDefinition = ParserServices.ParsesClass.ParseString(StrDefinition.ToString());
                if (0 == TableDefinition.Count || 0 == TableDefinition[0].Count)
                {
                    continue;
                }

                if (TableDefinition[0][0].ToString().ToUpper() == TargetTable)
                {
                    Page.TableDefinition = StrDefinition.ToString();
                    CommonModule.ParseTableDefinition(ref Page);
                    break;
                }
            }

            return Page;
        }

        private static void ReadAllRecordsFromStoragePages(ref AccessFunctions.PageParams PageHeader, InfobaseBinaryReader reader)
        {
            if (PageHeader.Fields is null) return;

            long FirstPage = PageHeader.BlockData;
            PageHeader.Records = new List<Dictionary<string, object>>();
            var DataPageBuffer = reader.ReadPage(FirstPage);

            var DataPage = ReadObjectPageDefinition(DataPageBuffer, PageHeader.PageSize);
            DataPage.BinaryData = ReadAllStoragePagesForObject(reader, DataPage);
            var bytesBlock = DataPage.BinaryData;
            long RowsCount = DataPage.Length / PageHeader.RowSize;

            var field_reader = new FieldReader838(reader, PageHeader, bytesBlock);

            for (int Row = 1; Row < RowsCount; Row++)
            {
                int RowOffset = (int)PageHeader.RowSize * Row;
                int FieldStartPos = 0;
                bool IsDeleted = BitConverter.ToBoolean(bytesBlock, RowOffset);
                var Dict = new Dictionary<string, object>
                {
                    { "IsDeleted", IsDeleted }
                };

                foreach (var Field in PageHeader.Fields)
                {
                    int FieldOffset = RowOffset + 1 + FieldStartPos;
                    field_reader.SetField(Field, FieldOffset);

                    if (Field.Name == "PASSWORD")
                    {
                        Dict.Add("OFFSET_PASSWORD", FieldOffset);
                    }

                    object FieldValue = field_reader.Value;

                    if (Field.Name == "DATA" && Field.Type == "I" && FieldValue is not null)
                    {
                        Dict.Add("DATA_POS", field_reader.BlobDataPos);
                        Dict.Add("DATA_SIZE", field_reader.BlobDataSize);
                        Dict.Add("DATA_BINARY", FieldValue);
                        
                        byte[] DataKey = new byte[1]; // value will be replaced
                        int DataKeySize = 0;
                        FieldValue = CommonModule.DecodePasswordStructure((byte[])FieldValue, ref DataKeySize, ref DataKey);
                        Dict.Add("DATA_KEYSIZE", DataKeySize);
                        Dict.Add("DATA_KEY", DataKey);                                               
                    }

                    Dict.Add(Field.Name, FieldValue);
                    FieldStartPos += Field.Size;
                }

                PageHeader.Records.Add(Dict);
            }
        }

        private static byte[] GetCleanDataFromBlob(int Dataindex, int Datasize, in byte[] bytesBlock, [Optional, DefaultParameterValue(null)] ref int[] DataPositions)
        {
            int NextBlock = 999; // any number gt 0
            int Pos = Dataindex * 256;
            var ByteBlock = new byte[Datasize];
            int i = 0;
            int BlocksCount = 0;
            while (NextBlock > 0)
            {
                NextBlock = BitConverter.ToInt32(bytesBlock, Pos);
                short BlockSize = BitConverter.ToInt16(bytesBlock, Pos + 4);
                Array.Resize(ref DataPositions, BlocksCount + 1);
                DataPositions[BlocksCount] = Pos + 6;

                bytesBlock.AsMemory(Pos + 6, BlockSize).CopyTo(ByteBlock.AsMemory(i));
                i += BlockSize;

                Pos = NextBlock * 256;
                BlocksCount++;
            }

            return ByteBlock;
        }

        private static byte[] ReadAllStoragePagesForObject(InfobaseBinaryReader reader, in AccessFunctions.PageParams Page)
        {
            return reader.ReadPages(Page.PagesNum);
        }

        private static AccessFunctions.PageParams ReadObjectPageDefinition(in byte[] Bytes, int PageSize)
        {
            var Page = new AccessFunctions.PageParams() { PageSize = PageSize };
            Page.PageType = BitConverter.ToInt32(Bytes, 0);
            Page.version = BitConverter.ToInt32(Bytes, 4);
            Page.version1 = BitConverter.ToInt32(Bytes, 8);
            Page.version2 = BitConverter.ToInt32(Bytes, 12);
            if (Page.PageType == 0xFD1C)
            {
                // 0xFD1C small storage table
                // ???
            }
            else if (Page.PageType == 0x1FD1C)
            {
                // 0x01FD1C  large storage table
                // ???
            }

            Page.Length = BitConverter.ToInt64(Bytes, 16);
            int Index = 24;
            Page.PagesNum = new List<long>();

            // Получим номера страниц размещения
            while (true)
            {
                int blk = BitConverter.ToInt32(Bytes, Index);
                if (blk == 0)
                {
                    break;
                }

                Page.PagesNum.Add(blk);
                Index += 4;
                if (Index > PageSize - 4)
                {
                    break;
                }
            }

            return Page;
        }

        public static void WritePasswordIntoInfoBaseIB(in string FileName, in AccessFunctions.PageParams PageHeader, in byte[] OldData, in byte[] NewData, int DataPos, int DataSize)
        {
            int PageSize = PageHeader.PageSize;
            long BlockBlob = PageHeader.BlockBlob;
            int[] DataPositions = null;
            byte[] TargetData;
            AccessFunctions.PageParams BlobPage;

            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var reader = new InfobaseBinaryReader(fs, PageHeader.PageSize);
                var BlobPageBuffer = reader.ReadPage(BlockBlob);
                BlobPage = ReadObjectPageDefinition(BlobPageBuffer, PageSize);
                BlobPage.BinaryData = ReadAllStoragePagesForObject(reader, BlobPage);
            }

            TargetData = GetCleanDataFromBlob(DataPos, DataSize, BlobPage.BinaryData, ref DataPositions);
            if (!TargetData.SequenceEqual(OldData))
            {
                throw new FileModifiedException();
            }

            if (OldData.Count() != NewData.Count())
            {
                throw new DataLengthNotMatchException(OldData.Count(), NewData.Count());
            }

            int CurrentByte = 0;
            // Data is stored in 256 bytes blocks (6 bytes reserved for next block number and size)
            foreach (var Position in DataPositions)
            {
                int CopyCount = Math.Min(250, NewData.Count() - CurrentByte);
                NewData.AsMemory(CurrentByte, CopyCount).CopyTo(BlobPage.BinaryData.AsMemory(Position));
                CurrentByte += CopyCount;
            }

            // Blob page(s) has been modified. Let's write it back to database
            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var writer = new InfobaseBinaryWriter(fs, PageHeader.PageSize);
                writer.WritePages(BlobPage.PagesNum, BlobPage.BinaryData);
            }
        }

        public static void WritePasswordIntoInfoBaseRepo(in string FileName, in AccessFunctions.PageParams PageHeader, int Offset, in string NewPass = null)
        {
            int PageSize = PageHeader.PageSize;
            AccessFunctions.PageParams DataPage;
            string PassStr = string.IsNullOrEmpty(NewPass) ? AccessFunctions.InfoBaseRepo_EmptyPassword : NewPass;
            var Pass = Encoding.Unicode.GetBytes(PassStr);

            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var reader = new AccessFunctions.InfobaseBinaryReader(fs, PageHeader.PageSize);
                var DataPageBuffer = reader.ReadPage(PageHeader.BlockData);
                DataPage = ReadObjectPageDefinition(DataPageBuffer, PageSize);
            }

            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var writer = new InfobaseBinaryWriter(fs, PageHeader.PageSize);
                writer.WriteToPagesAt(DataPage.PagesNum, Offset, Pass);
            }
        }
    }
}