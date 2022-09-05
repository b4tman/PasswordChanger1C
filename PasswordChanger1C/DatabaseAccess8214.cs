using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordChanger1C
{
    using static AccessFunctions;

    internal static class DatabaseAccess8214
    {
        private const int PageSize = PageSize82;

        internal class FieldReader8214 : FieldReaderBase
        {
            protected long BlockBlob { get; set; }

            public FieldReader8214(InfobaseBinaryReader reader, PageParams PageHeader, byte[] data, long BlockBlob) : base(reader, PageHeader, data)
            {
                this.BlockBlob = BlockBlob;
            }

            protected override byte[] Value_BlobData
            {
                get
                {
                    byte[] result = null;

                    if (BlobDataSize > 0)
                    {
                        result = GetBlobData(BlockBlob, BlobDataPos, BlobDataSize, reader);
                    }
                    return result;
                }
            }
        }

        public static PageParams ReadInfoBase(InfobaseBinaryReader reader, in string TargetTableName)
        {
            // корневой блок
            var RootPageBuffer = reader.ReadPage(2);

            var RootPage = ReadPage(reader, RootPageBuffer);
            RootPage.PageSize = PageSize;
            int NumberOfTables;
            var HeaderTables = new List<long>();
            int i = 0;
            foreach (var ST in RootPage.StorageTables)
            {
                var bytesStorageTables = reader.ReadPages(ST.DataBlocks);

                NumberOfTables = BitConverter.ToInt32(bytesStorageTables, 32);
                for (i = 0; i < NumberOfTables; i++)
                {
                    long PageNum = BitConverter.ToInt32(bytesStorageTables, 36 + i * 4);
                    HeaderTables.Add(PageNum);
                }
            }

            // прочитаем первые страницы таблиц
            foreach (var HT in HeaderTables)
            {
                var PageBuffer = reader.ReadPage(HT);
                var PageHeader = ReadPage(reader, PageBuffer);
                PageHeader.Fields = new List<TableFields>();
                PageHeader.PageSize = PageSize;
                foreach (var ST in PageHeader.StorageTables)
                {
                    foreach (var DB in ST.DataBlocks)
                    {
                        ReadDataFromTable(reader, DB, ref PageHeader, TargetTableName);
                        if (PageHeader.TableName == TargetTableName)
                        {
                            return PageHeader;
                        }
                    }
                }
            }

            return default;
        }

        public static PageParams ReadPage(InfobaseBinaryReader reader, in byte[] Bytes)
        {
            int Index = 24;
            var Page = new PageParams
            {
                Sign = Encoding.UTF8.GetString(Bytes, 0, 8),
                Length = BitConverter.ToInt32(Bytes, 8),
                version1 = BitConverter.ToInt32(Bytes, 12),
                version2 = BitConverter.ToInt32(Bytes, 16),
                version = BitConverter.ToInt32(Bytes, 20),
                PagesNum = new List<long>(),
                StorageTables = new List<StorageTable>()
            };

            // Получим номера страниц размещения
            while (Index <= PageSize - 4)
            {
                long blk = BitConverter.ToInt32(Bytes, Index);
                if (blk == 0)
                {
                    break;
                }

                Page.PagesNum.Add(blk);
                Index += 4;
            }

            foreach (var blk in Page.PagesNum)
            {
                var StorageTables = new StorageTable
                {
                    Number = blk,
                    DataBlocks = new List<long>()
                };
                var PageBuffer = reader.ReadPage(blk);

                int NumberOfPages = BitConverter.ToInt32(PageBuffer, 0);
                Index = 4;
                for (int ii = 0; ii < NumberOfPages && Index <= PageSize - 4; ii++)
                {
                    long dp = BitConverter.ToInt32(PageBuffer, Index);
                    if (dp == 0)
                    {
                        break;
                    }

                    StorageTables.DataBlocks.Add(dp);
                    Index += 4;
                }

                Page.StorageTables.Add(StorageTables);
            }

            return Page;
        }

        public static void ReadDataFromTable(InfobaseBinaryReader reader, long DB, ref PageParams PageHeader, in string TargetTableName)
        {
            var PageBuffer = reader.ReadPage(DB);

            String TableDescr;
            int descrLength = Math.Min((int)PageHeader.Length, PageSize / 2);
            TableDescr = Encoding.Unicode.GetString(PageBuffer, 0, descrLength);
            var ParsedString = ParserServices.ParsesClass.ParseString(TableDescr);
            if (0 == ParsedString.Count || 0 == ParsedString[0].Count)
            {
                return;
            }

            string TableName = ParsedString[0][0].ToString().Replace("\"", "").ToUpper();
            PageHeader.TableName = TableName;
            if (TableName != TargetTableName)
            {
                return;
            }

            PageHeader.TableDefinition = TableDescr;
            CommonModule.ParseTableDefinition(ref PageHeader);
            ReadDataPage(ref PageHeader, PageHeader.BlockData, PageHeader.BlockBlob, reader);
        }

        public static byte[] GetBlobData(long BlockBlob, int Dataindex, int Datasize, InfobaseBinaryReader reader)
        {
            var DataPageBuffer = reader.ReadPage(BlockBlob);

            var DataPage = ReadPage(reader, DataPageBuffer);
            var bytesBlock = reader.ReadPages(DataPage.StorageTables);

            int i = 0;
            int NextBlock = Dataindex;
            int Pos = Dataindex * 256;
            var ByteBlock = new byte[Datasize];
            while (NextBlock > 0)
            {
                NextBlock = BitConverter.ToInt32(bytesBlock, Pos);
                short BlockSize = BitConverter.ToInt16(bytesBlock, Pos + 4);

                bytesBlock.AsMemory(Pos + 6, BlockSize).CopyTo(ByteBlock.AsMemory(i));
                i += BlockSize;

                Pos = NextBlock * 256;
            }

            return ByteBlock;
        }

        public static void ReadDataPage(ref PageParams PageHeader, long block, long BlockBlob, InfobaseBinaryReader reader)
        {
            var DataPageBuffer = reader.ReadPage(block);
            var DataPage = ReadPage(reader, DataPageBuffer);

            PageHeader.Records = new List<Dictionary<string, object>>();
            var bytesBlock = reader.ReadPages(DataPage.StorageTables);
            
            var field_reader = new FieldReader8214(reader, PageHeader, bytesBlock, BlockBlob);

            long RowsCount = DataPage.Length / PageHeader.RowSize;
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
    }
}