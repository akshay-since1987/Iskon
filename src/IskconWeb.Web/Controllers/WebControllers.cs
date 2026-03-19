using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace IskconWeb.Web.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;

    public HomeController(
        IHttpClientFactory httpClientFactory,
        ILogger<HomeController> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync("/api/temples");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                ViewBag.TemplesJson = content;
            }

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading home page");
            return View();
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}

public class CoursesController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CoursesController> _logger;
    private readonly IConfiguration _configuration;

    public CoursesController(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<CoursesController> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index(Guid? templeId)
    {
        try
        {
            if (!templeId.HasValue)
            {
                // Get first temple to load
                var temples = await GetCachedTemples();
                if (temples != null && temples.Count > 0)
                {
                    templeId = temples.First().Value;
                }
            }

            if (!templeId.HasValue)
                return RedirectToAction("Index", "Home");

            var cacheKey = $"courses_temple_{templeId}";
            var cacheDuration = _configuration.GetValue<int>("CacheConfig:CourseCacheDurationMinutes", 30);

            if (!_cache.TryGetValue(cacheKey, out List<dynamic>? courses))
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                var response = await client.GetAsync($"/api/courses/temple/{templeId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ViewBag.CoursesJson = content;

                    _cache.Set(cacheKey, content, TimeSpan.FromMinutes(cacheDuration));
                }
            }

            ViewBag.TempleId = templeId;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading courses");
            return View();
        }
    }

    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/courses/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                ViewBag.CourseJson = content;
            }

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading course details for {CourseId}", id);
            return View();
        }
    }

    private async Task<Dictionary<string, Guid>?> GetCachedTemples()
    {
        const string cacheKey = "all_temples";

        if (!_cache.TryGetValue(cacheKey, out Dictionary<string, Guid>? temples))
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            try
            {
                var response = await client.GetAsync("/api/temples");
                if (response.IsSuccessStatusCode)
                {
                    // Parse and cache temples
                    var cacheDuration = _configuration.GetValue<int>("CacheConfig:TempleCacheDurationMinutes", 60);
                    _cache.Set(cacheKey, temples, TimeSpan.FromMinutes(cacheDuration));
                    return temples;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching temples");
            }
        }

        return temples;
    }
}

public class EventsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<EventsController> _logger;
    private readonly IConfiguration _configuration;

    public EventsController(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<EventsController> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index(Guid? templeId)
    {
        try
        {
            if (!templeId.HasValue)
            {
                // Get first temple to load
                templeId = new Guid("11111111-1111-1111-1111-111111111111"); // Default to Dhule
            }

            var cacheKey = $"events_temple_{templeId}";
            var cacheDuration = _configuration.GetValue<int>("CacheConfig:EventCacheDurationMinutes", 15);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/events/temple/{templeId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                ViewBag.EventsJson = content;

                _cache.Set(cacheKey, content, TimeSpan.FromMinutes(cacheDuration));
            }

            ViewBag.TempleId = templeId;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading events");
            return View();
        }
    }

    public async Task<IActionResult> Details(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/events/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                ViewBag.EventJson = content;
            }

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading event details for {EventId}", id);
            return View();
        }
    }
}

public class GalleryController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GalleryController> _logger;

    public GalleryController(
        IHttpClientFactory httpClientFactory,
        ILogger<GalleryController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index(Guid templeId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/temples/{templeId}/details");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                ViewBag.TempleDataJson = content;
            }

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading gallery for temple {TempleId}", templeId);
            return View();
        }
    }
}

public class TempleTimingsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TempleTimingsController> _logger;
    private readonly IConfiguration _configuration;

    public TempleTimingsController(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<TempleTimingsController> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index(Guid templeId)
    {
        try
        {
            var cacheKey = $"timings_temple_{templeId}";
            var cacheDuration = _configuration.GetValue<int>("CacheConfig:TempleCacheDurationMinutes", 60);

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/temples/{templeId}/details");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                ViewBag.TempleDataJson = content;

                _cache.Set(cacheKey, content, TimeSpan.FromMinutes(cacheDuration));
            }

            ViewBag.TempleId = templeId;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading timings for temple {TempleId}", templeId);
            return View();
        }
    }
}
