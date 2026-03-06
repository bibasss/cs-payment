using System.Text.RegularExpressions;
using PaymentApi.DTOs;

namespace PaymentApi.Services;

public class PaymentValidationService
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"^\+?[\d\s\-()]{10,20}$", RegexOptions.Compiled);

    public (bool IsValid, string? Error) Validate(CreatePaymentRequest? request)
    {
        if (request == null)
            return (false, "Request body is required.");

        if (string.IsNullOrWhiteSpace(request.WalletNumber))
            return (false, "WalletNumber is required.");
        if (string.IsNullOrWhiteSpace(request.Account))
            return (false, "Account is required.");
        if (string.IsNullOrWhiteSpace(request.Email))
            return (false, "Email is required.");
        if (!EmailRegex.IsMatch(request.Email))
            return (false, "Email format is invalid.");
        if (request.Amount <= 0)
            return (false, "Amount must be positive.");
        if (string.IsNullOrWhiteSpace(request.Currency))
            return (false, "Currency is required.");

        if (!string.IsNullOrWhiteSpace(request.Phone) && !PhoneRegex.IsMatch(request.Phone))
            return (false, "Phone format is invalid.");

        return (true, null);
    }
}
