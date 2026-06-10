using MediatR;
using PingApp.Application.Features.Scanning;

namespace PingApp.WinForms;

public partial class DiscoverHostsForm : Form
{
    private readonly IMediator _mediator;
    public string? SelectedIp { get; private set; }

    public DiscoverHostsForm(IMediator mediator)
    {
        _mediator = mediator;
        InitializeComponent();
    }

    private async void scanButton_Click(object sender, EventArgs e)
    {
        scanButton.Enabled = false;
        selectButton.Enabled = false;
        listBox.Items.Clear();
        statusLabel.Text = "Поиск активных устройств...";
        progressBar.Style = ProgressBarStyle.Marquee;

        try
        {
            var activeHosts = await _mediator.Send(new DiscoverActiveHosts.Query());

            if (activeHosts.Count == 0)
            {
                statusLabel.Text = "Активные устройства не найдены.";
            }
            else
            {
                foreach (var host in activeHosts)
                {
                    listBox.Items.Add(host);
                }
                statusLabel.Text = $"Найдено хостов: {activeHosts.Count}";
                selectButton.Enabled = true;
            }
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Произошла ошибка при сканировании.";
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            scanButton.Enabled = true;
            progressBar.Style = ProgressBarStyle.Blocks;
        }
    }

    private void selectButton_Click(object sender, EventArgs e)
    {
        if (listBox.SelectedItem is string ip)
        {
            SelectedIp = ip;
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            MessageBox.Show("Пожалуйста, выберите IP-адрес из списка.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}