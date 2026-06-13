using PingApp.Domain.Aggregates.UserAggregate.ValueObjects;
using PingApp.Domain.Common;

namespace PingApp.Domain.Aggregates.UserAggregate.Common;

public static class UserErrors
{
    public static readonly Error InvalidUsernameEmpty = new ValidationError(
        "User.UsernameEmpty", "Имя пользователя не может быть пустым.");

    public static readonly Error UsernameTooShort = new ValidationError(
        "User.UsernameTooShort", $"Имя пользователя должно содержать не менее {Username.MinLength} символов.");

    public static readonly Error UsernameTooLong = new ValidationError(
        "User.UsernameTooLong", $"Имя пользователя не должно превышать {Username.MaxLength} символов.");

    public static readonly Error InvalidCredentials = new ValidationError(
        "User.InvalidCredentials", "Неверный логин или пароль.");

    public static readonly Error ReservedName = new DomainError(
        "User.ReservedName", "Зарезервированное имя администратора невозможно зарегистрировать.");

    public static readonly Error DuplicateUsername = new DomainError(
        "User.DuplicateUsername", "Пользователь с таким именем уже зарегистрирован.");

    public static readonly Error EmptyPassword = new ValidationError(
        "User.EmptyPassword", "Пароль не может быть пустым.");

    public static readonly Error PasswordTooShort = new ValidationError(
        "User.PasswordTooShort", $"Пароль должен содержать не менее {Password.MinLength} символов.");

    public static readonly Error PasswordTooLong = new ValidationError(
        "User.PasswordTooLong", $"Пароль не должен превышать {Password.MaxLength} символов.");
}
