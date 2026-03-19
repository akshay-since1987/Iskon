using IskconWeb.API.DTOs;
using IskconWeb.Core.Data;
using IskconWeb.Core.Models;
using IskconWeb.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IskconWeb.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplesController(
    IskonMasterDbContext context,
    ILogger<TemplesController> logger) : ControllerBase
{
    /// <summary>
    /// Get all temples
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TempleDto>>> GetTemples()
    {
        try
        {
            var temples = await context.Temples
                .AsNoTracking()
                .ToListAsync();

            var dtos = temples.Select(t => new TempleDto
            {
                Id = t.Id,
                Name = t.Name,
                City = t.City,
                Address = t.Address,
                Phone = t.Phone,
                Email = t.Email
            }).ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving temples");
            return StatusCode(500, new { message = "Error retrieving temples" });
        }
    }

    /// <summary>
    /// Get temple by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TempleDto>> GetTemple(Guid id)
    {
        try
        {
            var temple = await context.Temples
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (temple == null)
                return NotFound(new { message = "Temple not found" });

            var dto = new TempleDto
            {
                Id = temple.Id,
                Name = temple.Name,
                City = temple.City,
                Address = temple.Address,
                Phone = temple.Phone,
                Email = temple.Email
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving temple {TempleId}", id);
            return StatusCode(500, new { message = "Error retrieving temple" });
        }
    }

    /// <summary>
    /// Get temple with all related data (events, courses, timings)
    /// </summary>
    [HttpGet("{id}/details")]
    public async Task<ActionResult<dynamic>> GetTempleDetails(Guid id)
    {
        try
        {
            var temple = await context.Temples
                .AsNoTracking()
                .Include(t => t.Events.Where(e => e.PublishStatus == PublishStatus.Published))
                .Include(t => t.Courses.Where(c => c.PublishStatus == PublishStatus.Published))
                .Include(t => t.Timings)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (temple == null)
                return NotFound(new { message = "Temple not found" });

            var response = new
            {
                temple = new TempleDto
                {
                    Id = temple.Id,
                    Name = temple.Name,
                    City = temple.City,
                    Address = temple.Address,
                    Phone = temple.Phone,
                    Email = temple.Email
                },
                events = temple.Events.Select(e => new EventDto
                {
                    Id = e.Id,
                    TempleId = e.TempleId,
                    Title = e.Title,
                    Description = e.Description,
                    ImageUrl = e.ImageUrl,
                    EventDate = e.EventDate,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Location = e.Location,
                    MaxRegistrations = e.MaxRegistrations,
                    PublishStatus = e.PublishStatus.ToString(),
                    PublishedAt = e.PublishedAt
                }).ToList(),
                courses = temple.Courses.Select(c => new CourseDto
                {
                    Id = c.Id,
                    TempleId = c.TempleId,
                    Title = c.Title,
                    Description = c.Description,
                    ImageUrl = c.ImageUrl,
                    Instructor = c.Instructor,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    MaxEnrollments = c.MaxEnrollments,
                    Location = c.Location,
                    MeetingDetails = c.MeetingDetails,
                    PublishStatus = c.PublishStatus.ToString(),
                    PublishedAt = c.PublishedAt
                }).ToList(),
                timings = temple.Timings.Select(t => new TempleTimingsDto
                {
                    Id = t.Id,
                    TempleId = t.TempleId,
                    AratiName = t.AratiName,
                    Time = t.Time,
                    SpecificDay = t.SpecificDay.HasValue ? (int)t.SpecificDay.Value : null
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving temple details for {TempleId}", id);
            return StatusCode(500, new { message = "Error retrieving temple details" });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class EventsController(
    IskonMasterDbContext context,
    ContentSyncService syncService,
    ILogger<EventsController> logger) : ControllerBase
{
    /// <summary>
    /// Get all published events for a temple
    /// </summary>
    [HttpGet("temple/{templeId}")]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetEventsByTemple(Guid templeId)
    {
        try
        {
            var events = await context.Events
                .AsNoTracking()
                .Where(e => e.TempleId == templeId && e.PublishStatus == PublishStatus.Published)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();

            var dtos = events.Select(EventToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving events for temple {TempleId}", templeId);
            return StatusCode(500, new { message = "Error retrieving events" });
        }
    }

    /// <summary>
    /// Get event by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<EventDto>> GetEvent(Guid id)
    {
        try
        {
            var @event = await context.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (@event == null)
                return NotFound(new { message = "Event not found" });

            return Ok(EventToDto(@event));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving event {EventId}", id);
            return StatusCode(500, new { message = "Error retrieving event" });
        }
    }

    /// <summary>
    /// Create a new event (admin only)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<EventDto>>> CreateEvent(CreateEventDto dto)
    {
        try
        {
            var @event = new Event
            {
                Id = Guid.NewGuid(),
                TempleId = dto.TempleId,
                Title = dto.Title,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                EventDate = dto.EventDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Location = dto.Location,
                MaxRegistrations = dto.MaxRegistrations,
                PublishStatus = PublishStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Events.Add(@event);
            await context.SaveChangesAsync();

            logger.LogInformation("Event created: {EventId}", @event.Id);

            return CreatedAtAction(nameof(GetEvent), new { id = @event.Id },
                new ApiResponse<EventDto>
                {
                    Success = true,
                    Message = "Event created successfully",
                    Data = EventToDto(@event)
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating event");
            return StatusCode(500, new ApiResponse { Success = false, Message = "Error creating event" });
        }
    }

    /// <summary>
    /// Update event
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<EventDto>>> UpdateEvent(Guid id, UpdateEventDto dto)
    {
        try
        {
            var @event = await context.Events.FindAsync(id);
            if (@event == null)
                return NotFound(new { message = "Event not found" });

            @event.Title = dto.Title;
            @event.Description = dto.Description;
            @event.ImageUrl = dto.ImageUrl;
            @event.EventDate = dto.EventDate;
            @event.StartTime = dto.StartTime;
            @event.EndTime = dto.EndTime;
            @event.Location = dto.Location;
            @event.MaxRegistrations = dto.MaxRegistrations;
            @event.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            logger.LogInformation("Event updated: {EventId}", id);

            return Ok(new ApiResponse<EventDto>
            {
                Success = true,
                Message = "Event updated successfully",
                Data = EventToDto(@event)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating event {EventId}", id);
            return StatusCode(500, new ApiResponse { Success = false, Message = "Error updating event" });
        }
    }

    /// <summary>
    /// Publish event to public website
    /// </summary>
    [HttpPost("{id}/publish")]
    public async Task<ActionResult<ApiResponse>> PublishEvent(Guid id)
    {
        try
        {
            var @event = await context.Events.FindAsync(id);
            if (@event == null)
                return NotFound(new { message = "Event not found" });

            @event.PublishStatus = PublishStatus.Published;
            @event.PublishedAt = DateTime.UtcNow;
            @event.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // Sync to web database
            await syncService.SyncEventAsync(id);

            logger.LogInformation("Event published: {EventId}", id);

            return Ok(new ApiResponse { Success = true, Message = "Event published successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing event {EventId}", id);
            return StatusCode(500, new ApiResponse { Success = false, Message = "Error publishing event" });
        }
    }

    /// <summary>
    /// Delete event
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteEvent(Guid id)
    {
        try
        {
            var @event = await context.Events.FindAsync(id);
            if (@event == null)
                return NotFound(new { message = "Event not found" });

            context.Events.Remove(@event);
            await context.SaveChangesAsync();

            // Remove from web if published
            if (@event.PublishStatus == PublishStatus.Published)
            {
                await syncService.RemoveUnpublishedContentAsync(id, "event");
            }

            logger.LogInformation("Event deleted: {EventId}", id);

            return Ok(new ApiResponse { Success = true, Message = "Event deleted successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting event {EventId}", id);
            return StatusCode(500, new ApiResponse { Success = false, Message = "Error deleting event" });
        }
    }

    private static EventDto EventToDto(Event e) => new()
    {
        Id = e.Id,
        TempleId = e.TempleId,
        Title = e.Title,
        Description = e.Description,
        ImageUrl = e.ImageUrl,
        EventDate = e.EventDate,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        Location = e.Location,
        MaxRegistrations = e.MaxRegistrations,
        PublishStatus = e.PublishStatus.ToString(),
        PublishedAt = e.PublishedAt
    };
}

[ApiController]
[Route("api/[controller]")]
public class CoursesController(
    IskonMasterDbContext context,
    ContentSyncService syncService,
    ILogger<CoursesController> logger) : ControllerBase
{
    /// <summary>
    /// Get all published courses for a temple
    /// </summary>
    [HttpGet("temple/{templeId}")]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetCoursesByTemple(Guid templeId)
    {
        try
        {
            var courses = await context.Courses
                .AsNoTracking()
                .Where(c => c.TempleId == templeId && c.PublishStatus == PublishStatus.Published)
                .OrderBy(c => c.StartDate)
                .ToListAsync();

            var dtos = courses.Select(CourseToDto).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving courses for temple {TempleId}", templeId);
            return StatusCode(500, new { message = "Error retrieving courses" });
        }
    }

    /// <summary>
    /// Get course by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CourseDto>> GetCourse(Guid id)
    {
        try
        {
            var course = await context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
                return NotFound(new { message = "Course not found" });

            return Ok(CourseToDto(course));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving course {CourseId}", id);
            return StatusCode(500, new { message = "Error retrieving course" });
        }
    }

    /// <summary>
    /// Create a new course
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CourseDto>>> CreateCourse(CreateCourseDto dto)
    {
        try
        {
            var course = new Course
            {
                Id = Guid.NewGuid(),
                TempleId = dto.TempleId,
                Title = dto.Title,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                Instructor = dto.Instructor,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                MaxEnrollments = dto.MaxEnrollments,
                Location = dto.Location,
                MeetingDetails = dto.MeetingDetails,
                PublishStatus = PublishStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Courses.Add(course);
            await context.SaveChangesAsync();

            logger.LogInformation("Course created: {CourseId}", course.Id);

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id },
                new ApiResponse<CourseDto>
                {
                    Success = true,
                    Message = "Course created successfully",
                    Data = CourseToDto(course)
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating course");
            return StatusCode(500, new ApiResponse { Success = false, Message = "Error creating course" });
        }
    }

    /// <summary>
    /// Update course
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CourseDto>>> UpdateCourse(Guid id, UpdateCourseDto dto)
    {
        try
        {
            var course = await context.Courses.FindAsync(id);
            if (course == null)
                return NotFound(new { message = "Course not found" });

            course.Title = dto.Title;
            course.Description = dto.Description;
            course.ImageUrl = dto.ImageUrl;
            course.Instructor = dto.Instructor;
            course.StartDate = dto.StartDate;
            course.EndDate = dto.EndDate;
            course.MaxEnrollments = dto.MaxEnrollments;
            course.Location = dto.Location;
            course.MeetingDetails = dto.MeetingDetails;
            course.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            logger.LogInformation("Course updated: {CourseId}", id);

            return Ok(new ApiResponse<CourseDto>
            {
                Success = true,
                Message = "Course updated successfully",
                Data = CourseToDto(course)
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating course {CourseId}", id);
            return StatusCode(500, new ApiResponse { Success = false, Message = "Error updating course" });
        }
    }

    /// <summary>
    /// Publish course
    /// </summary>
    [HttpPost("{id}/publish")]
    public async Task<ActionResult<ApiResponse>> PublishCourse(Guid id)
    {
        try
        {
            var course = await context.Courses.FindAsync(id);
            if (course == null)
                return NotFound(new { message = "Course not found" });

            course.PublishStatus = PublishStatus.Published;
            course.PublishedAt = DateTime.UtcNow;
            course.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            // Sync to web database
            await syncService.SyncCourseAsync(id);

            logger.LogInformation("Course published: {CourseId}", id);

            return Ok(new ApiResponse { Success = true, Message = "Course published successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing course {CourseId}", id);
            return StatusCode(500, new ApiResponse { Success = false, Message = "Error publishing course" });
        }
    }

    /// <summary>
    /// Delete course
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteCourse(Guid id)
    {
        try
        {
            var course = await context.Courses.FindAsync(id);
            if (course == null)
                return NotFound(new { message = "Course not found" });

            context.Courses.Remove(course);
            await context.SaveChangesAsync();

            // Remove from web if published
            if (course.PublishStatus == PublishStatus.Published)
            {
                await syncService.RemoveUnpublishedContentAsync(id, "course");
            }

            logger.LogInformation("Course deleted: {CourseId}", id);

            return Ok(new ApiResponse { Success = true, Message = "Course deleted successfully" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting course {CourseId}", id);
            return StatusCode(500, new ApiResponse { Success = false, Message = "Error deleting course" });
        }
    }

    private static CourseDto CourseToDto(Course c) => new()
    {
        Id = c.Id,
        TempleId = c.TempleId,
        Title = c.Title,
        Description = c.Description,
        ImageUrl = c.ImageUrl,
        Instructor = c.Instructor,
        StartDate = c.StartDate,
        EndDate = c.EndDate,
        MaxEnrollments = c.MaxEnrollments,
        Location = c.Location,
        MeetingDetails = c.MeetingDetails,
        PublishStatus = c.PublishStatus.ToString(),
        PublishedAt = c.PublishedAt
    };
}
