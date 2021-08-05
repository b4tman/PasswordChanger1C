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
    }
}