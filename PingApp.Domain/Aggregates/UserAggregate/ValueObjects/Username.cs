using PingApp.Domain.Common;

namespace PingApp.Domain.Aggregates.UserAggregate.ValueObjects;

public class Username : ValueObject<string>
{
    private Username(string value) : base(value)
    {

    }

    public static Result<Username> Create(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return Result.Failure<Username>("Имя пользователя не может быть пустым.");

        var trimmed = username.Trim();

        if (trimmed.Length < 3)
            return Result.Failure<Username>("Имя пользователя должно содержать не менее 3 символов.");

        if (trimmed.Length > 50)
            return Result.Failure<Username>("Имя пользователя не должно превышать 50 символов.");

        return Result.Success(new Username(trimmed));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }
}