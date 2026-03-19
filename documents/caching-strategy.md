# ISKCON Multi-Site Portal - Caching Strategy

**Last Updated:** March 19, 2026  
**Version:** 1.0  
**Scope:** Website pages (public-facing and registered user views), NOT admin operations

---

## Executive Summary

Caching is a critical performance optimization strategy for the ISKCON portal. This document defines caching policies for website pages to improve response times, reduce database load, and enhance user experience. Admin operations (CRUD, publishing) explicitly bypass cache to ensure data freshness.

**Key Principles:**
- Cache frequently accessed, slow-to-generate content
- Invalidate cache automatically on content changes
- Separate public cache from authenticated user cache
- Never cache sensitive or user-specific data beyond session lifetime
- Monitor cache hit rates and adjust TTLs based on metrics

---

## 1. Caching Architecture

### 1.1 Caching Layers

**Layer 1: HTTP Response Caching (Client-Side)**
- Browser cache: Images, CSS, JavaScript, fonts
- TTL: 30 days (static assets)
- Implementation: HTTP Cache-Control headers, ETag validation

**Layer 2: Output Caching (Server-Side)**
- Full page HTML responses for public views
- Partial view fragments (carousels, hero section)
- TTL: 5-30 minutes (depends on content freshness requirements)
- Implementation: MemoryCache or distributed Redis

**Layer 3: Application-Level Caching (Data Cache)**
- Database query results (courses, events, timings, gallery)
- Computed values (availability counts, status badges)
- TTL: 10-60 minutes
- Implementation: IMemoryCache or IDistributedCache

**Layer 4: Query Optimization (Database-Level)**
- Database indexes on frequently queried columns (TempleId, IsPublished, PublishedDate)
- Connection pooling (reduce connection overhead)
- SQL Server query plan caching (built-in)

---

## 2. Caching by Content Type

### 2.1 Static Assets (Highest cache priority)

**Content:**
- Images (.jpg, .png, .webp, .gif): Hero images, event posters, course thumbnails, gallery media
- CSS files: `/styles/main.css`, `/styles/components.css`
- JavaScript files: `/js/carousel.js`, `/js/form-validation.js`
- Fonts: Outfit (headings), Inter (body)

**Cache Strategy:**
- **HTTP Headers:**
  ```csharp
  app.UseStaticFiles(new StaticFileOptions
  {
      OnPrepareResponse = ctx =>
      {
          // Cache static assets for 30 days
          ctx.Context.Response.Headers.Add("Cache-Control", "public, max-age=2592000, immutable");
          ctx.Context.Response.Headers.Add("ETag", GetFileHash(ctx.File.PhysicalPath)); // For validation
      }
  });
  ```
- **Versioning:** Use fingerprinting (e.g., `main.abc123.css`) to bust cache on updates
- **CDN:** Optional - serve images via CDN for geographic distribution
- **TTL:** 30 days (change filename on update to invalidate)

---

### 2.2 Public Pages (Website Views)

**Pages:**
- `/` (Home/Index)
- `/Courses` (Course Listing)
- `/Programs` (Events/Programs Listing)
- `/Temple/Timings` (Temple timings page)
- `/Gallery` (Media gallery)
- `/Courses/{id}` (Course detail)
- `/Events/{id}` (Event detail)

**Cache Strategy:**
- **Output Caching:** Cache HTML response for public (unauthenticated) visitors
  ```csharp
  [OutputCache(Duration = 600, VaryByRouteData = new[] { "templeId" })]
  public IActionResult Index(Guid templeId)
  {
      var courses = _courseService.GetPublishedCourses(templeId);
      return View(courses);
  }
  ```
- **VaryBy Parameters:** Cache separately by:
  - `templeId` (different temple = different cache key)
  - `page` (pagination)
  - Culture/Language (if multilingual)
  
- **TTL: 10-15 minutes** (balance freshness vs. performance)
  - Home page: 10 minutes (content changes frequently with new events/courses)
  - Listing pages (courses, events): 10 minutes
  - Detail pages (course detail, event detail): 15 minutes (changes less frequently)
  - Timings page: 30 minutes (static, rarely changes)

- **Implementation Options:**
  ```csharp
  // Option 1: OutputCache middleware (ASP.NET Core 7+)
  [OutputCache(Duration = 600)] // 10 minutes
  public IActionResult Courses(Guid templeId)
  {
      var courses = _courseService.GetPublishedCourses(templeId);
      return View(courses);
  }

  // Option 2: Manual IMemoryCache
  public IActionResult Courses(Guid templeId)
  {
      const string cacheKey = $"courses_{templeId}";
      if (!_cache.TryGetValue(cacheKey, out List<CourseDTO> courses))
      {
          courses = _courseService.GetPublishedCourses(templeId);
          var cacheOptions = new MemoryCacheEntryOptions()
              .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
          _cache.Set(cacheKey, courses, cacheOptions);
      }
      return View(courses);
  }
  ```

