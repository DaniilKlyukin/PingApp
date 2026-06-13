using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Common;

namespace PingApp.Domain.Aggregates.UserAggregate.ValueObjects;

public class Password : ValueObject<string>
{
    public const int MinLength = 4;
    public const int MaxLength = 30;

    private Password(string value) : base(value) { }

    public static Result<Password> Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return UserErrors.EmptyPassword;

        if (password.Length < MinLength)
            return UserErrors.PasswordTooShort;

        if (password.Length > MaxLength)
            return UserErrors.PasswordTooLong;

        return new Password(password);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
