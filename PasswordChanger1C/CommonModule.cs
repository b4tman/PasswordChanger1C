using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PasswordChanger1C
{
    public static class CommonModule
    {
        public static string Format_AdmRole(in bool AdmRole)
        {
            return AdmRole ? "\u2714" : ""; // ✔
        }

        public static string DecodePasswordStructure(in byte[] bytes_Input, ref int KeySize, ref byte[] KeyData)
        {
            short Base = bytes_Input[0];
            KeySize = Base;
            KeyData = new byte[Base];

            bytes_Input.AsMemory(1, Base).CopyTo(KeyData.AsMemory(0, Base));

            int i = Base + 1;
            int j = 1;
            int MaxI = bytes_Input.Length;

            var BytesResult = new byte[MaxI - Base];
            while (i < MaxI)
            {
                if (j > Base)
                {
                    j = 1;
                }

                BytesResult[i - Base - 1] = Convert.ToByte(bytes_Input[i] ^ bytes_Input[j]); // 239 for first

                i++;
                j++;
            }

            return Encoding.UTF8.GetString(BytesResult);
        }

        public static byte[] EncodePasswordStructure(in string Str, int KeySize, in byte[] KeyData)
        {
            var bytes_Input = Encoding.UTF8.GetBytes(Str);
            int Base = KeySize;
            var BytesResult = new byte[bytes_Input.Length + Base];
            BytesResult[0] = Convert.ToByte(Base);

            KeyData.AsMemory(0, Base).CopyTo(BytesResult.AsMemory(1, Base));

            int MaxI = bytes_Input.Length - 1;
            int i = 1;
            int j = 1;

            while (i <= MaxI)
            {
                if (j > Base)
                {
                    j = 1;
                }

                BytesResult[i + Base] = Convert.ToByte(bytes_Input[i - 1] ^ BytesResult[j]);

                i++;
                j++;
            }

            return BytesResult;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "SecurityIntelliSenseCS:MS Security rules violation", Justification = "SHA-1 hash type")]
        public static string EncryptStringSHA1(in string Str)
        {
            var sha = new SHA1CryptoServiceProvider(); // declare sha as a new SHA1CryptoServiceProvider
            byte[] bytesToHash; // and here is a byte variable
            bytesToHash = Encoding.UTF8.GetBytes(Str); // covert the password into ASCII code
            bytesToHash = sha.ComputeHash(bytesToHash); // this is where the magic starts and the encryption begins
            return Convert.ToBase64String(bytesToHash);
        }

        public static bool IsPasswordHash(in string hashstr)
        {
            // SHA-1 hash size is 160 bit
            const int hash_size = 20; // 160 / 8
            const int base64_size = 28; // ((4 * hash_size / 3) + 3) & ~3;

            if (string.IsNullOrEmpty(hashstr)) return false;

            string base64String = hashstr.Trim('"');
            if (base64String.Length != base64_size
                || base64String.Contains(" ") || base64String.Contains("\t")
                || base64String.Contains("\r") || base64String.Contains("\n"))
                return false;
            try
            {
                byte[] decoded = Convert.FromBase64String(base64String);
                return hash_size == decoded.Length;
            }
            catch (Exception) { }
            return false;
        }

        public static Tuple<string, string> GetPasswordHashTuple(in ParserServices.ParserList AuthStructure)
        {
            string[] result = { "", "" };

            var Hashes = AuthStructure
                 .Where(Item => !Item.IsList)
                 .Select(Item => Item.ToString().Trim('"'))
                 .Where(Item => IsPasswordHash(Item))
                 .Take(2);

            int i = 0;
            foreach (var hash in Hashes)
            {
                result[i] = hash;
                i++;
            }
            return Tuple.Create(result[0], result[1]);
        }

        public static string ReplaceHashes(in string Data, in Tuple<string, string> OldHashes, in Tuple<string, string> NewHashes)
        {
            // Hashes can be with or without double quotes
            static string EnsureDQuotes(string str) => str.StartsWith("\"") && str.EndsWith("\"") ? str : $"\"{str}\"";

            string[] old_arr = { OldHashes.Item1, OldHashes.Item2 };
            string[] new_arr = { NewHashes.Item1, NewHashes.Item2 };
            var old_hashes = old_arr.Select(EnsureDQuotes);
            var new_hashes = new_arr.Select(EnsureDQuotes);
            string OldStr = string.Join(",", old_hashes);
            string NewStr = string.Join(",", new_hashes);
            return Data.Replace(OldStr, NewStr);
        }

        public static Tuple<string, string> GeneratePasswordHashes(in string password)
        {
            return Tuple.Create(EncryptStringSHA1(password), EncryptStringSHA1(password.ToUpper()));
        }

        private static int GetFieldSize(in string FieldType, int FieldLength, int CouldBeNull)
        {
            return FieldType switch
            {
                "B" => FieldLength,
                "L" => 1,
                "N" => (FieldLength + 2) / 2,
                "NC" => FieldLength * 2,
                "NVC" => FieldLength * 2 + 2,
                "RV" => 16,
                "I" => 8,
                "T" => 8,
                "DT" => 7,
                "NT" => 8,
                _ => 0,
            } + CouldBeNull;
        }

        public static int GetFieldSize(in AccessFunctions.TableFields Field)
        {
            return GetFieldSize(Field.Type, Field.Length, Field.CouldBeNull);
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
                if (!a.IsList)
                {
                    continue;
                }

                var Field = new AccessFunctions.TableFields();
                Field.Name = a[0].ToString().Replace("\"", "");
                Field.Type = a[1].ToString().Replace("\"", "");
                Field.CouldBeNull = Convert.ToInt32(a[2].ToString());
                Field.Length = Convert.ToInt32(a[3].ToString());
                Field.Precision = Convert.ToInt32(a[4].ToString());
                int FieldSize = GetFieldSize(Field);              

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
        }
    }
}