---

### 2.3 Authenticated User Pages (Conditional Caching)

**Pages:**
- `/Dashboard` (User's enrollments/registrations)
- `/Account/Profile` (User profile)
- Personalized content views

**Cache Strategy:**
- **Do NOT cache** user-specific data (dashboard, profile)
  - Each user has unique content
  - Cache key must include UserId, which is inefficient
  - Frequent updates (user actions)

- **Bypass Output Cache:** Mark with `[OutputCache(NoStore = true)]`
  ```csharp
  [Authorize]
  [OutputCache(NoStore = true)] // Never cache
  public IActionResult Dashboard()
  {
      var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      var enrollments = _enrollmentService.GetUserEnrollments(userId);
      return View(enrollments);
  }
  ```

- **Data Caching Only:** Cache the underlying data (courses/events), not the personalized view
  ```csharp
  [Authorize]
  public IActionResult Dashboard()
  {
      var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      
      // Cache course list (public data)
      const string coursesCacheKey = "all_courses";
      if (!_cache.TryGetValue(coursesCacheKey, out List<CourseDTO> courses))
      {
          courses = _courseService.GetPublishedCourses();
          _cache.Set(coursesCacheKey, courses, TimeSpan.FromMinutes(15));
      }
      
      // Do NOT cache user enrollments (user-specific)
      var enrollments = _enrollmentService.GetUserEnrollments(userId);
      
      return View(new DashboardVM { Courses = courses, Enrollments = enrollments });
  }
  ```

---

### 2.4 Partial Views (Fragment Caching)

**Content:**
- Header navigation (temple dropdown)
- Hero section
- Event carousel
- Course carousel
- Footer
- Filter buttons (gallery)

**Cache Strategy:**
- **Partial Output Caching:** Cache rendered HTML fragments
  ```csharp
  [OutputCache(Duration = 300, VaryByRouteData = new[] { "templeId" })]
  public IActionResult _EventCarousel(Guid templeId)
  {
      var events = _eventService.GetUpcomingEvents(templeId, limit: 3);
      return PartialView(events);
  }

  [OutputCache(Duration = 300, VaryByRouteData = new[] { "templeId" })]
  public IActionResult _CourseCarousel(Guid templeId)
  {
      var courses = _courseService.GetFeaturedCourses(templeId, limit: 3);
      return PartialView(courses);
  }
  ```

- **TTL: 5 minutes** (more frequent updates than full pages)
  - Hero section: 5 minutes
  - Carousels: 5 minutes (reflect new events/courses quickly)
  - Footer: 30 minutes (rarely changes)
  - Header nav: 15 minutes (temple list rarely changes)

---

### 2.5 API Responses (Future, for mobile/SPA)

**Endpoints:**
- `GET /api/temples/{templeId}/courses`
- `GET /api/temples/{templeId}/events`
- `GET /api/temples/{templeId}/timings`
- `GET /api/temples/{templeId}/media`

**Cache Strategy:**
- **HTTP Response Caching:** Include Cache-Control headers
  ```csharp
  [HttpGet("{templeId}/courses")]
  [OutputCache(Duration = 600)] // 10 minutes
  public async Task<IActionResult> GetCourses(Guid templeId)
  {
      Response.Headers.Add("Cache-Control", "public, max-age=600");
      var courses = await _courseService.GetPublishedCourses(templeId);
      return Ok(courses);
  }
  ```

- **ETag Support:** Return 304 Not Modified if resource unchanged
  ```csharp
  var etag = GenerateETag(courses); // Hash of data
  Request.Headers.IfNoneMatch == etag ? NotModified() : Ok(courses);
  ```

- **TTL: 10 minutes** (same as website listings)

---

## 3. Cache Invalidation Strategy

### 3.1 Time-Based Expiration (TTL)
- Simplest approach: Cache expires after N minutes
- Trade-off: Stale data for short period vs. reduced DB queries
- **Recommended TTLs:**
  - Listing pages: 10 minutes
  - Detail pages: 15 minutes
  - Static content (timings, footer): 30 minutes
  - Static assets (images, CSS): 30 days

### 3.2 Event-Based Invalidation (Cache Busting)
**Trigger: When content is published (admin action)**

```csharp
public class AdminEventsController : Controller
{
    private readonly IMemoryCache _cache;
    private readonly IEventService _eventService;

    [HttpPost("publish/{eventId}")]
    public async Task<IActionResult> PublishEvent(Guid eventId, Guid templeId)
    {
        // 1. Publish event in Master DB
        await _eventService.PublishEventAsync(eventId);
        
        // 2. Sync to Web DB
        await _syncService.SyncPublishedEvent(eventId);
        
        // 3. Invalidate cache for this temple
        _cache.Remove($"events_{templeId}");
        _cache.Remove($"event_detail_{eventId}");
        _cache.Remove($"courses_{templeId}"); // Course listing may show related events
        _cache.Remove("all_events");
        
        // 4. Invalidate carousel partials
        _cache.Remove($"eventCarousel_{templeId}");
        
        return Ok(new { message = "Event published and cache cleared" });
    }
}
```

**Cache Invalidation Checklist:**
- When **Event** is published: Clear
  - `events_{templeId}`
  - `event_detail_{eventId}`
  - `page_programs_{templeId}` (list page)
  - `eventCarousel_{templeId}` (partial)
  - `page_home_{templeId}` (may show featured events)

- When **Course** is published: Clear
  - `courses_{templeId}`
  - `course_detail_{courseId}`
  - `page_courses_{templeId}` (list page)
  - `courseCarousel_{templeId}` (partial)
  - `page_home_{templeId}` (may show featured courses)

- When **TempleTimings** are updated: Clear
  - `timings_{templeId}`
  - `page_timings_{templeId}`
  - `statusBadge_{templeId}` (hero section reflects open/close times)

- When **MediaGallery** is updated: Clear
  - `gallery_{templeId}`
  - `page_gallery_{templeId}`
  - `galleryPartial_{templeId}` (partial)

- When **Temple** details change: Clear
  - All caches for that temple (nuclear option)
  - `temple_{templeId}`
  - `events_{templeId}`
  - `courses_{templeId}`
  - `timings_{templeId}`
  - `media_{templeId}`

### 3.3 Distributed Cache Invalidation (Redis)

For multi-server deployments, use Redis for distributed cache:

```csharp
public class CacheInvalidationService
{
    private readonly IDistributedCache _cache;

    public async Task InvalidateTempleCache(Guid templeId)
    {
        var keysToRemove = new[]
        {
            $"events_{templeId}",
            $"courses_{templeId}",
            $"timings_{templeId}",
            $"media_{templeId}"
        };

        foreach (var key in keysToRemove)
        {
            await _cache.RemoveAsync(key);
        }
    }

    public async Task InvalidateEventCache(Guid eventId, Guid templeId)
    {
        await Task.WhenAll(
            _cache.RemoveAsync($"event_detail_{eventId}"),
            _cache.RemoveAsync($"events_{templeId}"),
            _cache.RemoveAsync($"eventCarousel_{templeId}"),
            _cache.RemoveAsync($"page_programs_{templeId}")
        );
    }
}
```

---

## 4. Cache Configuration (Startup)

### 4.1 In-Memory Cache (Development)

```csharp
// Startup.cs / Program.cs
builder.Services.AddMemoryCache();

app.UseOutputCache();
```

### 4.2 Distributed Cache (Production with Redis)

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetConnectionString("Redis");
    options.InstanceName = "iskcon_";
});

builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromMinutes(10)));
    
    // Override for specific routes
    options.AddPolicy("Events", builder =>
        builder
            .WithBasePolicy()
            .Expire(TimeSpan.FromMinutes(10))
            .VaryByRouteValue("templeId"));
            
    options.AddPolicy("Details", builder =>
        builder
            .WithBasePolicy()
            .Expire(TimeSpan.FromMinutes(15))
            .VaryByRouteValue("id"));
});

app.UseOutputCache();
```

### 4.3 appsettings.json

```json
{
  "CachingSettings": {
    "Enabled": true,
    "DefaultDurationMinutes": 10,
    "ContentDurationMinutes": 15,
    "StaticAssetsDurationDays": 30,
    "TextSearchDurationMinutes": 5
  },
  "Redis": {
    "Connection": "localhost:6379,allowAdmin=true"
  }
}
```

---

## 5. Monitoring & Metrics

### 5.1 Cache Hit Rate

Monitor these metrics:
- **Cache Hit Ratio:** (Hits / (Hits + Misses)) × 100
- **Target:** >80% for frequently accessed pages
- **Low hit rate:** Indicates too short TTL or low traffic pattern match

```csharp
public class CacheMetricsService
{
    private long _hits = 0;
    private long _misses = 0;

    public void RecordHit() => Interlocked.Increment(ref _hits);
    public void RecordMiss() => Interlocked.Increment(ref _misses);

