using System.Linq;

namespace Backend.Common;

public static class RegistrationPhoneHelper
{
    /// <summary>Digits only, for uniqueness and synthetic email.</summary>
    public static string NormalizeDigits(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;
        return new string(phone.Where(char.IsDigit).ToArray());
    }
}
