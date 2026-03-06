using Microsoft.EntityFrameworkCore;
using PaymentApi.Data;
using PaymentApi.Entities;
using PaymentApi.Interfaces;

namespace PaymentApi.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AppDbContext db, ILogger<UserRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _db.Users.AsNoTracking().ToListAsync(ct);
        return list;
    }

    public async Task<User> AddAsync(User user, CancellationToken ct = default)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("User created: {Email}", user.Email);
        return user;
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken ct = default)
    {
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id, ct);
        if (existing == null)
            return false;
        existing.Email = user.Email;
        existing.PasswordHash = user.PasswordHash;
        existing.Role = user.Role;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user == null)
            return false;
        _db.Users.Remove(user);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("User deleted: {Id}", id);
        return true;
    }
}
