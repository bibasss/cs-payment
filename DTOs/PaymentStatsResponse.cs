namespace PaymentApi.DTOs;

public class PaymentStatsResponse
{
    public decimal TotalAmount { get; set; }
    public int TotalCount { get; set; }
    public List<PaymentStatsByDay> ByDay { get; set; } = new();
}

public class PaymentStatsByDay
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public decimal Sum { get; set; }
}
