using System;
using System.Collections.Generic;
using System.Data;

namespace PasswordChanger1C
{
    public static class SQLInfobase
    {
        public enum DBMSType
        {
            MSSQLServer,
            PostgreSQL
        }
        public struct SQLUser
        {
            public byte[] ID;
            public string IDStr;
            public string Name;
            public string Descr;
            public byte[] Data;
            public string DataStr;
            public string PassHash;
            public string PassHash2;
            public string AdmRole;
            public int KeySize;
            public byte[] KeyData;
        }

        /// <summary>
        ///  Error when unknown DBMSType specified
        /// </summary>
        public class WrongDBMSTypeException : Exception
        {
            public WrongDBMSTypeException(string message) : base(message)
            {
            }
        }

        private abstract class SQLUserReader
        {
            public static SQLUserReader From_DBMSType(in DBMSType dbms_type)
            {
                return dbms_type switch
                {
                    DBMSType.MSSQLServer => new SQLUserReader_MSSQLServer(),
                    DBMSType.PostgreSQL => new SQLUserReader_PostgreSQL(),
                    _ => throw new WrongDBMSTypeException("unknown DBMS type"),
                };
            }
            public abstract string SelectSQL { get; }
            public abstract SQLUser ReadUser(IDataReader reader);
            protected static void ParseData(ref SQLUser SQLUser)
            {
                SQLUser.DataStr = CommonModule.DecodePasswordStructure(SQLUser.Data, ref SQLUser.KeySize, ref SQLUser.KeyData);
                var AuthStructure = ParserServices.ParsesClass.ParseString(SQLUser.DataStr);
                var Hashes = CommonModule.GetPasswordHashTuple(AuthStructure[0]);
                SQLUser.PassHash = Hashes.Item1;
                SQLUser.PassHash2 = Hashes.Item2;
            }
            protected static string Format_AdmRole(in bool AdmRole)
            {
                return AdmRole ? "\u2714" : ""; // ✔
            }
        }

        private class SQLUserReader_MSSQLServer : SQLUserReader
        {
            private const string _SelectSQL = "SELECT [ID], [Name], [Descr], [Data], [AdmRole] FROM [dbo].[v8users] ORDER BY [Name]";
            public override string SelectSQL
            {
                get { return _SelectSQL; }
            }

            public override SQLUser ReadUser(IDataReader reader)
            {
                var SQLUser = new SQLUser
                {
                    ID = (byte[])reader.GetValue(0),
                    Name = reader.GetString(1),
                    Descr = reader.GetString(2),
                    Data = (byte[])reader.GetValue(3),
                    AdmRole = Format_AdmRole(BitConverter.ToBoolean((byte[])reader.GetValue(4), 0)),
                };
                SQLUser.IDStr = new Guid(SQLUser.ID).ToString();
                ParseData(ref SQLUser);
                return SQLUser;
            }
        }

        private class SQLUserReader_PostgreSQL : SQLUserReader
        {
            private const string _SelectSQL = @"
            SELECT id, encode(id, 'hex') as idStr,
	               CAST(name AS VARCHAR(64)) AS Name,
	               CAST(descr AS VARCHAR(128)) AS Descr,
                   data,
                   admrole
            FROM public.v8users";
            public override string SelectSQL {
                get { return _SelectSQL; }
            }

            public override SQLUser ReadUser(IDataReader reader)
            {
                var SQLUser = new SQLUser
                {
                    ID = (byte[])reader.GetValue(0),
                    IDStr = reader.GetString(1),
                    Name = reader.GetString(2),
                    Descr = reader.GetString(3),
                    Data = (byte[])reader.GetValue(4),
                    AdmRole = Format_AdmRole(reader.GetBoolean(5)),
                };
                ParseData(ref SQLUser);
                return SQLUser;
            }
        }

        public class ReadUsers
        {
            private Func<IDbConnection> Factory { get; }
            private SQLUserReader UserReader { get; }

            public ReadUsers(in DBMSType dbms_type, in Func<IDbConnection> factory)
            {
                Factory = factory;
                UserReader = SQLUserReader.From_DBMSType(dbms_type);
            }

            // https://stackoverflow.com/questions/58375054/mocking-sqlconnection-sqlcommand-and-sqlreader-in-c-sharp-using-mstest
            public List<SQLUser> GetAll()
            {
                using IDbConnection connection = Factory.Invoke();
                using IDbCommand command = connection.CreateCommand();
                command.CommandText = UserReader.SelectSQL;

                connection.Open();
                using IDataReader reader = command.ExecuteReader();
                List<SQLUser> rows = new();
                while (reader.Read())
                {
                    rows.Add(UserReader.ReadUser(reader));
                }
                return rows;
            }
        }
    }
}