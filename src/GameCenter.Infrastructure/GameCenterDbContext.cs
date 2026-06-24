using GameCenter.Application;
using GameCenter.Domain;
using Microsoft.EntityFrameworkCore;

namespace GameCenter.Infrastructure;

public sealed class GameCenterDbContext(DbContextOptions<GameCenterDbContext> options, ICurrentUserService currentUser) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Cashier> Cashiers => Set<Cashier>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<GameMode> GameModes => Set<GameMode>();
    public DbSet<TransactionRecord> Transactions => Set<TransactionRecord>();
    public DbSet<BonusPointEntry> BonusPointEntries => Set<BonusPointEntry>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<LedgerEntry> LedgerEntries => Set<LedgerEntry>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<GameRegister> GameRegisters => Set<GameRegister>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(x => x.UserName).IsUnique();
            entity.Property(x => x.UserName).HasMaxLength(100);
            entity.Property(x => x.FullName).HasMaxLength(150);
            entity.Property(x => x.PasswordHash).HasMaxLength(300);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasIndex(x => new { x.Role, x.FormName }).IsUnique();
            entity.Property(x => x.FormName).HasMaxLength(80);
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasIndex(x => x.ClubName);
            entity.Property(x => x.ClubName).HasMaxLength(160);
            entity.Property(x => x.Email).HasMaxLength(160);
        });

        modelBuilder.Entity<Cashier>(entity =>
        {
            entity.HasIndex(x => x.LocationId);
            entity.HasIndex(x => x.CashierCode).IsUnique();
            entity.Property(x => x.CashierCode).HasMaxLength(40);
            entity.Property(x => x.CashRegisterBalance).HasPrecision(18, 2);
            entity.Property(x => x.RowVersion).IsRowVersion();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(x => x.LocationId);
            entity.HasIndex(x => x.CustomerCode).IsUnique();
            entity.Property(x => x.CustomerCode).HasMaxLength(40);
            entity.Property(x => x.Balance).HasPrecision(18, 2);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasOne(x => x.ReferralCustomer)
                .WithMany()
                .HasForeignKey(x => x.ReferralCustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasIndex(x => x.LocationId);
            entity.HasIndex(x => x.CustomerId);
            entity.HasIndex(x => x.MembershipNumber).IsUnique();
        });

        modelBuilder.Entity<GameMode>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(10);
        });

        modelBuilder.Entity<TransactionRecord>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasIndex(x => x.CustomerId);
            entity.HasIndex(x => x.CashierId);
            entity.HasIndex(x => x.LocationId);
            entity.HasIndex(x => x.GameModeId);
            entity.HasIndex(x => x.OccurredAt);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.CustomerBalanceAfter).HasPrecision(18, 2);
            entity.Property(x => x.CashierRegisterAfter).HasPrecision(18, 2);
        });

        modelBuilder.Entity<BonusPointEntry>(entity =>
        {
            entity.HasIndex(x => x.CustomerId);
            entity.HasIndex(x => x.CashierId);
            entity.HasIndex(x => x.LocationId);
            entity.HasIndex(x => x.OccurredAt);
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasIndex(x => x.AccountNumber).IsUnique();
            entity.Property(x => x.AccountNumber).HasMaxLength(30);
            entity.Property(x => x.AccountName).HasMaxLength(160);
        });

        modelBuilder.Entity<LedgerEntry>(entity =>
        {
            entity.HasIndex(x => x.LocationId);
            entity.HasIndex(x => x.EntryDate);
            entity.HasIndex(x => x.TransactionRecordId);
            entity.Property(x => x.Debit).HasPrecision(18, 2);
            entity.Property(x => x.Credit).HasPrecision(18, 2);
            entity.HasOne(x => x.TransactionRecord)
                .WithMany()
                .HasForeignKey(x => x.TransactionRecordId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Expense)
                .WithMany()
                .HasForeignKey(x => x.ExpenseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasIndex(x => x.LocationId);
            entity.HasIndex(x => x.ExpenseDate);
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Debit).HasPrecision(18, 2);
            entity.Property(x => x.Credit).HasPrecision(18, 2);
        });

        modelBuilder.Entity<GameRegister>(entity =>
        {
            entity.HasIndex(x => x.LocationId);
            entity.Property(x => x.PurchaseAmount).HasPrecision(18, 2);
            entity.Property(x => x.MaintenanceCosts).HasPrecision(18, 2);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var userName = currentUser.UserName;

        foreach (var entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = string.IsNullOrWhiteSpace(entry.Entity.CreatedBy) ? userName : entry.Entity.CreatedBy;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userName;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
