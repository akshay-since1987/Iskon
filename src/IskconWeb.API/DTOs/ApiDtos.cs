namespace IskconWeb.API.DTOs;

// Temple DTOs
public class TempleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// Event DTOs
public class EventDto
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
    public string PublishStatus { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
}

public class CreateEventDto
{
    public Guid TempleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public int? MaxRegistrations { get; set; }
}

public class UpdateEventDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public int? MaxRegistrations { get; set; }
}

// Course DTOs
public class CourseDto
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
    public string PublishStatus { get; set; } = string.Empty;
    public DateTime? PublishedAt { get; set; }
}

public class CreateCourseDto
{
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
}

public class UpdateCourseDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Instructor { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? MaxEnrollments { get; set; }
    public string? Location { get; set; }
    public string? MeetingDetails { get; set; }
}

// Temple Timings DTOs
public class TempleTimingsDto
{
    public Guid Id { get; set; }
    public Guid TempleId { get; set; }
    public string AratiName { get; set; } = string.Empty;
    public TimeSpan Time { get; set; }
    public int? SpecificDay { get; set; }
}

public class CreateTempleTimingsDto
{
    public Guid TempleId { get; set; }
    public string AratiName { get; set; } = string.Empty;
    public TimeSpan Time { get; set; }
    public int? SpecificDay { get; set; }
}

public class UpdateTempleTimingsDto
{
    public string AratiName { get; set; } = string.Empty;
    public TimeSpan Time { get; set; }
    public int? SpecificDay { get; set; }
}

// Media Gallery DTOs
public class MediaGalleryDto
{
    public Guid Id { get; set; }
    public Guid TempleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class CreateMediaGalleryDto
{
    public Guid TempleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MediaUrl { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

// Auth DTOs
public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Guid TempleId { get; set; }
}

public class AuthResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public UserDto? User { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid TempleId { get; set; }
}

// Generic Response DTOs
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
