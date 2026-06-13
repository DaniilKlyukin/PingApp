using PingApp.Domain.Aggregates.UserAggregate.Common;
using PingApp.Domain.Common;

namespace PingApp.Domain.Aggregates.UserAggregate.ValueObjects;

public class Username : ValueObject<string>
{
    public const int MinLength = 3;
    public const int MaxLength = 50;

    private Username(string value) : base(value)
    {

    }

    public static Result<Username> Create(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return UserErrors.InvalidUsernameEmpty;

        var trimmed = username.Trim();

        if (trimmed.Length < MinLength)
            return UserErrors.UsernameTooShort;

        if (trimmed.Length > MaxLength)
            return UserErrors.UsernameTooLong;

        return Result.Success(new Username(trimmed));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }
}
