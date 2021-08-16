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

        private List<SQLInfobase.SQLUser> SQLUsers = new();

        private void MainForm_Shown(object sender, EventArgs e)
        {
            var CommandLineArgs = Environment.GetCommandLineArgs();
            int ShowWarningParameterIndex = Array.IndexOf(CommandLineArgs, "nowarning");
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

            var Rez = MessageBox.Show("Запрещается использование приложения для несанкционированного доступа к данным!" + Environment.NewLine +
                                      "Используя данное приложение Вы подтверждаете, что базы данных, к которым будет предоставлен доступ, принадлежат Вашей организации " + Environment.NewLine +
                                      "и Вы являетесь Администратором с неограниченным доступом к информации этих баз данных." + Environment.NewLine +
                                      "Несанкционированный доступ к информации преследуются по ст. 1301 Гражданского кодекса РФ, ст. 7.12 Кодекса Российской Федерации " + Environment.NewLine +
                                      "об административных правонарушениях, ст. 146 Уголовного кодекса РФ." + Environment.NewLine +
                                      "Продолжить?",
                                      "Правила использования", MessageBoxButtons.YesNo);
            return Rez == DialogResult.Yes;
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
                var Hashes = CommonModule.GetPasswordHashTuple(AuthStructure);
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
            SQLInfobase.DBMSType dbms_type = cbDBType.SelectedIndex switch
            {
                0 => SQLInfobase.DBMSType.MSSQLServer,
                1 => SQLInfobase.DBMSType.PostgreSQL,
                _ => throw new SQLInfobase.WrongDBMSTypeException("unknown DBMS type"),
            };
            GetUsers_SQLInfobase(dbms_type);
        }

        public void GetUsers_SQLInfobase(in SQLInfobase.DBMSType dbms_type)
        {
            Func<IDbConnection> factory = dbms_type switch
            {
                SQLInfobase.DBMSType.MSSQLServer => () => new SqlConnection(ConnectionString.Text),
                SQLInfobase.DBMSType.PostgreSQL => () => new NpgsqlConnection(ConnectionString.Text),
                _ => throw new SQLInfobase.WrongDBMSTypeException("unknown DBMS type"),
            };

            SQLUserList.Items.Clear();
            try
            {
                var reader = new SQLInfobase.ReadUsers(dbms_type, factory);
                SQLUsers = reader.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при попытке чтения пользователей из базы данных:" + Environment.NewLine +
                                ex.Message,
                                "Ошибка работы с базой данных", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            Fill_SQLUserList();
        }

        private void Fill_SQLUserList()
        {
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
                using var Connection = new SqlConnection(ConnectionString.Text);
                Connection.Open();
#pragma warning disable SecurityIntelliSenseCS // MS Security rules violation
                using var command = new SqlCommand("UPDATE [dbo].[v8users] SET [Data] = @data WHERE [ID] = @user", Connection);
#pragma warning restore SecurityIntelliSenseCS // MS Security rules violation
                foreach (ListViewItem item in SQLUserList.SelectedItems)
                {
                    foreach (var SQLUser in SQLUsers)
                    {
                        if (SQLUser.IDStr == item.Text && SQLUser.PassHash != "")
                        {
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

                GetUsers_SQLInfobase(SQLInfobase.DBMSType.MSSQLServer);

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
                using var Connection = new NpgsqlConnection(ConnectionString.Text);
                Connection.Open();
                using var command = new NpgsqlCommand(@"UPDATE public.v8users 
                                             SET data = @NewData 
                                             WHERE id = decode(@id, 'hex')", Connection);
                foreach (ListViewItem item in SQLUserList.SelectedItems)
                {
                    foreach (var SQLUser in SQLUsers)
                    {
                        if (SQLUser.IDStr == item.Text && SQLUser.PassHash != "")
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

                GetUsers_SQLInfobase(SQLInfobase.DBMSType.PostgreSQL);

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
                if (Row["PASSWORD"].ToString() == AccessFunctions.InfoBaseRepo_EmptyPassword)
                {
                    itemUserList.SubItems.Add("<нет>");
                }
                else
                {
                    itemUserList.SubItems.Add("пароль установлен");
                }

                int RIGHTS = BitConverter.ToInt32((byte[])Row["RIGHTS"], 0);
                if (RIGHTS == 0xFFFF || RIGHTS == 0x8005)
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
                                AccessFunctions.WritePasswordIntoInfoBaseRepo(Repo1C.Text, TableParams, Convert.ToInt32(Row["OFFSET_PASSWORD"]));
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
                                AccessFunctions.WritePasswordIntoInfoBaseIB(FileIB.Text, TableParams, (byte[])OldDataBinary, NewBytes, Convert.ToInt32(Row["DATA_POS"]), Convert.ToInt32(Row["DATA_SIZE"]));
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