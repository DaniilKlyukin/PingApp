namespace PingApp.Domain.Common;

public abstract record Error(string Code, string Message)
{
    public static readonly Error None = new NoneError();

    private record NoneError() : Error(string.Empty, string.Empty);
}

public record DomainError(string Code, string Message) : Error(Code, Message);

public record NotFoundError(string Code, string Message) : Error(Code, Message);

public record ValidationError : Error
{
    public IReadOnlyDictionary<string, string[]> Failures { get; }

    public ValidationError(string code, string message, IReadOnlyDictionary<string, string[]> failures)
        : base(code, message)
    {
        Failures = failures;
    }

    public ValidationError(string code, string message)
        : base(code, message)
    {
        Failures = new Dictionary<string, string[]>();
    }
}