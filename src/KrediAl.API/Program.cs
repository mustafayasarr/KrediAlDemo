using KrediAl.Application.Interfaces;
using KrediAl.Infrastructure.Data;
using KrediAl.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Kredi Al API",
        Version = "v1",
        Description = "Credit Guarantee Platform API - Pazaryerleri ve müşteriler için kredi kefalet sistemi",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Kredi Al",
            Email = "info@kredial.com"
        }
    });
    
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Örnek: 'Bearer {token}'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    c.EnableAnnotations();
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSwagger", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddDbContext<KrediAlDbContext>(options =>
    options.UseInMemoryDatabase("KrediAlDb"));

var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyForDemoAtLeast32Characters!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "KrediAl";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "KrediAl";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IFindeksService, FindeksService>();
builder.Services.AddScoped<IBankService, BankService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IMarketplaceService, MarketplaceService>();

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

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<KrediAlDbContext>();
    context.Database.EnsureCreated();
    
    // In-Memory Database için seed data manuel yükleme
    if (!context.Banks.Any())
    {
        context.Banks.AddRange(
            new KrediAl.Domain.Entities.Bank
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Garanti BBVA",
                Code = "GARANTI",
                ApiUrl = "https://api.garanti.example.com",
                ApiKey = "garanti-api-key",
                IsActive = true,
                MinLoanAmount = 1000,
                MaxLoanAmount = 100000,
                MinInstallment = 3,
                MaxInstallment = 36,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new KrediAl.Domain.Entities.Bank
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Yapı Kredi",
                Code = "YAPIKREDI",
                ApiUrl = "https://api.yapikredi.example.com",
                ApiKey = "yapikredi-api-key",
                IsActive = true,
                MinLoanAmount = 1000,
                MaxLoanAmount = 150000,
                MinInstallment = 3,
                MaxInstallment = 48,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new KrediAl.Domain.Entities.Bank
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "İş Bankası",
                Code = "ISBANK",
                ApiUrl = "https://api.isbank.example.com",
                ApiKey = "isbank-api-key",
                IsActive = true,
                MinLoanAmount = 2000,
                MaxLoanAmount = 200000,
                MinInstallment = 6,
                MaxInstallment = 60,
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
        context.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Kredi Al API v1");
        c.RoutePrefix = string.Empty;
        c.DocumentTitle = "Kredi Al API Documentation";
    });
}

app.UseCors("AllowSwagger");
app.UseStaticFiles(); // wwwroot dosyalarını sunmak için
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