    public double GetHitRatio()
    {
        long totalAccess = _hits + _misses;
        return totalAccess == 0 ? 0 : ((double)_hits / totalAccess) * 100;
    }
}
```

### 5.2 Cache Size & Memory Usage

```csharp
// Monitor in Application Insights or custom logging
var cacheSize = GC.GetTotalMemory(false);
_logger.LogInformation($"Cache memory usage: {cacheSize / 1024 / 1024} MB");

// Alert if exceeds threshold
if (cacheSize > 500 * 1024 * 1024) // 500 MB
{
    _logger.LogWarning("Cache size exceeds 500 MB, consider increasing TTL or adding Redis");
}
```

### 5.3 Response Times

Monitor before/after caching:
- **Uncached response:** ~200ms (database query)
- **Cached response:** ~50ms (in-memory cache) or ~100ms (Redis)
- **Target:** 90% of requests served from cache (<100ms)

---

## 6. Cache Bypass Scenarios (High Priority)

**Admin operations** always bypass cache:
- Creating/updating/deleting events, courses, timings
- Publishing/unpublishing content
- Uploading images
- User registration/profile updates
- Role assignment/changes

**Implement with `[OutputCache(NoStore = true)]`:**

```csharp
[Authorize(Roles = "TempleAdmin,Moderator")]
[OutputCache(NoStore = true)] // Never cache admin operations
[HttpPost]
public async Task<IActionResult> CreateEvent(EventDTO dto)
{
    var result = await _eventService.CreateEventAsync(dto);
    // Clear related caches after successful creation
    await _cacheInvalidation.InvalidateTempleCache(dto.TempleId);
    return RedirectToAction("List", new { templeId = dto.TempleId });
}
```

---

## 7. Cache Warming (Optional)

Pre-populate cache on application startup:

```csharp
public class CacheWarmupService : IHostedService
{
    private readonly IMemoryCache _cache;
    private readonly ITempleService _templeService;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var temples = await _templeService.GetAllTemplesAsync();
        
        foreach (var temple in temples)
        {
            // Pre-warm event, course, timings caches
            var events = await _eventService.GetPublishedEvents(temple.Id);
            _cache.Set($"events_{temple.Id}", events, TimeSpan.FromMinutes(10));
            
            var courses = await _courseService.GetPublishedCourses(temple.Id);
            _cache.Set($"courses_{temple.Id}", courses, TimeSpan.FromMinutes(10));
            
            var timings = await _templeService.GetTimings(temple.Id);
            _cache.Set($"timings_{temple.Id}", timings, TimeSpan.FromMinutes(30));
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

// Register in Startup
builder.Services.AddHostedService<CacheWarmupService>();
```

---

## 8. Performance Targets

| Metric | Target | Current |
|--------|--------|---------|
| Home page load time | <2s | TBD |
| Listing pages (courses, events) | <2s | TBD |
| Detail pages | <2s | TBD |
| Cache hit ratio | >80% | TBD |
| API response time | <500ms | TBD |
| Time to first byte (TTFB) | <200ms | TBD |

---

## 9. Cache Testing Checklist

- [ ] Verify page loads correctly from cache (no missing content)
- [ ] Verify cache expires and refreshes after TTL
- [ ] Verify cache invalidates on publish action
- [ ] Verify authenticated users see fresh data (no cross-user cache leak)
- [ ] Verify admin operations don't return cached data
- [ ] Test memory usage with full cache load
- [ ] Test Redis failover (if using distributed cache)
- [ ] Verify hard refresh (Ctrl+Shift+R) bypasses browser cache
- [ ] Monitor cache hit rates in production
- [ ] Performance testing before/after caching enabled

---

## 10. Implementation Priority

**Phase 1 (MVP):**
- [ ] HTTP response caching for public pages (10-15 min TTL)
- [ ] Static asset cache-control headers (30 days)
- [ ] Time-based expiration only (no event-based invalidation yet)
- [ ] Memory cache (in-process)

**Phase 2 (Optimization):**
- [ ] Event-based cache invalidation on publish
- [ ] Distributed cache (Redis) for multi-server deployments
- [ ] Fragment caching (carousels, hero section)
- [ ] Cache warming on startup

**Phase 3 (Advanced):**
- [ ] Cache hit rate monitoring
- [ ] Dynamic TTL adjustment based on traffic patterns
- [ ] Cache compression (if exceeding size limits)
- [ ] Geo-distributed cache (CDN for images)

---

**Document Status:** APPROVED FOR IMPLEMENTATION  
**Next Review Date:** June 19, 2026