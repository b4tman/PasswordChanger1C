using Moq;
using System;
using System.Data;
using Xunit;

namespace PasswordChanger1C.Tests
{
    public class SQLInfobase_Tests
    {
        private const string Test_Password = "1";
        private readonly static byte[] Test_Id = FromBase64("k42MsT0XVTlFDlfbSk/naQ==");
        private readonly static string Test_IdStr = BitConverter.ToString(Test_Id).Replace("-", "");

        private readonly static byte[] Test_Data = FromBase64(@"
                GPJBHWrITsJOq25wcpiLMoP5YTI4Nr85ER36ohH8L/YozllGS7W+BeebTAYNBtoUKMF5eUfwLaB/mApBRa2+AbrVQ+Kq5g/
                okCPOP0bqoX3ziUJSogpbglJ4sL0aGo8JIcJxLVr4Y/J+m15dQqi7Aq7JUQIIG48JIcJxLVr4fvJ+m0J9eOO6HredUwoAVI
                1dPMMgf1nlevN9kkMREPvoH7efA1cPB90BJJZ0fxfkLaR9nwxCE62mCrXJVx8MVIoAPJB1LgzleaR7yQ9AS/27ArDLTQMUB
                5MVId5xMUiGGa0U4F0bJuvOStavUQJhQdAIVscrcT+DBbFziUJSPM/kaMjKCmZLc8dsR8JxRB2nf4V7wQIlOdP4D6HVUx4J
                Go0JI8NxJVr6f/p7mFpFXqinAq/0a0kIS5MIPf9LZlq1YvNim0JBXrqpTw=="
        );

        private static byte[] FromBase64(in string data)
        {
            var replaces = "\n,\r, ,\t".Split(',');
            string base64str = data;
            foreach (var replace in replaces)
            {
                base64str = base64str.Replace(replace, "");
            }
            return Convert.FromBase64String(base64str);
        }

        [Fact()]
        public void GetAll_PostgreSQL_Test()
        {
            // https://stackoverflow.com/questions/58375054/mocking-sqlconnection-sqlcommand-and-sqlreader-in-c-sharp-using-mstest

            var readerMock = new Mock<IDataReader>();
            readerMock.SetupSequence(_ => _.Read())
                .Returns(true)
                .Returns(false);

            readerMock.Setup(reader => reader.GetValue(0)).Returns(Test_Id);      //ID
            readerMock.Setup(reader => reader.GetString(1)).Returns(Test_IdStr);  //IDStr
            readerMock.Setup(reader => reader.GetString(2)).Returns("TestName");  //Name
            readerMock.Setup(reader => reader.GetString(3)).Returns("TestDescr"); //Descr
            readerMock.Setup(reader => reader.GetValue(4)).Returns(Test_Data);    //Data
            readerMock.Setup(reader => reader.GetBoolean(5)).Returns(true);       //AdmRole

            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(m => m.ExecuteReader()).Returns(readerMock.Object).Verifiable();

            var connectionMock = new Mock<IDbConnection>();
            connectionMock.Setup(m => m.CreateCommand()).Returns(commandMock.Object);

            var data = new SQLInfobase.ReadUsers(SQLInfobase.DBMSType.PostgreSQL, () => connectionMock.Object);
            var result = data.GetAll();

            Assert.Single(result);
            Assert.Collection(result, x =>
            {
                Assert.Equal("TestName", x.Name);
                Assert.Equal("TestDescr", x.Descr);
                Assert.Equal("938d8cb13d175539450e57db4a4fe769", x.IDStr);
                Assert.Equal("\u2714", x.AdmRole);

                var ExpectedHashes = CommonModule.GeneratePasswordHashes(Test_Password);
                Assert.Equal(ExpectedHashes.Item1, x.PassHash);
                Assert.Equal(ExpectedHashes.Item2, x.PassHash2);
            });
            commandMock.Verify();
        }
    }
}