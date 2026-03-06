using System.Security.Cryptography;
using System.Text;

namespace PaymentApi.Services;

public static class PasswordHelper
{
    public static string Hash(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public static bool Verify(string password, string hash)
    {
        var computed = Hash(password);
        return string.Equals(computed, hash, StringComparison.Ordinal);
    }
}
