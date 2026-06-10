using System.ComponentModel;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PingApp.Application.Features.Devices;

namespace PingApp.WinForms;

public partial class UserForm : Form
{
    private readonly IValidator<AddDevice.Command> _validator;
    private readonly IServiceProvider _serviceProvider;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? Address { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? Nickname { get; set; }

    public UserForm(IValidator<AddDevice.Command> validator, IServiceProvider serviceProvider)
    {
        _validator = validator;
        _serviceProvider = serviceProvider;
        InitializeComponent();
        ActiveControl = addressTextBox;
    }

    private void okButton_Click(object sender, EventArgs e)
    {
        CloseOk();
    }

    private void CloseOk()
    {
        var command = new AddDevice.Command(
            addressTextBox.Text.Trim(),
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

    private void findIpButton_Click(object sender, EventArgs e)
    {
        using var discoverForm = _serviceProvider.GetRequiredService<DiscoverHostsForm>();
        if (discoverForm.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(discoverForm.SelectedIp))
        {
            addressTextBox.Text = discoverForm.SelectedIp;
        }
    }

    private void nameOrAddressTextBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            CloseOk();
        }
    }
}