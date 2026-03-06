using Microsoft.EntityFrameworkCore;
using PaymentApi.Data;
using PaymentApi.DTOs;
using PaymentApi.Entities;
using PaymentApi.Interfaces;

namespace PaymentApi.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<PaymentRepository> _logger;

    public PaymentRepository(AppDbContext db, ILogger<PaymentRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Payments
            .Include(p => p.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IEnumerable<Payment>> GetAllAsync(bool sortByDateDesc = true, int? skip = null, int? take = null, CancellationToken ct = default)
    {
        var query = _db.Payments.AsNoTracking();
        query = sortByDateDesc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt);
        if (skip.HasValue)
            query = query.Skip(skip.Value);
        if (take.HasValue)
            query = query.Take(take.Value);
        return await query.ToListAsync(ct);
    }

    public async Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var list = await _db.Payments
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
        return list;
    }

    public async Task<IEnumerable<Payment>> GetByUserIdAndStatusAsync(Guid userId, PaymentStatus status, CancellationToken ct = default)
    {
        var list = await _db.Payments
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
        return list;
    }

    public async Task<PaymentStatsResponse> GetStatsAsync(CancellationToken ct = default)
    {
        var totalAmount = await _db.Payments.SumAsync(p => p.Amount, ct);
        var totalCount = await _db.Payments.CountAsync(ct);
        var items = await _db.Payments.AsNoTracking().Select(p => new { p.CreatedAt, p.Amount }).ToListAsync(ct);
        var byDay = items
            .GroupBy(p => p.CreatedAt.Date)
            .Select(g => new PaymentStatsByDay { Date = g.Key, Count = g.Count(), Sum = g.Sum(p => p.Amount) })
            .OrderBy(x => x.Date)
            .ToList();
        return new PaymentStatsResponse { TotalAmount = totalAmount, TotalCount = totalCount, ByDay = byDay };
    }

    public async Task<Payment> AddAsync(Payment payment, CancellationToken ct = default)
    {
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Payment created: {Id}", payment.Id);
        return payment;
    }

    public async Task<bool> UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        var existing = await _db.Payments.FirstOrDefaultAsync(p => p.Id == payment.Id, ct);
        if (existing == null)
            return false;
        existing.WalletNumber = payment.WalletNumber;
        existing.Account = payment.Account;
        existing.Email = payment.Email;
        existing.Phone = payment.Phone;
        existing.Amount = payment.Amount;
        existing.Currency = payment.Currency;
        existing.Comment = payment.Comment;
        existing.Status = payment.Status;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (payment == null)
            return false;
        _db.Payments.Remove(payment);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Payment deleted: {Id}", id);
        return true;
    }
}
