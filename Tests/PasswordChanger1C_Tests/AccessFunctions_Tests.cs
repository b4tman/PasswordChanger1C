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

                Assert.Equal(PassHash, NewHash);
                
                break; // first user only
            }
        }
    }
}