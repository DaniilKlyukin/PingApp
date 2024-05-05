namespace PingApp
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            timer = new System.Windows.Forms.Timer(components);
            startButton = new Button();
            stopButton = new Button();
            saveCheckBox = new CheckBox();
            notifyIcon = new NotifyIcon(components);
            showPlotButton = new Button();
            clearButton = new Button();
            label2 = new Label();
            checkPeriodNumeric = new NumericUpDown();
            addUserButton = new Button();
            deleteUserButton = new Button();
            dataGridView = new DataGridView();
            nameOrAddressDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            statusStringDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
            atWorkDataGridViewCheckBoxColumn = new DataGridViewCheckBoxColumn();
            datagridUserItemBindingSource = new BindingSource(components);
            showUsersButton = new Button();
            ((System.ComponentModel.ISupportInitialize)checkPeriodNumeric).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)datagridUserItemBindingSource).BeginInit();
            SuspendLayout();
            // 
            // timer
            // 
            timer.Interval = 5000;
            timer.Tick += timer_Tick;
            // 
            // startButton
            // 
            startButton.Location = new Point(12, 12);
            startButton.Name = "startButton";
            startButton.Size = new Size(128, 36);
            startButton.TabIndex = 0;
            startButton.Text = "Запустить";
            startButton.UseVisualStyleBackColor = true;
            startButton.Click += startButton_Click;
            // 
            // stopButton
            // 
            stopButton.Location = new Point(146, 12);
            stopButton.Name = "stopButton";
            stopButton.Size = new Size(128, 36);
            stopButton.TabIndex = 1;
            stopButton.Text = "Остановить";
            stopButton.UseVisualStyleBackColor = true;
            stopButton.Click += stopButton_Click;
            // 
            // saveCheckBox
            // 
            saveCheckBox.AutoSize = true;
            saveCheckBox.Checked = true;
            saveCheckBox.CheckState = CheckState.Checked;
            saveCheckBox.Location = new Point(87, 125);
            saveCheckBox.Name = "saveCheckBox";
            saveCheckBox.Size = new Size(187, 19);
            saveCheckBox.TabIndex = 5;
            saveCheckBox.Text = "Сохранять статистику в файл";
            saveCheckBox.UseVisualStyleBackColor = true;
            // 
            // notifyIcon
            // 
            notifyIcon.Icon = (Icon)resources.GetObject("notifyIcon.Icon");
            notifyIcon.Visible = true;
            notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;
            // 
            // showPlotButton
            // 
            showPlotButton.Location = new Point(146, 150);
            showPlotButton.Name = "showPlotButton";
            showPlotButton.Size = new Size(128, 36);
            showPlotButton.TabIndex = 6;
            showPlotButton.Text = "Показать график";
            showPlotButton.UseVisualStyleBackColor = true;
            showPlotButton.Click += showPlotButton_Click;
            // 
            // clearButton
            // 
            clearButton.Location = new Point(12, 150);
            clearButton.Name = "clearButton";
            clearButton.Size = new Size(128, 36);
            clearButton.TabIndex = 7;
            clearButton.Text = "Очистить стат.";
            clearButton.UseVisualStyleBackColor = true;
            clearButton.Click += clearButton_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 96);
            label2.Name = "label2";
            label2.Size = new Size(132, 15);
            label2.TabIndex = 9;
            label2.Text = "Период проверки, сек:";
            // 
            // checkPeriodNumeric
            // 
            checkPeriodNumeric.Location = new Point(150, 96);
            checkPeriodNumeric.Maximum = new decimal(new int[] { 10000000, 0, 0, 0 });
            checkPeriodNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            checkPeriodNumeric.Name = "checkPeriodNumeric";
            checkPeriodNumeric.Size = new Size(124, 23);
            checkPeriodNumeric.TabIndex = 10;
            checkPeriodNumeric.Value = new decimal(new int[] { 10, 0, 0, 0 });
            checkPeriodNumeric.ValueChanged += checkPeriodNumeric_ValueChanged;
            // 
            // addUserButton
            // 
            addUserButton.Location = new Point(12, 54);
            addUserButton.Name = "addUserButton";
            addUserButton.Size = new Size(128, 36);
            addUserButton.TabIndex = 11;
            addUserButton.Text = "Добавить";
            addUserButton.UseVisualStyleBackColor = true;
            addUserButton.Click += addUserButton_Click;
            // 
            // deleteUserButton
            // 
            deleteUserButton.Location = new Point(146, 54);
            deleteUserButton.Name = "deleteUserButton";
            deleteUserButton.Size = new Size(128, 36);
            deleteUserButton.TabIndex = 12;
            deleteUserButton.Text = "Удалить";
            deleteUserButton.UseVisualStyleBackColor = true;
            deleteUserButton.Click += deleteUserButton_Click;
            // 
            // dataGridView
            // 
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView.AutoGenerateColumns = false;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.Columns.AddRange(new DataGridViewColumn[] { nameOrAddressDataGridViewTextBoxColumn, statusStringDataGridViewTextBoxColumn, atWorkDataGridViewCheckBoxColumn });
            dataGridView.DataSource = datagridUserItemBindingSource;
            dataGridView.Location = new Point(280, 12);
            dataGridView.Name = "dataGridView";
            dataGridView.ReadOnly = true;
            dataGridView.RowTemplate.Height = 25;
            dataGridView.Size = new Size(571, 216);
            dataGridView.TabIndex = 13;
            dataGridView.PreviewKeyDown += dataGridView_PreviewKeyDown;
            // 
            // nameOrAddressDataGridViewTextBoxColumn
            // 
            nameOrAddressDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            nameOrAddressDataGridViewTextBoxColumn.DataPropertyName = "NameOrAddress";
            nameOrAddressDataGridViewTextBoxColumn.HeaderText = "Имя или адрес";
            nameOrAddressDataGridViewTextBoxColumn.Name = "nameOrAddressDataGridViewTextBoxColumn";
            nameOrAddressDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // statusStringDataGridViewTextBoxColumn
            // 
            statusStringDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            statusStringDataGridViewTextBoxColumn.DataPropertyName = "StatusString";
            statusStringDataGridViewTextBoxColumn.HeaderText = "Статус";
            statusStringDataGridViewTextBoxColumn.Name = "statusStringDataGridViewTextBoxColumn";
            statusStringDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // atWorkDataGridViewCheckBoxColumn
            // 
            atWorkDataGridViewCheckBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            atWorkDataGridViewCheckBoxColumn.DataPropertyName = "AtWork";
            atWorkDataGridViewCheckBoxColumn.HeaderText = "На работе";
            atWorkDataGridViewCheckBoxColumn.Name = "atWorkDataGridViewCheckBoxColumn";
            atWorkDataGridViewCheckBoxColumn.ReadOnly = true;
            // 
            // datagridUserItemBindingSource
            // 
            datagridUserItemBindingSource.DataSource = typeof(DatagridUserItem);
            // 
            // showUsersButton
            // 
            showUsersButton.Location = new Point(12, 192);
            showUsersButton.Name = "showUsersButton";
            showUsersButton.Size = new Size(262, 36);
            showUsersButton.TabIndex = 14;
            showUsersButton.Text = "Показать пользователей в сети";
            showUsersButton.UseVisualStyleBackColor = true;
            showUsersButton.Click += showLocalAddressButton_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(863, 241);
            Controls.Add(showUsersButton);
            Controls.Add(dataGridView);
            Controls.Add(deleteUserButton);
            Controls.Add(addUserButton);
            Controls.Add(checkPeriodNumeric);
            Controls.Add(label2);
            Controls.Add(clearButton);
            Controls.Add(showPlotButton);
            Controls.Add(saveCheckBox);
            Controls.Add(stopButton);
            Controls.Add(startButton);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Text = "Сканер компьютеров в сети";
            ((System.ComponentModel.ISupportInitialize)checkPeriodNumeric).EndInit();
            ((System.ComponentModel.ISupportInitialize)dataGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)datagridUserItemBindingSource).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Timer timer;
        private Button startButton;
        private Button stopButton;
        private CheckBox saveCheckBox;
        private NotifyIcon notifyIcon;
        private Button showPlotButton;
        private Button clearButton;
        private Label label2;
        private NumericUpDown checkPeriodNumeric;
        private Button addUserButton;
        private Button deleteUserButton;
        private DataGridView dataGridView;
        private BindingSource datagridUserItemBindingSource;
        private DataGridViewTextBoxColumn nameOrAddressDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn statusStringDataGridViewTextBoxColumn;
        private DataGridViewCheckBoxColumn atWorkDataGridViewCheckBoxColumn;
        private Button showUsersButton;
    }
}