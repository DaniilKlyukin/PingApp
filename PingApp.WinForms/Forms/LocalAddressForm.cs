using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PingApp.WinForms
{
    public partial class LocalAddressForm : Form
    {
        public LocalAddressForm()
        {
            InitializeComponent();
        }

        private void LocalAddressForm_Load(object sender, EventArgs e)
        {
            ShowLocalAddresses();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            ShowLocalAddresses();
        }

        private void ShowLocalAddresses()
        {
            addressTextBox.Clear();

            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                addressTextBox.Text = "Сетевое подключение отсутствует.";
                return;
            }

            try
            {
                var hostName = Dns.GetHostName();
                var hostEntry = Dns.GetHostEntry(hostName);

                foreach (var ip in hostEntry.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        addressTextBox.AppendText($"{ip}{Environment.NewLine}");
                    }
                }
            }
            catch (Exception ex)
            {
                addressTextBox.Text = $"Не удалось получить адреса: {ex.Message}";
            }
        }
    }
}
