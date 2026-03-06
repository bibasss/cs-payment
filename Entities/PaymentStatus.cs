namespace PaymentApi.Entities;

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Cancelled = 3,
    Created = 4,
    Rejected = 5
}
