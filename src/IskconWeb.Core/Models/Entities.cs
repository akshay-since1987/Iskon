using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace IskconWeb.Core.Models;

/// <summary>
/// Core domain entities for ISKCON multi-site portal
/// All entities support multi-tenancy via TempleId
/// </summary>
/// 
public class Temple
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<TempleTimings> Timings { get; set; } = new List<TempleTimings>();
    public ICollection<MediaGallery> MediaGalleries { get; set; } = new List<MediaGallery>();
}

public class User : IdentityUser<Guid>
{
    public Guid TempleId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Temple? Temple { get; set; }
    public ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
    public ICollection<CourseEnrollment> CourseEnrollments { get; set; } = new List<CourseEnrollment>();
}

public class Event
{
    public Guid Id { get; set; }
    public Guid TempleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public int? MaxRegistrations { get; set; }
    public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }

    // Navigation properties
    public Temple? Temple { get; set; }
    public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
}

public class Course
{
    public Guid Id { get; set; }
    public Guid TempleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Instructor { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? MaxEnrollments { get; set; }
    public string? Location { get; set; }
    public string? MeetingDetails { get; set; }
    public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }

    // Navigation properties
    public Temple? Temple { get; set; }
    public ICollection<CourseEnrollment> Enrollments { get; set; } = new List<CourseEnrollment>();
}

public class TempleTimings
{
    public Guid Id { get; set; }
    public Guid TempleId { get; set; }
    public string AratiName { get; set; } = string.Empty; // Mangala-Aarati, Bhoga-Aarati, etc.
    public TimeSpan Time { get; set; }
    public DayOfWeek? SpecificDay { get; set; } // null = everyday, otherwise specific day
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Temple? Temple { get; set; }
}

public class MediaGallery
{
    public Guid Id { get; set; }
    public Guid TempleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public MediaType Type { get; set; } = MediaType.Image;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Temple? Temple { get; set; }
}

public class EventRegistration
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Registered;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Event? Event { get; set; }
    public User? User { get; set; }
}

public class CourseEnrollment
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Enrolled;
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public Course? Course { get; set; }
    public User? User { get; set; }
}

// Enums
public enum PublishStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2
}

public enum RegistrationStatus
{
    Registered = 0,
    Cancelled = 1,
    Completed = 2
}

public enum EnrollmentStatus
{
    Enrolled = 0,
    Unenrolled = 1,
    Completed = 2
}

public enum MediaType
{
    Image = 0,
    Video = 1
}
