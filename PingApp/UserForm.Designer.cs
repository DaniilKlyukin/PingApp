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
            addressTextBox = new TextBox();
            label1 = new Label();
            label2 = new Label();
            nicknameTextBox = new TextBox();
            SuspendLayout();
            // 
            // okButton
            // 
            okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            okButton.Location = new Point(64, 74);
            okButton.Name = "okButton";
            okButton.Size = new Size(169, 36);
            okButton.TabIndex = 1;
            okButton.Text = "Подвтердить";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += okButton_Click;
            // 
            // addressTextBox
            // 
            addressTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            addressTextBox.Location = new Point(61, 12);
            addressTextBox.Name = "addressTextBox";
            addressTextBox.Size = new Size(222, 23);
            addressTextBox.TabIndex = 2;
            addressTextBox.PreviewKeyDown += nameOrAddressTextBox_PreviewKeyDown;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 15);
            label1.Name = "label1";
            label1.Size = new Size(43, 15);
            label1.TabIndex = 3;
            label1.Text = "Адрес:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(23, 44);
            label2.Name = "label2";
            label2.Size = new Size(32, 15);
            label2.TabIndex = 5;
            label2.Text = "Имя:";
            // 
            // nicknameTextBox
            // 
            nicknameTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            nicknameTextBox.Location = new Point(61, 41);
            nicknameTextBox.Name = "nicknameTextBox";
            nicknameTextBox.Size = new Size(222, 23);
            nicknameTextBox.TabIndex = 4;
            // 
            // UserForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(295, 122);
            Controls.Add(label2);
            Controls.Add(nicknameTextBox);
            Controls.Add(label1);
            Controls.Add(addressTextBox);
            Controls.Add(okButton);
            Name = "UserForm";
            Text = "Пользователь";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button okButton;
        private TextBox addressTextBox;
        private Label label1;
        private Label label2;
        private TextBox nicknameTextBox;
    }
}