namespace PingApp
{
    partial class UserForm
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
            okButton = new Button();
            nameOrAddressTextBox = new TextBox();
            label1 = new Label();
            SuspendLayout();
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            okButton.Location = new Point(64, 52);
            okButton.Name = "okButton";
            okButton.Size = new Size(169, 36);
            okButton.TabIndex = 1;
            okButton.Text = "Подвтердить";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += okButton_Click;
            // 
            // nameOrAddressTextBox
            // 
            nameOrAddressTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            nameOrAddressTextBox.Location = new Point(110, 12);
            nameOrAddressTextBox.Name = "nameOrAddressTextBox";
            nameOrAddressTextBox.Size = new Size(173, 23);
            nameOrAddressTextBox.TabIndex = 2;
            nameOrAddressTextBox.PreviewKeyDown += nameOrAddressTextBox_PreviewKeyDown;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(92, 15);
            label1.TabIndex = 3;
            label1.Text = "Имя или адрес:";
            // 
            // UserForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(295, 100);
            Controls.Add(label1);
            Controls.Add(nameOrAddressTextBox);
            Controls.Add(okButton);
            Name = "UserForm";
            Text = "Пользователь";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button okButton;
        private TextBox nameOrAddressTextBox;
        private Label label1;
    }
}