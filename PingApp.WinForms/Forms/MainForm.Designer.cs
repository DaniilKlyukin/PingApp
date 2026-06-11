using PingApp.WinForms.Models;

namespace PingApp.WinForms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        timer = new System.Windows.Forms.Timer(components);
        notifyIcon = new NotifyIcon(components);
        showPlotButton = new Button();
        addUserButton = new Button();
        deleteUserButton = new Button();
        dataGridView = new DataGridView();
        nameOrAddressDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
        Nickname = new DataGridViewTextBoxColumn();
        statusStringDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
        atWorkDataGridViewCheckBoxColumn = new DataGridViewCheckBoxColumn();
        datagridUserItemBindingSource = new BindingSource(components);
        adminButton = new Button();
        ((System.ComponentModel.ISupportInitialize)dataGridView).BeginInit();
        ((System.ComponentModel.ISupportInitialize)datagridUserItemBindingSource).BeginInit();
        SuspendLayout();
        // 
        // timer
        // 
        timer.Enabled = true;
        timer.Interval = 5000;
        timer.Tick += timer_Tick;
        // 
        // notifyIcon
        // 
        notifyIcon.Icon = (Icon)resources.GetObject("notifyIcon.Icon");
        notifyIcon.Visible = true;
        notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;
        // 
        // showPlotButton
        // 
        showPlotButton.Location = new Point(12, 96);
        showPlotButton.Name = "showPlotButton";
        showPlotButton.Size = new Size(256, 36);
        showPlotButton.TabIndex = 6;
        showPlotButton.Text = "Показать график";
        showPlotButton.UseVisualStyleBackColor = true;
        showPlotButton.Click += showPlotButton_Click;
        // 
        // addUserButton
        // 
        addUserButton.Location = new Point(12, 12);
        addUserButton.Name = "addUserButton";
        addUserButton.Size = new Size(125, 36);
        addUserButton.TabIndex = 11;
        addUserButton.Text = "Добавить";
        addUserButton.UseVisualStyleBackColor = true;
        addUserButton.Click += addUserButton_Click;
        // 
        // deleteUserButton
        // 
        deleteUserButton.Location = new Point(143, 12);
        deleteUserButton.Name = "deleteUserButton";
        deleteUserButton.Size = new Size(125, 36);
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
        dataGridView.Columns.AddRange(new DataGridViewColumn[] { nameOrAddressDataGridViewTextBoxColumn, Nickname, statusStringDataGridViewTextBoxColumn, atWorkDataGridViewCheckBoxColumn });
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
        nameOrAddressDataGridViewTextBoxColumn.DataPropertyName = "Address";
        nameOrAddressDataGridViewTextBoxColumn.HeaderText = "Адрес";
        nameOrAddressDataGridViewTextBoxColumn.Name = "nameOrAddressDataGridViewTextBoxColumn";
        nameOrAddressDataGridViewTextBoxColumn.ReadOnly = true;
        // 
        // Nickname
        // 
        Nickname.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        Nickname.DataPropertyName = "Nickname";
        Nickname.HeaderText = "Имя";
        Nickname.Name = "Nickname";
        Nickname.ReadOnly = true;
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
        atWorkDataGridViewCheckBoxColumn.HeaderText = "В сети";
        atWorkDataGridViewCheckBoxColumn.Name = "atWorkDataGridViewCheckBoxColumn";
        atWorkDataGridViewCheckBoxColumn.ReadOnly = true;
        // 
        // datagridUserItemBindingSource
        // 
        datagridUserItemBindingSource.DataSource = typeof(DataGridUserItem);
        // 
        // adminButton
        // 
        adminButton.Location = new Point(12, 54);
        adminButton.Name = "adminButton";
        adminButton.Size = new Size(256, 36);
        adminButton.TabIndex = 15;
        adminButton.Text = "Администрирование";
        adminButton.UseVisualStyleBackColor = true;
        adminButton.Visible = false;
        adminButton.Click += adminButton_Click;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(863, 241);
        Controls.Add(adminButton);
        Controls.Add(dataGridView);
        Controls.Add(deleteUserButton);
        Controls.Add(addUserButton);
        Controls.Add(showPlotButton);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        Text = "Сканер компьютеров в сети";
        Load += MainForm_Load;
        ((System.ComponentModel.ISupportInitialize)dataGridView).EndInit();
        ((System.ComponentModel.ISupportInitialize)datagridUserItemBindingSource).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.Timer timer;
    private NotifyIcon notifyIcon;
    private Button showPlotButton;
    private Button addUserButton;
    private Button deleteUserButton;
    private DataGridView dataGridView;
    private BindingSource datagridUserItemBindingSource;
    private DataGridViewTextBoxColumn nameOrAddressDataGridViewTextBoxColumn;
    private DataGridViewTextBoxColumn Nickname;
    private DataGridViewTextBoxColumn statusStringDataGridViewTextBoxColumn;
    private DataGridViewCheckBoxColumn atWorkDataGridViewCheckBoxColumn;
    private Button adminButton;
}