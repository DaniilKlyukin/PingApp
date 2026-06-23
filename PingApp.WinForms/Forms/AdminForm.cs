using MediatR;
using PingApp.Application.Features.Admin;
using PingApp.Application.Features.Scanning.Common;
using PingApp.Application.Features.Statistics;

namespace PingApp.WinForms;

public partial class AdminForm : Form
{
    private readonly IMediator _mediator;
    private readonly IScanConfiguration _scanConfig;

    public AdminForm(IMediator mediator, IScanConfiguration scanConfig)
    {
        _mediator = mediator;
        _scanConfig = scanConfig;
        InitializeComponent();

        dataGridView.CurrentCellDirtyStateChanged += DataGridView_CurrentCellDirtyStateChanged;
    }

    private void DataGridView_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
    {
        if (dataGridView.IsCurrentCellDirty)
        {
            dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }
    }

    private async void AdminForm_Load(object sender, EventArgs e)
    {
        await LoadAdminDataAsync();
    }

    private async Task LoadAdminDataAsync()
    {
        dataGridView.Rows.Clear();
        var data = await _mediator.Send(new GetAdminData.Query());

        intervalNumeric.Value = data.ScanIntervalSeconds;

        foreach (var device in data.Devices)
        {
            dataGridView.Rows.Add(device.Address, device.IsAllowedToPing, device.IsVisibleToUsers);
        }
    }

    private async void saveButton_Click(object sender, EventArgs e)
    {
        dataGridView.EndEdit();

        var toggles = new List<UpdateAdminSettings.DeviceToggleDto>();

        foreach (DataGridViewRow row in dataGridView.Rows)
        {
            var address = row.Cells["AddressColumn"].Value?.ToString();
            if (address == null) continue;

            var isAllowedToPing = Convert.ToBoolean(row.Cells["AllowedColumn"].Value);
            var isVisibleToUsers = Convert.ToBoolean(row.Cells["VisibleColumn"].Value);

            toggles.Add(new UpdateAdminSettings.DeviceToggleDto(address, isAllowedToPing, isVisibleToUsers));
        }

        var interval = (int)intervalNumeric.Value;

        await _mediator.Send(new UpdateAdminSettings.Command(toggles, interval));

        _scanConfig.Interval = TimeSpan.FromSeconds(interval);

        MessageBox.Show("Глобальные параметры успешно сохранены!", "Администрирование", MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }

    private async void allowAllPingButton_Click(object sender, EventArgs e)
    {
        await RunBulkActionAsync(UpdateAdminSettings.BulkActionType.AllowAllPing, "Пинг включен для всех устройств.");
    }

    private async void denyAllPingButton_Click(object sender, EventArgs e)
    {
        await RunBulkActionAsync(UpdateAdminSettings.BulkActionType.DenyAllPing, "Пинг отключен для всех устройств.");
    }

    private async void allowAllVisibleButton_Click(object sender, EventArgs e)
    {
        await RunBulkActionAsync(UpdateAdminSettings.BulkActionType.AllowAllVisible, "Все устройства открыты для отслеживания пользователями.");
    }

    private async void denyAllVisibleButton_Click(object sender, EventArgs e)
    {
        await RunBulkActionAsync(UpdateAdminSettings.BulkActionType.DenyAllVisible, "Все устройства скрыты от пользователей.");
    }

    private async Task RunBulkActionAsync(UpdateAdminSettings.BulkActionType actionType, string message)
    {
        var interval = (int)intervalNumeric.Value;
        await _mediator.Send(new UpdateAdminSettings.Command([], interval, actionType));
        _scanConfig.Interval = TimeSpan.FromSeconds(interval);

        MessageBox.Show(message, "Администрирование", MessageBoxButtons.OK, MessageBoxIcon.Information);
        await LoadAdminDataAsync();
    }

    private async void clearStatsButton_Click(object sender, EventArgs e)
    {
        var confirmResult = MessageBox.Show(
            "Вы действительно хотите очистить всю историю статусов СУБД для всех пользователей?",
            "Очистка БД",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirmResult == DialogResult.Yes)
        {
            await _mediator.Send(new ClearStatisticsData.Command());
            MessageBox.Show("Вся статистика успешно удалена.", "База данных", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}