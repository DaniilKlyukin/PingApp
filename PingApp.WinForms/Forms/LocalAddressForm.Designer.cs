namespace PingApp.WinForms
{
    partial class LocalAddressForm
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
            addressTextBox = new TextBox();
            refreshButton = new Button();
            SuspendLayout();
            // 
            // addressTextBox
            // 
            addressTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            addressTextBox.Location = new Point(12, 12);
            addressTextBox.Multiline = true;
            addressTextBox.Name = "addressTextBox";
            addressTextBox.ReadOnly = true;
            addressTextBox.ScrollBars = ScrollBars.Vertical;
            addressTextBox.Size = new Size(776, 384);
            addressTextBox.TabIndex = 0;
            // 
            // refreshButton
            // 
            refreshButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            refreshButton.Location = new Point(12, 402);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(776, 36);
            refreshButton.TabIndex = 1;
            refreshButton.Text = "Обновить список";
            refreshButton.UseVisualStyleBackColor = true;
            refreshButton.Click += refreshButton_Click;
            // 
            // LocalAddressForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(refreshButton);
            Controls.Add(addressTextBox);
            Name = "LocalAddressForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Локальные адреса";
            Load += LocalAddressForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox addressTextBox;
        private Button refreshButton;
    }
}