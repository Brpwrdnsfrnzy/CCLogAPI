#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CCLogAPI.Models;

public partial class CCLogContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public CCLogContext()
    {
    }

    public CCLogContext(DbContextOptions<CCLogContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Audit> Audits { get; set; }

    public virtual DbSet<Credit> Credits { get; set; }

    public virtual DbSet<CreditReason> CreditReasons { get; set; }

    public virtual DbSet<DiaryEntry> DiaryEntries { get; set; }

    public virtual DbSet<GoodsReceivedStatus> GoodsReceivedStatuses { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<LogStatus> LogStatuses { get; set; }

    public virtual DbSet<LogType> LogTypes { get; set; }

    public virtual DbSet<ReportedIssue> ReportedIssues { get; set; }

    public virtual DbSet<ReturnMethod> ReturnMethods { get; set; }

    // Preferred, non-conflicting name for the domain Role DbSet
    public virtual DbSet<Role> AppRoles { get; set; }

    // Backwards-compatible alias so existing code that references _context.Roles continues to compile.
    [Obsolete("Use AppRoles instead.")]
    public new virtual DbSet<Role> Roles
    {
        get => AppRoles;
        set => AppRoles = value;
    }

    public virtual DbSet<SignOffReason> SignOffReasons { get; set; }

    public virtual DbSet<SubIssue> SubIssues { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Runtime must provide DbContext options via DI in Program.cs.
        // Do not silently fall back to a hardcoded connection string.
        if (!optionsBuilder.IsConfigured)
        {
            throw new InvalidOperationException("CCLogContext is not configured. Ensure ConnectionStrings:DefaultConnection is provided and AddDbContext<CCLogContext> is registered.");
        }
    }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ensure Identity mappings are applied first
        base.OnModelCreating(modelBuilder);

        // Map Identity entities to your existing table names
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User"); // Use your existing User table instead of AspNetUsers

            // Map Id property to uppercase ID column in database
            entity.Property(e => e.Id).HasColumnName("ID");

            // Configure Identity properties to match your database schema
            entity.Property(e => e.UserName).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);

            // Email is varchar(255) in your database, not nvarchar(256)
            entity.Property(e => e.Email).HasMaxLength(255).IsUnicode(false).IsRequired();
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);

            // PhoneNumber is nvarchar(MAX) in your database
            entity.Property(e => e.PhoneNumber);

            entity.Property(e => e.EmailConfirmed).IsRequired();
            entity.Property(e => e.PhoneNumberConfirmed).IsRequired();
            entity.Property(e => e.TwoFactorEnabled).IsRequired();
            entity.Property(e => e.LockoutEnabled).IsRequired();
            entity.Property(e => e.AccessFailedCount).IsRequired();

            // Configure domain properties - tell EF these are simple value columns, NOT foreign keys
            entity.Property(e => e.Role).HasColumnName("Role").IsRequired();
            entity.Property(e => e.Team).HasColumnName("Team").IsRequired();
            entity.Property(e => e.Name).HasMaxLength(50).IsUnicode(false).IsRequired();
            // New Assignable flag (bit) - indicates whether the user can be assigned to logs/tasks
            entity.Property(e => e.Assignable).HasColumnName("Assignable").IsRequired();

            // CRITICAL: Ignore ALL navigation properties to prevent EF from creating shadow FK properties
            entity.Ignore(e => e.RoleNavigation);
            entity.Ignore(e => e.TeamNavigation);
            entity.Ignore(e => e.LogLastUpdatedByNavigations);
            entity.Ignore(e => e.LogLoggedByNavigations);
            entity.Ignore(e => e.LogSignedOffByNavigations);
        });

        modelBuilder.Entity<IdentityRole<int>>(entity =>
        {
            entity.ToTable("AspNetRoles");
        });

        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("AspNetUserClaims");
        modelBuilder.Entity<IdentityUserRole<int>>().ToTable("AspNetUserRoles");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("AspNetUserLogins");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("AspNetRoleClaims");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("AspNetUserTokens");

        // Continue with your existing Credit, CreditReason, DiaryEntry, etc. configurations below...

        modelBuilder.Entity<Audit>(entity =>
        {
            entity.ToTable("Audit");

            entity.Property(e => e.ID);
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LogID);
            entity.Property(e => e.Action)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())", "DF_Table_1_createdDate")
                .HasColumnType("datetime");
            entity.Property(e => e.NewStatusID)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<Credit>(entity =>
        {
            entity.ToTable("Credit");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Details)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.CreditNote)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Refund)
                .HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.LogNavigation).WithMany(p => p.Credits)
                .HasForeignKey(d => d.Log)
                .HasConstraintName("FK_Credit_Log");

            entity.HasOne(d => d.ReasonNavigation).WithMany(p => p.Credits)
                .HasForeignKey(d => d.Reason)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Credit_CreditReason");
        });

        modelBuilder.Entity<CreditReason>(entity =>
        {
            entity.ToTable("CreditReason");

            entity.Property(e => e.Reason)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.OrderBy);
        });

        modelBuilder.Entity<DiaryEntry>(entity =>
        {
            entity.ToTable("DiaryEntry");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())", "DF_DiaryEntry_CreatedDate")
                .HasColumnType("datetime");
            entity.Property(e => e.Text)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.ParentId)
                .IsRequired(false);  // Allow NULL for root entries

            entity.HasOne(d => d.Log).WithMany(p => p.DiaryEntries)
                .HasForeignKey(d => d.LogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiaryEntry_Log");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .IsRequired(false)  // Parent is optional (nullable FK)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DiaryEntry_DiaryEntry");
        });

        modelBuilder.Entity<GoodsReceivedStatus>(entity =>
        {
            entity.ToTable("GoodsReceivedStatus");

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(6)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.ToTable("Log");

            entity.Property(e => e.CustomerAccount)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CustomerContact)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CustomerEmail)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.CustomerPhone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.DateLastUpdated)
                .HasDefaultValueSql("(getdate())", "DF_Log_DateLastUpdated")
                .HasColumnType("datetime");
            entity.Property(e => e.DateLogged)
                .HasDefaultValueSql("(getdate())", "DF_Log_DateLogged")
                .HasColumnType("datetime");
            entity.Property(e => e.LogText)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.SignOffDate).HasColumnType("datetime");
            entity.Property(e => e.SubIssue)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.RemindedOnThisStatus);
            entity.Property(e => e.EscalatedOnThisStatus);
            entity.Property(e => e.TrackingNo)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.LocksmithReq);
            entity.Property(e => e.LocksmithCost)
                .HasColumnType("money");
            entity.Property(e => e.LocksmithBriCost)
                .HasColumnType("money");
            entity.Property(e => e.LocksmithBriToPay);
            entity.Property(e => e.LocksmithOOH);
            entity.Property(e => e.LocksmithCylinder);
            entity.Property(e => e.OrderNo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.InvoiceNo)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.OtherReason)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsOtherReason);
            entity.Property(e => e.ToBlame);

            entity.HasOne(d => d.GoodsReceivedNavigation).WithMany(p => p.Logs)
                .HasForeignKey(d => d.GoodsReceived)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Log_GoodsReceivedStatus");

            entity.HasOne(d => d.LastUpdatedByNavigation).WithMany(p => p.LogLastUpdatedByNavigations)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Log_User1");

            entity.HasOne(d => d.LogTypeNavigation).WithMany(p => p.Logs)
                .HasForeignKey(d => d.LogType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Log_LogType");

            entity.HasOne(d => d.LoggedByNavigation).WithMany(p => p.LogLoggedByNavigations)
                .HasForeignKey(d => d.LoggedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Log_User");

            entity.HasOne(d => d.ReturnMethodNavigation).WithMany(p => p.Logs)
                .HasForeignKey(d => d.ReturnMethod)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Log_ReturnMethod");

            entity.HasOne(d => d.SignOffReasonNavigation).WithMany(p => p.Logs)
                .HasForeignKey(d => d.SignOffReason)
                .HasConstraintName("FK_Log_SignOffReason");

            entity.HasOne(d => d.SignedOffByNavigation).WithMany(p => p.LogSignedOffByNavigations)
                .HasForeignKey(d => d.SignedOffBy)
                .HasConstraintName("FK_Log_User2");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.Logs)
                .HasForeignKey(d => d.Status)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Log_LogStatus");
        });

        modelBuilder.Entity<LogStatus>(entity =>
        {
            entity.ToTable("LogStatus");

            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.EscalateDays).HasDefaultValue(10, "DF_LogStatus_EscalateDays");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RemindDays).HasDefaultValue(5, "DF_LogStatus_RemindDays");
            entity.Property(e => e.OrderBy);
        });

        modelBuilder.Entity<LogType>(entity =>
        {
            entity.ToTable("LogType");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ReportedIssue>(entity =>
        {
            entity.ToTable("ReportedIssue");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Code)
                .HasMaxLength(4)
                .IsUnicode(false);
            entity.Property(e => e.OrderBy);
        });

        modelBuilder.Entity<ReturnMethod>(entity =>
        {
            entity.ToTable("ReturnMethod");

            entity.Property(e => e.MethodName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            // Ignore the Users collection to prevent EF from creating a relationship
            entity.Ignore(e => e.Users);
        });

        modelBuilder.Entity<SignOffReason>(entity =>
        {
            entity.ToTable("SignOffReason");

            entity.Property(e => e.Text)
                .IsRequired()
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Code)
                .HasMaxLength(48)
                .IsUnicode(false);
            entity.Property(e => e.OrderBy);
        });

        modelBuilder.Entity<SubIssue>(entity =>
        {
            entity.ToTable("SubIssue");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.OrderBy);

            entity.HasOne(d => d.IssueNavigation).WithMany(p => p.SubIssues)
                .HasForeignKey(d => d.Issue)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SubIssue_ReportedIssue");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.ToTable("Team");

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(2);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ViewCategories)
                .HasMaxLength(30)
                .IsUnicode(false);

            // Ignore the Users collection to prevent EF from creating a relationship
            entity.Ignore(e => e.Users);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}