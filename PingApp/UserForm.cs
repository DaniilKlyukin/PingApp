namespace PingApp
{
    public partial class UserForm : Form
    {
        public string? Address { get; set; }
        public string? Nickname { get; set; }

        public UserForm()
        {
            InitializeComponent();

            ActiveControl = addressTextBox;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            CloseOk();
        }

        private void CloseOk()
        {
            Address = addressTextBox.Text;
            Nickname = nicknameTextBox.Text;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void nameOrAddressTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CloseOk();
            }
        }
    }
}
