using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PaymentApi.DTOs;
using PaymentApi.Entities;
using PaymentApi.Interfaces;
using PaymentApi.Options;
using PaymentApi.Services;

namespace PaymentApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUserRepository _userRepository;
    private readonly PaymentValidationService _validation;
    private readonly IOptions<ApiKeySettings> _apiKeyOptions;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentRepository paymentRepository,
        IUserRepository userRepository,
        PaymentValidationService validation,
        IOptions<ApiKeySettings> apiKeyOptions,
        ILogger<PaymentsController> logger)
    {
        _paymentRepository = paymentRepository;
        _userRepository = userRepository;
        _validation = validation;
        _apiKeyOptions = apiKeyOptions;
        _logger = logger;
    }

    private bool IsValidApiKey()
    {
        var key = Request.Headers["X-Api-Key"].FirstOrDefault();
        var configured = _apiKeyOptions.Value.Key;
        return !string.IsNullOrEmpty(configured) && configured == key;
    }

    private Guid? GetCurrentUserId()
    {
        if (!User.Identity?.IsAuthenticated == true) return null;
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(id, out var guid) ? guid : null;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IResult> GetPayments(
        [FromQuery] string? sort,
        [FromQuery] int? skip,
        [FromQuery] int? take,
        CancellationToken ct)
    {
        if (IsValidApiKey())
        {
            var sortByDateDesc = sort != "asc";
            var list = await _paymentRepository.GetAllAsync(sortByDateDesc, skip, take, ct);
            return Results.Ok(list.Select(MapToResponse).ToList());
        }
        var userId = GetCurrentUserId();
        if (userId == null)
            return Results.Unauthorized();
        var payments = await _paymentRepository.GetByUserIdAsync(userId.Value, ct);
        return Results.Ok(payments.Select(MapToResponse).ToList());
    }

    [HttpGet("stats")]
    [AllowAnonymous]
    public async Task<IResult> GetStats(CancellationToken ct)
    {
        if (!IsValidApiKey() && User.Identity?.IsAuthenticated != true)
            return Results.Unauthorized();
        var stats = await _paymentRepository.GetStatsAsync(ct);
        return Results.Ok(stats);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IResult> GetById(Guid id, CancellationToken ct)
    {
        if (!IsValidApiKey())
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Results.Unauthorized();
            var payment = await _paymentRepository.GetByIdAsync(id, ct);
            if (payment == null) return Results.NotFound();
            if (payment.UserId != userId) return Results.Forbid();
            return Results.Ok(MapToResponse(payment));
        }
        var p = await _paymentRepository.GetByIdAsync(id, ct);
        if (p == null) return Results.NotFound();
        return Results.Ok(MapToResponse(p));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IResult> Create([FromBody] CreatePaymentRequest request, CancellationToken ct)
    {
        var (isValid, error) = _validation.Validate(request);
        if (!isValid)
            return Results.BadRequest(new { error = error });

        if (!IsValidApiKey() && User.Identity?.IsAuthenticated != true)
            return Results.Unauthorized();

        var payment = new Payment
        {
            WalletNumber = request!.WalletNumber,
            Account = request.Account,
            Email = request.Email,
            Phone = request.Phone,
            Amount = request.Amount,
            Currency = request.Currency ?? "USD",
            Comment = request.Comment,
            Status = PaymentStatus.Created,
            UserId = GetCurrentUserId()
        };
        await _paymentRepository.AddAsync(payment, ct);
        return Results.Created($"/api/payments/{payment.Id}", MapToResponse(payment));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IResult> Update(Guid id, [FromBody] CreatePaymentRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Results.Unauthorized();
        if (request == null) return Results.BadRequest();
        var payment = await _paymentRepository.GetByIdAsync(id, ct);
        if (payment == null) return Results.NotFound();
        if (payment.UserId != userId) return Results.Forbid();

        payment.WalletNumber = request.WalletNumber;
        payment.Account = request.Account;
        payment.Email = request.Email;
        payment.Phone = request.Phone;
        payment.Amount = request.Amount;
        payment.Currency = request.Currency ?? "USD";
        payment.Comment = request.Comment;
        await _paymentRepository.UpdateAsync(payment, ct);
        return Results.Ok(MapToResponse(payment));
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Results.Unauthorized();
        var payment = await _paymentRepository.GetByIdAsync(id, ct);
        if (payment == null) return Results.NotFound();
        if (payment.UserId != userId) return Results.Forbid();
        await _paymentRepository.DeleteAsync(id, ct);
        return Results.NoContent();
    }

    private static PaymentResponse MapToResponse(Payment p)
    {
        return new PaymentResponse
        {
            Id = p.Id,
            WalletNumber = p.WalletNumber,
            Account = p.Account,
            Email = p.Email,
            Phone = p.Phone,
            Amount = p.Amount,
            Currency = p.Currency,
            Comment = p.Comment,
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt
        };
    }
}
