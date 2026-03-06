using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PaymentApi.Data;
using PaymentApi.Interfaces;
using PaymentApi.Middleware;
using PaymentApi.Options;
using PaymentApi.Repositories;
using PaymentApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация (appsettings.json, IOptions)
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<ApiKeySettings>(builder.Configuration.GetSection(ApiKeySettings.SectionName));

// DbContext + PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Port=5432;Database=calc3d;Username=postgres;Password=";
    options.UseNpgsql(conn);
});

// DI: репозитории и сервисы (Scoped)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PaymentValidationService>();

// Контроллеры
builder.Services.AddControllers();

// JWT
var jwtSection = builder.Configuration.GetSection(JwtSettings.SectionName);
var secret = jwtSection["Secret"] ?? "PaymentApiSecretKeyMinimum32Characters!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"] ?? "PaymentApi",
            ValidAudience = jwtSection["Audience"] ?? "PaymentApi",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
        };
    });
builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware: обработка исключений
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Терминальная конечная точка для проверки (конвейер, Map)
app.MapGet("/", () => Results.Ok(new { message = "Payment API", version = "1.0" }));

app.Run();
