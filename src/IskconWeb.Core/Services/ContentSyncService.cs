using IskconWeb.Core.Data;
using IskconWeb.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IskconWeb.Core.Services;

/// <summary>
/// Syncs published content from Master database to Web database
/// Triggered when content is published or updated
/// Web database is read-only optimized for public website
/// </summary>
public class ContentSyncService
{
    private readonly IskonMasterDbContext _masterContext;
    private readonly IskonWebDbContext _webContext;
    private readonly ILogger<ContentSyncService> _logger;

    public ContentSyncService(
        IskonMasterDbContext masterContext,
        IskonWebDbContext webContext,
        ILogger<ContentSyncService> logger)
    {
        _masterContext = masterContext;
        _webContext = webContext;
        _logger = logger;
    }

    /// <summary>
    /// Syncs all published content from Master to Web database
    /// </summary>
    public async Task SyncAllPublishedContentAsync()
    {
        try
        {
            _logger.LogInformation("Starting full content sync from Master to Web database");

            // Sync temples
            await SyncTemplesAsync();

            // Sync published events
            await SyncPublishedEventsAsync();

            // Sync published courses
            await SyncPublishedCoursesAsync();

            // Sync temple timings
            await SyncTempleTimingsAsync();

            // Sync media gallery
            await SyncMediaGalleryAsync();

            _logger.LogInformation("Content sync completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during content sync");
            throw;
        }
    }

    /// <summary>
    /// Syncs a specific published event to Web database
    /// </summary>
    public async Task SyncEventAsync(Guid eventId)
    {
        try
        {
            var masterEvent = await _masterContext.Events
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == eventId && e.PublishStatus == PublishStatus.Published);

            if (masterEvent == null)
            {
                _logger.LogWarning("Event {EventId} not found or not published", eventId);
                return;
            }

            var webEvent = await _webContext.Events
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (webEvent == null)
            {
                _webContext.Events.Add(masterEvent);
            }
            else
            {
                _webContext.Events.Update(masterEvent);
            }

            await _webContext.SaveChangesAsync();
            _logger.LogInformation("Event {EventId} synced successfully", eventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing event {EventId}", eventId);
            throw;
        }
    }

    /// <summary>
    /// Syncs a specific published course to Web database
    /// </summary>
    public async Task SyncCourseAsync(Guid courseId)
    {
        try
        {
            var masterCourse = await _masterContext.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == courseId && c.PublishStatus == PublishStatus.Published);

            if (masterCourse == null)
            {
                _logger.LogWarning("Course {CourseId} not found or not published", courseId);
                return;
            }

            var webCourse = await _webContext.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (webCourse == null)
            {
                _webContext.Courses.Add(masterCourse);
            }
            else
            {
                _webContext.Courses.Update(masterCourse);
            }

            await _webContext.SaveChangesAsync();
            _logger.LogInformation("Course {CourseId} synced successfully", courseId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing course {CourseId}", courseId);
            throw;
        }
    }

