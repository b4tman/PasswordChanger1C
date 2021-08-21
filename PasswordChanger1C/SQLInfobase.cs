using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace PasswordChanger1C
{
    public static partial class SQLInfobase
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

        public class Users
        {
            private Func<IDbConnection> Factory { get; }
            private SQLUserAdapter Adapter { get; }

            public Users(in DBMSType dbms_type, in Func<IDbConnection> factory)
            {
                Factory = factory;
                Adapter = SQLUserAdapter.From_DBMSType(dbms_type);
            }

            // https://stackoverflow.com/questions/58375054/mocking-sqlconnection-sqlcommand-and-sqlreader-in-c-sharp-using-mstest
            public List<SQLUser> GetAll()
            {
                using IDbConnection connection = Factory.Invoke();
                using IDbCommand command = connection.CreateCommand();
                command.CommandText = Adapter.SelectSQL;

                connection.Open();
                using IDataReader reader = command.ExecuteReader();
                List<SQLUser> rows = new();
                while (reader.Read())
                {
                    rows.Add(Adapter.ReadUser(reader));
                }
                return rows;
            }

            public bool Update(in SQLUser SQLUser)
            {
                using IDbConnection connection = Factory.Invoke();
                using IDbCommand command = connection.CreateCommand();
                command.CommandText = Adapter.UpdateSQL;
                Adapter.SetUpdateParams(command, SQLUser);
                connection.Open();
                int rows = command.ExecuteNonQuery();
                return 0 < rows;
            }
        }

        public static void UpdatePassword(ref SQLUser SQLUser, in string NewPassword)
        {
            var NewHashes = CommonModule.GeneratePasswordHashes(NewPassword);
            var OldHashes = Tuple.Create(SQLUser.PassHash, SQLUser.PassHash2);
            string NewData = CommonModule.ReplaceHashes(SQLUser.DataStr, OldHashes, NewHashes);
            var NewBytes = CommonModule.EncodePasswordStructure(NewData, SQLUser.KeySize, SQLUser.KeyData);
            SQLUser.PassHash = NewHashes.Item1;
            SQLUser.PassHash2 = NewHashes.Item2;
            SQLUser.DataStr = NewData;
            SQLUser.Data = NewBytes;
        }

        public static Func<IDbConnection> CreateConnectionFactory(in DBMSType dbms_type, string connection_str)
        {
            return dbms_type switch
            {
                SQLInfobase.DBMSType.MSSQLServer => () => new SqlConnection(connection_str),
                SQLInfobase.DBMSType.PostgreSQL => () => new NpgsqlConnection(connection_str),
                _ => throw new SQLInfobase.WrongDBMSTypeException("unknown DBMS type"),
            };
        }
    }
}