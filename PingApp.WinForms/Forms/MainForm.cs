using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PingApp.Application.Features.Devices;
using PingApp.Application.Features.Scanning;
using PingApp.Application.Features.Scanning.Common;
using PingApp.Application.Features.Statistics;
using PingApp.Application.Interfaces;
using PingApp.WinForms.Models;

namespace PingApp.WinForms;

public partial class MainForm : Form
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainForm> _logger;
    private readonly BindingSource _bindingSource = new();

    public bool LogoutRequested { get; private set; } = false;

    public MainForm(
        IMediator mediator,
        IUiEventBridge uiEventBridge,
        IServiceProvider serviceProvider,
        ILogger<MainForm> logger)
    {
        InitializeComponent();
        _mediator = mediator;
        _serviceProvider = serviceProvider;
        _logger = logger;

        uiEventBridge.DeviceStatusChanged += OnDeviceStatusChanged;
    }

    private async void MainForm_Load(object sender, EventArgs e)
    {
        await RefreshDataGridAsync();

        var userContext = _serviceProvider.GetRequiredService<IUserContext>();
        adminButton.Visible = userContext.IsAdmin;
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

            if (IsDisposed || Disposing)
            {
                return;
            }

            _bindingSource.DataSource = gridItems;
            dataGridView.DataSource = _bindingSource;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении таблицы устройств.");
        }
    }

    private async void timer_Tick(object sender, EventArgs e)
    {
        await RefreshDataGridAsync();
    }

    private void logoutButton_Click(object sender, EventArgs e)
    {
        LogoutRequested = true;
        this.Close();
    }

    private void OnDeviceStatusChanged(DeviceStatusChanged.Notification notification)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => OnDeviceStatusChanged(notification)));
            return;
        }

        string deviceLabel = notification.Address;
        if (_bindingSource.DataSource is List<DataGridUserItem> items)
        {
            var matchedItem = items.FirstOrDefault(i => i.Address == notification.Address);
            if (matchedItem != null && !string.IsNullOrEmpty(matchedItem.NickName))
            {
                deviceLabel = $"{matchedItem.NickName} ({notification.Address})";
            }
        }

        var localTime = notification.DateTime.ToLocalTime();

        if (notification.AtWork)
        {
            notifyIcon.ShowBalloonTip(
                3000,
                "Устройство вошло в сеть",
                $"{deviceLabel} теперь доступен ({localTime:HH:mm:ss})",
                ToolTipIcon.Info);
        }
        else
        {
            notifyIcon.ShowBalloonTip(
                3000,
                "Устройство отключилось",
                $"{deviceLabel} покинул сеть ({localTime:HH:mm:ss})",
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
                $"Вы действительно хотите прекратить слежение за {selectedItem.Address}?",
                "Подтверждение",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
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

    private void adminButton_Click(object sender, EventArgs e)
    {
        var adminForm = _serviceProvider.GetRequiredService<AdminForm>();
        adminForm.ShowDialog();
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

    private void showLocalAddressButton_Click(object sender, EventArgs e)
    {
        var localAddressForm = _serviceProvider.GetRequiredService<LocalAddressForm>();
        localAddressForm.Show();
    }
}