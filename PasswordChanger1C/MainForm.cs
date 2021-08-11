using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Windows.Forms;
using Npgsql;

namespace PasswordChanger1C
{
    public partial class MainForm
    {
        public MainForm()
        {
            InitializeComponent();
            FileIB.Text = @"C:\Users\1C\1Cv8.1CD";
            ConnectionString.Text = "Data Source=MSSQL1;Server=localhost;Integrated Security=true;Database=zup";
            cbDBType.SelectedIndex = 0;
            _Button6.Name = "Button6";
            _ButtonGetUsers.Name = "ButtonGetUsers";
            _Button2.Name = "Button2";
            _ButtonRepo.Name = "ButtonRepo";
            _ButtonGetRepoUsers.Name = "ButtonGetRepoUsers";
            _ButtonSetRepoPassword.Name = "ButtonSetRepoPassword";
            _ButtonChangePwdFileDB.Name = "ButtonChangePwdFileDB";
            _cbDBType.Name = "cbDBType";
            _ButtonChangePassSQL.Name = "ButtonChangePassSQL";
            _LinkLabel1.Name = "LinkLabel1";
            _LinkLabel2.Name = "LinkLabel2";
        }

        private AccessFunctions.PageParams TableParams;

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

        private List<SQLUser> SQLUsers = new List<SQLUser>();

        private void MainForm_Shown(object sender, EventArgs e)
        {
            int ShowWarningParameterIndex = My.MyProject.Application.CommandLineArgs.IndexOf("nowarning");
            if (ShowWarningParameterIndex == -1)
            {
                if (!ShowWarning())
                {
                    Application.Exit();
                }
            }
        }

        private static bool ShowWarning()
        {

            // TEMP
            // Return True

            var Rez = MessageBox.Show("Запрещается использование приложения для несанкционированного доступа к данным!" + Environment.NewLine + 
                                      "Используя данное приложение Вы подтверждаете, что базы данных, к которым будет предоставлен доступ, принадлежат Вашей организации " + Environment.NewLine + 
                                      "и Вы являетесь Администратором с неограниченным доступом к информации этих баз данных." + Environment.NewLine + 
                                      "Несанкционированный доступ к информации преследуются по ст. 1301 Гражданского кодекса РФ, ст. 7.12 Кодекса Российской Федерации " + Environment.NewLine + 
                                      "об административных правонарушениях, ст. 146 Уголовного кодекса РФ." + Environment.NewLine + 
                                      "Продолжить?", 
                                      "Правила использования", MessageBoxButtons.YesNo);
            if (Rez == DialogResult.Yes)
            {
                return true;
            }

            return false;
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog.FileName = FileIB.Text;
            OpenFileDialog.ShowDialog();
            FileIB.Text = OpenFileDialog.FileName;
            GetUsers();
        }

        private void ButtonGetUsers_Click(object sender, EventArgs e)
        {
            GetUsers();
        }

