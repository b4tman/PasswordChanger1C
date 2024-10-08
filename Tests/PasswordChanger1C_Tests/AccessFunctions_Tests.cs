﻿using System;
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
                    Assert.Equal("", Row["NAME"].ToString());
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

            AccessFunctions.ParseUsersData_Repo(ref TableParams.Records);

            Assert.Collection(TableParams.Records,
                (Row) =>
                {
                    Assert.Equal("Test", Row["NAME"].ToString());
                    Assert.False((bool)Row["EMPTY_PASS"]);
                },
                (Row) =>
                {
                    Assert.Equal("TestNoPassword", Row["NAME"].ToString());
                    Assert.True((bool)Row["EMPTY_PASS"]);
                }
            );
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

                // change value
                AccessFunctions.UpdatePassword_IB(ref Row, NewPassword);

                // write
                AccessFunctions.WritePasswordIntoInfoBaseIB(tmp_filename, TableParams, Row);

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

                // read infobase
                var TableParams = AccessFunctions.ReadInfoBase(tmp_filename, "USERS");
                Assert.NotNull(TableParams.Records);
                AccessFunctions.ParseUsersData_Repo(ref TableParams.Records);

                var Row = TableParams.Records[0];
                Assert.Equal("Test", Row["NAME"].ToString());
                Assert.False((bool)Row["EMPTY_PASS"]);

                // write
                AccessFunctions.WritePasswordIntoInfoBaseRepo(tmp_filename, TableParams, Convert.ToInt32(Row["OFFSET_PASSWORD"]));

                // read new infobase
                var TableParams_New = AccessFunctions.ReadInfoBase(tmp_filename, "USERS");
                Assert.NotNull(TableParams_New.Records);
                AccessFunctions.ParseUsersData_Repo(ref TableParams_New.Records);

                // check data
                var Row_New = TableParams_New.Records[0];
                Assert.Equal(Row["NAME"].ToString(), Row_New["NAME"].ToString());
                Assert.True((bool)Row_New["EMPTY_PASS"]);
                Assert.NotEqual(Row["PASSWORD"].ToString(), Row_New["PASSWORD"].ToString());
            }
        }
    }
}