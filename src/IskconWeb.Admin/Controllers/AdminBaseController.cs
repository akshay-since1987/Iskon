using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IskconWeb.Admin.Controllers;

/// <summary>
/// Base controller for all admin controllers. Requires cookie authentication and
/// provides a helper that creates an HttpClient pre-loaded with the JWT from session.
/// </summary>
[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
public abstract class AdminBaseController : Controller
{
    protected HttpClient CreateAuthorizedClient(IHttpClientFactory factory)
    {
        var client = factory.CreateClient("ApiClient");
        var token = HttpContext.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
