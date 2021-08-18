using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

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

        private abstract class SQLUserAdapter
        {
            public static SQLUserAdapter From_DBMSType(in DBMSType dbms_type)
            {
                return dbms_type switch
                {
                    DBMSType.MSSQLServer => new SQLUserAdapter_MSSQLServer(),
                    DBMSType.PostgreSQL => new SQLUserAdapter_PostgreSQL(),
                    _ => throw new WrongDBMSTypeException("unknown DBMS type"),
                };
            }

            public abstract string SelectSQL { get; }
            public abstract string UpdateSQL { get; }

            public abstract SQLUser ReadUser(IDataReader reader);
            public abstract void SetUpdateParams(IDbCommand command, in SQLUser SQLUser);

            protected static void ParseData(ref SQLUser SQLUser)
            {
                SQLUser.DataStr = CommonModule.DecodePasswordStructure(SQLUser.Data, ref SQLUser.KeySize, ref SQLUser.KeyData);
                var AuthStructure = ParserServices.ParsesClass.ParseString(SQLUser.DataStr);
                var Hashes = CommonModule.GetPasswordHashTuple(AuthStructure[0]);
                SQLUser.PassHash = Hashes.Item1.Trim('"');
                SQLUser.PassHash2 = Hashes.Item2.Trim('"');
            }

            protected static string Format_AdmRole(in bool AdmRole)
            {
                return AdmRole ? "\u2714" : ""; // ✔
            }
        }

        private class SQLUserAdapter_MSSQLServer : SQLUserAdapter
        {
            private const string _SelectSQL = "SELECT [ID], [Name], [Descr], [Data], [AdmRole] FROM [dbo].[v8users] ORDER BY [Name]";
            private const string _UpdateSQL = "UPDATE [dbo].[v8users] SET [Data] = @data WHERE [ID] = @user";

            public override string SelectSQL
            {
                get { return _SelectSQL; }
            }
            public override string UpdateSQL
            {
                get { return _UpdateSQL; }
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

            public override void SetUpdateParams(IDbCommand command, in SQLUser SQLUser)
            {
                command.Parameters.Clear();
                var param_user = command.CreateParameter();
                param_user.ParameterName = "@user";
                param_user.DbType = DbType.Binary;
                param_user.Value = SQLUser.ID;

                var param_data = command.CreateParameter();
                param_data.ParameterName = "@data";
                param_data.DbType = DbType.Binary;
                param_data.Value = SQLUser.Data;

                command.Parameters.Add(param_user);
                command.Parameters.Add(param_data);
            }
        }

        private class SQLUserAdapter_PostgreSQL : SQLUserAdapter
        {
            private const string _SelectSQL = @"
            SELECT id, encode(id, 'hex') as idStr,
	               CAST(name AS VARCHAR(64)) AS Name,
	               CAST(descr AS VARCHAR(128)) AS Descr,
                   data,
                   admrole
            FROM public.v8users";
            private const string _UpdateSQL = @"
            UPDATE public.v8users 
            SET data = @NewData 
            WHERE id = decode(@id, 'hex')";

            public override string SelectSQL
            {
                get { return _SelectSQL; }
            }
            public override string UpdateSQL
            {
                get { return _UpdateSQL; }
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

            public override void SetUpdateParams(IDbCommand command, in SQLUser SQLUser)
            {
                command.Parameters.Clear();
                var param_id = command.CreateParameter();
                param_id.ParameterName = "id";
                param_id.DbType = DbType.String;
                param_id.Value = SQLUser.IDStr;

                var param_data = command.CreateParameter();
                param_data.ParameterName = "NewData";
                param_data.DbType = DbType.Binary;
                param_data.Value = SQLUser.Data;

                command.Parameters.Add(param_id);
                command.Parameters.Add(param_data);
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