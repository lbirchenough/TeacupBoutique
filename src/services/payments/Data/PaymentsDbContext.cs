using Microsoft.EntityFrameworkCore;
using payments.Entities;

namespace payments.Data;

public class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
{
    public DbSet<StripeEventRecord> StripeEvents { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PendingPaymentAmount> PendingPaymentAmounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StripeEventRecord>()
            .HasKey(e => e.StripeEventId);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<PendingPaymentAmount>()
            .HasKey(p => p.OrderId);

        modelBuilder.Entity<PendingPaymentAmount>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);
    }
}
