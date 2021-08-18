using Moq;
using System;
using System.Data;
using Xunit;

namespace PasswordChanger1C.Tests
{
    // https://stackoverflow.com/questions/58375054/mocking-sqlconnection-sqlcommand-and-sqlreader-in-c-sharp-using-mstest
    public class SQLInfobase_Tests
    {
        private const string Test_Password = "test_password_123";
        private readonly static byte[] Test_Id = FromBase64("k42MsT0XVTlFDlfbSk/naQ==");

        private readonly static byte[] Test_Data = FromBase64(@"
                GPJBHWrITsJOq25wcpiLMoP5YTI4Nr85ER36ohH8L/YozllGS7W+BeebTAYNBtoUKMF5eUfwLaB/mApBRa2+AbrVQ+Kq5g/
                okCPOP0bqoX3ziUJSogpbglJ4sL0aGo8JIcJxLVr4Y/J+m15dQqi7Aq7JUQIIG48JIcJxLVr4fvJ+m0J9eOO6HredUwoAVI
                1dPMMgf1nlevN9kkMREPvoH7efA1cPB90BJJZ0fxfkLaR9nwxCE62mCrXJVx8MVIoAPJB1LgzleaR7yQ9AS/27ArDLTQMUB
                5MVId5xMUiGGa0U4F0bJuvOStavUQJhQdAIVscrcT+DBbFziUJSPM/kaMjKCmZLc8dsR8JxRB2nf4V7wQIlOdP4D6HVUx4J
                Go0JI8NxJVr6f/p7mFpFXqinAq/0a0kIS5MIPf9LZlq1YvNim0JBXrqpTw=="
        );
        private readonly static Tuple<string, string> Test_Hashes = CommonModule.GeneratePasswordHashes(Test_Password);

        private class SQLUser_MockData
        {
            public SQLInfobase.SQLUser SQLUser
            {
                get 
                { 
                    return new SQLInfobase.SQLUser {
                        ID = ID,
                        IDStr = IDStr,
                        Name = Name,
                        Descr = Descr,
                        Data = Data,
                        DataStr = DataStr,
                        PassHash = PasswordHashes.Item1,
                        PassHash2 = PasswordHashes.Item2,
                        AdmRole = AdmRole ? "\u2714" : "",
                        KeySize = KeySize,
                        KeyData = KeyData
                    }; 
                }
            }

            public SQLInfobase.DBMSType DBMSType { get; set; }
            public byte[] ID { get; set; }
            public byte[] Data { get; set; }
            public string Name { get; set; }
            public string Descr { get; set; }            
            public bool AdmRole { get; set; }
            private int KeySize { get; set; }
            private byte[] KeyData { get; set; }

            public Tuple<string,string> PasswordHashes
            {
                get
                {
                    var AuthStructure = ParserServices.ParsesClass.ParseString(DataStr);
                    return CommonModule.GetPasswordHashTuple(AuthStructure[0]);
                }
            }

            private string _Password;
            public string Password 
            {
                get
                {
                    return _Password;
                }
                set
                {
                    _Password = value;
                    var NewHashes = CommonModule.GeneratePasswordHashes(_Password);
                    DataStr = CommonModule.ReplaceHashes(DataStr, PasswordHashes, NewHashes);
                } 
            }

            public string IDStr
            { 
                get 
                {
                    string result = "";
                    if (SQLInfobase.DBMSType.PostgreSQL == DBMSType)
                        result = BitConverter.ToString(ID).Replace("-", "").ToLower();
                    else if (SQLInfobase.DBMSType.MSSQLServer == DBMSType)
                        result = new Guid(ID).ToString();
                    return result;                    
                } 
            }

            public string DataStr
            {
                get
                {
                    int _KeySize = KeySize;
                    byte[] _KeyData = KeyData;
                    string result = CommonModule.DecodePasswordStructure(Data, ref _KeySize, ref _KeyData);
                    KeySize = _KeySize;
                    KeyData = _KeyData;
                    return result;
                }
                set
                {
                    Data = CommonModule.EncodePasswordStructure(value, KeySize, KeyData);
                }
            }

            public SQLUser_MockData()
            {   // default data
                ID = Test_Id;
                Data = Test_Data;
                Password = Test_Password;
                Name = "TestName";
                Descr = "TestDescr";
                AdmRole = true;
                DBMSType = SQLInfobase.DBMSType.PostgreSQL;
            }
        }

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

        private static Mock<IDbConnection> SetupConnectionMock(in SQLUser_MockData data)
        {
            var readerMock = new Mock<IDataReader>();
            readerMock.SetupSequence(_ => _.Read())
                .Returns(true)
                .Returns(false);

            if (SQLInfobase.DBMSType.PostgreSQL == data.DBMSType)
            {
                readerMock.Setup(reader => reader.GetValue(0)).Returns(data.ID);        //ID
                readerMock.Setup(reader => reader.GetString(1)).Returns(data.IDStr);    //IDStr
                readerMock.Setup(reader => reader.GetString(2)).Returns(data.Name);     //Name
                readerMock.Setup(reader => reader.GetString(3)).Returns(data.Descr);    //Descr
                readerMock.Setup(reader => reader.GetValue(4)).Returns(data.Data);      //Data
                readerMock.Setup(reader => reader.GetBoolean(5)).Returns(data.AdmRole); //AdmRole
            }
            else if (SQLInfobase.DBMSType.MSSQLServer == data.DBMSType)
            {
                byte[] AdmRole = BitConverter.GetBytes(data.AdmRole);

                readerMock.Setup(reader => reader.GetValue(0)).Returns(data.ID);     //ID
                readerMock.Setup(reader => reader.GetString(1)).Returns(data.Name);  //Name
                readerMock.Setup(reader => reader.GetString(2)).Returns(data.Descr); //Descr
                readerMock.Setup(reader => reader.GetValue(3)).Returns(data.Data);   //Data
                readerMock.Setup(reader => reader.GetValue(4)).Returns(AdmRole);     //AdmRole
            }
            else
            {
                throw new NotImplementedException();
            }

            var commandMock = new Mock<IDbCommand>();
            commandMock.Setup(m => m.ExecuteReader()).Returns(readerMock.Object);

            var connectionMock = new Mock<IDbConnection>();
            connectionMock.Setup(m => m.CreateCommand()).Returns(commandMock.Object);

            return connectionMock;
        }

        [Theory]
        [InlineData(SQLInfobase.DBMSType.PostgreSQL)]
        [InlineData(SQLInfobase.DBMSType.MSSQLServer)]
        public void GetAll_Test(in SQLInfobase.DBMSType DBMSType)
        {
            var data = new SQLUser_MockData
            {
                DBMSType = DBMSType
            };
            var Expected = data.SQLUser;

            var connectionMock = SetupConnectionMock(data);
            var Users = new SQLInfobase.Users(DBMSType, () => connectionMock.Object);
            var result = Users.GetAll();

            Assert.Single(result);
            Assert.Collection(result, Actual =>
            {
                Assert.Equal(Expected.Name, Actual.Name);
                Assert.Equal(Expected.Descr, Actual.Descr);
                Assert.Equal(Expected.IDStr, Actual.IDStr);
                Assert.Equal(Expected.AdmRole, Actual.AdmRole);

                Assert.Equal(Test_Hashes.Item1, Actual.PassHash);
                Assert.Equal(Test_Hashes.Item2, Actual.PassHash2);
            });
        }
    }
}