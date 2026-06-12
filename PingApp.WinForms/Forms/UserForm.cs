using System.ComponentModel;
using MediatR;
using PingApp.Application.Features.Devices;
using PingApp.Domain.ValueObjects;

namespace PingApp.WinForms;

public partial class UserForm : Form
{
    private readonly IMediator _mediator;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? Address { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? Nickname { get; set; }

    public UserForm(IMediator mediator)
    {
        _mediator = mediator;
        InitializeComponent();
    }

    private async void UserForm_Load(object sender, EventArgs e)
    {
        try
        {
            var allowedIpList = await _mediator.Send(new GetAllowedDevices.Query());

            if (allowedIpList.Count == 0)
            {
                MessageBox.Show("В базе нет разрешенных устройств для мониторинга.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Close();
                return;
            }

            addressComboBox.DataSource = allowedIpList;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при загрузке пула адресов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void okButton_Click(object sender, EventArgs e)
    {
        CloseOk();
    }

    private void CloseOk()
    {
        var selectedAddress = addressComboBox.SelectedItem?.ToString() ?? string.Empty;

        var addressValidationResult = DeviceAddress.Create(selectedAddress);
        if (addressValidationResult.IsFailure)
        {
            MessageBox.Show(addressValidationResult.Error, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Address = addressValidationResult.Value.Value;
        Nickname = string.IsNullOrWhiteSpace(nicknameTextBox.Text) ? null : nicknameTextBox.Text.Trim();

        DialogResult = DialogResult.OK;
        Close();
    }

    private void nameOrAddressTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            CloseOk();
        }
    }
}