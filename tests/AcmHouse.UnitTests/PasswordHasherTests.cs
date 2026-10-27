using AcmHouse.Infrastructure.Security;
using Xunit;

namespace AcmHouse.UnitTests;

public class PasswordHasherTests
{
    private readonly Pbkdf2PasswordHasher hasher = new();

    [Fact]
    public void Verify_AcceptsTheOriginalPasswordOnly()
    {
        string hash = hasher.Hash("Demo1234");

        Assert.True(hasher.Verify("Demo1234", hash));
        Assert.False(hasher.Verify("demo1234", hash));
    }

    [Fact]
    public void Hash_UsesRandomSaltAndDoesNotContainThePassword()
    {
        string first = hasher.Hash("Demo1234");

        Assert.NotEqual(first, hasher.Hash("Demo1234"));
        Assert.DoesNotContain("Demo1234", first);
    }

    [Fact]
    public void Verify_IsCompatibleWithTheDesktopAppHashFormat()
    {
        // A hash created by the AcmHouse desktop app (same PBKDF2 format) for the password Demo1234.
        const string desktopHash = "PBKDF2$100000$4g4wzrh642a/hOhUktOw0Q==$KYNXFNQXq/pPuFFlLQkGYTRokjlPPb13fO0GppEVfDo=";

        Assert.True(hasher.Verify("Demo1234", desktopHash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("plain-text")]
    [InlineData("PBKDF2$x$c2FsdA==$aGFzaA==")]
    [InlineData("PBKDF2$100000$not-base64$aGFzaA==")]
    public void Verify_ReturnsFalseForMalformedHashes(string hash)
    {
        Assert.False(hasher.Verify("Demo1234", hash));
    }
}
