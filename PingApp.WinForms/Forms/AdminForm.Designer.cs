namespace PingApp.WinForms
{
    partial class AdminForm
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

        private void InitializeComponent()
        {
            dataGridView = new DataGridView();
            AddressColumn = new DataGridViewTextBoxColumn();
            AllowedColumn = new DataGridViewCheckBoxColumn();
            VisibleColumn = new DataGridViewCheckBoxColumn();
            saveButton = new Button();
            allowAllPingButton = new Button();
            denyAllPingButton = new Button();
            allowAllVisibleButton = new Button();
            denyAllVisibleButton = new Button();
            intervalNumeric = new NumericUpDown();
            label1 = new Label();
            clearStatsButton = new Button();
            ((System.ComponentModel.ISupportInitialize)dataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)intervalNumeric).BeginInit();
            SuspendLayout();
            // 
            // dataGridView
            // 
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.Columns.AddRange(new DataGridViewColumn[] { AddressColumn, AllowedColumn, VisibleColumn });
            dataGridView.Location = new Point(12, 12);
            dataGridView.Name = "dataGridView";
            dataGridView.RowTemplate.Height = 25;
            dataGridView.Size = new Size(480, 230);
            dataGridView.TabIndex = 0;
            // 
            // AddressColumn
            // 
            AddressColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            AddressColumn.HeaderText = "Адрес";
            AddressColumn.Name = "AddressColumn";
            AddressColumn.ReadOnly = true;
            // 
            // AllowedColumn
            // 
            AllowedColumn.HeaderText = "Пинговать";
            AllowedColumn.Name = "AllowedColumn";
            AllowedColumn.Width = 80;
            // 
            // VisibleColumn
            // 
            VisibleColumn.HeaderText = "Показывать";
            VisibleColumn.Name = "VisibleColumn";
            VisibleColumn.Width = 90;
            // 
            // saveButton
            // 
            saveButton.Location = new Point(312, 380);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(180, 36);
            saveButton.TabIndex = 1;
            saveButton.Text = "Сохранить и закрыть";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += saveButton_Click;
            // 
            // allowAllPingButton
            // 
            allowAllPingButton.Location = new Point(12, 290);
            allowAllPingButton.Name = "allowAllPingButton";
            allowAllPingButton.Size = new Size(230, 30);
            allowAllPingButton.TabIndex = 2;
            allowAllPingButton.Text = "Разрешить пинг всем";
            allowAllPingButton.UseVisualStyleBackColor = true;
            allowAllPingButton.Click += allowAllPingButton_Click;
            // 
            // denyAllPingButton
            // 
            denyAllPingButton.Location = new Point(12, 326);
            denyAllPingButton.Name = "denyAllPingButton";
            denyAllPingButton.Size = new Size(230, 30);
            denyAllPingButton.TabIndex = 7;
            denyAllPingButton.Text = "Запретить пинг всем";
            denyAllPingButton.UseVisualStyleBackColor = true;
            denyAllPingButton.Click += denyAllPingButton_Click;
            // 
            // allowAllVisibleButton
            // 
            allowAllVisibleButton.Location = new Point(262, 290);
            allowAllVisibleButton.Name = "allowAllVisibleButton";
            allowAllVisibleButton.Size = new Size(230, 30);
            allowAllVisibleButton.TabIndex = 8;
            allowAllVisibleButton.Text = "Показать всем пользователям";
            allowAllVisibleButton.UseVisualStyleBackColor = true;
            allowAllVisibleButton.Click += allowAllVisibleButton_Click;
            // 
            // denyAllVisibleButton
            // 
            denyAllVisibleButton.Location = new Point(262, 326);
            denyAllVisibleButton.Name = "denyAllVisibleButton";
            denyAllVisibleButton.Size = new Size(230, 30);
            denyAllVisibleButton.TabIndex = 9;
            denyAllVisibleButton.Text = "Скрыть от всех пользователей";
            denyAllVisibleButton.UseVisualStyleBackColor = true;
            denyAllVisibleButton.Click += denyAllVisibleButton_Click;
            // 
            // intervalNumeric
            // 
            intervalNumeric.Location = new Point(372, 252);
            intervalNumeric.Maximum = new decimal(new int[] { 3600, 0, 0, 0 });
            intervalNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            intervalNumeric.Name = "intervalNumeric";
            intervalNumeric.Size = new Size(120, 23);
            intervalNumeric.TabIndex = 3;
            intervalNumeric.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 254);
            label1.Name = "label1";
            label1.Size = new Size(160, 15);
            label1.TabIndex = 4;
            label1.Text = "Интервал мониторинга (сек):";
            // 
            // clearStatsButton
            // 
            clearStatsButton.Location = new Point(12, 380);
            clearStatsButton.Name = "clearStatsButton";
            clearStatsButton.Size = new Size(280, 36);
            clearStatsButton.TabIndex = 6;
            clearStatsButton.Text = "Очистить всю историю статусов";
            clearStatsButton.UseVisualStyleBackColor = true;
            clearStatsButton.Click += clearStatsButton_Click;
            // 
            // AdminForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(504, 431);
            Controls.Add(denyAllVisibleButton);
            Controls.Add(allowAllVisibleButton);
            Controls.Add(denyAllPingButton);
            Controls.Add(clearStatsButton);
            Controls.Add(label1);
            Controls.Add(intervalNumeric);
            Controls.Add(allowAllPingButton);
            Controls.Add(saveButton);
            Controls.Add(dataGridView);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AdminForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Панель администратора";
            Load += AdminForm_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView).EndInit();
            ((System.ComponentModel.ISupportInitialize)intervalNumeric).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private DataGridView dataGridView;
        private DataGridViewTextBoxColumn AddressColumn;
        private DataGridViewCheckBoxColumn AllowedColumn;
        private DataGridViewCheckBoxColumn VisibleColumn;
        private Button saveButton;
        private Button allowAllPingButton;
        private Button denyAllPingButton;
        private Button allowAllVisibleButton;
        private Button denyAllVisibleButton;
        private NumericUpDown intervalNumeric;
        private Label label1;
        private Button clearStatsButton;
    }
}