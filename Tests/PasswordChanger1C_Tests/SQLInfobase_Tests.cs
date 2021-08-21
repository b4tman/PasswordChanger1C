using Moq;
using System;
using System.Data;
using Xunit;

namespace PasswordChanger1C.Tests
{
    using static SQLInfobase;

    // https://stackoverflow.com/questions/58375054/mocking-sqlconnection-sqlcommand-and-sqlreader-in-c-sharp-using-mstest
    public class SQLInfobase_Tests
    {
        private const string Test_Password = "test_password_123";
        private static readonly byte[] Test_Id = FromBase64("k42MsT0XVTlFDlfbSk/naQ==");

        private static readonly byte[] Test_Data = FromBase64(@"
                GPJBHWrITsJOq25wcpiLMoP5YTI4Nr85ER36ohH8L/YozllGS7W+BeebTAYNBtoUKMF5eUfwLaB/mApBRa2+AbrVQ+Kq5g/
                okCPOP0bqoX3ziUJSogpbglJ4sL0aGo8JIcJxLVr4Y/J+m15dQqi7Aq7JUQIIG48JIcJxLVr4fvJ+m0J9eOO6HredUwoAVI
                1dPMMgf1nlevN9kkMREPvoH7efA1cPB90BJJZ0fxfkLaR9nwxCE62mCrXJVx8MVIoAPJB1LgzleaR7yQ9AS/27ArDLTQMUB
                5MVId5xMUiGGa0U4F0bJuvOStavUQJhQdAIVscrcT+DBbFziUJSPM/kaMjKCmZLc8dsR8JxRB2nf4V7wQIlOdP4D6HVUx4J
                Go0JI8NxJVr6f/p7mFpFXqinAq/0a0kIS5MIPf9LZlq1YvNim0JBXrqpTw=="
        );

        private static readonly Tuple<string, string> Test_Hashes = CommonModule.GeneratePasswordHashes(Test_Password);

        private class SQLUser_MockData
        {
            public SQLUser SQLUser
            {
                get => new SQLUser
                {
                    ID = ID,
                    IDStr = IDStr,
                    Name = Name,
                    Descr = Descr,
                    Data = Data,
                    DataStr = DataStr,
                    PassHash = PasswordHashes.Item1.Trim('\"'),
                    PassHash2 = PasswordHashes.Item2.Trim('\"'),
                    AdmRole = AdmRole ? "\u2714" : "",
                    KeySize = KeySize,
                    KeyData = KeyData
                };
            }

            public DBMSType DBMSType { get; set; }
            public byte[] ID { get; set; }
            public byte[] Data { get; set; }
            public string Name { get; set; }
            public string Descr { get; set; }
            public bool AdmRole { get; set; }
            private int KeySize { get; set; }
            private byte[] KeyData { get; set; }

            public Tuple<string, string> PasswordHashes
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
                get => _Password;
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
                    if (DBMSType.PostgreSQL == DBMSType)
                        result = BitConverter.ToString(ID).Replace("-", "").ToLower();
                    else if (DBMSType.MSSQLServer == DBMSType)
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
                DBMSType = DBMSType.PostgreSQL;
            }

