using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace PasswordChanger1C
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            _Button6 = new Button();
            _Button6.Click += new EventHandler(Button6_Click);
            FileIB = new TextBox();
            OpenFileDialog = new OpenFileDialog();
            _ButtonGetUsers = new Button();
            _ButtonGetUsers.Click += new EventHandler(ButtonGetUsers_Click);
            ListViewUsers = new ListView();
            UserGUID = new ColumnHeader();
            UserName = new ColumnHeader();
            UserDescr = new ColumnHeader();
            UserPassHash = new ColumnHeader();
            UserAdmRole = new ColumnHeader();
            ConnectionString = new TextBox();
            _Button2 = new Button();
            _Button2.Click += new EventHandler(Button2_Click);
            Label4 = new Label();
            Repo1C = new TextBox();
            _ButtonRepo = new Button();
            _ButtonRepo.Click += new EventHandler(ButtonRepo_Click);
            OpenFileDialogRepo = new OpenFileDialog();
            _ButtonGetRepoUsers = new Button();
            _ButtonGetRepoUsers.Click += new EventHandler(ButtonGetRepoUsers_Click);
            _ButtonSetRepoPassword = new Button();
            _ButtonSetRepoPassword.Click += new EventHandler(ButtonSetRepoPassword_Click);
            TabControl1 = new TabControl();
            TabPage1 = new TabPage();
            LabelDatabaseVersion = new Label();
            NewPassword = new TextBox();
            Label2 = new Label();
            TextBox3 = new TextBox();
            _ButtonChangePwdFileDB = new Button();
            _ButtonChangePwdFileDB.Click += new EventHandler(ButtonChangePwdFileDB_Click);
            Label1 = new Label();
            TabPage2 = new TabPage();
            Label7 = new Label();
            _cbDBType = new ComboBox();
            _cbDBType.SelectedIndexChanged += new EventHandler(CbDBType_SelectedIndexChanged);
            TextBox2 = new TextBox();
            _ButtonChangePassSQL = new Button();
            _ButtonChangePassSQL.Click += new EventHandler(ButtonChangePassSQL_Click);
            Label6 = new Label();
            NewPassSQL = new TextBox();
            Label5 = new Label();
            SQLUserList = new ListView();
            ColumnHeader1 = new ColumnHeader();
            ColumnHeader2 = new ColumnHeader();
            ColumnHeader3 = new ColumnHeader();
            ColumnHeader4 = new ColumnHeader();
            ColumnHeader5 = new ColumnHeader();
            TabPage3 = new TabPage();
            LabelDatabaseVersionRepo = new Label();
            TextBox1 = new TextBox();
            RepoUserList = new ListView();
            RepoUserGUID = new ColumnHeader();
            RepoUserName = new ColumnHeader();
            RepoHasPwd = new ColumnHeader();
            RepoAdmin = new ColumnHeader();
            _LinkLabel1 = new LinkLabel();
            _LinkLabel1.Click += new EventHandler(LinkLabel1_Click);
            Label3 = new Label();
            _LinkLabel2 = new LinkLabel();
            _LinkLabel2.Click += new EventHandler(LinkLabel2_Click);
            TabControl1.SuspendLayout();
            TabPage1.SuspendLayout();
            TabPage2.SuspendLayout();
            TabPage3.SuspendLayout();
            SuspendLayout();
            // 
            // Button6
            // 
            _Button6.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _Button6.Location = new Point(636, 24);
            _Button6.Name = "_Button6";
            _Button6.Size = new Size(95, 22);
            _Button6.TabIndex = 11;
            _Button6.Text = "Выбрать файл";
            _Button6.UseVisualStyleBackColor = true;
            // 
            // FileIB
            // 
            FileIB.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            FileIB.Location = new Point(9, 25);
            FileIB.Name = "FileIB";
            FileIB.Size = new Size(625, 20);
            FileIB.TabIndex = 9;
            // 
            // OpenFileDialog
            // 
            OpenFileDialog.Filter = "1C DB files|*.1cd";
            OpenFileDialog.RestoreDirectory = true;
            OpenFileDialog.Title = "Выберите файл информационной базы 1С";
            // 
            // ButtonGetUsers
            // 
            _ButtonGetUsers.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _ButtonGetUsers.Location = new Point(733, 24);
            _ButtonGetUsers.Name = "_ButtonGetUsers";
            _ButtonGetUsers.Size = new Size(183, 22);
            _ButtonGetUsers.TabIndex = 12;
            _ButtonGetUsers.Text = "Получить список пользователей";
            _ButtonGetUsers.UseVisualStyleBackColor = true;
            // 
            // ListViewUsers
            // 
            ListViewUsers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            ListViewUsers.Columns.AddRange(new ColumnHeader[] { UserGUID, UserName, UserDescr, UserPassHash, UserAdmRole });
            ListViewUsers.FullRowSelect = true;
            ListViewUsers.HideSelection = false;
            ListViewUsers.Location = new Point(9, 50);
            ListViewUsers.Name = "ListViewUsers";
            ListViewUsers.Size = new Size(905, 343);
            ListViewUsers.TabIndex = 14;
            ListViewUsers.UseCompatibleStateImageBehavior = false;
            ListViewUsers.View = View.Details;
            // 
            // UserGUID
            // 
            UserGUID.Text = "GUID";
            UserGUID.Width = 158;
            // 
            // UserName
            // 
            UserName.Text = "Имя";
            UserName.Width = 164;
            // 
            // UserDescr
            // 
            UserDescr.Text = "Полное имя";
            UserDescr.Width = 147;
            // 
            // UserPassHash
            // 
            UserPassHash.Text = "Хеш пароля";
            UserPassHash.Width = 164;
            // 
            // UserAdmRole
            // 
            UserAdmRole.Text = "Адм.роль";
            UserAdmRole.Width = 66;
            // 
            // ConnectionString
            // 
            ConnectionString.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ConnectionString.Font = new Font("Microsoft Sans Serif", 12.0f, FontStyle.Regular, GraphicsUnit.Point, 0);
            ConnectionString.Location = new Point(9, 48);
            ConnectionString.Name = "ConnectionString";
            ConnectionString.Size = new Size(734, 26);
            ConnectionString.TabIndex = 9;
            ConnectionString.Text = "Data Source=MSSQL1;Server=SERVER;Integrated Security=true;Database=DATABASE";
            // 
            // Button2
            // 
            _Button2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _Button2.Location = new Point(745, 47);
            _Button2.Name = "_Button2";
            _Button2.Size = new Size(172, 22);
            _Button2.TabIndex = 12;
            _Button2.Text = "Получить пользователей";
            _Button2.UseVisualStyleBackColor = true;
            // 
            // Label4
            // 
            Label4.AutoSize = true;
            Label4.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
            Label4.Location = new Point(12, 6);
            Label4.Name = "Label4";
            Label4.Size = new Size(265, 16);
            Label4.TabIndex = 10;
            Label4.Text = "Файл хранилища конфигурации 1С";
            // 
            // Repo1C
            // 
            Repo1C.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            Repo1C.Location = new Point(9, 25);
            Repo1C.Name = "Repo1C";
            Repo1C.Size = new Size(616, 20);
            Repo1C.TabIndex = 9;
            // 
            // ButtonRepo
            // 
            _ButtonRepo.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _ButtonRepo.Location = new Point(627, 23);
            _ButtonRepo.Name = "_ButtonRepo";
            _ButtonRepo.Size = new Size(98, 23);
            _ButtonRepo.TabIndex = 11;
            _ButtonRepo.Text = "Выбрать файл";
            _ButtonRepo.UseVisualStyleBackColor = true;
            // 
            // OpenFileDialogRepo
            // 
            OpenFileDialogRepo.FileName = "OpenFileDialogRepo";
            OpenFileDialogRepo.Filter = "1C DB files|*.1cd";
            OpenFileDialogRepo.Title = "Выберите файл хранилища 1С";
            // 
            // ButtonGetRepoUsers
            // 
            _ButtonGetRepoUsers.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _ButtonGetRepoUsers.Location = new Point(725, 23);
            _ButtonGetRepoUsers.Name = "_ButtonGetRepoUsers";
            _ButtonGetRepoUsers.Size = new Size(192, 23);
            _ButtonGetRepoUsers.TabIndex = 12;
            _ButtonGetRepoUsers.Text = "Получить список пользователей";
            _ButtonGetRepoUsers.UseVisualStyleBackColor = true;
            // 
            // ButtonSetRepoPassword
            // 
            _ButtonSetRepoPassword.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _ButtonSetRepoPassword.Font = new Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
            _ButtonSetRepoPassword.ForeColor = Color.FromArgb(0, 64, 0);
            _ButtonSetRepoPassword.Location = new Point(625, 413);
            _ButtonSetRepoPassword.Name = "_ButtonSetRepoPassword";
            _ButtonSetRepoPassword.Size = new Size(289, 51);
            _ButtonSetRepoPassword.TabIndex = 16;
            _ButtonSetRepoPassword.Text = "Установить выбранным пользователям пустой пароль в хранилище";
            _ButtonSetRepoPassword.UseVisualStyleBackColor = true;
            // 
            // TabControl1
            // 
            TabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            TabControl1.Controls.Add(TabPage1);
            TabControl1.Controls.Add(TabPage2);
            TabControl1.Controls.Add(TabPage3);
            TabControl1.Location = new Point(4, 4);
            TabControl1.Name = "TabControl1";
            TabControl1.SelectedIndex = 0;
            TabControl1.Size = new Size(928, 496);
            TabControl1.TabIndex = 17;
            // 
            // TabPage1
            // 
            TabPage1.Controls.Add(LabelDatabaseVersion);
            TabPage1.Controls.Add(NewPassword);
            TabPage1.Controls.Add(Label2);
            TabPage1.Controls.Add(TextBox3);
            TabPage1.Controls.Add(_ButtonChangePwdFileDB);
            TabPage1.Controls.Add(Label1);
            TabPage1.Controls.Add(ListViewUsers);
            TabPage1.Controls.Add(FileIB);
            TabPage1.Controls.Add(_Button6);
            TabPage1.Controls.Add(_ButtonGetUsers);
            TabPage1.Location = new Point(4, 22);
            TabPage1.Name = "TabPage1";
            TabPage1.Padding = new Padding(3);
            TabPage1.Size = new Size(920, 470);
            TabPage1.TabIndex = 0;
            TabPage1.Text = "Файловая ИБ";
            TabPage1.UseVisualStyleBackColor = true;
            // 
            // LabelDatabaseVersion
            // 
            LabelDatabaseVersion.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            LabelDatabaseVersion.AutoSize = true;
            LabelDatabaseVersion.Location = new Point(11, 396);
            LabelDatabaseVersion.Name = "LabelDatabaseVersion";
            LabelDatabaseVersion.Size = new Size(132, 13);
            LabelDatabaseVersion.TabIndex = 26;
            LabelDatabaseVersion.Text = "Internal database version: ";
            // 
            // NewPassword
            // 
            NewPassword.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            NewPassword.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
            NewPassword.Location = new Point(128, 443);
            NewPassword.Name = "NewPassword";
            NewPassword.Size = new Size(144, 22);
            NewPassword.TabIndex = 24;
            NewPassword.Text = "12345";
            // 
            // Label2
            // 
            Label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Label2.AutoSize = true;
            Label2.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
            Label2.Location = new Point(9, 445);
            Label2.Name = "Label2";
            Label2.Size = new Size(113, 16);
            Label2.TabIndex = 25;
            Label2.Text = "Новый пароль";
            // 
            // TextBox3
            // 
            TextBox3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TextBox3.BorderStyle = BorderStyle.None;
            TextBox3.Font = new Font("Arial", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
            TextBox3.Location = new Point(9, 413);
            TextBox3.Multiline = true;
            TextBox3.Name = "TextBox3";
            TextBox3.Size = new Size(613, 29);
            TextBox3.TabIndex = 23;
            TextBox3.Text = "Файл информационной базы не должен быть открыт никакими другими приложениями.";
            // 
            // ButtonChangePwdFileDB
            // 
            _ButtonChangePwdFileDB.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _ButtonChangePwdFileDB.Font = new Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
            _ButtonChangePwdFileDB.ForeColor = Color.FromArgb(0, 64, 0);
            _ButtonChangePwdFileDB.Location = new Point(625, 414);
            _ButtonChangePwdFileDB.Name = "_ButtonChangePwdFileDB";
            _ButtonChangePwdFileDB.Size = new Size(289, 51);
            _ButtonChangePwdFileDB.TabIndex = 22;
            _ButtonChangePwdFileDB.Text = "Установить выбранным пользователям указанный пароль";
            _ButtonChangePwdFileDB.UseVisualStyleBackColor = true;
            // 
            // Label1
            // 
            Label1.AutoSize = true;
            Label1.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
            Label1.Location = new Point(10, 6);
            Label1.Name = "Label1";
            Label1.Size = new Size(244, 16);
            Label1.TabIndex = 20;
            Label1.Text = "Файл информационной базы 1С";
            // 
            // TabPage2
            // 
            TabPage2.Controls.Add(Label7);
            TabPage2.Controls.Add(_cbDBType);
            TabPage2.Controls.Add(TextBox2);
            TabPage2.Controls.Add(_ButtonChangePassSQL);
            TabPage2.Controls.Add(Label6);
            TabPage2.Controls.Add(NewPassSQL);
            TabPage2.Controls.Add(Label5);
            TabPage2.Controls.Add(ConnectionString);
            TabPage2.Controls.Add(_Button2);
            TabPage2.Controls.Add(SQLUserList);
            TabPage2.Location = new Point(4, 22);
            TabPage2.Name = "TabPage2";
            TabPage2.Padding = new Padding(3);
            TabPage2.Size = new Size(920, 470);
            TabPage2.TabIndex = 1;
            TabPage2.Text = "Клиент-серверная ИБ";
            TabPage2.UseVisualStyleBackColor = true;
            // 
            // Label7
            // 
            Label7.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
            Label7.Location = new Point(9, 9);
            Label7.Name = "Label7";
            Label7.Size = new Size(166, 17);
            Label7.TabIndex = 24;
            Label7.Text = "Тип базы данных:";
            // 
            // cbDBType
            // 
            _cbDBType.DropDownStyle = ComboBoxStyle.DropDownList;
            _cbDBType.FormattingEnabled = true;
            _cbDBType.Items.AddRange(new object[] { "Microsoft SQL Server", "PostgreSQL Server" });
            _cbDBType.Location = new Point(181, 7);
            _cbDBType.Name = "_cbDBType";
            _cbDBType.Size = new Size(185, 21);
            _cbDBType.TabIndex = 23;
            // 
            // TextBox2
            // 
            TextBox2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TextBox2.BorderStyle = BorderStyle.None;
            TextBox2.Font = new Font("Arial", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
            TextBox2.Location = new Point(9, 413);
            TextBox2.Multiline = true;
            TextBox2.Name = "TextBox2";
            TextBox2.Size = new Size(610, 24);
            TextBox2.TabIndex = 22;
            TextBox2.Text = "Монопольного режима доступа к базе не требуется";
            // 
            // ButtonChangePassSQL
            // 
            _ButtonChangePassSQL.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _ButtonChangePassSQL.Font = new Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
            _ButtonChangePassSQL.ForeColor = Color.FromArgb(0, 64, 0);
            _ButtonChangePassSQL.Location = new Point(625, 414);
            _ButtonChangePassSQL.Name = "_ButtonChangePassSQL";
            _ButtonChangePassSQL.Size = new Size(289, 51);
            _ButtonChangePassSQL.TabIndex = 21;
            _ButtonChangePassSQL.Text = "Установить выбранным пользователям указанный пароль";
            _ButtonChangePassSQL.UseVisualStyleBackColor = true;
            // 
            // Label6
            // 
            Label6.AutoSize = true;
            Label6.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
            Label6.Location = new Point(9, 29);
            Label6.Name = "Label6";
            Label6.Size = new Size(297, 16);
            Label6.TabIndex = 19;
            Label6.Text = "Строка соединения с базой данных 1С:";
            // 
            // NewPassSQL
            // 
            NewPassSQL.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            NewPassSQL.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
            NewPassSQL.Location = new Point(128, 443);
            NewPassSQL.Name = "NewPassSQL";
            NewPassSQL.Size = new Size(144, 22);
            NewPassSQL.TabIndex = 17;
            NewPassSQL.Text = "12345";
            // 
            // Label5
            // 
            Label5.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Label5.AutoSize = true;
            Label5.Font = new Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 0);
            Label5.Location = new Point(9, 445);
            Label5.Name = "Label5";
            Label5.Size = new Size(113, 16);
            Label5.TabIndex = 18;
            Label5.Text = "Новый пароль";
            // 
            // SQLUserList
            // 
            SQLUserList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            SQLUserList.Columns.AddRange(new ColumnHeader[] { ColumnHeader1, ColumnHeader2, ColumnHeader3, ColumnHeader4, ColumnHeader5 });
            SQLUserList.FullRowSelect = true;
            SQLUserList.HideSelection = false;
            SQLUserList.Location = new Point(9, 76);
            SQLUserList.Name = "SQLUserList";
            SQLUserList.Size = new Size(905, 335);
            SQLUserList.TabIndex = 20;
            SQLUserList.UseCompatibleStateImageBehavior = false;
            SQLUserList.View = View.Details;
            // 
            // ColumnHeader1
            // 
            ColumnHeader1.Text = "GUID";
            ColumnHeader1.Width = 232;
            // 
            // ColumnHeader2
            // 
            ColumnHeader2.Text = "Имя";
            ColumnHeader2.Width = 183;
            // 
            // ColumnHeader3
            // 
            ColumnHeader3.Text = "Полное имя";
            ColumnHeader3.Width = 199;
            // 
            // ColumnHeader4
            // 
            ColumnHeader4.Text = "Хеш пароля";
            ColumnHeader4.Width = 220;
            // 
            // ColumnHeader5
            // 
            ColumnHeader5.Text = "Адм.роль";
            ColumnHeader5.Width = 66;
            // 
            // TabPage3
            // 
            TabPage3.Controls.Add(LabelDatabaseVersionRepo);
            TabPage3.Controls.Add(TextBox1);
            TabPage3.Controls.Add(RepoUserList);
            TabPage3.Controls.Add(Label4);
            TabPage3.Controls.Add(_ButtonSetRepoPassword);
            TabPage3.Controls.Add(_ButtonGetRepoUsers);
            TabPage3.Controls.Add(Repo1C);
            TabPage3.Controls.Add(_ButtonRepo);
            TabPage3.Location = new Point(4, 22);
            TabPage3.Name = "TabPage3";
            TabPage3.Padding = new Padding(3);
            TabPage3.Size = new Size(920, 470);
            TabPage3.TabIndex = 2;
            TabPage3.Text = "Хранилище конфигурации";
            TabPage3.UseVisualStyleBackColor = true;
            // 
            // LabelDatabaseVersionRepo
            // 
            LabelDatabaseVersionRepo.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            LabelDatabaseVersionRepo.AutoSize = true;
            LabelDatabaseVersionRepo.Location = new Point(12, 398);
            LabelDatabaseVersionRepo.Name = "LabelDatabaseVersionRepo";
            LabelDatabaseVersionRepo.Size = new Size(132, 13);
            LabelDatabaseVersionRepo.TabIndex = 27;
            LabelDatabaseVersionRepo.Text = "Internal database version: ";
            // 
            // TextBox1
            // 
            TextBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TextBox1.BorderStyle = BorderStyle.None;
            TextBox1.Font = new Font("Arial", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 0);
            TextBox1.Location = new Point(6, 413);
            TextBox1.Multiline = true;
            TextBox1.Name = "TextBox1";
            TextBox1.Size = new Size(613, 50);
            TextBox1.TabIndex = 18;
            TextBox1.Text = "Файл хранилища конфигурации не должен быть открыт никакими другими приложениями.";
            // 
            // RepoUserList
            // 
            RepoUserList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            RepoUserList.Columns.AddRange(new ColumnHeader[] { RepoUserGUID, RepoUserName, RepoHasPwd, RepoAdmin });
            RepoUserList.FullRowSelect = true;
            RepoUserList.HideSelection = false;
            RepoUserList.Location = new Point(9, 50);
            RepoUserList.Name = "RepoUserList";
            RepoUserList.Size = new Size(905, 345);
            RepoUserList.TabIndex = 17;
            RepoUserList.UseCompatibleStateImageBehavior = false;
            RepoUserList.View = View.Details;
            // 
            // RepoUserGUID
            // 
            RepoUserGUID.Text = "GUID";
            RepoUserGUID.Width = 221;
            // 
            // RepoUserName
            // 
            RepoUserName.Text = "Имя";
            RepoUserName.Width = 183;
            // 
            // RepoHasPwd
            // 
            RepoHasPwd.Text = "Пароль установлен";
            RepoHasPwd.Width = 134;
            // 
            // RepoAdmin
            // 
            RepoAdmin.Text = "Это администратор";
            RepoAdmin.Width = 115;
            // 
            // LinkLabel1
            // 
            _LinkLabel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            _LinkLabel1.AutoSize = true;
            _LinkLabel1.Location = new Point(230, 501);
            _LinkLabel1.Name = "_LinkLabel1";
            _LinkLabel1.Size = new Size(282, 13);
            _LinkLabel1.TabIndex = 18;
            _LinkLabel1.TabStop = true;
            _LinkLabel1.Text = "https://github.com/alekseybochkov/PasswordChanger1C";
            // 
            // Label3
            // 
            Label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            Label3.AutoSize = true;
            Label3.Location = new Point(3, 501);
            Label3.Name = "Label3";
            Label3.Size = new Size(227, 13);
            Label3.TabIndex = 19;
            Label3.Text = "Страница приложения для обратной связи:";
            // 
            // LinkLabel2
            // 
            _LinkLabel2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            _LinkLabel2.AutoSize = true;
            _LinkLabel2.Location = new Point(831, 501);
            _LinkLabel2.Name = "_LinkLabel2";
            _LinkLabel2.Size = new Size(102, 13);
            _LinkLabel2.TabIndex = 18;
            _LinkLabel2.TabStop = true;
            _LinkLabel2.Text = "© Aleksey.Bochkov";
            // 
            // MainForm
            // 
            AccessibleRole = AccessibleRole.Application;
            AutoScaleDimensions = new SizeF(6.0f, 13.0f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(934, 516);
            Controls.Add(Label3);
            Controls.Add(_LinkLabel2);
            Controls.Add(_LinkLabel1);
            Controls.Add(TabControl1);
            HelpButton = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Изменение паролей для информационных баз и хранилища 1С";
            TabControl1.ResumeLayout(false);
            TabPage1.ResumeLayout(false);
            TabPage1.PerformLayout();
            TabPage2.ResumeLayout(false);
            TabPage2.PerformLayout();
            TabPage3.ResumeLayout(false);
            TabPage3.PerformLayout();
            Shown += new EventHandler(MainForm_Shown);
            ResumeLayout(false);
            PerformLayout();
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