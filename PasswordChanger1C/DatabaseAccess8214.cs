using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PasswordChanger1C
{
    static class DatabaseAccess8214
    {
        const int PageSize = 4096;

        public static AccessFunctions.PageParams ReadInfoBase(BinaryReader reader, in string TableNameUsers)
        {
            var bytesBlock = new byte[PageSize];

            // второй блок пропускаем
            reader.BaseStream.Seek((long)PageSize, SeekOrigin.Current);

            // корневой блок
            reader.Read(bytesBlock, 0, PageSize);
            var Param = ReadPage(reader, bytesBlock);
            Param.PageSize = PageSize;
            string Language;
            int NumberOfTables;
            var HeaderTables = new List<long>();
            int i = 0;
            foreach (var ST in Param.StorageTables)
            {
                var bytesStorageTables = new byte[PageSize * ST.DataBlocks.Count];
                foreach (var DB in ST.DataBlocks)
                {
                    var TempBlock = new byte[PageSize];
                    reader.BaseStream.Seek(DB * (long)PageSize, SeekOrigin.Begin);
                    reader.Read(TempBlock, 0, PageSize);

                    TempBlock.AsMemory().CopyTo(bytesStorageTables.AsMemory(i));
                    i += TempBlock.Length;
                }

                Language = Encoding.UTF8.GetString(bytesStorageTables, 0, 32);
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
                reader.BaseStream.Seek(HT * (long)PageSize, SeekOrigin.Begin);
                reader.Read(bytesBlock, 0, PageSize);
                var PageHeader = ReadPage(reader, bytesBlock);
                PageHeader.Fields = new List<AccessFunctions.TableFields>();
                PageHeader.PageSize = PageSize;
                foreach (var ST in PageHeader.StorageTables)
                {
                    foreach (var DB in ST.DataBlocks)
                    {
                        ReadDataFromTable(reader, DB, bytesBlock, ref PageHeader, TableNameUsers);
                        if (PageHeader.TableName == TableNameUsers)
                        {
                            return PageHeader;
                        }
                    }
                }
            }

            return default;
        }

        public static AccessFunctions.PageParams ReadPage(BinaryReader reader, in byte[] Bytes)
        {
            int Index = 24;
            var Page = new AccessFunctions.PageParams
            {
                Sign = Encoding.UTF8.GetString(Bytes, 0, 8),
                Length = BitConverter.ToInt32(Bytes, 8),
                version1 = BitConverter.ToInt32(Bytes, 12),
                version2 = BitConverter.ToInt32(Bytes, 16),
                version = BitConverter.ToInt32(Bytes, 20),
                PagesNum = new List<long>(),
                StorageTables = new List<AccessFunctions.StorageTable>()
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
                var StorageTables = new AccessFunctions.StorageTable
                {
                    Number = blk,
                    DataBlocks = new List<long>()
                };
                var bytesBlock = new byte[PageSize];
                reader.BaseStream.Seek(blk * (long)PageSize, SeekOrigin.Begin);
                reader.Read(bytesBlock, 0, PageSize);
                int NumberOfPages = BitConverter.ToInt32(bytesBlock, 0);
                Index = 4;
                for (int ii = 0; ii < NumberOfPages && Index <= PageSize - 4; ii++)
                {
                    long dp = BitConverter.ToInt32(bytesBlock, Index);
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

        public static void ReadDataFromTable(BinaryReader reader, in long DB, byte[] bytesBlock, ref AccessFunctions.PageParams PageHeader, in string TableNameUsers)
        {
            reader.BaseStream.Seek(DB * (long)PageSize, SeekOrigin.Begin);
            reader.Read(bytesBlock, 0, PageSize);
            string TableDescr = "";
            long descrLength = Math.Min((int)PageHeader.Length, PageSize / 2);
            for (int i = 0; i < descrLength; i++)
                TableDescr += Encoding.UTF8.GetString(bytesBlock, i * 2, 1);
            var ParsedString = ParserServices.ParsesClass.ParseString(TableDescr);
            long RowSize = 1;
            string TableName = ParsedString[0][0].ToString().Replace("\"", "").ToUpper();
            PageHeader.TableName = TableName;
            if (TableName != TableNameUsers)
            {
                return;
            }

            foreach (var a in ParsedString[0][2])
            {
                if (!a.IsList)
                {
                    continue;
                }

                var Field = new AccessFunctions.TableFields
                {
                    Name = a[0].ToString().Replace("\"", ""),
                    Type = a[1].ToString().Replace("\"", ""),
                    CouldBeNull = Convert.ToInt32(a[2].ToString()),
                    Length = Convert.ToInt32(a[3].ToString()),
                    Precision = Convert.ToInt32(a[4].ToString())
                };
                int FieldSize = Field.CouldBeNull;
                if (Field.Type == "B")
                {
                    FieldSize += Field.Length;
                }
                else if (Field.Type == "L")
                {
                    FieldSize++;
                }
                else if (Field.Type == "N")
                {
                    FieldSize += (Field.Length + 2) / 2;
                }
                else if (Field.Type == "NC")
                {
                    FieldSize += Field.Length * 2;
                }
                else if (Field.Type == "NVC")
                {
                    FieldSize += Field.Length * 2 + 2;
                }
                else if (Field.Type == "RV")
                {
                    FieldSize += 16;
                }
                else if (Field.Type == "I")
                {
                    FieldSize += 8;
                }
                else if (Field.Type == "T")
                {
                    FieldSize += 8;
                }
                else if (Field.Type == "DT")
                {
                    FieldSize += 7;
                }
                else if (Field.Type == "NT")
                {
                    FieldSize += 8;
                }

                Field.Size = FieldSize;
                Field.Offset = RowSize;
                RowSize += FieldSize;
                PageHeader.Fields.Add(Field);
            }

            PageHeader.RowSize = RowSize;

            // {"Files",118,119,96}
            // Данные, BLOB, индексы

            int BlockData = Convert.ToInt32(ParsedString[0][5][1].ToString());
            int BlockBlob = Convert.ToInt32(ParsedString[0][5][2].ToString());
            PageHeader.BlockData = BlockData;
            PageHeader.BlockBlob = BlockBlob;
            ReadDataPage(ref PageHeader, BlockData, BlockBlob, reader);
        }

        public static byte[] GetBlobData(in long BlockBlob, in int Dataindex, in int Datasize, BinaryReader reader)
        {
            var bytesBlock1 = new byte[PageSize];
            reader.BaseStream.Seek(BlockBlob * (long)PageSize, SeekOrigin.Begin);
            reader.Read(bytesBlock1, 0, PageSize);
            var DataPage = ReadPage(reader, bytesBlock1);
            int TotalBlocks = DataPage.StorageTables.Sum(ST => ST.DataBlocks.Count);
            var bytesBlock = new byte[PageSize * TotalBlocks];
            int i = 0;
            foreach (var ST in DataPage.StorageTables)
            {
                foreach (var DB in ST.DataBlocks)
                {
                    var TempBlock = new byte[PageSize];
                    reader.BaseStream.Seek(DB * (long)PageSize, SeekOrigin.Begin);
                    reader.Read(TempBlock, 0, PageSize);
                    TempBlock.AsMemory().CopyTo(bytesBlock.AsMemory(i));
                    i += TempBlock.Length;
                }
            }

            int NextBlock = Dataindex;
            int Pos = Dataindex * 256;
            var ByteBlock = new byte[Datasize];
            i = 0;
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

        public static void ReadDataPage(ref AccessFunctions.PageParams PageHeader, in long block, in long BlockBlob, BinaryReader reader)
        {
            PageHeader.Records = new List<Dictionary<string, object>>();
            var bytesBlock1 = new byte[PageSize];
            reader.BaseStream.Seek(block * (long)PageSize, SeekOrigin.Begin);
            reader.Read(bytesBlock1, 0, PageSize);
            var DataPage = ReadPage(reader, bytesBlock1);
            int TotalBlocks = DataPage.StorageTables.Sum(ST => ST.DataBlocks.Count);
            var bytesBlock = new byte[PageSize * TotalBlocks];
            int i = 0;
            foreach (var ST in DataPage.StorageTables)
            {
                foreach (var DB in ST.DataBlocks)
                {
                    var TempBlock = new byte[PageSize];
                    reader.BaseStream.Seek(DB * (long)PageSize, SeekOrigin.Begin);
                    reader.Read(TempBlock, 0, PageSize);
                    TempBlock.AsMemory().CopyTo(bytesBlock.AsMemory(i));
                    i += TempBlock.Length;
                }
            }

            long Size = DataPage.Length / PageHeader.RowSize;
            for (i = 1; i < Size; i++)
            {
                int Pos = (int)PageHeader.RowSize * i;
                int FieldStartPos = 0;
                bool IsDeleted = BitConverter.ToBoolean(bytesBlock, Pos);
                var Dict = new Dictionary<string, object>
                {
                    { "IsDeleted", IsDeleted }
                };
                foreach (var Field in PageHeader.Fields)
                {
                    int Pos1 = Pos + 1 + FieldStartPos;
                    if (Field.Name == "PASSWORD")
                    {
                        Dict.Add("OFFSET_PASSWORD", Pos1);
                    }
                    else if (Field.Name == "DATA")
                    {
                        Dict.Add("DATA_POS", BitConverter.ToInt32(bytesBlock, Pos1));
                        Dict.Add("DATA_SIZE", BitConverter.ToInt32(bytesBlock, Pos1 + 4));
                    }

                    object BytesVal = null;
                    if (Field.Type == "B")
                    {
                        string Strguid = Convert.ToBase64String(bytesBlock, Pos1 + Field.CouldBeNull, Field.Size - Field.CouldBeNull);
                        BytesVal = Convert.FromBase64String(Strguid);
                    }
                    else if (Field.Type == "L")
                    {
                        BytesVal = BitConverter.ToBoolean(bytesBlock, Pos1 + Field.CouldBeNull);
                    }
                    else if (Field.Type == "DT")
                    {
                        var BytesDate = new byte[7]; // 7 байт
                        for (int AA = 0; AA <= 6; AA++)
                            BytesDate[AA] = Convert.ToByte(Convert.ToString(bytesBlock[Pos1 + AA], 16));
                        try
                        {
                            BytesVal = new DateTime(BytesDate[0] * 100 + BytesDate[1],
                                                    BytesDate[2],
                                                    BytesDate[3],
                                                    BytesDate[4],
                                                    BytesDate[5],
                                                    BytesDate[6]);
                        }
                        catch (Exception)
                        {
                            BytesVal = "";
                        }
                    }
                    else if (Field.Type == "I")
                    {
                        // двоичные данные неограниченной длины
                        // в рамках хранилища 8.3.6 их быть не должно

                        int DataPos = BitConverter.ToInt32(bytesBlock, Pos1);
                        int DataSize = BitConverter.ToInt32(bytesBlock, Pos1 + 4);

                        if (DataSize > 0)
                        {
                            var BytesValTemp = GetBlobData(BlockBlob, DataPos, DataSize, reader);
                            var DataKey = new byte[1];
                            int DataKeySize = 0;
                            BytesVal = CommonModule.DecodePasswordStructure(BytesValTemp, ref DataKeySize, ref DataKey);
                            Dict.Add("DATA_KEYSIZE", DataKeySize);
                            Dict.Add("DATA_KEY", DataKey);
                            Dict.Add("DATA_BINARY", BytesValTemp);
                        }
                    }
                    else if (Field.Type == "NT")
                    {
                        // Строка неограниченной длины
                        BytesVal = ""; // TODO
                    }
                    else if (Field.Type == "N")
                    {
                        // число
                        BytesVal = 0;
                        string StrNumber = "";
                        for (int AA = 0; AA < Field.Size; AA++)
                        {
                            string character = Convert.ToString(bytesBlock[Pos1 + AA], 16);
                            StrNumber = StrNumber + (character.Length == 1 ? "0" : "") + character;
                        }

                        string FirstSimbol = StrNumber.Substring(0, 1);
                        StrNumber = StrNumber.Substring(1, Field.Length);
                        if (string.IsNullOrEmpty(StrNumber))
                        {
                            BytesVal = 0;
                        }
                        else
                        {
                            BytesVal = Convert.ToInt32(StrNumber) / (Field.Precision > 0 ? Field.Precision * 10 : 1);
                            if (FirstSimbol == "0")
                            {
                                BytesVal = (int)BytesVal * -1;
                            }
                        }
                    }
                    else if (Field.Type == "NVC")
                    {
                        // Строка переменной длины
                        var BytesStr = new byte[2];
                        for (int AA = 0; AA <= 1; AA++)
                            BytesStr[AA] = bytesBlock[Pos1 + AA + Field.CouldBeNull];
                        var L = Math.Min(Field.Size, (BytesStr[0] + (BytesStr[1] * 256)) * 2);
                        BytesVal = Encoding.Unicode.GetString(bytesBlock, Pos1 + 2 + Field.CouldBeNull, Convert.ToInt32(L)).Trim(); // was L- 2
                    }
                    else if (Field.Type == "NC")
                    {
                        // строка фиксированной длины
                        BytesVal = Encoding.Unicode.GetString(bytesBlock, Pos1, Field.Size);
                    }

                    Dict.Add(Field.Name, BytesVal);
                    FieldStartPos += Field.Size;
                }

                PageHeader.Records.Add(Dict);
            }
        }
    }
}