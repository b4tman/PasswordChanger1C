using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PasswordChanger1C
{
    using static AccessFunctions;

    static class DatabaseAccess838
    {
        public static AccessFunctions.PageParams ReadInfoBase(InfobaseBinaryReader reader, in string TargetTableName)
        {
            // второй блок пропускаем
            //reader.BaseStream.Seek(reader.PageSize, SeekOrigin.Current);

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
                string StrDefinition = Encoding.UTF8.GetString(BytesTableStructure, Position + 6, StringLen);
                while (NextBlock > 0)
                {
                    Position = NextBlock * 256;
                    NextBlock = BitConverter.ToInt32(BytesTableStructure, Position);
                    StringLen = BitConverter.ToInt16(BytesTableStructure, Position + 4);
                    StrDefinition += Encoding.UTF8.GetString(BytesTableStructure, Position + 6, StringLen);
                }

                var TableDefinition = ParserServices.ParsesClass.ParseString(StrDefinition);
                if (TableDefinition[0][0].ToString().ToUpper() == TargetTable)
                {
                    Page.TableDefinition = StrDefinition;
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
            long BlockBlob = PageHeader.BlockBlob;
            int PageSize = PageHeader.PageSize;
            PageHeader.Records = new List<Dictionary<string, object>>();
            var DataPageBuffer = reader.ReadPage(FirstPage);

            var DataPage = ReadObjectPageDefinition(DataPageBuffer, PageSize);
            DataPage.BinaryData = ReadAllStoragePagesForObject(reader, DataPage);
            var bytesBlock = DataPage.BinaryData;
            long Size = DataPage.Length / PageHeader.RowSize;
            for (int i = 1; i < Size; i++)
            {
                int Pos = (int)PageHeader.RowSize * i;
                int FieldStartPos = 0;
                bool IsDeleted = BitConverter.ToBoolean(bytesBlock, Pos);
                var Dict = new Dictionary<string, object>();
                Dict.Add("IsDeleted", IsDeleted);
                foreach (var Field in PageHeader.Fields)
                {
                    int Pos1 = Pos + 1 + FieldStartPos;
                    if (Field.Name == "PASSWORD")
                    {
                        Dict.Add("OFFSET_PASSWORD", Pos1);
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
                            BytesVal = new DateTime((BytesDate[0] * 100) + BytesDate[1],
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
                            if (Field.Name == "DATA")
                            {
                                Dict.Add("DATA_POS", DataPos);
                                Dict.Add("DATA_SIZE", DataSize);
                            }

                            var BlobPageBuffer = reader.ReadPage(BlockBlob);
                            var BlobPage = ReadObjectPageDefinition(BlobPageBuffer, PageSize);
                            BlobPage.BinaryData = ReadAllStoragePagesForObject(reader, BlobPage);
                            int[] argDataPositions = null;
                            var BytesValTemp = GetCleanDataFromBlob(DataPos, DataSize, BlobPage.BinaryData, DataPositions: ref argDataPositions);
                            // ***************************************

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
                            var character = Convert.ToString(bytesBlock[Pos1 + AA], 16);
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
                        int L = Math.Min(Field.Size, (BytesStr[0] + BytesStr[1] * 256) * 2);
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

        private static byte[] GetCleanDataFromBlob(in int Dataindex, in int Datasize, in byte[] bytesBlock, [Optional, DefaultParameterValue(null)] ref int[] DataPositions)
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
            int PagesCountTableStructure = Page.PagesNum.Count;
            var BytesTableStructure = new byte[PagesCountTableStructure * Page.PageSize];
            int i = 0;
            foreach (var blk in Page.PagesNum)
            {
                var PageBuffer = reader.ReadPage(blk);
                PageBuffer.AsMemory(0, Page.PageSize).CopyTo(BytesTableStructure.AsMemory(i, Page.PageSize));
                i += Page.PageSize;
            }

            return BytesTableStructure;
        }

        private static AccessFunctions.PageParams ReadObjectPageDefinition(in byte[] Bytes, in int PageSize)
        {

            // struct {
            // unsigned int object_type; //0xFD1C или 0x01FD1C 
            // unsigned Int version1; 
            // unsigned Int version2; 
            // unsigned Int version3; 
            // unsigned Long int length; //64-разрядное целое! 
            // unsigned Int pages[]; 
            // }

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

        public static void WritePasswordIntoInfoBaseIB(in string FileName, in AccessFunctions.PageParams PageHeader, in byte[] OldData, in byte[] NewData, in int DataPos, in int DataSize)
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
            if (TargetData.SequenceEqual(OldData))
            {
                if (OldData.Count() == NewData.Count())
                {
                    int CurrentByte = 0;
                    // Data is stored in 256 bytes blocks (6 bytes reserved for next block number and size)
                    foreach (var Position in DataPositions)
                    {
                        int CopyCount = Math.Min(250, NewData.Count() - CurrentByte);
                        NewData.AsMemory(CurrentByte, CopyCount)
                            .CopyTo(BlobPage.BinaryData.AsMemory(Position));
                        CurrentByte += CopyCount;
                    }

                    // Blob page(s) has been modified. Let's write it back to database
                    using var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write);
                    using var writer = new BinaryWriter(fs);
                    CurrentByte = 0;
                    foreach (var Position in BlobPage.PagesNum)
                    {
                        var TempBlock = new byte[PageSize];
                        BlobPage.BinaryData.AsMemory(CurrentByte, PageSize).CopyTo(TempBlock.AsMemory());
                        CurrentByte += PageSize;

                        writer.BaseStream.Seek(Position * (long)PageSize, SeekOrigin.Begin);
                        writer.Write(TempBlock);
                    }
                }
                else
                {
                    throw new Exception("Новый байтовый массив должен совпадать по размерам со старым массивом (т.к. мы только заменяем хэши одинаковой длины)." + Environment.NewLine + "Сообщите пожалуйста об этой ошибке!");
                }
            }
            else
            {
                throw new Exception("Информация в БД была изменена другим процессом! Прочитайте список пользователей заново.");
            }
        }

        public static void WritePasswordIntoInfoBaseRepo(in string FileName, in AccessFunctions.PageParams PageHeader, in int Offset, in string NewPass = null)
        {
            int PageSize = PageHeader.PageSize;
            AccessFunctions.PageParams DataPage;
            string PassStr = string.IsNullOrEmpty(NewPass) ? AccessFunctions.InfoBaseRepo_EmptyPassword : NewPass;
            var Pass = Encoding.Unicode.GetBytes(PassStr);
            int CurrentByte;

            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var reader = new AccessFunctions.InfobaseBinaryReader(fs, PageHeader.PageSize);
                var DataPageBuffer = reader.ReadPage(PageHeader.BlockData);
                DataPage = ReadObjectPageDefinition(DataPageBuffer, PageSize);
                DataPage.BinaryData = ReadAllStoragePagesForObject(reader, DataPage);
            }
            //Encoding.Unicode.GetString(DataPage.BinaryData, Offset, Pass.Length)

            Pass.CopyTo(DataPage.BinaryData.AsMemory(Offset, Pass.Length));

            // Data page(s) has been modified. Let's write it back to database
            using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Write))
            {
                using var writer = new BinaryWriter(fs);
                CurrentByte = 0;
                foreach (var Position in DataPage.PagesNum)
                {
                    var TempBlock = new byte[PageSize];
                    DataPage.BinaryData.AsMemory(CurrentByte, PageSize).CopyTo(TempBlock.AsMemory());
                    CurrentByte += PageSize;

                    writer.BaseStream.Seek(Position * (long)PageSize, SeekOrigin.Begin);
                    writer.Write(TempBlock);
                }
            }
        }
    }
}