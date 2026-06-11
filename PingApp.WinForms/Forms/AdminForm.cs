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
            dataGridView.Rows.Add(device.Address, device.IsAllowed);
        }
    }

    private async void saveButton_Click(object sender, EventArgs e)
    {
        var toggles = new List<UpdateAdminSettings.DeviceToggleDto>();

        foreach (DataGridViewRow row in dataGridView.Rows)
        {
            var address = row.Cells["AddressColumn"].Value?.ToString();
            if (address == null) continue;

            var isAllowed = Convert.ToBoolean(row.Cells["AllowedColumn"].Value);
            toggles.Add(new UpdateAdminSettings.DeviceToggleDto(address, isAllowed));
        }

        var interval = (int)intervalNumeric.Value;

        await _mediator.Send(new UpdateAdminSettings.Command(toggles, interval, AllowAll: false));

        _scanConfig.Interval = TimeSpan.FromSeconds(interval);

        MessageBox.Show("Глобальные параметры применены!", "Администрирование", MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }

    private async void allowAllButton_Click(object sender, EventArgs e)
    {
        var interval = (int)intervalNumeric.Value;

        await _mediator.Send(new UpdateAdminSettings.Command([], interval, AllowAll: true));
        _scanConfig.Interval = TimeSpan.FromSeconds(interval);

        MessageBox.Show("Доступ разрешен ко всем устройствам!", "Администрирование", MessageBoxButtons.OK, MessageBoxIcon.Information);
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