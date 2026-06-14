using MediatR;
using PingApp.Application.Features.Users;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;

namespace PingApp.WinForms;

public partial class LoginForm : Form
{
    private readonly IMediator _mediator;
    private readonly IUserContext _userContext;

    public LoginForm(IMediator mediator, IUserContext userContext)
    {
        _mediator = mediator;
        _userContext = userContext;

        InitializeComponent();
    }

    private async void loginButton_Click(object sender, EventArgs e)
    {
        var result = await _mediator.Send(new Login.Command(usernameTextBox.Text, passwordTextBox.Text));

        if (result.IsFailure)
        {
            MessageBox.Show(result.Error.Message, "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var user = result.Value;

        _userContext.UserId = new UserId(user.UserId);
        _userContext.Username = Username.Create(user.Username).Value;
        _userContext.IsAdmin = user.IsAdmin;
        _userContext.IsGuest = user.IsGuest;

        DialogResult = DialogResult.OK;
        Close();
    }

    private async void registerButton_Click(object sender, EventArgs e)
    {
        var result = await _mediator.Send(new Register.Command(usernameTextBox.Text.Trim(), passwordTextBox.Text));

        if (result.IsFailure)
        {
            MessageBox.Show(result.Error.Message, "Регистрация отклонена", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        MessageBox.Show("Регистрация завершена успешно! Теперь вы можете войти.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void guestButton_Click(object sender, EventArgs e)
    {
        var result = await _mediator.Send(new LoginGuest.Command());

        if (result.IsFailure)
        {
            MessageBox.Show($"Не удалось войти как гость: {result.Error.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var guest = result.Value;
        _userContext.UserId = new UserId(guest.UserId);
        _userContext.Username = Username.Create(guest.Username).Value;
        _userContext.IsGuest = guest.IsGuest;

        DialogResult = DialogResult.OK;
        Close();
    }
}