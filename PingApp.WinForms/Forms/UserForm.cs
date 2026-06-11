using System.ComponentModel;
using FluentValidation;
using MediatR;
using PingApp.Application.Features.Devices;

namespace PingApp.WinForms;

public partial class UserForm : Form
{
    private readonly IValidator<AddDevice.Command> _validator;
    private readonly IMediator _mediator;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? Address { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? Nickname { get; set; }

    public UserForm(IValidator<AddDevice.Command> validator, IMediator mediator)
    {
        _validator = validator;
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
                MessageBox.Show("В базе нет разрешенных устройств для мониторинга. Обратитесь к администратору.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        var selectedAddress = addressComboBox.SelectedItem?.ToString();

        var command = new AddDevice.Command(
            selectedAddress ?? string.Empty,
            string.IsNullOrWhiteSpace(nicknameTextBox.Text) ? null : nicknameTextBox.Text.Trim()
        );

        var validationResult = _validator.Validate(command);

        if (!validationResult.IsValid)
        {
            var errors = string.Join(Environment.NewLine, validationResult.Errors.Select(x => x.ErrorMessage));
            MessageBox.Show(errors, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Address = command.Address;
        Nickname = command.Nickname;

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