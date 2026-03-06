using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentApi.DTOs;
using PaymentApi.Entities;
using PaymentApi.Interfaces;
using PaymentApi.Services;

namespace PaymentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserRepository userRepository, ILogger<UsersController> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> GetAll(CancellationToken ct)
    {
        var users = await _userRepository.GetAllAsync(ct);
        var list = users.Select(u => new UserResponse
        {
            Id = u.Id,
            Email = u.Email,
            Role = u.Role,
            CreatedAt = u.CreatedAt
        }).ToList();
        return Results.Ok(list);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IResult> GetById(Guid id, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null)
            return Results.NotFound();
        return Results.Ok(new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IResult> Create([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Results.BadRequest(new { error = "Email and Password are required." });

        var existing = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existing != null)
            return Results.Conflict(new { error = "User with this email already exists." });

        var user = new User(
            request.Email,
            PasswordHelper.Hash(request.Password),
            request.Role ?? "User");
        await _userRepository.AddAsync(user, ct);

        return Results.Created($"/api/users/{user.Id}", new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IResult> Update(Guid id, [FromBody] CreateUserRequest request, CancellationToken ct)
    {
        if (request == null)
            return Results.BadRequest();

        var user = await _userRepository.GetByIdAsync(id, ct);
        if (user == null)
            return Results.NotFound();

        user.Email = request.Email;
        user.PasswordHash = PasswordHelper.Hash(request.Password);
        user.Role = request.Role ?? user.Role;
        await _userRepository.UpdateAsync(user, ct);
        return Results.Ok(new UserResponse { Id = user.Id, Email = user.Email, Role = user.Role, CreatedAt = user.CreatedAt });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> Delete(Guid id, CancellationToken ct)
    {
        var ok = await _userRepository.DeleteAsync(id, ct);
        if (!ok)
            return Results.NotFound();
        return Results.NoContent();
    }
}
