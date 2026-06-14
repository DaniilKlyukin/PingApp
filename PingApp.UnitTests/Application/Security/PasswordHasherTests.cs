using FluentAssertions;
using PingApp.Application.Features.Security;

namespace PingApp.UnitTests.Application.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut;

    public PasswordHasherTests()
    {
        _sut = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_ShouldGenerateValidHash_ThatCanBeVerified()
    {
        var password = "SuperSecurePassword123!";

        var hash = _sut.HashPassword(password);
        var isVerified = _sut.VerifyPassword(password, hash);

        hash.Should().NotBeNullOrWhiteSpace();

        var parts = hash.Split(':');
        parts.Should().HaveCount(3);
        parts[0].Should().Be("600000"); 

        isVerified.Should().BeTrue("Original password must match its generated hash.");
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordIsIncorrect()
    {
        var correctPassword = "MySecretPassword";
        var wrongPassword = "WrongPassword";

        var hash = _sut.HashPassword(correctPassword);

        var isVerified = _sut.VerifyPassword(wrongPassword, hash);

        isVerified.Should().BeFalse("Incorrect password must fail verification.");
    }

    [Fact]
    public void HashPassword_ShouldGenerateDifferentHashes_ForSamePassword()
    {
        var password = "SamePassword123";

        var hash1 = _sut.HashPassword(password);
        var hash2 = _sut.HashPassword(password);

        hash1.Should().NotBe(hash2, "Each hashing process must generate a unique, cryptographically random salt.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-format-without-colons")]
    [InlineData("600000:only-two-parts")]
    [InlineData("not-an-integer-iterations:salt:hash")]
    public void VerifyPassword_ShouldReturnFalse_WhenHashedPasswordIsMalformed(string malformedHash)
    {
        var password = "AnyPassword";

        var result = _sut.VerifyPassword(password, malformedHash);

        result.Should().BeFalse("Malformed structure must be gracefully rejected with false.");
    }

    [Theory]
    [InlineData("600000:invalid-base64-salt!@#:hash")]
    [InlineData("600000:c2FsdA==:invalid-base64-hash!@#")]
    public void VerifyPassword_ShouldThrowFormatException_WhenBase64IsInvalid(string invalidBase64Hash)
    {
        var password = "AnyPassword";

        Action act = () => _sut.VerifyPassword(password, invalidBase64Hash);

        act.Should().Throw<FormatException>();
    }
}
