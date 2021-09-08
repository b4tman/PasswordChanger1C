using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace PasswordChanger1C.UI
{
    public partial class MainForm : Form
    {

        // Form overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components is object)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components=null;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this._Button6 = new System.Windows.Forms.Button();
            this.FileIB = new System.Windows.Forms.TextBox();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this._ButtonGetUsers = new System.Windows.Forms.Button();
            this.ListViewUsers = new System.Windows.Forms.ListView();
            this.UserGUID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.UserName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.UserDescr = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.UserPassHash = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.UserAdmRole = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ConnectionString = new System.Windows.Forms.TextBox();
            this._Button2 = new System.Windows.Forms.Button();
            this.Label4 = new System.Windows.Forms.Label();
            this.Repo1C = new System.Windows.Forms.TextBox();
            this._ButtonRepo = new System.Windows.Forms.Button();
            this.OpenFileDialogRepo = new System.Windows.Forms.OpenFileDialog();
            this._ButtonGetRepoUsers = new System.Windows.Forms.Button();
            this._ButtonSetRepoPassword = new System.Windows.Forms.Button();
            this.TabControl1 = new System.Windows.Forms.TabControl();
            this.TabPage1 = new System.Windows.Forms.TabPage();
            this.LabelDatabaseVersion = new System.Windows.Forms.Label();
            this.NewPassword = new System.Windows.Forms.TextBox();
            this.Label2 = new System.Windows.Forms.Label();
            this.TextBox3 = new System.Windows.Forms.TextBox();
            this._ButtonChangePwdFileDB = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.TabPage2 = new System.Windows.Forms.TabPage();
            this.Label7 = new System.Windows.Forms.Label();
            this._cbDBType = new System.Windows.Forms.ComboBox();
            this.TextBox2 = new System.Windows.Forms.TextBox();
            this._ButtonChangePassSQL = new System.Windows.Forms.Button();
            this.Label6 = new System.Windows.Forms.Label();
            this.NewPassSQL = new System.Windows.Forms.TextBox();
            this.Label5 = new System.Windows.Forms.Label();
            this.SQLUserList = new System.Windows.Forms.ListView();
            this.ColumnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ColumnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.TabPage3 = new System.Windows.Forms.TabPage();
            this.LabelDatabaseVersionRepo = new System.Windows.Forms.Label();
            this.TextBox1 = new System.Windows.Forms.TextBox();
            this.RepoUserList = new System.Windows.Forms.ListView();
            this.RepoUserGUID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RepoUserName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RepoHasPwd = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RepoAdmin = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this._LinkLabel1 = new System.Windows.Forms.LinkLabel();
            this.Label3 = new System.Windows.Forms.Label();
            this._LinkLabel2 = new System.Windows.Forms.LinkLabel();
            this.TabControl1.SuspendLayout();
            this.TabPage1.SuspendLayout();
            this.TabPage2.SuspendLayout();
            this.TabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // _Button6
            // 
            this._Button6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._Button6.Location = new System.Drawing.Point(636, 24);
            this._Button6.Name = "_Button6";
            this._Button6.Size = new System.Drawing.Size(95, 22);
            this._Button6.TabIndex = 11;
            this._Button6.Text = "Выбрать файл";
            this._Button6.UseVisualStyleBackColor = true;
            this._Button6.Click += new System.EventHandler(this.Button6_Click);
            // 
            // FileIB
            // 
            this.FileIB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FileIB.Location = new System.Drawing.Point(9, 25);
            this.FileIB.Name = "FileIB";
            this.FileIB.Size = new System.Drawing.Size(625, 20);
            this.FileIB.TabIndex = 9;
            // 
            // OpenFileDialog
            // 
            this.OpenFileDialog.Filter = "1CD files|*.1cd; *.1CD; *.1Cd; *.1cD|All files (*.*)|*.*";
            this.OpenFileDialog.RestoreDirectory = true;
            this.OpenFileDialog.Title = "Выберите файл информационной базы 1С";
            // 
            // _ButtonGetUsers
            // 
            this._ButtonGetUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._ButtonGetUsers.Location = new System.Drawing.Point(733, 24);
            this._ButtonGetUsers.Name = "_ButtonGetUsers";
            this._ButtonGetUsers.Size = new System.Drawing.Size(183, 22);
            this._ButtonGetUsers.TabIndex = 12;
            this._ButtonGetUsers.Text = "Получить список пользователей";
            this._ButtonGetUsers.UseVisualStyleBackColor = true;
            this._ButtonGetUsers.Click += new System.EventHandler(this.ButtonGetUsers_Click);
            // 
            // ListViewUsers
            // 
            this.ListViewUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ListViewUsers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.UserGUID,
            this.UserName,
            this.UserDescr,
            this.UserPassHash,
            this.UserAdmRole});
            this.ListViewUsers.FullRowSelect = true;
            this.ListViewUsers.HideSelection = false;
            this.ListViewUsers.Location = new System.Drawing.Point(9, 50);
            this.ListViewUsers.Name = "ListViewUsers";
            this.ListViewUsers.Size = new System.Drawing.Size(905, 343);
            this.ListViewUsers.TabIndex = 14;
            this.ListViewUsers.UseCompatibleStateImageBehavior = false;
            this.ListViewUsers.View = System.Windows.Forms.View.Details;
            // 
            // UserGUID
            // 
            this.UserGUID.Text = "GUID";
            this.UserGUID.Width = 158;
            // 
            // UserName
            // 
            this.UserName.Text = "Имя";
            this.UserName.Width = 164;
            // 
            // UserDescr
            // 
            this.UserDescr.Text = "Полное имя";
            this.UserDescr.Width = 147;
            // 
            // UserPassHash
            // 
            this.UserPassHash.Text = "Хеш пароля";
            this.UserPassHash.Width = 164;
            // 
            // UserAdmRole
            // 
            this.UserAdmRole.Text = "Адм.роль";
            this.UserAdmRole.Width = 66;
            // 
            // ConnectionString
            // 
            this.ConnectionString.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ConnectionString.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ConnectionString.Location = new System.Drawing.Point(9, 48);
            this.ConnectionString.Name = "ConnectionString";
            this.ConnectionString.Size = new System.Drawing.Size(734, 26);
            this.ConnectionString.TabIndex = 9;
            this.ConnectionString.Text = "Data Source=MSSQL1;Server=SERVER;Integrated Security=true;Database=DATABASE";
            // 
            // _Button2
            // 
            this._Button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._Button2.Location = new System.Drawing.Point(745, 47);
            this._Button2.Name = "_Button2";
            this._Button2.Size = new System.Drawing.Size(172, 22);
            this._Button2.TabIndex = 12;
            this._Button2.Text = "Получить пользователей";
            this._Button2.UseVisualStyleBackColor = true;
            this._Button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label4.Location = new System.Drawing.Point(12, 6);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(265, 16);
            this.Label4.TabIndex = 10;
            this.Label4.Text = "Файл хранилища конфигурации 1С";
            // 
            // Repo1C
            // 
            this.Repo1C.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Repo1C.Location = new System.Drawing.Point(9, 25);
            this.Repo1C.Name = "Repo1C";
            this.Repo1C.Size = new System.Drawing.Size(616, 20);
            this.Repo1C.TabIndex = 9;
            // 
            // _ButtonRepo
            // 
            this._ButtonRepo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._ButtonRepo.Location = new System.Drawing.Point(627, 23);
            this._ButtonRepo.Name = "_ButtonRepo";
            this._ButtonRepo.Size = new System.Drawing.Size(98, 23);
            this._ButtonRepo.TabIndex = 11;
            this._ButtonRepo.Text = "Выбрать файл";
            this._ButtonRepo.UseVisualStyleBackColor = true;
            this._ButtonRepo.Click += new System.EventHandler(this.ButtonRepo_Click);
            // 
            // OpenFileDialogRepo
            // 
            this.OpenFileDialogRepo.FileName = "OpenFileDialogRepo";
            this.OpenFileDialogRepo.Filter = "1CD files|*.1cd; *.1CD; *.1Cd; *.1cD|All files (*.*)|*.*";
            this.OpenFileDialogRepo.Title = "Выберите файл хранилища 1С";
            // 
            // _ButtonGetRepoUsers
            // 
            this._ButtonGetRepoUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._ButtonGetRepoUsers.Location = new System.Drawing.Point(725, 23);
            this._ButtonGetRepoUsers.Name = "_ButtonGetRepoUsers";
            this._ButtonGetRepoUsers.Size = new System.Drawing.Size(192, 23);
            this._ButtonGetRepoUsers.TabIndex = 12;
            this._ButtonGetRepoUsers.Text = "Получить список пользователей";
            this._ButtonGetRepoUsers.UseVisualStyleBackColor = true;
            this._ButtonGetRepoUsers.Click += new System.EventHandler(this.ButtonGetRepoUsers_Click);
            // 
            // _ButtonSetRepoPassword
            // 
            this._ButtonSetRepoPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._ButtonSetRepoPassword.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._ButtonSetRepoPassword.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this._ButtonSetRepoPassword.Location = new System.Drawing.Point(625, 413);
            this._ButtonSetRepoPassword.Name = "_ButtonSetRepoPassword";
            this._ButtonSetRepoPassword.Size = new System.Drawing.Size(289, 51);
            this._ButtonSetRepoPassword.TabIndex = 16;
            this._ButtonSetRepoPassword.Text = "Установить выбранным пользователям пустой пароль в хранилище";
            this._ButtonSetRepoPassword.UseVisualStyleBackColor = true;
            this._ButtonSetRepoPassword.Click += new System.EventHandler(this.ButtonSetRepoPassword_Click);
            // 
            // TabControl1
            // 
            this.TabControl1.AccessibleDescription = "";
            this.TabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TabControl1.Controls.Add(this.TabPage1);
            this.TabControl1.Controls.Add(this.TabPage2);
            this.TabControl1.Controls.Add(this.TabPage3);
            this.TabControl1.Location = new System.Drawing.Point(4, 4);
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.SelectedIndex = 0;
            this.TabControl1.Size = new System.Drawing.Size(928, 496);
            this.TabControl1.TabIndex = 17;
            // 
            // TabPage1
            // 
            this.TabPage1.Controls.Add(this.LabelDatabaseVersion);
            this.TabPage1.Controls.Add(this.NewPassword);
            this.TabPage1.Controls.Add(this.Label2);
            this.TabPage1.Controls.Add(this.TextBox3);
            this.TabPage1.Controls.Add(this._ButtonChangePwdFileDB);
            this.TabPage1.Controls.Add(this.Label1);
            this.TabPage1.Controls.Add(this.ListViewUsers);
            this.TabPage1.Controls.Add(this.FileIB);
            this.TabPage1.Controls.Add(this._Button6);
            this.TabPage1.Controls.Add(this._ButtonGetUsers);
            this.TabPage1.Location = new System.Drawing.Point(4, 22);
            this.TabPage1.Name = "TabPage1";
            this.TabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.TabPage1.Size = new System.Drawing.Size(920, 470);
            this.TabPage1.TabIndex = 0;
            this.TabPage1.Text = "Файловая ИБ";
            this.TabPage1.UseVisualStyleBackColor = true;
            // 
            // LabelDatabaseVersion
            // 
            this.LabelDatabaseVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LabelDatabaseVersion.AutoSize = true;
            this.LabelDatabaseVersion.Location = new System.Drawing.Point(11, 396);
            this.LabelDatabaseVersion.Name = "LabelDatabaseVersion";
            this.LabelDatabaseVersion.Size = new System.Drawing.Size(132, 13);
            this.LabelDatabaseVersion.TabIndex = 26;
            this.LabelDatabaseVersion.Text = "Internal database version: ";
            // 
            // NewPassword
            // 
            this.NewPassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NewPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NewPassword.Location = new System.Drawing.Point(128, 443);
            this.NewPassword.Name = "NewPassword";
            this.NewPassword.Size = new System.Drawing.Size(144, 22);
            this.NewPassword.TabIndex = 24;
            this.NewPassword.Text = "12345";
            // 
            // Label2
            // 
            this.Label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label2.Location = new System.Drawing.Point(9, 445);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(113, 16);
            this.Label2.TabIndex = 25;
            this.Label2.Text = "Новый пароль";
            // 
            // TextBox3
            // 
            this.TextBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBox3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBox3.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBox3.Location = new System.Drawing.Point(9, 413);
            this.TextBox3.Multiline = true;
            this.TextBox3.Name = "TextBox3";
            this.TextBox3.Size = new System.Drawing.Size(613, 29);
            this.TextBox3.TabIndex = 23;
            this.TextBox3.Text = "Файл информационной базы не должен быть открыт никакими другими приложениями.";
            // 
            // _ButtonChangePwdFileDB
            // 
            this._ButtonChangePwdFileDB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._ButtonChangePwdFileDB.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._ButtonChangePwdFileDB.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this._ButtonChangePwdFileDB.Location = new System.Drawing.Point(625, 414);
            this._ButtonChangePwdFileDB.Name = "_ButtonChangePwdFileDB";
            this._ButtonChangePwdFileDB.Size = new System.Drawing.Size(289, 51);
            this._ButtonChangePwdFileDB.TabIndex = 22;
            this._ButtonChangePwdFileDB.Text = "Установить выбранным пользователям указанный пароль";
            this._ButtonChangePwdFileDB.UseVisualStyleBackColor = true;
            this._ButtonChangePwdFileDB.Click += new System.EventHandler(this.ButtonChangePwdFileDB_Click);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label1.Location = new System.Drawing.Point(10, 6);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(244, 16);
            this.Label1.TabIndex = 20;
            this.Label1.Text = "Файл информационной базы 1С";
            // 
            // TabPage2
            // 
            this.TabPage2.Controls.Add(this.Label7);
            this.TabPage2.Controls.Add(this._cbDBType);
            this.TabPage2.Controls.Add(this.TextBox2);
            this.TabPage2.Controls.Add(this._ButtonChangePassSQL);
            this.TabPage2.Controls.Add(this.Label6);
            this.TabPage2.Controls.Add(this.NewPassSQL);
            this.TabPage2.Controls.Add(this.Label5);
            this.TabPage2.Controls.Add(this.ConnectionString);
            this.TabPage2.Controls.Add(this._Button2);
            this.TabPage2.Controls.Add(this.SQLUserList);
            this.TabPage2.Location = new System.Drawing.Point(4, 22);
            this.TabPage2.Name = "TabPage2";
            this.TabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.TabPage2.Size = new System.Drawing.Size(920, 470);
            this.TabPage2.TabIndex = 1;
            this.TabPage2.Text = "Клиент-серверная ИБ";
            this.TabPage2.UseVisualStyleBackColor = true;
            // 
            // Label7
            // 
            this.Label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label7.Location = new System.Drawing.Point(9, 9);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(166, 17);
            this.Label7.TabIndex = 24;
            this.Label7.Text = "Тип базы данных:";
            // 
            // _cbDBType
            // 
            this._cbDBType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._cbDBType.FormattingEnabled = true;
            this._cbDBType.Items.AddRange(new object[] {
            "Microsoft SQL Server",
            "PostgreSQL Server"});
            this._cbDBType.Location = new System.Drawing.Point(181, 7);
            this._cbDBType.Name = "_cbDBType";
            this._cbDBType.Size = new System.Drawing.Size(185, 21);
            this._cbDBType.TabIndex = 23;
            this._cbDBType.SelectedIndexChanged += new System.EventHandler(this.CbDBType_SelectedIndexChanged);
            // 
            // TextBox2
            // 
            this.TextBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBox2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBox2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBox2.Location = new System.Drawing.Point(9, 413);
            this.TextBox2.Multiline = true;
            this.TextBox2.Name = "TextBox2";
            this.TextBox2.Size = new System.Drawing.Size(610, 24);
            this.TextBox2.TabIndex = 22;
            this.TextBox2.Text = "Монопольного режима доступа к базе не требуется";
            // 
            // _ButtonChangePassSQL
            // 
            this._ButtonChangePassSQL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._ButtonChangePassSQL.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._ButtonChangePassSQL.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(0)))));
            this._ButtonChangePassSQL.Location = new System.Drawing.Point(625, 414);
            this._ButtonChangePassSQL.Name = "_ButtonChangePassSQL";
            this._ButtonChangePassSQL.Size = new System.Drawing.Size(289, 51);
            this._ButtonChangePassSQL.TabIndex = 21;
            this._ButtonChangePassSQL.Text = "Установить выбранным пользователям указанный пароль";
            this._ButtonChangePassSQL.UseVisualStyleBackColor = true;
            this._ButtonChangePassSQL.Click += new System.EventHandler(this.ButtonChangePassSQL_Click);
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label6.Location = new System.Drawing.Point(9, 29);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(297, 16);
            this.Label6.TabIndex = 19;
            this.Label6.Text = "Строка соединения с базой данных 1С:";
            // 
            // NewPassSQL
            // 
            this.NewPassSQL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NewPassSQL.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NewPassSQL.Location = new System.Drawing.Point(128, 443);
            this.NewPassSQL.Name = "NewPassSQL";
            this.NewPassSQL.Size = new System.Drawing.Size(144, 22);
            this.NewPassSQL.TabIndex = 17;
            this.NewPassSQL.Text = "12345";
            // 
            // Label5
            // 
            this.Label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Label5.AutoSize = true;
            this.Label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label5.Location = new System.Drawing.Point(9, 445);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(113, 16);
            this.Label5.TabIndex = 18;
            this.Label5.Text = "Новый пароль";
            // 
            // SQLUserList
            // 
            this.SQLUserList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SQLUserList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ColumnHeader1,
            this.ColumnHeader2,
            this.ColumnHeader3,
            this.ColumnHeader4,
            this.ColumnHeader5});
            this.SQLUserList.FullRowSelect = true;
            this.SQLUserList.HideSelection = false;
            this.SQLUserList.Location = new System.Drawing.Point(9, 76);
            this.SQLUserList.Name = "SQLUserList";
            this.SQLUserList.Size = new System.Drawing.Size(905, 335);
            this.SQLUserList.TabIndex = 20;
            this.SQLUserList.UseCompatibleStateImageBehavior = false;
            this.SQLUserList.View = System.Windows.Forms.View.Details;
            // 
            // ColumnHeader1
            // 
            this.ColumnHeader1.Text = "GUID";
            this.ColumnHeader1.Width = 232;
            // 
            // ColumnHeader2
            // 
            this.ColumnHeader2.Text = "Имя";
            this.ColumnHeader2.Width = 183;
            // 
            // ColumnHeader3
            // 
            this.ColumnHeader3.Text = "Полное имя";
            this.ColumnHeader3.Width = 199;
            // 
            // ColumnHeader4
            // 
            this.ColumnHeader4.Text = "Хеш пароля";
            this.ColumnHeader4.Width = 220;
            // 
            // ColumnHeader5
            // 
            this.ColumnHeader5.Text = "Адм.роль";
            this.ColumnHeader5.Width = 66;
            // 
            // TabPage3
            // 
            this.TabPage3.Controls.Add(this.LabelDatabaseVersionRepo);
            this.TabPage3.Controls.Add(this.TextBox1);
            this.TabPage3.Controls.Add(this.RepoUserList);
            this.TabPage3.Controls.Add(this.Label4);
            this.TabPage3.Controls.Add(this._ButtonSetRepoPassword);
            this.TabPage3.Controls.Add(this._ButtonGetRepoUsers);
            this.TabPage3.Controls.Add(this.Repo1C);
            this.TabPage3.Controls.Add(this._ButtonRepo);
            this.TabPage3.Location = new System.Drawing.Point(4, 22);
            this.TabPage3.Name = "TabPage3";
            this.TabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.TabPage3.Size = new System.Drawing.Size(920, 470);
            this.TabPage3.TabIndex = 2;
            this.TabPage3.Text = "Хранилище конфигурации";
            this.TabPage3.UseVisualStyleBackColor = true;
            // 
            // LabelDatabaseVersionRepo
            // 
            this.LabelDatabaseVersionRepo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LabelDatabaseVersionRepo.AutoSize = true;
            this.LabelDatabaseVersionRepo.Location = new System.Drawing.Point(12, 398);
            this.LabelDatabaseVersionRepo.Name = "LabelDatabaseVersionRepo";
            this.LabelDatabaseVersionRepo.Size = new System.Drawing.Size(132, 13);
            this.LabelDatabaseVersionRepo.TabIndex = 27;
            this.LabelDatabaseVersionRepo.Text = "Internal database version: ";
            // 
            // TextBox1
            // 
            this.TextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBox1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBox1.Location = new System.Drawing.Point(6, 413);
            this.TextBox1.Multiline = true;
            this.TextBox1.Name = "TextBox1";
            this.TextBox1.Size = new System.Drawing.Size(613, 50);
            this.TextBox1.TabIndex = 18;
            this.TextBox1.Text = "Файл хранилища конфигурации не должен быть открыт никакими другими приложениями.";
            // 
            // RepoUserList
            // 
            this.RepoUserList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RepoUserList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.RepoUserGUID,
            this.RepoUserName,
            this.RepoHasPwd,
            this.RepoAdmin});
            this.RepoUserList.FullRowSelect = true;
            this.RepoUserList.HideSelection = false;
            this.RepoUserList.Location = new System.Drawing.Point(9, 50);
            this.RepoUserList.Name = "RepoUserList";
            this.RepoUserList.Size = new System.Drawing.Size(905, 345);
            this.RepoUserList.TabIndex = 17;
            this.RepoUserList.UseCompatibleStateImageBehavior = false;
            this.RepoUserList.View = System.Windows.Forms.View.Details;
            // 
            // RepoUserGUID
            // 
            this.RepoUserGUID.Text = "GUID";
            this.RepoUserGUID.Width = 221;
            // 
            // RepoUserName
            // 
            this.RepoUserName.Text = "Имя";
            this.RepoUserName.Width = 183;
            // 
            // RepoHasPwd
            // 
            this.RepoHasPwd.Text = "Пароль установлен";
            this.RepoHasPwd.Width = 134;
            // 
            // RepoAdmin
            // 
            this.RepoAdmin.Text = "Это администратор";
            this.RepoAdmin.Width = 115;
            // 
            // _LinkLabel1
            // 
            this._LinkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._LinkLabel1.AutoSize = true;
            this._LinkLabel1.Location = new System.Drawing.Point(230, 501);
            this._LinkLabel1.Name = "_LinkLabel1";
            this._LinkLabel1.Size = new System.Drawing.Size(282, 13);
            this._LinkLabel1.TabIndex = 18;
            this._LinkLabel1.TabStop = true;
            this._LinkLabel1.Text = "https://github.com/alekseybochkov/PasswordChanger1C";
            this._LinkLabel1.Click += new System.EventHandler(this.LinkLabel1_Click);
            // 
            // Label3
            // 
            this.Label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(3, 501);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(227, 13);
            this.Label3.TabIndex = 19;
            this.Label3.Text = "Страница приложения для обратной связи:";
            // 
            // _LinkLabel2
            // 
            this._LinkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._LinkLabel2.AutoSize = true;
            this._LinkLabel2.Location = new System.Drawing.Point(831, 501);
            this._LinkLabel2.Name = "_LinkLabel2";
            this._LinkLabel2.Size = new System.Drawing.Size(102, 13);
            this._LinkLabel2.TabIndex = 18;
            this._LinkLabel2.TabStop = true;
            this._LinkLabel2.Text = "© Aleksey.Bochkov";
            this._LinkLabel2.Click += new System.EventHandler(this.LinkLabel2_Click);
            // 
            // MainForm
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.Application;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(934, 516);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this._LinkLabel2);
            this.Controls.Add(this._LinkLabel1);
            this.Controls.Add(this.TabControl1);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Изменение паролей для информационных баз и хранилища 1С";
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.TabControl1.ResumeLayout(false);
            this.TabPage1.ResumeLayout(false);
            this.TabPage1.PerformLayout();
            this.TabPage2.ResumeLayout(false);
            this.TabPage2.PerformLayout();
            this.TabPage3.ResumeLayout(false);
            this.TabPage3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private Button _Button6;

        internal Button Button6
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _Button6;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_Button6 != null)
                {
                    _Button6.Click -= Button6_Click;
                }

                _Button6 = value;
                if (_Button6 != null)
                {
                    _Button6.Click += Button6_Click;
                }
            }
        }

        internal TextBox FileIB;
        internal OpenFileDialog OpenFileDialog;
        private Button _ButtonGetUsers;

        internal Button ButtonGetUsers
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _ButtonGetUsers;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_ButtonGetUsers != null)
                {
                    _ButtonGetUsers.Click -= ButtonGetUsers_Click;
                }

                _ButtonGetUsers = value;
                if (_ButtonGetUsers != null)
                {
                    _ButtonGetUsers.Click += ButtonGetUsers_Click;
                }
            }
        }

        internal ListView ListViewUsers;
        internal ColumnHeader UserGUID;
        internal ColumnHeader UserName;
        internal ColumnHeader UserDescr;
        internal ColumnHeader UserPassHash;
        internal TextBox ConnectionString;
        private Button _Button2;

        internal Button Button2
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _Button2;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_Button2 != null)
                {
                    _Button2.Click -= Button2_Click;
                }

                _Button2 = value;
                if (_Button2 != null)
                {
                    _Button2.Click += Button2_Click;
                }
            }
        }

        internal ColumnHeader UserAdmRole;
        internal Label Label4;
        internal TextBox Repo1C;
        private Button _ButtonRepo;

        internal Button ButtonRepo
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _ButtonRepo;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_ButtonRepo != null)
                {
                    _ButtonRepo.Click -= ButtonRepo_Click;
                }

                _ButtonRepo = value;
                if (_ButtonRepo != null)
                {
                    _ButtonRepo.Click += ButtonRepo_Click;
                }
            }
        }

        internal OpenFileDialog OpenFileDialogRepo;
        private Button _ButtonGetRepoUsers;

        internal Button ButtonGetRepoUsers
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _ButtonGetRepoUsers;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_ButtonGetRepoUsers != null)
                {
                    _ButtonGetRepoUsers.Click -= ButtonGetRepoUsers_Click;
                }

                _ButtonGetRepoUsers = value;
                if (_ButtonGetRepoUsers != null)
                {
                    _ButtonGetRepoUsers.Click += ButtonGetRepoUsers_Click;
                }
            }
        }

        private Button _ButtonSetRepoPassword;

        internal Button ButtonSetRepoPassword
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _ButtonSetRepoPassword;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_ButtonSetRepoPassword != null)
                {
                    _ButtonSetRepoPassword.Click -= ButtonSetRepoPassword_Click;
                }

                _ButtonSetRepoPassword = value;
                if (_ButtonSetRepoPassword != null)
                {
                    _ButtonSetRepoPassword.Click += ButtonSetRepoPassword_Click;
                }
            }
        }

        internal TabControl TabControl1;
        internal TabPage TabPage1;
        internal TabPage TabPage2;
        internal TabPage TabPage3;
        internal ListView RepoUserList;
        internal ColumnHeader RepoUserGUID;
        internal ColumnHeader RepoUserName;
        internal ColumnHeader RepoHasPwd;
        internal ColumnHeader RepoAdmin;
        internal TextBox TextBox1;
        internal Label Label6;
        internal TextBox NewPassSQL;
        internal Label Label5;
        internal TextBox TextBox2;
        private Button _ButtonChangePassSQL;

        internal Button ButtonChangePassSQL
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _ButtonChangePassSQL;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_ButtonChangePassSQL != null)
                {
                    _ButtonChangePassSQL.Click -= ButtonChangePassSQL_Click;
                }

                _ButtonChangePassSQL = value;
                if (_ButtonChangePassSQL != null)
                {
                    _ButtonChangePassSQL.Click += ButtonChangePassSQL_Click;
                }
            }
        }

        internal ListView SQLUserList;
        internal ColumnHeader ColumnHeader1;
        internal ColumnHeader ColumnHeader2;
        internal ColumnHeader ColumnHeader3;
        internal ColumnHeader ColumnHeader4;
        internal ColumnHeader ColumnHeader5;
        private Button _ButtonChangePwdFileDB;

        internal Button ButtonChangePwdFileDB
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _ButtonChangePwdFileDB;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_ButtonChangePwdFileDB != null)
                {
                    _ButtonChangePwdFileDB.Click -= ButtonChangePwdFileDB_Click;
                }

                _ButtonChangePwdFileDB = value;
                if (_ButtonChangePwdFileDB != null)
                {
                    _ButtonChangePwdFileDB.Click += ButtonChangePwdFileDB_Click;
                }
            }
        }

        internal Label Label1;
        internal TextBox NewPassword;
        internal Label Label2;
        internal TextBox TextBox3;
        private LinkLabel _LinkLabel1;

        internal LinkLabel LinkLabel1
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _LinkLabel1;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_LinkLabel1 != null)
                {
                    _LinkLabel1.Click -= LinkLabel1_Click;
                }

                _LinkLabel1 = value;
                if (_LinkLabel1 != null)
                {
                    _LinkLabel1.Click += LinkLabel1_Click;
                }
            }
        }

        internal Label Label3;
        private LinkLabel _LinkLabel2;

        internal LinkLabel LinkLabel2
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _LinkLabel2;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_LinkLabel2 != null)
                {
                    _LinkLabel2.Click -= LinkLabel2_Click;
                }

                _LinkLabel2 = value;
                if (_LinkLabel2 != null)
                {
                    _LinkLabel2.Click += LinkLabel2_Click;
                }
            }
        }

        internal Label LabelDatabaseVersion;
        internal Label LabelDatabaseVersionRepo;
        internal Label Label7;
        private ComboBox _cbDBType;

        internal ComboBox cbDBType
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cbDBType;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cbDBType != null)
                {
                    _cbDBType.SelectedIndexChanged -= CbDBType_SelectedIndexChanged;
                }

                _cbDBType = value;
                if (_cbDBType != null)
                {
                    _cbDBType.SelectedIndexChanged += CbDBType_SelectedIndexChanged;
                }
            }
        }
    }
}