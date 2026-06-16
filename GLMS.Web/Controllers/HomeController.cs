using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly HttpClient _httpClient;

        public HomeController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001");
        }

        private void AddAuthHeader()
        {
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<IActionResult> Index()
        {
            AddAuthHeader();
            try
            {
                var response = await _httpClient.GetAsync("api/dashboard/stats");
                if (response.IsSuccessStatusCode)
                {
                    var stats = await response.Content.ReadFromJsonAsync<DashboardStats>();
                    if (stats != null)
                    {
                        ViewBag.ClientCount = stats.ClientCount;
                        ViewBag.ContractCount = stats.ContractCount;
                        ViewBag.ServiceRequestCount = stats.ServiceRequestCount;
                        ViewBag.ActiveContracts = stats.ActiveContracts;
                    }
                }
            }
            catch
            {
                // Fallback defaults if API is down
                ViewBag.ClientCount = 0;
                ViewBag.ContractCount = 0;
                ViewBag.ServiceRequestCount = 0;
                ViewBag.ActiveContracts = 0;
            }

            return View();
        }

        private class DashboardStats
        {
            public int ClientCount { get; set; }
            public int ContractCount { get; set; }
            public int ServiceRequestCount { get; set; }
            public int ActiveContracts { get; set; }
        }
    }
}