    /// <summary>
    /// Removes unpublished content from Web database
    /// </summary>
    public async Task RemoveUnpublishedContentAsync(Guid contentId, string contentType)
    {
        try
        {
            switch (contentType.ToLower())
            {
                case "event":
                    var webEvent = await _webContext.Events.FirstOrDefaultAsync(e => e.Id == contentId);
                    if (webEvent != null)
                    {
                        _webContext.Events.Remove(webEvent);
                        await _webContext.SaveChangesAsync();
                        _logger.LogInformation("Event {EventId} removed from Web database", contentId);
                    }
                    break;

                case "course":
                    var webCourse = await _webContext.Courses.FirstOrDefaultAsync(c => c.Id == contentId);
                    if (webCourse != null)
                    {
                        _webContext.Courses.Remove(webCourse);
                        await _webContext.SaveChangesAsync();
                        _logger.LogInformation("Course {CourseId} removed from Web database", contentId);
                    }
                    break;

                default:
                    _logger.LogWarning("Unknown content type for removal: {ContentType}", contentType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing {ContentType} {ContentId} from Web database", contentType, contentId);
            throw;
        }
    }

    private async Task SyncTemplesAsync()
    {
        _logger.LogInformation("Syncing temples...");
        var masterTemples = await _masterContext.Temples.AsNoTracking().ToListAsync();
        var webTemples = await _webContext.Temples.ToListAsync();

        foreach (var masterTemple in masterTemples)
        {
            var webTemple = webTemples.FirstOrDefault(t => t.Id == masterTemple.Id);
            if (webTemple == null)
            {
                _webContext.Temples.Add(masterTemple);
            }
            else
            {
                _webContext.Temples.Update(masterTemple);
            }
        }

        await _webContext.SaveChangesAsync();
        _logger.LogInformation("Temples synced: {Count}", masterTemples.Count);
    }

    private async Task SyncPublishedEventsAsync()
    {
        _logger.LogInformation("Syncing published events...");
        var publishedEvents = await _masterContext.Events
            .AsNoTracking()
            .Where(e => e.PublishStatus == PublishStatus.Published)
            .ToListAsync();

        var webEvents = await _webContext.Events.ToListAsync();

        foreach (var masterEvent in publishedEvents)
        {
            var webEvent = webEvents.FirstOrDefault(e => e.Id == masterEvent.Id);
            if (webEvent == null)
            {
                _webContext.Events.Add(masterEvent);
            }
            else
            {
                _webContext.Events.Update(masterEvent);
            }
        }

        // Remove unpublished events from web
        var publishedIds = publishedEvents.Select(e => e.Id).ToList();
        var orphanedWebEvents = webEvents.Where(e => !publishedIds.Contains(e.Id)).ToList();
        _webContext.Events.RemoveRange(orphanedWebEvents);

        await _webContext.SaveChangesAsync();
        _logger.LogInformation("Events synced: {PublishedCount} published, {RemovedCount} removed",
            publishedEvents.Count, orphanedWebEvents.Count);
    }

    private async Task SyncPublishedCoursesAsync()
    {
        _logger.LogInformation("Syncing published courses...");
        var publishedCourses = await _masterContext.Courses
            .AsNoTracking()
            .Where(c => c.PublishStatus == PublishStatus.Published)
            .ToListAsync();

        var webCourses = await _webContext.Courses.ToListAsync();

        foreach (var masterCourse in publishedCourses)
        {
            var webCourse = webCourses.FirstOrDefault(c => c.Id == masterCourse.Id);
            if (webCourse == null)
            {
                _webContext.Courses.Add(masterCourse);
            }
            else
            {
                _webContext.Courses.Update(masterCourse);
            }
        }

        // Remove unpublished courses from web
        var publishedIds = publishedCourses.Select(c => c.Id).ToList();
        var orphanedWebCourses = webCourses.Where(c => !publishedIds.Contains(c.Id)).ToList();
        _webContext.Courses.RemoveRange(orphanedWebCourses);

        await _webContext.SaveChangesAsync();
        _logger.LogInformation("Courses synced: {PublishedCount} published, {RemovedCount} removed",
            publishedCourses.Count, orphanedWebCourses.Count);
    }

    private async Task SyncTempleTimingsAsync()
    {
        _logger.LogInformation("Syncing temple timings...");
        var masterTimings = await _masterContext.TempleTimings.AsNoTracking().ToListAsync();
        var webTimings = await _webContext.TempleTimings.ToListAsync();

        foreach (var masterTiming in masterTimings)
        {
            var webTiming = webTimings.FirstOrDefault(t => t.Id == masterTiming.Id);
            if (webTiming == null)
            {
                _webContext.TempleTimings.Add(masterTiming);
            }
            else
            {
                _webContext.TempleTimings.Update(masterTiming);
            }
        }

        await _webContext.SaveChangesAsync();
        _logger.LogInformation("Temple timings synced: {Count}", masterTimings.Count);
    }

    private async Task SyncMediaGalleryAsync()
    {
        _logger.LogInformation("Syncing media gallery...");
        var masterMedias = await _masterContext.MediaGalleries.AsNoTracking().ToListAsync();
        var webMedias = await _webContext.MediaGalleries.ToListAsync();

        foreach (var masterMedia in masterMedias)
        {
            var webMedia = webMedias.FirstOrDefault(m => m.Id == masterMedia.Id);
            if (webMedia == null)
            {
                _webContext.MediaGalleries.Add(masterMedia);
            }
            else
            {
                _webContext.MediaGalleries.Update(masterMedia);
            }
        }

        await _webContext.SaveChangesAsync();
        _logger.LogInformation("Media gallery items synced: {Count}", masterMedias.Count);
    }
}
