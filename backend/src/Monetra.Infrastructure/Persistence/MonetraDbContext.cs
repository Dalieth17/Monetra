using Microsoft.EntityFrameworkCore;
using Monetra.Domain.Entities;
using Monetra.Domain.Enums;

namespace Monetra.Infrastructure.Persistence;

public class MonetraDbContext : DbContext
{
    public MonetraDbContext(DbContextOptions<MonetraDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Debt> Debts => Set<Debt>();
    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<ExchangeRateCache> ExchangeRateCaches => Set<ExchangeRateCache>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Role Configuration & Seed
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(r => r.Name).IsUnique();

            entity.HasData(
                new Role { Id = 1, Name = AppRoles.Usuario },
                new Role { Id = 2, Name = AppRoles.Administrador },
                new Role { Id = 3, Name = AppRoles.Soporte },
                new Role { Id = 4, Name = AppRoles.Auditor }
            );
        });

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();

            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Account Configuration
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(100);
            entity.Property(a => a.CurrencyCode).IsRequired().HasMaxLength(10);
            entity.Property(a => a.CurrentBalance).HasPrecision(18, 4);

            entity.HasOne(a => a.User)
                .WithMany(u => u.Accounts)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Category Configuration & Seed
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Icon).HasMaxLength(50);

            entity.HasData(
                new Category { Id = 1, Name = "Alimentación", Icon = "restaurant", IsDefault = true },
                new Category { Id = 2, Name = "Transporte", Icon = "directions_bus", IsDefault = true },
                new Category { Id = 3, Name = "Vivienda", Icon = "home", IsDefault = true },
                new Category { Id = 4, Name = "Salud", Icon = "local_hospital", IsDefault = true },
                new Category { Id = 5, Name = "Entretenimiento", Icon = "movie", IsDefault = true },
                new Category { Id = 6, Name = "Educación", Icon = "school", IsDefault = true },
                new Category { Id = 7, Name = "Servicios Básicos", Icon = "bolt", IsDefault = true },
                new Category { Id = 8, Name = "Suscripciones", Icon = "subscriptions", IsDefault = true },
                new Category { Id = 9, Name = "Salario", Icon = "payments", IsDefault = true },
                new Category { Id = 10, Name = "Inversiones", Icon = "trending_up", IsDefault = true },
                new Category { Id = 11, Name = "Otros", Icon = "more_horiz", IsDefault = true }
            );
        });

        // Transaction Configuration
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Amount).HasPrecision(18, 4);
            entity.Property(t => t.Description).HasMaxLength(255);

            entity.HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Debt Configuration
        modelBuilder.Entity<Debt>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Creditor).IsRequired().HasMaxLength(100);
            entity.Property(d => d.TotalAmount).HasPrecision(18, 4);
            entity.Property(d => d.RemainingAmount).HasPrecision(18, 4);
            entity.Property(d => d.InterestRate).HasPrecision(18, 4);
            entity.Property(d => d.RateType).HasMaxLength(10);

            entity.HasOne(d => d.User)
                .WithMany(u => u.Debts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SavingsGoal Configuration
        modelBuilder.Entity<SavingsGoal>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Name).IsRequired().HasMaxLength(100);
            entity.Property(g => g.TargetAmount).HasPrecision(18, 4);
            entity.Property(g => g.CurrentAmount).HasPrecision(18, 4);

            entity.HasOne(g => g.User)
                .WithMany(u => u.SavingsGoals)
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Subscription Configuration
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Amount).HasPrecision(18, 4);
            entity.Property(s => s.CurrencyCode).HasMaxLength(10);
            entity.Property(s => s.BillingCycle).HasMaxLength(50);

            entity.HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ExchangeRateCache Configuration & Seed
        modelBuilder.Entity<ExchangeRateCache>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BaseCurrency).HasMaxLength(10);
            entity.Property(e => e.TargetCurrency).HasMaxLength(10);
            entity.Property(e => e.Rate).HasPrecision(18, 4);

            entity.HasData(
                new ExchangeRateCache
                {
                    Id = 1,
                    BaseCurrency = "USD",
                    TargetCurrency = "COP",
                    Rate = 4150.00m,
                    LastUpdatedAt = new DateTime(2026, 11, 10, 0, 0, 0, DateTimeKind.Utc),
                    Source = "YahooFinance"
                }
            );
        });
    }
}

