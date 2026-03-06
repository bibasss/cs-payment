using PaymentApi.DTOs;
using PaymentApi.Entities;

namespace PaymentApi.Interfaces;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Payment>> GetAllAsync(bool sortByDateDesc = true, int? skip = null, int? take = null, CancellationToken ct = default);
    Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<IEnumerable<Payment>> GetByUserIdAndStatusAsync(Guid userId, PaymentStatus status, CancellationToken ct = default);
    Task<PaymentStatsResponse> GetStatsAsync(CancellationToken ct = default);
    Task<Payment> AddAsync(Payment payment, CancellationToken ct = default);
    Task<bool> UpdateAsync(Payment payment, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
