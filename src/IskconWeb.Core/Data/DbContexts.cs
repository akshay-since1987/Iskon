using IskconWeb.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IskconWeb.Core.Data;

/// <summary>
/// Master Database Context - Authoring database (drafts + published)
/// Used by API for all write operations and content management
/// </summary>
public class IskonMasterDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public IskonMasterDbContext(DbContextOptions<IskonMasterDbContext> options)
        : base(options)
    {
    }

    public DbSet<Temple> Temples { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<TempleTimings> TempleTimings { get; set; }
    public DbSet<MediaGallery> MediaGalleries { get; set; }
    public DbSet<EventRegistration> EventRegistrations { get; set; }
    public DbSet<CourseEnrollment> CourseEnrollments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Identity tables
        modelBuilder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");

        // Configure Temple
        modelBuilder.Entity<Temple>()
            .HasKey(t => t.Id);
        modelBuilder.Entity<Temple>()
            .Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Configure User (IdentityUser)
        modelBuilder.Entity<User>()
            .Property(u => u.FirstName)
            .HasMaxLength(100);
        modelBuilder.Entity<User>()
            .Property(u => u.LastName)
            .HasMaxLength(100);
        modelBuilder.Entity<User>()
            .HasOne(u => u.Temple)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TempleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Event
        modelBuilder.Entity<Event>()
            .HasKey(e => e.Id);
        modelBuilder.Entity<Event>()
            .Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(250);
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Temple)
            .WithMany(t => t.Events)
            .HasForeignKey(e => e.TempleId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Event>()
            .HasIndex(e => new { e.TempleId, e.PublishStatus });

        // Configure Course
        modelBuilder.Entity<Course>()
            .HasKey(c => c.Id);
        modelBuilder.Entity<Course>()
            .Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(250);
        modelBuilder.Entity<Course>()
            .HasOne(c => c.Temple)
            .WithMany(t => t.Courses)
            .HasForeignKey(c => c.TempleId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Course>()
            .HasIndex(c => new { c.TempleId, c.PublishStatus });

        // Configure TempleTimings
        modelBuilder.Entity<TempleTimings>()
            .HasKey(tt => tt.Id);
        modelBuilder.Entity<TempleTimings>()
            .Property(tt => tt.AratiName)
            .IsRequired()
            .HasMaxLength(100);
        modelBuilder.Entity<TempleTimings>()
            .HasOne(tt => tt.Temple)
            .WithMany(t => t.Timings)
            .HasForeignKey(tt => tt.TempleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure MediaGallery
        modelBuilder.Entity<MediaGallery>()
            .HasKey(mg => mg.Id);
        modelBuilder.Entity<MediaGallery>()
            .Property(mg => mg.Title)
            .IsRequired()
            .HasMaxLength(200);
        modelBuilder.Entity<MediaGallery>()
            .HasOne(mg => mg.Temple)
            .WithMany(t => t.MediaGalleries)
            .HasForeignKey(mg => mg.TempleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure EventRegistration
        modelBuilder.Entity<EventRegistration>()
            .HasKey(er => er.Id);
        modelBuilder.Entity<EventRegistration>()
            .HasOne(er => er.Event)
            .WithMany(e => e.Registrations)
            .HasForeignKey(er => er.EventId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<EventRegistration>()
            .HasOne(er => er.User)
            .WithMany(u => u.EventRegistrations)
            .HasForeignKey(er => er.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<EventRegistration>()
            .HasIndex(er => new { er.EventId, er.UserId })
            .IsUnique();

        // Configure CourseEnrollment
        modelBuilder.Entity<CourseEnrollment>()
            .HasKey(ce => ce.Id);
        modelBuilder.Entity<CourseEnrollment>()
            .HasOne(ce => ce.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(ce => ce.CourseId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<CourseEnrollment>()
            .HasOne(ce => ce.User)
            .WithMany(u => u.CourseEnrollments)
            .HasForeignKey(ce => ce.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<CourseEnrollment>()
            .HasIndex(ce => new { ce.CourseId, ce.UserId })
            .IsUnique();
    }
}

/// <summary>
/// Web Database Context - Read-only published content
/// Used by public website for optimized reads
/// Contents synced from Master DB via ContentSyncService
/// </summary>
public class IskonWebDbContext : DbContext
{
    public IskonWebDbContext(DbContextOptions<IskonWebDbContext> options)
        : base(options)
    {
    }

    public DbSet<Temple> Temples { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<TempleTimings> TempleTimings { get; set; }
    public DbSet<MediaGallery> MediaGalleries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mirror Master DB schema for published content only
        // Configure Temple
        modelBuilder.Entity<Temple>()
            .HasKey(t => t.Id);
        modelBuilder.Entity<Temple>()
            .Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Configure Event (published only)
        modelBuilder.Entity<Event>()
            .HasKey(e => e.Id);
        modelBuilder.Entity<Event>()
            .Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(250);
        modelBuilder.Entity<Event>()
            .HasOne(e => e.Temple)
            .WithMany(t => t.Events)
            .HasForeignKey(e => e.TempleId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Event>()
            .HasIndex(e => new { e.TempleId, e.PublishStatus });

        // Configure Course (published only)
        modelBuilder.Entity<Course>()
            .HasKey(c => c.Id);
        modelBuilder.Entity<Course>()
            .Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(250);
        modelBuilder.Entity<Course>()
            .HasOne(c => c.Temple)
            .WithMany(t => t.Courses)
            .HasForeignKey(c => c.TempleId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Course>()
            .HasIndex(c => new { c.TempleId, c.PublishStatus });

        // Configure TempleTimings
        modelBuilder.Entity<TempleTimings>()
            .HasKey(tt => tt.Id);
        modelBuilder.Entity<TempleTimings>()
            .Property(tt => tt.AratiName)
            .IsRequired()
            .HasMaxLength(100);
        modelBuilder.Entity<TempleTimings>()
            .HasOne(tt => tt.Temple)
            .WithMany(t => t.Timings)
            .HasForeignKey(tt => tt.TempleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure MediaGallery
        modelBuilder.Entity<MediaGallery>()
            .HasKey(mg => mg.Id);
        modelBuilder.Entity<MediaGallery>()
            .Property(mg => mg.Title)
            .IsRequired()
            .HasMaxLength(200);
        modelBuilder.Entity<MediaGallery>()
            .HasOne(mg => mg.Temple)
            .WithMany(t => t.MediaGalleries)
            .HasForeignKey(mg => mg.TempleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
