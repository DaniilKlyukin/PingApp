namespace PingApp
{
    partial class LocalAddressForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            addressTextBox = new TextBox();
            refreshButton = new Button();
            SuspendLayout();
            // 
            // addressTextBox
            // 
            addressTextBox.Location = new Point(0, 0);
            addressTextBox.Multiline = true;
            addressTextBox.Name = "addressTextBox";
            addressTextBox.ReadOnly = true;
            addressTextBox.Size = new Size(800, 395);
            addressTextBox.TabIndex = 0;
            // 
            // refreshButton
            // 
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