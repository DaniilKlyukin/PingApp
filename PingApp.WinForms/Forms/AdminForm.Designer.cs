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
            saveButton = new Button();
            allowAllButton = new Button();
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
            dataGridView.Columns.AddRange(new DataGridViewColumn[] { AddressColumn, AllowedColumn });
            dataGridView.Location = new Point(12, 12);
            dataGridView.Name = "dataGridView";
            dataGridView.RowTemplate.Height = 25;
            dataGridView.Size = new Size(330, 230);
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
            AllowedColumn.HeaderText = "Разрешить";
            AllowedColumn.Name = "AllowedColumn";
            AllowedColumn.Width = 80;
            // 
            // saveButton
            // 
            saveButton.Location = new Point(182, 310);
            saveButton.Name = "saveButton";
            saveButton.Size = new Size(160, 36);
            saveButton.TabIndex = 1;
            saveButton.Text = "Сохранить";
            saveButton.UseVisualStyleBackColor = true;
            saveButton.Click += saveButton_Click;
            // 
            // allowAllButton
            // 
            allowAllButton.Location = new Point(12, 310);
            allowAllButton.Name = "allowAllButton";
            allowAllButton.Size = new Size(160, 36);
            allowAllButton.TabIndex = 2;
            allowAllButton.Text = "Пинговать все";
            allowAllButton.UseVisualStyleBackColor = true;
            allowAllButton.Click += allowAllButton_Click;
            // 
            // intervalNumeric
            // 
            intervalNumeric.Location = new Point(222, 252);
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
            clearStatsButton.Location = new Point(12, 355);
            clearStatsButton.Name = "clearStatsButton";
            clearStatsButton.Size = new Size(330, 36);
            clearStatsButton.TabIndex = 6;
            clearStatsButton.Text = "Очистить всю историю статусов";
            clearStatsButton.UseVisualStyleBackColor = true;
            clearStatsButton.Click += clearStatsButton_Click;
            // 
            // AdminForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(354, 405);
            Controls.Add(clearStatsButton);
            Controls.Add(label1);
            Controls.Add(intervalNumeric);
            Controls.Add(allowAllButton);
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
        private Button saveButton;
        private Button allowAllButton;
        private NumericUpDown intervalNumeric;
        private Label label1;
        private Button clearStatsButton;
    }
}