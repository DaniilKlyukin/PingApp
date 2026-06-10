using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PingApp.Application.Features.Devices;
using PingApp.Application.Features.Scanning;
using PingApp.Application.Features.Scanning.Common;
using PingApp.Application.Features.Statistics;
using PingApp.WinForms.Models;

namespace PingApp.WinForms;

public partial class MainForm : Form
{
    private readonly IMediator _mediator;
    private readonly IScanConfiguration _scanConfig;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainForm> _logger;
    private readonly BindingSource _bindingSource = new();

    public MainForm(
        IMediator mediator,
        IScanConfiguration scanConfig,
        IUiEventBridge uiEventBridge,
        IServiceProvider serviceProvider,
        ILogger<MainForm> logger)
    {
        InitializeComponent();
        _mediator = mediator;
        _scanConfig = scanConfig;
        _serviceProvider = serviceProvider;
        _logger = logger;

        uiEventBridge.DeviceStatusChanged += OnDeviceStatusChanged;
    }

    private async void MainForm_Load(object sender, EventArgs e)
    {
        await RefreshDataGridAsync();
        startButton.Enabled = true;
    }

    private async Task RefreshDataGridAsync()
    {
        try
        {
            var devicesList = await _mediator.Send(new GetDevicesList.Query());

            var gridItems = devicesList.Select(d => new DataGridUserItem
            {
                Address = d.Address,
                NickName = d.Nickname,
                AtWork = d.AtWork,
                StatusString = d.StatusString
            }).ToList();

            _bindingSource.DataSource = gridItems;
            dataGridView.DataSource = _bindingSource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении таблицы устройств.");
        }
    }

    private async void startButton_Click(object sender, EventArgs e)
    {
        startButton.Enabled = false;
        stopButton.Enabled = true;

        _scanConfig.Interval = TimeSpan.FromSeconds((double)checkPeriodNumeric.Value);
        _scanConfig.SaveToDatabase = saveCheckBox.Checked;
        _scanConfig.IsEnabled = true;

        timer.Interval = (int)(checkPeriodNumeric.Value * 1000);
        timer.Start();

        await _mediator.Send(new ScanAllDevices.Command());
        await RefreshDataGridAsync();
    }

    private void stopButton_Click(object sender, EventArgs e)
    {
        _scanConfig.IsEnabled = false;
        timer.Stop();
        startButton.Enabled = true;
        stopButton.Enabled = false;
    }

    private async void timer_Tick(object sender, EventArgs e)
    {
        await RefreshDataGridAsync();
    }

    private void checkPeriodNumeric_ValueChanged(object sender, EventArgs e)
    {
        _scanConfig.Interval = TimeSpan.FromSeconds((double)checkPeriodNumeric.Value);
        if (timer.Enabled)
        {
            timer.Interval = (int)(checkPeriodNumeric.Value * 1000);
        }
    }

    private void OnDeviceStatusChanged(DeviceStatusChanged.Notification notification)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => OnDeviceStatusChanged(notification)));
            return;
        }

        var deviceLabel = notification.Nickname ?? notification.Address;
        if (notification.AtWork)
        {
            notifyIcon.ShowBalloonTip(
                3000,
                "Устройство вошло в сеть",
                $"{deviceLabel} теперь доступен ({notification.DateTime:HH:mm:ss})",
                ToolTipIcon.Info);
        }
        else
        {
            notifyIcon.ShowBalloonTip(
                3000,
                "Устройство отключилось",
                $"{deviceLabel} покинул сеть ({notification.DateTime:HH:mm:ss})",
                ToolTipIcon.Warning);
        }
    }

    private async void addUserButton_Click(object sender, EventArgs e)
    {
        using var userForm = _serviceProvider.GetRequiredService<UserForm>();
        if (userForm.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(userForm.Address))
        {
            await _mediator.Send(new AddDevice.Command(userForm.Address, userForm.Nickname));
            await RefreshDataGridAsync();
        }
    }

    private async void deleteUserButton_Click(object sender, EventArgs e)
    {
        await DeleteSelectedDeviceAsync();
    }

    private async Task DeleteSelectedDeviceAsync()
    {
        if (dataGridView.CurrentRow?.DataBoundItem is DataGridUserItem selectedItem)
        {
            var confirmResult = MessageBox.Show(
                $"Вы действительно хотите удалить {selectedItem.Address}?",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                // Удаляем устройство через команду MediatR
                await _mediator.Send(new RemoveDevice.Command(selectedItem.Address));
                await RefreshDataGridAsync();
            }
        }
    }

    private async void dataGridView_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
    {
        if (e.KeyCode == Keys.Delete)
        {
            await DeleteSelectedDeviceAsync();
        }
    }

    private async void clearButton_Click(object sender, EventArgs e)
    {
        var confirmResult = MessageBox.Show(
            "Вы уверены, что хотите очистить всю историю статусов для всех устройств?",
            "Очистка статистики",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirmResult == DialogResult.Yes)
        {
            await _mediator.Send(new ClearStatisticsData.Command());
            await RefreshDataGridAsync();
        }
    }

    private void showLocalAddressButton_Click(object sender, EventArgs e)
    {
        var localAddressForm = _serviceProvider.GetRequiredService<LocalAddressForm>();
        localAddressForm.Show();
    }

    private async void showPlotButton_Click(object sender, EventArgs e)
    {
        var statistics = await _mediator.Send(new GetStatisticsList.Query());

        var statForm = new StatisticsForm(statistics);
        statForm.Show();
    }

    private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        if (WindowState == FormWindowState.Minimized)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }
        else
        {
            Hide();
            WindowState = FormWindowState.Minimized;
        }
    }
}