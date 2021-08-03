using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualBasic.CompilerServices;

namespace PasswordChanger1C
{
    static class CommonModule
    {
        public static string DecodePasswordStructure(byte[] bytes_Input, ref int KeySize, ref byte[] KeyData)
        {
            short Base = Convert.ToInt16(Conversions.ToString(bytes_Input[0]), 10);
            KeySize = Base;
            KeyData = new byte[Base];
            for (int a = 1, loopTo = Base; a <= loopTo; a++)
                KeyData[a - 1] = bytes_Input[a];
            int i = Base + 1;
            int j = 1;
            int MaxI = bytes_Input.Length;
            var BytesResult = new byte[(MaxI - Base)];
            while (i < MaxI)
            {
                if (j > Base)
                {
                    j = 1;
                }

                short AA = Convert.ToInt16(Conversions.ToString(bytes_Input[i]), 10);
                short BB = Convert.ToInt16(Conversions.ToString(bytes_Input[j]), 10);
                byte CC = Convert.ToByte(AA ^ BB); // 239 for first
                BytesResult.SetValue(CC, i - Base - 1);
                i = i + 1;
                j = j + 1;
            }

            return Encoding.UTF8.GetString(BytesResult);
        }

        public static byte[] EncodePasswordStructure(string Str, int KeySize, byte[] KeyData)
        {
            var bytes_Input = Encoding.UTF8.GetBytes(Str);
            int Base = KeySize;
            var BytesResult = new byte[(bytes_Input.Length + Base)];
            BytesResult.SetValue(Convert.ToByte(Base), 0);
            for (int ii = 1, loopTo = Base; ii <= loopTo; ii++)
                BytesResult.SetValue(KeyData[ii - 1], ii);
            int MaxI = bytes_Input.Length - 1;
            int i = 1;
            int j = 1;
            while (i <= MaxI)
            {
                if (j > Base)
                {
                    j = 1;
                }

                short AA = Convert.ToInt16(Conversions.ToString(bytes_Input[i - 1]), 10);
                short BB = Convert.ToInt16(Conversions.ToString(BytesResult[j]), 10);
                byte CC = Convert.ToByte(AA ^ BB);
                BytesResult.SetValue(CC, i + Base);
                i = i + 1;
                j = j + 1;
            }

            return BytesResult;
        }

        public static string EncryptStringSHA1(string Str)
        {
            var sha = new SHA1CryptoServiceProvider(); // declare sha as a new SHA1CryptoServiceProvider
            byte[] bytesToHash; // and here is a byte variable
            bytesToHash = Encoding.UTF8.GetBytes(Str); // covert the password into ASCII code
            bytesToHash = sha.ComputeHash(bytesToHash); // this is where the magic starts and the encryption begins
            return Convert.ToBase64String(bytesToHash);
            string result = "";
            foreach (byte b in bytesToHash)
                result += b.ToString("x2");
            return result;
        }

        public static void ParseTableDefinition(ref AccessFunctions.PageParams PageHeader)
        {
            var ParsedString = ParserServices.ParsesClass.ParseString(PageHeader.TableDefinition);
            PageHeader.Fields = new List<AccessFunctions.TableFields>();
            int RowSize = 1;
            string TableName = ParsedString[0][0].ToString().Replace("\"", "").ToUpper();
            PageHeader.TableName = TableName;
            foreach (var a in ParsedString[0][2])
            {
                if (Conversions.ToBoolean(!a.IsList))
                {
                    continue;
                }

                var Field = new AccessFunctions.TableFields();
                Field.Name = a[0].ToString().Replace("\"", "");
                Field.Type = a[1].ToString().Replace("\"", "");
                Field.CouldBeNull = Conversions.ToInteger(a[2].ToString());
                Field.Length = Conversions.ToInteger(a[3].ToString());
                Field.Precision = Conversions.ToInteger(a[4].ToString());
                int FieldSize = Field.CouldBeNull;
                if (Field.Type == "B")
                {
                    FieldSize = FieldSize + Field.Length;
                }
                else if (Field.Type == "L")
                {
                    FieldSize = FieldSize + 1;
                }
                else if (Field.Type == "N")
                {
                    FieldSize = (int)Math.Round(FieldSize + Math.Truncate((Field.Length + 2) / 2d));
                }
                else if (Field.Type == "NC")
                {
                    FieldSize = FieldSize + Field.Length * 2;
                }
                else if (Field.Type == "NVC")
                {
                    FieldSize = FieldSize + Field.Length * 2 + 2;
                }
                else if (Field.Type == "RV")
                {
                    FieldSize = FieldSize + 16;
                }
                else if (Field.Type == "I")
                {
                    FieldSize = FieldSize + 8;
                }
                else if (Field.Type == "T")
                {
                    FieldSize = FieldSize + 8;
                }
                else if (Field.Type == "DT")
                {
                    FieldSize = FieldSize + 7;
                }
                else if (Field.Type == "NT")
                {
                    FieldSize = FieldSize + 8;
                }

                Field.Size = FieldSize;
                Field.Offset = RowSize;
                RowSize = RowSize + FieldSize;
                PageHeader.Fields.Add(Field);
            }

            PageHeader.RowSize = RowSize;

            // {"Files",118,119,96}
            // Данные, BLOB, индексы

            int BlockData = Convert.ToInt32(ParsedString[0][5][1].ToString());
            int BlockBlob = Convert.ToInt32(ParsedString[0][5][2].ToString());
            PageHeader.BlockData = BlockData;
            PageHeader.BlockBlob = BlockBlob;
        }
    }
}