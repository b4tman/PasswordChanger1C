using Xunit;
using PasswordChanger1C;
using Newtonsoft.Json;
using System;
using System.IO;

namespace PasswordChanger1C.Tests
{
    public class AccessFunctions_Tests
    {
        const string File838 = "./Resources/8.3.8.1CD";
        const string File8214 = "./Resources/8.2.14.1CD";
        const string Password838 = "1";
        const string Password8214 = "1";

        [Theory]
        [InlineData(File838)]
        [InlineData(File8214)]
        public void Resources_Test(string filename)
        {
            Assert.True(File.Exists(filename));
        }

        [Theory]
        [InlineData(File838, "8.3.8")]
        [InlineData(File8214, "8.2.14")]
        public void ReadInfoBase_CheckVersions(string filename, string version)
        {
            var TableParams = AccessFunctions.ReadInfoBase(filename, "V8USERS");
            Assert.Equal(TableParams.DatabaseVersion, version);
        }

        [Theory]
        [InlineData(File838)]
        [InlineData(File8214)]
        public void ReadInfoBase_TestPasswords(string filename)
        {
            // from MainForm.cs
            var TableParams = AccessFunctions.ReadInfoBase(filename, "V8USERS");
            foreach (var Row in TableParams.Records)
            {
                if (string.IsNullOrEmpty(Row["NAME"].ToString()))
                {
                    continue; // skip default user
                }
                var AuthStructure = ParserServices.ParsesClass.ParseString(Row["DATA"].ToString())[0];
                var hash_offset = 11; // 8.2.14 hash_offset == 8.3.8
                string PassHash = AuthStructure[0][hash_offset].ToString().Trim('"');
                string PassHash2 = AuthStructure[0][hash_offset + 1].ToString().Trim('"');

                Assert.Equal(PassHash, PassHash2);

                string Password = (TableParams.DatabaseVersion == "8.3.8") ? Password838 : Password8214;
                string NewHash = CommonModule.EncryptStringSHA1(Password);

                Assert.Equal(NewHash, PassHash);

                break; // first user only
            }
        }

        [Theory]
        [InlineData(File838)]
        [InlineData(File8214)]
        public void WritePasswordIntoInfoBaseIB_Test(string original_filename)
        {
            string tmp_folder = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());
            using (TempStorage tmp_storage = new TempStorage(tmp_folder))
            {
                string tmp_filename = Path.Join(tmp_folder, "test.1cd");
                File.Copy(original_filename, tmp_filename);

                string NewPassword = "test123";
                string NewHash = CommonModule.EncryptStringSHA1(NewPassword);
                string NewHash2 = CommonModule.EncryptStringSHA1(NewPassword.ToUpper());

                // read infobase
                var TableParams = AccessFunctions.ReadInfoBase(tmp_filename, "V8USERS");
                var Row = TableParams.Records[1];
                var AuthStructure = ParserServices.ParsesClass.ParseString(Row["DATA"].ToString())[0];
                var hash_offset = 11; // 8.2.14 hash_offset == 8.3.8
                string PassHash = AuthStructure[0][hash_offset].ToString();
                string PassHash2 = AuthStructure[0][hash_offset + 1].ToString();

                var OldDataBinary = Row["DATA_BINARY"];
                string OldData = Row["DATA"].ToString();
                
                // change value
                string NewData = OldData.Replace($"{PassHash},{PassHash2}", $"\"{NewHash}\",\"{NewHash2}\"");
                var NewBytes = CommonModule.EncodePasswordStructure(NewData, Convert.ToInt32(Row["DATA_KEYSIZE"]), (byte[])Row["DATA_KEY"]);

                // write
                AccessFunctions.WritePasswordIntoInfoBaseIB(tmp_filename, TableParams, (byte[])Row["ID"], (byte[])OldDataBinary, NewBytes, Convert.ToInt32(Row["DATA_POS"]), Convert.ToInt32(Row["DATA_SIZE"]));

                // read new infobase
                var TableParams_New = AccessFunctions.ReadInfoBase(tmp_filename, "V8USERS");
                var Row_New = TableParams_New.Records[1];
                var AuthStructure_New = ParserServices.ParsesClass.ParseString(Row_New["DATA"].ToString())[0];
                string PassHash_New = AuthStructure_New[0][hash_offset].ToString().Trim('"');
                string PassHash2_New = AuthStructure_New[0][hash_offset + 1].ToString().Trim('"');

                // check passwords
                Assert.Equal(NewHash, PassHash_New);
                Assert.Equal(NewHash2, PassHash2_New);
            }
        }
    }
}