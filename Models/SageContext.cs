#nullable disable
using Microsoft.EntityFrameworkCore;

namespace CCLogAPI.Models;

/// <summary>
/// Read-only DbContext for the Sage200_Brisant_Live_New database.
/// Provides access to the SS_Bris_cust view.
/// </summary>
public class SageContext : DbContext
{
    public SageContext(DbContextOptions<SageContext> options)
        : base(options)
    {
    }

    public virtual DbSet<SageBrisCustomer> SageBrisCustomers { get; set; }

    public virtual DbSet<SageBrisCcLu> SageBrisCcLus { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SageBrisCustomer>(entity =>
        {
            entity.HasNoKey(); // View has no primary key
            entity.ToView("SS_Bris_cust");

            entity.Property(e => e.Code)
                .HasColumnName("code")
                .HasMaxLength(8)
                .IsUnicode(false);

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.Property(e => e.AddressLine1)
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.Property(e => e.AddressLine2)
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.Property(e => e.AddressLine3)
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.Property(e => e.AddressLine4)
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.Property(e => e.City)
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.Property(e => e.PostCode)
                .HasMaxLength(10)
                .IsUnicode(false);

            entity.Property(e => e.County)
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.Property(e => e.Country)
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.Property(e => e.AnalysisCode1)
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.Property(e => e.MainTelephoneAreaCode)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.MainTelephoneSubscriberNumber)
                .HasMaxLength(200)
                .IsUnicode(false);

            entity.Property(e => e.ContactName)
                .HasMaxLength(235)
                .IsUnicode(false);

            entity.Property(e => e.Description)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.ContactValue)
                .HasMaxLength(227)
                .IsUnicode(false);
        });

        modelBuilder.Entity<SageBrisCcLu>(entity =>
        {
            entity.HasNoKey(); // View has no primary key
            entity.ToView("SS_BRIS_CC_LU");

            entity.Property(e => e.GDN_NUMBER)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.ORDER_ITEM)
                .IsRequired();

            entity.Property(e => e.STOCK_CODE)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.Property(e => e.ORDER_NUMBER)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.custorder)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.Property(e => e.weborder)
                .IsRequired()
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.Property(e => e.QTY_DESPATCHED)
                .IsRequired()
                .HasColumnType("decimal(18, 2)");

            entity.Property(e => e.DateDespatch)
                .HasMaxLength(30)
                .IsUnicode(false);

            entity.Property(e => e.OrderReturnNo)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.InvoiceCreditNo)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.ItemDescription)
                .IsRequired()
                .IsUnicode(false); // varchar(max)

            entity.Property(e => e.accno)
                .IsRequired()
                .HasMaxLength(8)
                .IsUnicode(false);

            entity.Property(e => e.accname)
                .IsRequired()
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.Property(e => e.dname)
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.Property(e => e.dcode)
                .HasMaxLength(10)
                .IsUnicode(false);
        });
    }
}
