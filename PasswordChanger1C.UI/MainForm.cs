using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace PasswordChanger1C.UI
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

        private void ButtonGetUsers_Click(object sender, EventArgs e)
        {
            GetUsers();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            GetUsers_SQLInfobase(Selected_DBMSType());
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

            AccessFunctions.ParseUsersData_IB(ref TableParams.Records);

            Fill_itemUserList_IB();
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

            AccessFunctions.ParseUsersData_Repo(ref TableParams.Records);

            Fill_itemUserList_Repo();
        }

        private SQLInfobase.DBMSType Selected_DBMSType()
        {
            return cbDBType.SelectedIndex switch
            {
                0 => SQLInfobase.DBMSType.MSSQLServer,
                1 => SQLInfobase.DBMSType.PostgreSQL,
                _ => throw new SQLInfobase.WrongDBMSTypeException("unknown DBMS type"),
            };
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

            SetUsers_SQLInfobase(Selected_DBMSType());
        }

        public void GetUsers_SQLInfobase(in SQLInfobase.DBMSType dbms_type)
        {
            SQLUserList.Items.Clear();

            var factory = SQLInfobase.CreateConnectionFactory(dbms_type, ConnectionString.Text);

            try
            {
                var Users = new SQLInfobase.Users(dbms_type, factory);
                SQLUsers = Users.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при попытке чтения пользователей из базы данных:" + Environment.NewLine +
                                ex.Message,
                                "Ошибка работы с базой данных", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            Fill_itemUserList_SQL();
        }

        private void Fill_itemUserList_SQL()
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

        private void Fill_itemUserList_IB()
        {
            foreach (var Row in TableParams.Records)
            {
                if (string.IsNullOrEmpty(Row["NAME"].ToString()))
                {
                    continue;
                }

                var itemUserList = new ListViewItem(Row["UserGuidStr"].ToString());
                itemUserList.SubItems.Add(Row["NAME"].ToString());
                itemUserList.SubItems.Add(Row["DESCR"].ToString());
                itemUserList.SubItems.Add(Row["UserPassHash"].ToString());
                itemUserList.SubItems.Add(CommonModule.Format_AdmRole((bool)Row["ADMROLE"]));
                ListViewUsers.Items.Add(itemUserList);
            }
        }

        private void Fill_itemUserList_Repo()
        {
            RepoUserList.Items.Clear();
            foreach (var Row in TableParams.Records)
            {
                if (string.IsNullOrEmpty(Row["NAME"].ToString()))
                {
                    continue;
                }

                var itemUserList = new ListViewItem(Row["UserGuidStr"].ToString());
                itemUserList.SubItems.Add(Row["NAME"].ToString());
                itemUserList.SubItems.Add((bool)Row["EMPTY_PASS"] ? "<нет>" : "пароль установлен");
                itemUserList.SubItems.Add(CommonModule.Format_AdmRole((bool)Row["ADMROLE"]));
                RepoUserList.Items.Add(itemUserList);
            }
        }

        public void SetUsers_SQLInfobase(in SQLInfobase.DBMSType dbms_type)
        {
            bool is_Success = true;
            string NewPassword = NewPassSQL.Text.Trim();
            List<string> Selected_ID = new();
            foreach (ListViewItem item in SQLUserList.SelectedItems) Selected_ID.Add(item.Text);

            var SelectedUsers = SQLUsers.FindAll(user => Selected_ID.Contains(user.IDStr) && user.PassHash != "");
            if (0 == SelectedUsers.Count)
            {
                MessageBox.Show("Невозможно изменить пароль выбранным пользователям, так как авторизация средставми 1С не используется",
                                "Ошибка изменения пароля", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            var UserNames = string.Join(", ", SelectedUsers.Select(user => user.Name));

            var factory = SQLInfobase.CreateConnectionFactory(dbms_type, ConnectionString.Text);

            try
            {
                var Users = new SQLInfobase.Users(dbms_type, factory);
                foreach (var SelectedUser in SelectedUsers)
                {
                    var User = SelectedUser;
                    SQLInfobase.UpdatePassword(ref User, NewPassword);
                    is_Success = is_Success && Users.Update(User);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при попытке записи новых данных пользователей в базу данных:" + Environment.NewLine +
                                ex.Message,
                                "Ошибка работы с базой данных", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            if (is_Success)
            {
                MessageBox.Show("Успешно установлен пароль '" + NewPassword + "' для пользователей:" + Environment.NewLine + UserNames,
                                "Операция успешно выполнена", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                GetUsers_SQLInfobase(dbms_type);
            }
            else
            {
                MessageBox.Show("Не удалось установить пароль '" + NewPassword + "' пользователям:" + Environment.NewLine + UserNames,
                                "Ошибка установки пароля", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }
        
        private void ButtonSetRepoPassword_Click(object sender, EventArgs e)
        {
            if (RepoUserList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Не выделены строки с пользователями для установки нового пароля!",
                                "Не выделены строки с пользователями", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                return;
            }

            var Rez = MessageBox.Show("Внесение изменений в файл хранилища конфигурации может привести к непредсказуемым последствиям, вплоть до полного разрушения базы. " + Environment.NewLine +
                        "Продолжая операцию Вы осознаете это и понимаете, что восстановление будет возможно только из резервной копии." + Environment.NewLine +
                        "Установить пустой пароль выбранным пользователям?",
                        "Уверены?", MessageBoxButtons.YesNo);
            if (Rez != DialogResult.Yes)
            {
                return;
            }

            List<string> Selected_ID = new();
            foreach (ListViewItem item in RepoUserList.SelectedItems) Selected_ID.Add(item.Text);

            var SelectedRows = TableParams.Records.FindAll(Row => Selected_ID.Contains(Row["UserGuidStr"].ToString()));
            var UserNames = string.Join(", ", SelectedRows.Select(Row => Row["NAME"].ToString()));

            try
            {
                foreach (var Row in SelectedRows)
                {
                    AccessFunctions.WritePasswordIntoInfoBaseRepo(Repo1C.Text, TableParams, Convert.ToInt32(Row["OFFSET_PASSWORD"]));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при попытке записи данных в файл хранилища:" + Environment.NewLine +
                            ex.Message,
                            "Ошибка работы с файлом", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Успешно установлены пустые пароли для пользователей:" + Environment.NewLine + UserNames,
                    "Операция успешно выполнена", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

            GetUsersRepoUsers();
        }

        private void ButtonChangePwdFileDB_Click(object sender, EventArgs e)
        {
            if (ListViewUsers.SelectedItems.Count == 0)
            {
                MessageBox.Show("Не выделены строки с пользователями для сброса пароля!",
                                "Не выделены строки с пользователями", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                return;
            }

            var Rez = MessageBox.Show("Внесение изменений в файл информационной базы может привести к непредсказуемым последствиям, вплоть до полного разрушения базы! " + Environment.NewLine +
                        "Продолжая операцию Вы осознаете это и понимаете, что восстановление будет возможно только из резервной копии." + Environment.NewLine +
                        "Установить новый пароль выбранным пользователям?",
                        "ВНИМАНИЕ!", MessageBoxButtons.YesNo);
            if (Rez != DialogResult.Yes)
            {
                return;
            }

            List<string> Selected_ID = new();
            foreach (ListViewItem item in ListViewUsers.SelectedItems) Selected_ID.Add(item.Text);

            var SelectedRows = TableParams.Records.FindAll(Row => Selected_ID.Contains(Row["UserGuidStr"].ToString()));
            var UserNames = string.Join(", ", SelectedRows.Select(Row => Row["NAME"].ToString()));

            try
            {
                foreach (var Row in SelectedRows)
                {
                    var Current_Row = Row;
                    AccessFunctions.UpdatePassword_IB(ref Current_Row, NewPassword.Text.Trim());
                    AccessFunctions.WritePasswordIntoInfoBaseIB(FileIB.Text, TableParams, Current_Row);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при попытке записи данных в файл информационной базы:" + Environment.NewLine +
                            ex.Message,
                            "Ошибка работы с файлом", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Успешно установлен пароль '" + NewPassword.Text.Trim() + "' для пользователей:" + UserNames,
                            "Операция успешно выполнена", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            GetUsers();
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
            ConnectionString.Text = Selected_DBMSType() switch
            {
                SQLInfobase.DBMSType.MSSQLServer => "Data Source=MSSQL1;Server=localhost;Integrated Security=true;Database=zup",
                SQLInfobase.DBMSType.PostgreSQL => "Host=localhost;Username=postgres;Password=password;Database=database",
                _ => throw new SQLInfobase.WrongDBMSTypeException("unknown DBMS type"),
            };
        }
    }
}