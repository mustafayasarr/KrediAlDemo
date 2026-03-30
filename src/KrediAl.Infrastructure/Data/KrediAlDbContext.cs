using KrediAl.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KrediAl.Infrastructure.Data;

public class KrediAlDbContext : DbContext
{
    public KrediAlDbContext(DbContextOptions<KrediAlDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Marketplace> Marketplaces { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Bank> Banks { get; set; }
    public DbSet<BankOffer> BankOffers { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.NationalId).IsUnique();
            entity.Property(e => e.GuaranteeAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Marketplace>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderId);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            
            entity.HasOne(e => e.Marketplace)
                .WithMany(m => m.Transactions)
                .HasForeignKey(e => e.MarketplaceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.Tax).HasPrecision(5, 2);
            
            entity.HasOne(e => e.Transaction)
                .WithMany(t => t.OrderItems)
                .HasForeignKey(e => e.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Bank>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.MinLoanAmount).HasPrecision(18, 2);
            entity.Property(e => e.MaxLoanAmount).HasPrecision(18, 2);
        });

        modelBuilder.Entity<BankOffer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LoanAmount).HasPrecision(18, 2);
            entity.Property(e => e.InterestRate).HasPrecision(5, 2);
            entity.Property(e => e.MonthlyPayment).HasPrecision(18, 2);
            entity.Property(e => e.TotalPayment).HasPrecision(18, 2);
            
            entity.HasOne(e => e.Transaction)
                .WithMany(t => t.BankOffers)
                .HasForeignKey(e => e.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Bank)
                .WithMany(b => b.BankOffers)
                .HasForeignKey(e => e.BankId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CommissionAmount).HasPrecision(18, 2);
            entity.Property(e => e.CommissionRate).HasPrecision(5, 2);
            
            entity.HasOne(e => e.Transaction)
                .WithOne(t => t.Payment)
                .HasForeignKey<Payment>(e => e.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var marketplaceId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        modelBuilder.Entity<Marketplace>().HasData(new Marketplace
        {
            Id = marketplaceId,
            Name = "Demo Marketplace",
            Username = "demo_mp",
            PasswordHash = "$2a$11$UidHhTKyxTOlQHOOkjKhK.bChCsX8JO7Ter6Xe.FHIX0xgvvpoWHi",
            ApiKey = "demo-api-key-12345",
            CallbackUrl = "https://marketplace.example.com/callback",
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        var bank1Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var bank2Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var bank3Id = Guid.Parse("44444444-4444-4444-4444-444444444444");

        modelBuilder.Entity<Bank>().HasData(
            new Bank
            {
                Id = bank1Id,
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
            new Bank
            {
                Id = bank2Id,
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
            new Bank
            {
                Id = bank3Id,
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
    }
}
