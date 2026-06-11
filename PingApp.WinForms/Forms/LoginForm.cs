using Microsoft.Extensions.Configuration;
using PingApp.Application.Features.Security;
using PingApp.Application.Interfaces;
using PingApp.Domain.Entities;

namespace PingApp.WinForms;

public partial class LoginForm : Form
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IConfiguration _configuration;

    public LoginForm(IUserRepository userRepository, IUserContext userContext, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _userContext = userContext;
        _configuration = configuration;
        InitializeComponent();
    }

    private async void loginButton_Click(object sender, EventArgs e)
    {
        var username = usernameTextBox.Text.Trim();
        var password = passwordTextBox.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Введите логин и пароль.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var adminUser = _configuration["AdminSettings:Username"] ?? "admin";
        var adminPass = _configuration["AdminSettings:Password"] ?? "admin";

        if (username.Equals(adminUser, StringComparison.OrdinalIgnoreCase))
        {
            if (password != adminPass)
            {
                MessageBox.Show("Неверный логин или пароль администратора.", "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var dbAdmin = await _userRepository.GetUserByUsernameAsync(username);
            if (dbAdmin == null)
            {
                dbAdmin = new User
                {
                    Username = username,
                    PasswordHash = null,
                    IsGuest = false,
                    IsAdmin = true
                };
                await _userRepository.AddUserAsync(dbAdmin);
            }
            else if (!dbAdmin.IsAdmin)
            {
                dbAdmin.IsAdmin = true;
                await _userRepository.UpdateUserAsync(dbAdmin);
            }

            _userContext.UserId = dbAdmin.Id;
            _userContext.Username = dbAdmin.Username;
            _userContext.IsAdmin = true;
            _userContext.IsGuest = false;

            DialogResult = DialogResult.OK;
            Close();
            return;
        }

        var user = await _userRepository.GetUserByUsernameAsync(username);
        if (user == null || user.IsGuest || string.IsNullOrEmpty(user.PasswordHash) || !PasswordHasher.VerifyPassword(password, user.PasswordHash))
        {
            MessageBox.Show("Неверный логин или пароль.", "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _userContext.UserId = user.Id;
        _userContext.Username = user.Username;
        _userContext.IsAdmin = user.IsAdmin;
        _userContext.IsGuest = false;

        DialogResult = DialogResult.OK;
        Close();
    }

    private async void registerButton_Click(object sender, EventArgs e)
    {
        var username = usernameTextBox.Text.Trim();
        var password = passwordTextBox.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || password.Length < 4)
        {
            MessageBox.Show("Логин и пароль обязательны. Пароль должен быть не менее 4 символов.", "Регистрация", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var adminUser = _configuration["AdminSettings:Username"] ?? "admin";
        if (username.Equals(adminUser, StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show("Невозможно зарегистрировать учетную запись с зарезервированным именем администратора.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var existingUser = await _userRepository.GetUserByUsernameAsync(username);
        if (existingUser != null)
        {
            MessageBox.Show("Пользователь с таким именем уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var newUser = new User
        {
            Username = username,
            PasswordHash = PasswordHasher.HashPassword(password),
            IsGuest = false
        };

        await _userRepository.AddUserAsync(newUser);
        MessageBox.Show("Регистрация завершена успешно! Теперь вы можете войти.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void guestButton_Click(object sender, EventArgs e)
    {
        var guestUsername = $"Guest_{Guid.NewGuid().ToString()[..8]}";

        var guestUser = new User
        {
            Username = guestUsername,
            PasswordHash = null,
            IsGuest = true
        };

        await _userRepository.AddUserAsync(guestUser);

        _userContext.UserId = guestUser.Id;
        _userContext.Username = guestUser.Username;
        _userContext.IsGuest = true;

        DialogResult = DialogResult.OK;
        Close();
    }
}