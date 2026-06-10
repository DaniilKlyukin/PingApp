using FluentValidation;
using PingApp.Domain.Entities;
using System.ComponentModel;

namespace PingApp.WinForms;

public partial class UserForm : Form
{
    private readonly IValidator<Device> _validator;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? Address { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string? Nickname { get; set; }

    public UserForm(IValidator<Device> validator)
    {
        _validator = validator;
        InitializeComponent();
        ActiveControl = addressTextBox;
    }

    private void okButton_Click(object sender, EventArgs e)
    {
        CloseOk();
    }

    private void CloseOk()
    {
        var tempDevice = new Device
        {
            Address = addressTextBox.Text.Trim(),
            Nickname = string.IsNullOrWhiteSpace(nicknameTextBox.Text) ? null : nicknameTextBox.Text.Trim()
        };

        var validationResult = _validator.Validate(tempDevice);

        if (!validationResult.IsValid)
        {
            var errors = string.Join(Environment.NewLine, validationResult.Errors.Select(x => x.ErrorMessage));
            MessageBox.Show(errors, "Ошибка валидации", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Address = tempDevice.Address;
        Nickname = tempDevice.Nickname;

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