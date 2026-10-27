using System.Security.Cryptography;
using AcmHouse.Application.Abstractions;

namespace AcmHouse.Infrastructure.Security;

/// <summary>
/// PBKDF2 (HMAC-SHA256) with a random salt. Stored format: PBKDF2$iterations$saltBase64$hashBase64.
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Prefix = "PBKDF2";
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);

        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        if (password is null || string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        string[] parts = passwordHash.Split('$');
        if (parts.Length != 4 || parts[0] != Prefix || !int.TryParse(parts[1], out int iterations) || iterations <= 0)
        {
            return false;
        }

        try
        {
            byte[] salt = Convert.FromBase64String(parts[2]);
            byte[] expected = Convert.FromBase64String(parts[3]);
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
