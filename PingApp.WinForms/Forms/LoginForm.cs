using Microsoft.Extensions.Configuration;
using PingApp.Application.Features.Security;
using PingApp.Application.Interfaces;
using PingApp.Domain.Aggregates.UserAggregate;
using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using PingApp.Domain.Common;

namespace PingApp.WinForms;

public partial class LoginForm : Form
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;

    public LoginForm(
        IUserRepository userRepository,
        IUserContext userContext,
        IConfiguration configuration,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _userContext = userContext;
        _configuration = configuration;
        _passwordHasher = passwordHasher;

        InitializeComponent();
    }

    private async void loginButton_Click(object sender, EventArgs e)
    {
        var authResult = await AuthenticateUserAsync(usernameTextBox.Text, passwordTextBox.Text);

        if (authResult.IsFailure)
        {
            MessageBox.Show(authResult.Error, "Доступ запрещен", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var user = authResult.Value;
        _userContext.UserId = user.Id;
        _userContext.Username = user.Username;
        _userContext.IsAdmin = user.IsAdmin;
        _userContext.IsGuest = user.IsGuest;

        DialogResult = DialogResult.OK;
        Close();
    }

    private async Task<Result<User>> AuthenticateUserAsync(string rawUsername, string rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword))
        {
            return Result.Failure<User>("Введите логин и пароль.");
        }

        var usernameResult = Username.Create(rawUsername);
        if (usernameResult.IsFailure)
        {
            return Result.Failure<User>(usernameResult.Error);
        }

        var username = usernameResult.Value;

        var adminUser = _configuration["AdminSettings:Username"] ?? "admin";
        var adminPass = _configuration["AdminSettings:Password"] ?? "admin";

        if (username.Value.Equals(adminUser, StringComparison.OrdinalIgnoreCase))
        {
            if (rawPassword != adminPass)
            {
                return Result.Failure<User>("Неверный пароль администратора.");
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

            return dbAdmin;
        }

        var user = await _userRepository.GetUserByUsernameAsync(username);

        if (user == null ||
            user.IsGuest ||
            string.IsNullOrEmpty(user.PasswordHash) ||
            !_passwordHasher.VerifyPassword(rawPassword, user.PasswordHash))
        {
            return Result.Failure<User>("Неверный логин или пароль.");
        }

        return user;
    }

    private async void registerButton_Click(object sender, EventArgs e)
    {
        var username = usernameTextBox.Text.Trim();
        var password = passwordTextBox.Text;

        var registrationResult = await RegisterUserAsync(username, password);
        if (registrationResult.IsFailure)
        {
            MessageBox.Show(registrationResult.Error, "Регистрация отклонена", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        MessageBox.Show("Регистрация завершена успешно! Теперь вы можете войти.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task<Result> RegisterUserAsync(string rawUsername, string rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword) || rawPassword.Length < 4)
        {
            return Result.Failure("Пароль должен содержать не менее 4 символов.");
        }

        var usernameResult = Username.Create(rawUsername);
        if (usernameResult.IsFailure)
        {
            return Result.Failure(usernameResult.Error);
        }

        var username = usernameResult.Value;

        var adminUser = _configuration["AdminSettings:Username"] ?? "admin";
        if (username.Value.Equals(adminUser, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure("Зарезервированное имя администратора невозможно зарегистрировать.");
        }

        var existingUser = await _userRepository.GetUserByUsernameAsync(username);
        if (existingUser != null)
        {
            return Result.Failure("Пользователь с таким именем уже зарегистрирован.");
        }

        var newUser = new User
        {
            Username = username,
            PasswordHash = _passwordHasher.HashPassword(rawPassword),
            IsGuest = false
        };

        await _userRepository.AddUserAsync(newUser);
        return Result.Success();
    }

    private async void guestButton_Click(object sender, EventArgs e)
    {
        try
        {
            var rawGuestUsername = $"Guest_{Guid.NewGuid().ToString()[..8]}";

            var usernameResult = Username.Create(rawGuestUsername);
            if (usernameResult.IsFailure)
            {
                MessageBox.Show($"Ошибка генерации имени гостя: {usernameResult.Error}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var guestUsername = usernameResult.Value;

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
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось войти как гость: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}