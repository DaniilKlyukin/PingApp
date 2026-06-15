using FluentAssertions;
using PingApp.Domain.Common;

namespace PingApp.UnitTests.Domain.Common;

public class ResultTests
{
    private record TestError() : Error("Test.Code", "Test message");

    [Fact]
    public void Success_ShouldCreateValidSuccessResult()
    {
        var result = Result.Success();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
        result.ToString().Should().Be("Success");
    }

    [Fact]
    public void Failure_ShouldCreateValidFailureResult()
    {
        var error = new TestError();
        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.ToString().Should().Be($"Failure: {error}");
    }

    [Fact]
    public void GenericSuccess_ShouldContainValue()
    {
        var result = Result.Success(42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        result.ToString().Should().Be("Success: 42");
    }

    [Fact]
    public void GenericFailure_ShouldThrow_WhenAccessingValue()
    {
        var error = new TestError();
        var result = Result.Failure<int>(error);

        result.IsSuccess.Should().BeFalse();

        Action act = () => _ = result.Value;
        act.Should().Throw<InvalidOperationException>();
        result.ToString().Should().Be($"Failure: {error}");
    }

    [Fact]
    public void Success_ShouldThrow_IfErrorIsProvided()
    {
        Action act = () => new FakeResult(isSuccess: true, new TestError());
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Failure_ShouldThrow_IfNoneErrorIsProvided()
    {
        Action act = () => new FakeResult(isSuccess: false, Error.None);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailureResult()
    {
        var error = new TestError();
        Result result = error;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void GenericImplicitConversion_FromValue_ShouldCreateSuccessResult()
    {
        Result<string> result = "test-data";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test-data");
    }

    [Fact]
    public void GenericImplicitConversion_FromError_ShouldCreateFailureResult()
    {
        var error = new TestError();
        Result<string> result = error;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    private class FakeResult : Result
    {
        public FakeResult(bool isSuccess, Error error) : base(isSuccess, error) { }
    }
}
