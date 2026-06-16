using System.Net.Http.Headers;
using System.Net.Http.Json;
using GLMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web.Controllers
{
    [Authorize]
    public class ServiceInquiriesController : Controller
    {
        private readonly HttpClient _httpClient;

        public ServiceInquiriesController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
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
            var response = await _httpClient.GetAsync("api/serviceinquiries");
            if (response.IsSuccessStatusCode)
            {
                var queries = await response.Content.ReadFromJsonAsync<List<ServiceInquiry>>();
                return View(queries ?? new List<ServiceInquiry>());
            }
            return View(new List<ServiceInquiry>());
        }

        public async Task<IActionResult> Delete(int id)
        {
            AddAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/serviceinquiries/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Customer query deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting customer query.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}