        public void GetUsers()
        {
            // Try

            ListViewUsers.Items.Clear();
            try
            {
                TableParams = AccessFunctions.ReadInfoBase(FileIB.Text, "V8USERS");
                LabelDatabaseVersion.Text = "Internal database version: " + TableParams.DatabaseVersion;
            }
            catch (Exception ex)
            {
                TableParams = default;
                MessageBox.Show("Ошибка при попытке чтения данных из файла информационной базы:" + Environment.NewLine +
                                ex.Message,
                                "Ошибка работы с файлом", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            if (TableParams.Records is null)
            {
                return;
            }

            foreach (var Row in TableParams.Records)
            {
                if (string.IsNullOrEmpty(Row["NAME"].ToString()))
                {
                    Row.Add("UserGuidStr", "");
                    Row.Add("UserPassHash", "");
                    Row.Add("UserPassHash2", "");
                    continue;
                }

                var AuthStructure = ParserServices.ParsesClass.ParseString(Row["DATA"].ToString())[0];
                var Hashes = CommonModule.GetPasswordHashTuple(AuthStructure[0]);
                string PassHash = Hashes.Item1;
                var G = new Guid((byte[])Row["ID"]);
                Row.Add("UserGuidStr", G.ToString());
                Row.Add("UserPassHash", Hashes.Item1);
                Row.Add("UserPassHash2", Hashes.Item2);

                var itemUserList = new ListViewItem(G.ToString());
                itemUserList.SubItems.Add(Row["NAME"].ToString());
                itemUserList.SubItems.Add(Row["DESCR"].ToString());
                itemUserList.SubItems.Add(PassHash);
                itemUserList.SubItems.Add(Convert.ToBoolean(Row["ADMROLE"]) ? "Да" : "");
                ListViewUsers.Items.Add(itemUserList);
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            switch (cbDBType.SelectedIndex)
            {
                case 0:
                    {
                        GetUsersMSSQL();
                        break;
                    }

                case 1:
                    {
                        GetUsersPostgreSQL();
                        break;
                    }
            }
        }

        public void GetUsersMSSQL()
        {

            // *****************************************************
            SQLUsers.Clear();
            SQLUserList.Items.Clear();
            try
            {
                var Connection = new SqlConnection(ConnectionString.Text);
                Connection.Open();
                var command = new SqlCommand("SELECT [ID], [Name], [Descr], [Data], [AdmRole] FROM [dbo].[v8users] ORDER BY [Name]", Connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    try
                    {
                        var SQLUser = new SQLUser();
                        SQLUser.ID = (byte[])reader.GetSqlBinary(0);
                        SQLUser.Name = reader.GetString(1);
                        SQLUser.Descr = reader.GetString(2);
                        SQLUser.Data = (byte[])reader.GetSqlBinary(3);
                        SQLUser.AdmRole = BitConverter.ToBoolean((byte[])reader.GetSqlBinary(4), 0) ? "Да" : "";
                        SQLUser.IDStr = new Guid(SQLUser.ID).ToString();
                        SQLUser.DataStr = CommonModule.DecodePasswordStructure(SQLUser.Data, ref SQLUser.KeySize, ref SQLUser.KeyData);
                        ParserServices.ParserList AuthStructure;
                        if (!(SQLUser.DataStr[0] == '{'))
                        {
                            // postgres in my test has weird first symbol
                            AuthStructure = ParserServices.ParsesClass.ParseString(SQLUser.DataStr.Substring(1));
                        }
                        else
                        {
                            AuthStructure = ParserServices.ParsesClass.ParseString(SQLUser.DataStr);
                        }

                        if (AuthStructure[0][7].ToString() == "0")
                        {
                            // нет авторизации 1С
                            SQLUser.PassHash = "нет авторизации 1С";
                        }
                        else
                        {
                            var Hashes = CommonModule.GetPasswordHashTuple(AuthStructure[0]);
                            SQLUser.PassHash = Hashes.Item1;
                            SQLUser.PassHash2 = Hashes.Item2;
                        }

                        SQLUsers.Add(SQLUser);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при попытке чтения пользователей из базы данных:" + Environment.NewLine +
                                        ex.Message,
                                        "Ошибка работы с базой данных", MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при попытке чтения пользователей из базы данных:" + Environment.NewLine +
                                 ex.Message,
                                 "Ошибка работы с базой данных", MessageBoxButtons.OK,
                                 MessageBoxIcon.Error);
                return;
            }

            // *****************************************************

            foreach (var Row in SQLUsers)
            {
                if (string.IsNullOrEmpty(Row.Name))
                {
                    continue;
                }

                var itemUserList = new ListViewItem(Row.IDStr);
                itemUserList.SubItems.Add(Row.Name);
                itemUserList.SubItems.Add(Row.Descr);
                itemUserList.SubItems.Add(Row.PassHash);
                itemUserList.SubItems.Add(Row.AdmRole);
                SQLUserList.Items.Add(itemUserList);
            }
            // *****************************************************

        }

        public void GetUsersPostgreSQL()
        {

            // *****************************************************
            SQLUsers.Clear();
            SQLUserList.Items.Clear();
            try
            {
                var Connection = new NpgsqlConnection(ConnectionString.Text);
                Connection.Open();
                var command = new NpgsqlCommand(@"SELECT id,
	                                            encode(id, 'hex') as idStr,
	                                            CAST(name AS VARCHAR(64)) AS Name,
	                                            CAST(descr AS VARCHAR(128)) AS Descr,
                                                data,
                                                admrole
                                            FROM public.v8users", Connection);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    try
                    {
                        var SQLUser = new SQLUser();
                        SQLUser.ID = (byte[])reader[0];
                        SQLUser.IDStr = reader.GetString(1);
                        SQLUser.Name = reader.GetString(2);
                        SQLUser.Descr = reader.GetString(3);
                        SQLUser.Data = (byte[])reader[4];
                        SQLUser.AdmRole = reader.GetBoolean(5) ? "Да" : "";
                        SQLUser.DataStr = CommonModule.DecodePasswordStructure(SQLUser.Data, ref SQLUser.KeySize, ref SQLUser.KeyData);
                        ParserServices.ParserList AuthStructure;
                        if (!(SQLUser.DataStr[0] == '{'))
                        {
                            // postgres in my test has weird first symbol
                            AuthStructure = ParserServices.ParsesClass.ParseString(SQLUser.DataStr.Substring(1));
                        }
                        else
                        {
                            AuthStructure = ParserServices.ParsesClass.ParseString(SQLUser.DataStr);
                        }

                        if (AuthStructure[0][7].ToString() == "0")
                        {
                            // нет авторизации 1С
                            SQLUser.PassHash = "нет авторизации 1С";
                        }
                        else
                        {
                            var Hashes = CommonModule.GetPasswordHashTuple(AuthStructure[0]);
                            SQLUser.PassHash = Hashes.Item1;
                            SQLUser.PassHash2 = Hashes.Item2;
                        }

                        SQLUsers.Add(SQLUser);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Ошибка при попытке чтения пользователей из базы данных:" + Environment.NewLine +
                                        ex.Message,
                                        "Ошибка работы с базой данных", MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при попытке чтения пользователей из базы данных:" + Environment.NewLine +
                                ex.Message,
                                "Ошибка работы с базой данных", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            // *****************************************************

            foreach (var Row in SQLUsers)
            {
                if (string.IsNullOrEmpty(Row.Name))
                {
                    continue;
                }

                var itemUserList = new ListViewItem(Row.IDStr);
                itemUserList.SubItems.Add(Row.Name);
                itemUserList.SubItems.Add(Row.Descr);
                itemUserList.SubItems.Add(Row.PassHash);
                itemUserList.SubItems.Add(Row.AdmRole);
                SQLUserList.Items.Add(itemUserList);
            }
            // *****************************************************

        }

        private void ButtonChangePassSQL_Click(object sender, EventArgs e)
        {
            if (SQLUserList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Не выделены строки с пользователями для установки нового пароля!",
                                "Не выделены строки с пользователями", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                return;
            }

            var Rez = MessageBox.Show("Внесение изменений в базу данных может привести к непредсказуемым последствиям, вплоть до полного разрушения базы. " + Environment.NewLine +
                            "Продолжая операцию Вы осознаете это и понимаете, что восстановление будет возможно только из резервной копии." + Environment.NewLine +
                            "Установить новый пароль выбранным пользователям?",
                            "ВНИМАНИЕ!", MessageBoxButtons.YesNo);
            if (Rez != DialogResult.Yes)
            {
                return;
            }

            switch (cbDBType.SelectedIndex)
            {
                case 0:
                    {
                        SetUsersMSSQL();
                        break;
                    }

                case 1:
                    {
                        SetUsersPostgreSQL();
                        break;
                    }
            }
        }

        public void SetUsersMSSQL()
        {
            try
            {
                string Str = "";
                var Connection = new SqlConnection(ConnectionString.Text);
                Connection.Open();
                var command = new SqlCommand("UPDATE [dbo].[v8users] SET [Data] = @data WHERE [ID] = @user", Connection);
                foreach (ListViewItem item in SQLUserList.SelectedItems)
                {
                    foreach (var SQLUser in SQLUsers)
                    {
                        if (SQLUser.IDStr == item.Text && !(SQLUser.PassHash == "\"\""))
                        {
                            int a = 0;
                            Str = Str + Environment.NewLine + SQLUser.Name;
                            var NewHashes = CommonModule.GeneratePasswordHashes(NewPassSQL.Text.Trim());
                            var OldHashes = Tuple.Create(SQLUser.PassHash, SQLUser.PassHash2);
                            string NewData = CommonModule.ReplaceHashes(SQLUser.DataStr, OldHashes, NewHashes);
                            var NewBytes = CommonModule.EncodePasswordStructure(NewData, SQLUser.KeySize, SQLUser.KeyData);
                            command.Parameters.Clear();
                            command.Parameters.Add(new SqlParameter("@user", SqlDbType.Binary)).Value = SQLUser.ID;
                            command.Parameters.Add(new SqlParameter("@data", SqlDbType.Binary)).Value = NewBytes;
                            command.ExecuteNonQuery();
                        }
                    }
                }

                GetUsersMSSQL();
                
                MessageBox.Show("Успешно установлен пароль '" + NewPassSQL.Text.Trim() + "' для пользователей:" + Str,
                                "Операция успешно выполнена", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при попытке записи новых данных пользователей в базу данных:" + Environment.NewLine + 
                                ex.Message,
                                "Ошибка работы с базой данных", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        public void SetUsersPostgreSQL()
        {
            try
            {
                string Str = "";
                int a = 0;
                var Connection = new NpgsqlConnection(ConnectionString.Text);
                Connection.Open();
                var command = new NpgsqlCommand(@"UPDATE public.v8users 
                                             SET data = @NewData 
                                             WHERE id = decode(@id, 'hex')", Connection);
                foreach (ListViewItem item in SQLUserList.SelectedItems)
                {
                    foreach (var SQLUser in SQLUsers)
                    {
                        if (SQLUser.IDStr == item.Text && !(SQLUser.PassHash == "\"\""))
                        {
                            Str = Str + Environment.NewLine + SQLUser.Name;
                            var NewHashes = CommonModule.GeneratePasswordHashes(NewPassSQL.Text.Trim());
                            var OldHashes = Tuple.Create(SQLUser.PassHash, SQLUser.PassHash2);
                            string NewData = CommonModule.ReplaceHashes(SQLUser.DataStr, OldHashes, NewHashes);
                            var NewBytes = CommonModule.EncodePasswordStructure(NewData, SQLUser.KeySize, SQLUser.KeyData);
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("NewData", NewBytes);
                            command.Parameters.AddWithValue("id", SQLUser.IDStr);
                            a = command.ExecuteNonQuery();
                        }
                    }
                }

                GetUsersPostgreSQL();
                if (a > 0)
                {
                    MessageBox.Show("Успешно установлен пароль '" + NewPassSQL.Text.Trim() + "' для пользователей:" + Str,
                                "Операция успешно выполнена", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при попытке записи новых данных пользователей в базу данных:" + Environment.NewLine +
                                ex.Message,
                                "Ошибка работы с базой данных", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void ButtonRepo_Click(object sender, EventArgs e)
        {
            OpenFileDialogRepo.FileName = Repo1C.Text;
            OpenFileDialogRepo.ShowDialog();
            Repo1C.Text = OpenFileDialogRepo.FileName;
            GetUsersRepoUsers();
        }

        private void ButtonGetRepoUsers_Click(object sender, EventArgs e)
        {
            GetUsersRepoUsers();
        }

        public void GetUsersRepoUsers()
        {
            RepoUserList.Items.Clear();
            try
            {
                TableParams = AccessFunctions.ReadInfoBase(Repo1C.Text, "USERS");
                LabelDatabaseVersionRepo.Text = "Internal database version: " + TableParams.DatabaseVersion;
            }
            catch (Exception ex)
            {
                TableParams = default;
                MessageBox.Show("Ошибка при попытке чтения данных из файла хранилища:" + Environment.NewLine +
                                ex.Message,
                                "Ошибка работы с файлом", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            if (TableParams.Records is null)
            {
                return;
            }

            foreach (var Row in TableParams.Records)
            {
                if (string.IsNullOrEmpty(Row["NAME"].ToString()))
                {
                    continue;
                }

                var G = new Guid((byte[])Row["USERID"]);
                var itemUserList = new ListViewItem(G.ToString());
                Row.Add("UserGuidStr", G.ToString());
                itemUserList.SubItems.Add(Row["NAME"].ToString());
                if (Row["PASSWORD"].ToString() == "d41d8cd98f00b204e9800998ecf8427e")
                {
                    itemUserList.SubItems.Add("<нет>");
                }
                else
                {
                    itemUserList.SubItems.Add("пароль установлен");
                }

                int RIGHTS = BitConverter.ToInt32((byte[])Row["RIGHTS"], 0);
                if (RIGHTS == 65535 | RIGHTS == 32773)
                {
                    itemUserList.SubItems.Add("Да");
                }

                RepoUserList.Items.Add(itemUserList);
            }
        }

        private void ButtonSetRepoPassword_Click(object sender, EventArgs e)
        {
            if (RepoUserList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Не выделены строки с пользователями для установки нового пароля!",
                                "Не выделены строки с пользователями", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            else
            {
                var Rez = MessageBox.Show("Внесение изменений в файл хранилища конфигурации может привести к непредсказуемым последствиям, вплоть до полного разрушения базы. " + Environment.NewLine +
                            "Продолжая операцию Вы осознаете это и понимаете, что восстановление будет возможно только из резервной копии." + Environment.NewLine +
                            "Установить пустой пароль выбранным пользователям?",
                            "Уверены?", MessageBoxButtons.YesNo);
                if (Rez != DialogResult.Yes)
                {
                    return;
                }

                try
                {
                    string Str = "";
                    foreach (ListViewItem item in RepoUserList.SelectedItems)
                    {
                        foreach (var Row in TableParams.Records)
                        {
                            if (Row["UserGuidStr"].ToString() == item.Text)
                            {
                                Str = Str + Environment.NewLine + Row["NAME"].ToString();
                                AccessFunctions.WritePasswordIntoInfoBaseRepo(Repo1C.Text, TableParams, (byte[])Row["USERID"], "d41d8cd98f00b204e9800998ecf8427e", Convert.ToInt32(Row["OFFSET_PASSWORD"]));
                            }
                        }
                    }

                    GetUsersRepoUsers();
                    MessageBox.Show("Успешно установлены пустые пароли для пользователей:" + Str,
                                "Операция успешно выполнена", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при попытке записи данных в файл хранилища:" + Environment.NewLine +
                                ex.Message,
                                "Ошибка работы с файлом", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                }
            }
        }

        private void ButtonChangePwdFileDB_Click(object sender, EventArgs e)
        {
            if (ListViewUsers.SelectedItems.Count == 0)
            {
                MessageBox.Show("Не выделены строки с пользователями для сброса пароля!",
                                "Не выделены строки с пользователями", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            else
            {
                var Rez = MessageBox.Show("Внесение изменений в файл информационной базы может привести к непредсказуемым последствиям, вплоть до полного разрушения базы! " + Environment.NewLine +
                            "Продолжая операцию Вы осознаете это и понимаете, что восстановление будет возможно только из резервной копии." + Environment.NewLine +
                            "Установить новый пароль выбранным пользователям?",
                            "ВНИМАНИЕ!", MessageBoxButtons.YesNo);              
                if (Rez != DialogResult.Yes)
                {
                    return;
                }

                try
                {
                    string Str = "";
                    foreach (ListViewItem item in ListViewUsers.SelectedItems)
                    {
                        foreach (var Row in TableParams.Records)
                        {
                            if (Row["UserGuidStr"].ToString() == item.Text)
                            {
                                Str = Str + Environment.NewLine + Row["NAME"].ToString();
                                var OldDataBinary = Row["DATA_BINARY"];
                                string OldData = Row["DATA"].ToString();
                                var NewHashes = CommonModule.GeneratePasswordHashes(NewPassword.Text.Trim());
                                var OldHashes = Tuple.Create(Row["UserPassHash"].ToString(), Row["UserPassHash2"].ToString());
                                string NewData = CommonModule.ReplaceHashes(OldData, OldHashes, NewHashes);
                                var NewBytes = CommonModule.EncodePasswordStructure(NewData, Convert.ToInt32(Row["DATA_KEYSIZE"]), (byte[])Row["DATA_KEY"]);
                                AccessFunctions.WritePasswordIntoInfoBaseIB(FileIB.Text, TableParams, (byte[])Row["ID"], (byte[])OldDataBinary, NewBytes, Convert.ToInt32(Row["DATA_POS"]), Convert.ToInt32(Row["DATA_SIZE"]));
                            }
                        }
                    }

                    GetUsers();
                    MessageBox.Show("Успешно установлен пароль '" + NewPassword.Text.Trim() + "' для пользователей:" + Str,
                                "Операция успешно выполнена", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при попытке записи данных в файл информационной базы:" + Environment.NewLine +
                                ex.Message,
                                "Ошибка работы с файлом", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                }
            }
        }

        private void LinkLabel2_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/alekseybochkov/");
        }

        private void LinkLabel1_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/alekseybochkov/PasswordChanger1C");
        }

        private void CbDBType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbDBType.SelectedIndex)
            {
                case 0:
                    {
                        ConnectionString.Text = "Data Source=MSSQL1;Server=localhost;Integrated Security=true;Database=zup";
                        break;
                    }

                case 1:
                    {
                        ConnectionString.Text = "Host=localhost;Username=postgres;Password=password;Database=database";
                        break;
                    }
            }
        }
    }
}