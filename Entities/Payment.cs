namespace PaymentApi.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public string WalletNumber { get; set; } = string.Empty;
    public string Account { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Comment { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Created;
    public DateTime CreatedAt { get; set; }

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public Payment()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
}
