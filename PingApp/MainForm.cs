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
            notifyIcon.BalloonTipTitle = "� ����";

            var nick = pinger.GetNickname(status.Address);

            var name = nick == null ? status.Address : $"{status.Address} ({nick})";

            notifyIcon.BalloonTipText = $"{name} � ����!";
            notifyIcon.ShowBalloonTip(5000);
        }

        private void userLoggedOut(UserStatus status)
        {
            notifyIcon.BalloonTipTitle = "����� �� ����";
            var nick = pinger.GetNickname(status.Address);

            var name = nick == null ? status.Address : $"{status.Address} ({nick})";

            notifyIcon.BalloonTipText = $"{name} �� � ����!";
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
            var data = new List<DataGridUserItem>();

            foreach (var statistics in pinger)
            {
                data.Add(new DataGridUserItem
                {
                    Address = statistics.Address,
                    NickName = statistics.Nickname,
                    AtWork = statistics.Statuses.LastOrDefault()?.AtWork ?? false,
                    StatusString = getUserWorkStatusString(pinger, statistics.Address)
                });
            }

            dataGridView.DataSource = data;
        }
        private string convertTimeSpanToText(TimeSpan span)
        {
            var sb = new StringBuilder();

            if (span.Hours > 0)
                sb.Append($"{span.Hours} �. ");

            if (span.Minutes > 0)
                sb.Append($"{span.Minutes} ���. ");

            if (span.Seconds > 0)
                sb.Append($"{span.Seconds} ���.");

            return sb.ToString();
        }

        private string getUserWorkStatusString(Pinger pinger, string nameOrAddress)
        {
            var parsed = pinger.TryGetUserLastTimeAtWork(nameOrAddress, out var atWork, out _, out var dt);

            if (!parsed)
                return $"�� ������";

            if (atWork)
            {
                return $"�� �����";
            }
            else
            {
                return $"��� {convertTimeSpanToText(dt)} �����";
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
                    new UserStatistics
                    {
                        Address = x.Address,
                        Nickname = x.NickName,
                        Statuses = pinger.GetUserStatuses(x.Address)
                    })
                .ToList());

            statisticsForm.ShowDialog();
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("�� �������? ���� ����� ������.", "�������� ����������", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
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

            if (userForm.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(userForm.Address))
            {
                MessageBox.Show("������������ ��������");
                pinger.AddUser(userForm.Address);

                if (userForm.Nickname != null)
                    pinger.AddNickname(userForm.Address, userForm.Nickname);

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
                if (item.Address != null)
                    pinger.RemoveUser(item.Address);
            }

            UpdateDataGrid();
        }

        private IEnumerable<DataGridUserItem> getSelectedDatagridItems()
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                for (int index = 0; index < dataGridView.RowCount; index++)
                {
                    yield return (DataGridUserItem)dataGridView.Rows[index].DataBoundItem;
                }
            }
            else
            {
                for (int index = 0; index < dataGridView.SelectedRows.Count; index++)
                {
                    var selectedRow = dataGridView.SelectedRows[index];
                    yield return (DataGridUserItem)selectedRow.DataBoundItem;
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