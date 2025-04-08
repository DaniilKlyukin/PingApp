using System.Text;

namespace PingApp
{
    public partial class MainForm : Form
    {
        private readonly Pinger pinger = new Pinger();
        private readonly string fileName = "statistics.json";

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

            updateStartButton();
            updateStopButton();
        }

        private void updateStartButton()
        {
            startButton.Enabled = !timer.Enabled && pinger.UsersCount > 0;
        }

        private void updateStopButton()
        {
            stopButton.Enabled = timer.Enabled;
        }

        private void userLoggedIn(UserStatus status)
        {
            notifyIcon.BalloonTipTitle = "В сети";

            var nick = pinger.GetNickname(status.Address);

            var name = nick == null ? status.Address : $"{status.Address} ({nick})";

            notifyIcon.BalloonTipText = $"{name} в сети!";
            notifyIcon.ShowBalloonTip(5000);
        }

        private void userLoggedOut(UserStatus status)
        {
            notifyIcon.BalloonTipTitle = "Вышел из сети";
            var nick = pinger.GetNickname(status.Address);

            var name = nick == null ? status.Address : $"{status.Address} ({nick})";

            notifyIcon.BalloonTipText = $"{name} не в сети!";
            notifyIcon.ShowBalloonTip(5000);
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            timer.Start();
            updateStartButton();
            updateStopButton();
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            timer.Stop();
            updateStartButton();
            updateStopButton();
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
            if (MessageBox.Show("Вы уверены? Файл будет удален.",
                "Удаление статистики",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question) == DialogResult.OK)
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
                MessageBox.Show($"Пользователь {userForm.Nickname} добавлен",
                    "Новый пользователь", MessageBoxButtons.OK, MessageBoxIcon.Information);
                pinger.AddUser(userForm.Address);

                if (userForm.Nickname != null)
                    pinger.AddNickname(userForm.Address, userForm.Nickname);

                UpdateDataGrid();
                updateStartButton();
                updateStopButton();
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
            var toDelete = getSelectedDatagridItems()
                .Where(x => x.Address != null)
                .ToArray();

            if (toDelete.Length == 0)
                return;

            if (MessageBox.Show($"Вы уверены, что хотите удалить {toDelete.Length} пользователей",
                "Удаление пользователей", MessageBoxButtons.OKCancel, MessageBoxIcon.Question)
                == DialogResult.Cancel)
                return;

            foreach (var item in toDelete)
            {
                pinger.RemoveUser(item.Address);
            }

            UpdateDataGrid();
            updateStartButton();
            updateStopButton();
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