using Microsoft.AspNetCore.Mvc;

namespace IskconWeb.Admin.Controllers;

public class DashboardController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IHttpClientFactory httpClientFactory,
        ILogger<DashboardController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            
            // Get temples count
            var templesResponse = await client.GetAsync("/api/temples");
            int templesCount = 0;
            
            if (templesResponse.IsSuccessStatusCode)
            {
                var content = await templesResponse.Content.ReadAsStringAsync();
                ViewBag.TemplesJson = content;
                // Simple count estimation from JSON
                templesCount = content.Split("\"Id\"").Length - 1;
            }

            ViewBag.TemplesCount = templesCount;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading admin dashboard");
            return View();
        }
    }
}

public class EventsAdminController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EventsAdminController> _logger;
    private readonly IConfiguration _configuration;

    public EventsAdminController(
        IHttpClientFactory httpClientFactory,
        ILogger<EventsAdminController> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index(Guid? templeId)
    {
        try
        {
            if (!templeId.HasValue)
            {
                var defaultTempleId = _configuration["AdminConfig:DefaultTempleId"];
                templeId = Guid.Parse(defaultTempleId ?? "11111111-1111-1111-1111-111111111111");
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/events/temple/{templeId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                ViewBag.EventsJson = content;
            }

            ViewBag.TempleId = templeId;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading events for admin");
            return View();
        }
    }

    public async Task<IActionResult> Create(Guid templeId)
    {
        ViewBag.TempleId = templeId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] dynamic eventData)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var json = System.Text.Json.JsonSerializer.Serialize(eventData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/events", content);

            if (response.IsSuccessStatusCode)
            {
                var templeId = ((dynamic)eventData)?.TempleId;
                return RedirectToAction("Index", new { templeId });
            }

            ModelState.AddModelError("", "Failed to create event");
            return View(eventData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            ModelState.AddModelError("", "An error occurred while creating the event");
            return View(eventData);
        }
    }

    public async Task<IActionResult> Edit(Guid id, Guid templeId)
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

            ViewBag.TempleId = templeId;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading event for edit");
            return RedirectToAction("Index", new { templeId });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, [FromBody] dynamic eventData)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var json = System.Text.Json.JsonSerializer.Serialize(eventData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/api/events/{id}", content);

            if (response.IsSuccessStatusCode)
            {
                var templeId = ((dynamic)eventData)?.TempleId;
                return RedirectToAction("Index", new { templeId });
            }

            ModelState.AddModelError("", "Failed to update event");
            return View(eventData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event");
            ModelState.AddModelError("", "An error occurred while updating the event");
            return View(eventData);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Publish(Guid id, Guid templeId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsync($"/api/events/{id}/publish", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Event published successfully";
                return RedirectToAction("Index", new { templeId });
            }

            TempData["Error"] = "Failed to publish event";
            return RedirectToAction("Index", new { templeId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event");
            TempData["Error"] = "An error occurred while publishing the event";
            return RedirectToAction("Index", new { templeId });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id, Guid templeId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.DeleteAsync($"/api/events/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Event deleted successfully";
                return RedirectToAction("Index", new { templeId });
            }

            TempData["Error"] = "Failed to delete event";
            return RedirectToAction("Index", new { templeId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event");
            TempData["Error"] = "An error occurred while deleting the event";
            return RedirectToAction("Index", new { templeId });
        }
    }
}

public class CoursesAdminController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<CoursesAdminController> _logger;
    private readonly IConfiguration _configuration;

    public CoursesAdminController(
        IHttpClientFactory httpClientFactory,
        ILogger<CoursesAdminController> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index(Guid? templeId)
    {
        try
        {
            if (!templeId.HasValue)
            {
                var defaultTempleId = _configuration["AdminConfig:DefaultTempleId"];
                templeId = Guid.Parse(defaultTempleId ?? "11111111-1111-1111-1111-111111111111");
            }

            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.GetAsync($"/api/courses/temple/{templeId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                ViewBag.CoursesJson = content;
            }

            ViewBag.TempleId = templeId;
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading courses for admin");
            return View();
        }
    }

    public async Task<IActionResult> Create(Guid templeId)
    {
        ViewBag.TempleId = templeId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] dynamic courseData)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var json = System.Text.Json.JsonSerializer.Serialize(courseData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/api/courses", content);

            if (response.IsSuccessStatusCode)
            {
                var templeId = ((dynamic)courseData)?.TempleId;
                return RedirectToAction("Index", new { templeId });
            }

            ModelState.AddModelError("", "Failed to create course");
            return View(courseData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating course");
            ModelState.AddModelError("", "An error occurred while creating the course");
            return View(courseData);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Publish(Guid id, Guid templeId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ApiClient");
            var response = await client.PostAsync($"/api/courses/{id}/publish", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Course published successfully";
                return RedirectToAction("Index", new { templeId });
            }

            TempData["Error"] = "Failed to publish course";
            return RedirectToAction("Index", new { templeId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing course");
            TempData["Error"] = "An error occurred while publishing the course";
            return RedirectToAction("Index", new { templeId });
        }
    }
}
