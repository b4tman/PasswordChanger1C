using System;
using System.IO;
using Xunit;

namespace PasswordChanger1C.Tests
{
    public class AccessFunctions_Tests
    {
        const string File838 = "./Resources/8.3.8.1CD";
        const string File8214 = "./Resources/8.2.14.1CD";
        const string FileRepo8214 = "./Resources/repo_8.2.14.1CD";
        const string FileRepo838 = "./Resources/repo_8.3.8.1CD";
        const string Password838 = "1";
        const string Password8214 = "1";

        [Theory]
        [InlineData(File838)]
        [InlineData(File8214)]
        [InlineData(FileRepo8214)]
        [InlineData(FileRepo838)]
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
            var TableParams = AccessFunctions.ReadInfoBase(filename, "V8USERS");
            Assert.NotNull(TableParams.Records);

            AccessFunctions.ParseUsersData_IB(ref TableParams.Records);

            string Password = (TableParams.DatabaseVersion == "8.3.8") ? Password838 : Password8214;
            var ExpectedHashes = CommonModule.GeneratePasswordHashes(Password);

            Assert.Collection(TableParams.Records, 
                (Row) => 
                {
                    Assert.Equal("", Row["NAME"]);
                },
                (Row) =>
                {
                    Assert.NotEqual("", Row["NAME"].ToString());
                    Assert.Equal(ExpectedHashes.Item1, Row["UserPassHash"].ToString());
                    Assert.Equal(ExpectedHashes.Item2, Row["UserPassHash2"].ToString());
                }
            );
        }

        [Theory]
        [InlineData(FileRepo8214)]
        [InlineData(FileRepo838)]
        public void ReadInfoBase_RepoTest(string filename)
        {
            var TableParams = AccessFunctions.ReadInfoBase(filename, "USERS");
            Assert.NotNull(TableParams.Records);

            foreach (var Row in TableParams.Records)
            {
                string username = Row["NAME"].ToString();
                if ("TestNoPassword" == username)
                {
                    Assert.Equal(AccessFunctions.InfoBaseRepo_EmptyPassword, Row["PASSWORD"].ToString());
                }
                else if ("Test" == username)
                {
                    Assert.NotEqual(AccessFunctions.InfoBaseRepo_EmptyPassword, Row["PASSWORD"].ToString());
                }
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
                var NewHashes = CommonModule.GeneratePasswordHashes(NewPassword);

                // read infobase
                var TableParams = AccessFunctions.ReadInfoBase(tmp_filename, "V8USERS");
                Assert.NotNull(TableParams.Records);
                AccessFunctions.ParseUsersData_IB(ref TableParams.Records);

                var Row = TableParams.Records[1];
                var Hashes = Tuple.Create(Row["UserPassHash"].ToString(), Row["UserPassHash2"].ToString());

                var OldDataBinary = Row["DATA_BINARY"];
                string OldData = Row["DATA"].ToString();

                // change value
                string NewData = CommonModule.ReplaceHashes(OldData, Hashes, NewHashes);
                var NewBytes = CommonModule.EncodePasswordStructure(NewData, Convert.ToInt32(Row["DATA_KEYSIZE"]), (byte[])Row["DATA_KEY"]);

                // write
                AccessFunctions.WritePasswordIntoInfoBaseIB(tmp_filename, TableParams, (byte[])OldDataBinary, NewBytes, Convert.ToInt32(Row["DATA_POS"]), Convert.ToInt32(Row["DATA_SIZE"]));

                // read new infobase
                var TableParams_New = AccessFunctions.ReadInfoBase(tmp_filename, "V8USERS");
                Assert.NotNull(TableParams_New.Records);
                AccessFunctions.ParseUsersData_IB(ref TableParams_New.Records);

                var Row_New = TableParams_New.Records[1];
                var Hashes_New = Tuple.Create(Row_New["UserPassHash"].ToString(), Row_New["UserPassHash2"].ToString());

                // check passwords
                Assert.Equal(NewHashes.Item1, Hashes_New.Item1);
                Assert.Equal(NewHashes.Item2, Hashes_New.Item2);
            }
        }

        [Theory]
        [InlineData(FileRepo8214)]
        [InlineData(FileRepo838)]
        public void WritePasswordIntoInfoBaseRepo_Test(string original_filename)
        {
            string tmp_folder = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString());
            using (TempStorage tmp_storage = new TempStorage(tmp_folder))
            {
                string tmp_filename = Path.Join(tmp_folder, "test.1cd");
                File.Copy(original_filename, tmp_filename);

                string NewPassword = "test123";
                var NewHashes = CommonModule.GeneratePasswordHashes(NewPassword);

                // read infobase
                var TableParams = AccessFunctions.ReadInfoBase(tmp_filename, "USERS");
                var Row = TableParams.Records[0];
                Assert.Equal("Test", Row["NAME"].ToString());
                Assert.NotEqual(AccessFunctions.InfoBaseRepo_EmptyPassword, Row["PASSWORD"].ToString());

                // write
                AccessFunctions.WritePasswordIntoInfoBaseRepo(tmp_filename, TableParams, Convert.ToInt32(Row["OFFSET_PASSWORD"]));

                // read new infobase
                var TableParams_New = AccessFunctions.ReadInfoBase(tmp_filename, "USERS");
                var Row_New = TableParams_New.Records[0];

                // check data
                Assert.Equal(Row["NAME"].ToString(), Row_New["NAME"].ToString());
                Assert.NotEqual(Row["PASSWORD"].ToString(), Row_New["PASSWORD"].ToString());
                Assert.Equal(AccessFunctions.InfoBaseRepo_EmptyPassword, Row_New["PASSWORD"].ToString());
            }
        }
    }
}