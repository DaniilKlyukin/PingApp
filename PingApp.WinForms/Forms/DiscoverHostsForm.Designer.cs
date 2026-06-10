using System.Xml.Linq;

namespace PingApp.WinForms
{
    partial class DiscoverHostsForm
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
            listBox = new ListBox();
            scanButton = new Button();
            selectButton = new Button();
            progressBar = new ProgressBar();
            statusLabel = new Label();
            SuspendLayout();
            // 
            // listBox
            // 
            listBox.FormattingEnabled = true;
            listBox.ItemHeight = 15;
            listBox.Location = new Point(12, 12);
            listBox.Name = "listBox";
            listBox.Size = new Size(295, 259);
            listBox.TabIndex = 0;
            // 
            // scanButton
            // 
            scanButton.Location = new Point(12, 301);
            scanButton.Name = "scanButton";
            scanButton.Size = new Size(140, 36);
            scanButton.TabIndex = 1;
            scanButton.Text = "Сканировать сеть";
            scanButton.UseVisualStyleBackColor = true;
            scanButton.Click += scanButton_Click;
            // 
            // selectButton
            // 
            selectButton.Enabled = false;
            selectButton.Location = new Point(167, 301);
            selectButton.Name = "selectButton";
            selectButton.Size = new Size(140, 36);
            selectButton.TabIndex = 2;
            selectButton.Text = "Выбрать";
            selectButton.UseVisualStyleBackColor = true;
            selectButton.Click += selectButton_Click;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(12, 277);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(295, 15);
            progressBar.TabIndex = 3;
            // 
            // statusLabel
            // 
            statusLabel.AutoSize = true;
            statusLabel.Location = new Point(12, 345);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(187, 15);
            statusLabel.TabIndex = 4;
            statusLabel.Text = "Готов к сканированию подсети.";
            // 
            // DiscoverHostsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(319, 372);
            Controls.Add(statusLabel);
            Controls.Add(progressBar);
            Controls.Add(selectButton);
            Controls.Add(scanButton);
            Controls.Add(listBox);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "DiscoverHostsForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Поиск устройств в сети";
            ResumeLayout(false);
            PerformLayout();
        }

        private ListBox listBox;
        private Button scanButton;
        private Button selectButton;
        private ProgressBar progressBar;
        private Label statusLabel;
    }
}