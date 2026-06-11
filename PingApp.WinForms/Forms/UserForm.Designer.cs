namespace PingApp.WinForms;

partial class UserForm
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
        okButton = new Button();
        addressComboBox = new ComboBox();
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
        okButton.TabIndex = 2;
        okButton.Text = "Подтвердить";
        okButton.UseVisualStyleBackColor = true;
        okButton.Click += okButton_Click;
        // 
        // addressComboBox
        // 
        addressComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        addressComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        addressComboBox.Location = new Point(61, 12);
        addressComboBox.Name = "addressComboBox";
        addressComboBox.Size = new Size(222, 23);
        addressComboBox.TabIndex = 0;
        addressComboBox.PreviewKeyDown += nameOrAddressTextBox_PreviewKeyDown;
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
        nicknameTextBox.TabIndex = 1;
        nicknameTextBox.PreviewKeyDown += nameOrAddressTextBox_PreviewKeyDown;
        // 
        // UserForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(295, 122);
        Controls.Add(label2);
        Controls.Add(nicknameTextBox);
        Controls.Add(label1);
        Controls.Add(addressComboBox);
        Controls.Add(okButton);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "UserForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Пользователь";
        Load += UserForm_Load;
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Button okButton;
    private ComboBox addressComboBox;
    private Label label1;
    private Label label2;
    private TextBox nicknameTextBox;
}