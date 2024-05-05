using System.Net.Sockets;
using System.Net;

namespace PingApp
{
    public partial class LocalAddressForm : Form
    {
        public LocalAddressForm()
        {
            InitializeComponent();
        }

        private void LocalAddressForm_Load(object sender, EventArgs e)
        {
            showLocalAddress();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            showLocalAddress();
        }

        private void showLocalAddress()
        {
            addressTextBox.Clear();
            // доступно ли сетевое подключение
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                return;
            // запросить у DNS-сервера IP-адрес, связанный с именем узла
            var host = Dns.GetHostEntry(Dns.GetHostName());
            // Пройдем по списку IP-адресов, связанных с узлом
            foreach (var ip in host.AddressList)
            {
                // если текущий IP-адрес версии IPv4, то выведем его 
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    addressTextBox.Text += $"{ip}{Environment.NewLine}";
                }
            }
        }
    }
}
