namespace PingApp
{
    public partial class UserForm : Form
    {
        public string? NameOrAddress { get; set; }

        public UserForm()
        {
            InitializeComponent();

            ActiveControl = nameOrAddressTextBox;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            CloseOk();
        }

        private void CloseOk()
        {
            NameOrAddress = nameOrAddressTextBox.Text;

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
