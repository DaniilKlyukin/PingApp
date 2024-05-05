using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Text;

namespace PingApp
{
    public partial class MainForm : Form
    {
        Pinger pinger = new Pinger();
        string fileName = "statistics.json";

        public MainForm()
        {
            InitializeComponent();

            if (File.Exists(fileName))
            {
                var json = File.ReadAllText(fileName);
                pinger = PingerJson.Deserialize(json);
            }

            UpdateDataGrid();

            pinger.OnLoggedIn += userLoggedIn;
            pinger.OnLoggedOut += userLoggedOut;

            timer.Interval = (int)checkPeriodNumeric.Value * 1000;
        }

        private void userLoggedIn(UserStatus status)
        {
            notifyIcon.BalloonTipTitle = "В сети";
            notifyIcon.BalloonTipText = $"{status.NameOrAddress} в сети!";
            notifyIcon.ShowBalloonTip(5000);
        }

        private void userLoggedOut(UserStatus status)
        {
            notifyIcon.BalloonTipTitle = "Вышел из сети";
            notifyIcon.BalloonTipText = $"{status.NameOrAddress} не в сети!";
            notifyIcon.ShowBalloonTip(5000);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            timer.Start();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            timer.Stop();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            pinger.UpdateStatistics();

            UpdateDataGrid();

            if (saveCheckBox.Checked)
                SaveToFile();
        }

        private void UpdateDataGrid()
        {
            var data = new List<DatagridUserItem>();

            foreach (var (nameOrAddress, stack) in pinger)
            {
                data.Add(new DatagridUserItem
                {
                    NameOrAddress = nameOrAddress,
                    AtWork = stack.FirstOrDefault()?.AtWork ?? false,
                    StatusString = getUserWorkStatusString(pinger, nameOrAddress)
                });
            }

            dataGridView.DataSource = data;
        }
        private string convertTimeSpanToText(TimeSpan span)
        {
            var sb = new StringBuilder();

            if (span.Hours > 0)
                sb.Append($"{span.Hours} ч. ");

            if (span.Minutes > 0)
                sb.Append($"{span.Minutes} мин. ");

            if (span.Seconds > 0)
                sb.Append($"{span.Seconds} сек.");

            return sb.ToString();
        }

        private string getUserWorkStatusString(Pinger pinger, string nameOrAddress)
        {
            var parsed = pinger.TryGetUserLastTimeAtWork(nameOrAddress, out var atWork, out _, out var dt);

            if (!parsed)
                return $"не найден";

            if (atWork)
            {
                return $"на месте";
            }
            else
            {
                return $"был {convertTimeSpanToText(dt)} назад";
            }
        }

        private void SaveToFile()
        {
            var json = PingerJson.Serialize(pinger);

            File.WriteAllText(fileName, json);
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Visible)
                Hide();
            else
                Show();
        }

        private void showPlotButton_Click(object sender, EventArgs e)
        {
            var selectedAddresses = getSelectedDatagridItems().ToList();

            var statisticsForm = new StatisticsForm(
                selectedAddresses
                .Select(x =>
                    (x.NameOrAddress, pinger.GetUserStatuses(x.NameOrAddress)))
                .ToList());

            statisticsForm.ShowDialog();
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Вы уверены? Файл будет удален.", "Удаление статистики", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                File.Delete(fileName);
                pinger.ClearSatistics();
            }
        }

        private void checkPeriodNumeric_ValueChanged(object sender, EventArgs e)
        {
            timer.Interval = (int)checkPeriodNumeric.Value * 1000;
        }

        private void addUserButton_Click(object sender, EventArgs e)
        {
            var userForm = new UserForm();

            if (userForm.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(userForm.NameOrAddress))
            {
                MessageBox.Show("Пользователь добавлен");
                pinger.AddUser(userForm.NameOrAddress);
                UpdateDataGrid();
            }
        }

        private void deleteUserButton_Click(object sender, EventArgs e)
        {
            deleteUsers();
        }

        private void dataGridView_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                deleteUsers();
        }

        private void deleteUsers()
        {
            foreach (var item in getSelectedDatagridItems())
            {
                if (item.NameOrAddress != null)
                    pinger.RemoveUser(item.NameOrAddress);
            }

            UpdateDataGrid();
        }

        private IEnumerable<DatagridUserItem> getSelectedDatagridItems()
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                for (int index = 0; index < dataGridView.RowCount; index++)
                {
                    yield return (DatagridUserItem)dataGridView.Rows[index].DataBoundItem;
                }
            }
            else
            {
                for (int index = 0; index < dataGridView.SelectedRows.Count; index++)
                {
                    var selectedRow = dataGridView.SelectedRows[index];
                    yield return (DatagridUserItem)selectedRow.DataBoundItem;
                }
            }
        }

        private void getLocalAddress()
        {
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
                    Debug.WriteLine(ip.ToString());
                }
            }
        }

        private void showLocalAddressButton_Click(object sender, EventArgs e)
        {
            LocalAddressForm localAddressForm = new LocalAddressForm();

            localAddressForm.Show();
        }
    }
}