            public SQLUser_MockData(DBMSType _DBMSType) : this()
            {
                DBMSType = _DBMSType;
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

        private static Mock<IDbConnection> SetupConnectionMock_GetAll(in SQLUser_MockData data)
        {
            var readerMock = new Mock<IDataReader>();
            readerMock.SetupSequence(_ => _.Read())
                .Returns(true)
                .Returns(false);

            if (DBMSType.PostgreSQL == data.DBMSType)
            {
                readerMock.Setup(reader => reader.GetValue(0)).Returns(data.ID);        //ID
                readerMock.Setup(reader => reader.GetString(1)).Returns(data.IDStr);    //IDStr
                readerMock.Setup(reader => reader.GetString(2)).Returns(data.Name);     //Name
                readerMock.Setup(reader => reader.GetString(3)).Returns(data.Descr);    //Descr
                readerMock.Setup(reader => reader.GetValue(4)).Returns(data.Data);      //Data
                readerMock.Setup(reader => reader.GetBoolean(5)).Returns(data.AdmRole); //AdmRole
            }
            else if (DBMSType.MSSQLServer == data.DBMSType)
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

        private static Mock<IDbConnection> SetupConnectionMock_Update(in SQLUser ExpectedUser)
        {
            var user = ExpectedUser;
            var data_is_set = false;
            var id_is_set = false;

            IDbDataParameter NewParameter()
            {
                var ParamMock = new Mock<IDbDataParameter>();
                ParamMock.SetupAllProperties();
                ParamMock.SetupSet(_ => _.Value = It.IsAny<byte[]>()).Callback<object>(data =>
                {
                    // check if new Data param value is equal with testing user Data

                    var b = (byte[])data;
                    bool is_data = b.Length > 20;
                    if (is_data)
                    {
                        Assert.False(data_is_set);
                        Assert.Equal(user.Data, b);
                        data_is_set = true;
                    }
                    else // check ID (MSSQLServer)
                    {
                        Assert.False(id_is_set);
                        Assert.Equal(user.ID, b);
                        id_is_set = true;
                    }
                });
                ParamMock.SetupSet(_ => _.Value = It.IsAny<string>()).Callback<object>(val =>
                {
                    // check if new IDStr param value is equal with testing user IDStr (PostgreSQL)

                    var s = (string)val;
                    Assert.False(id_is_set);
                    Assert.Equal(user.IDStr, s);
                    id_is_set = true;
                });
                return ParamMock.Object;
            };

            var ParamsMock = new Mock<IDataParameterCollection>();
            ParamsMock.SetupAllProperties();

            var commandMock = new Mock<IDbCommand>();
            commandMock.SetupGet(_ => _.Parameters).Returns(ParamsMock.Object);
            commandMock.Setup(m => m.CreateParameter()).Returns(NewParameter);
            commandMock.Setup(m => m.ExecuteNonQuery()).Returns(() => data_is_set && id_is_set ? 1 : 0);
            //commandMock.Setup(m => m.Dispose()).Callback(() =>
            //{
            //    Assert.True(data_is_set);
            //    Assert.True(id_is_set);
            //    ParamsMock.Verify();
            //    commandMock.VerifyAll();
            //});

            var connectionMock = new Mock<IDbConnection>();
            connectionMock.Setup(m => m.CreateCommand()).Returns(commandMock.Object);
            //connectionMock.Setup(m => m.Dispose()).Callback(() =>
            //{
            //    connectionMock.VerifyAll();
            //});

            return connectionMock;
        }

        [Theory]
        [InlineData(DBMSType.PostgreSQL)]
        [InlineData(DBMSType.MSSQLServer)]
        public void GetAll_Test(in DBMSType DBMSType)
        {
            var data = new SQLUser_MockData(DBMSType);
            var Expected = data.SQLUser;

            var connectionMock = SetupConnectionMock_GetAll(data);
            var Users = new Users(DBMSType, () => connectionMock.Object);
            var result = Users.GetAll();

            connectionMock.VerifyAll();

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

        [Fact]
        public void UpdatePassword_ChangeHashes()
        {
            var data = new SQLUser_MockData();
            var User = data.SQLUser;
            var NewPassword = Guid.NewGuid().ToString();

            data.Password = NewPassword;
            var ExpectedUser = data.SQLUser;

            Assert.NotEqual(ExpectedUser.PassHash, User.PassHash);
            Assert.NotEqual(ExpectedUser.PassHash2, User.PassHash2);

            UpdatePassword(ref User, NewPassword);

            Assert.Equal(ExpectedUser.PassHash, User.PassHash);
            Assert.Equal(ExpectedUser.PassHash2, User.PassHash2);
        }

        [Theory]
        [InlineData(DBMSType.PostgreSQL)]
        [InlineData(DBMSType.MSSQLServer)]
        public void Update_Test(in DBMSType DBMSType)
        {
            var data = new SQLUser_MockData(DBMSType);
            var User = data.SQLUser;
            var NewPassword = Guid.NewGuid().ToString();

            data.Password = NewPassword;
            var ExpectedUser = data.SQLUser;

            var connectionMock = SetupConnectionMock_Update(ExpectedUser);
            var Users = new Users(DBMSType, () => connectionMock.Object);

            UpdatePassword(ref User, NewPassword);
            var is_ok = Users.Update(User);

            connectionMock.VerifyAll();

            Assert.True(is_ok);
        }
    }
}