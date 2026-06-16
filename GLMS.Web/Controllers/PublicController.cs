using System.Net.Http.Json;
using GLMS.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web.Controllers
{
    public class PublicController : Controller
    {
        private readonly HttpClient _httpClient;

        public PublicController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Services()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ServiceInquiry query)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/serviceinquiries", query);
                    if (response.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Thank you. Your logistics query has been submitted successfully.";
                        return RedirectToAction(nameof(Contact));
                    }
                    ModelState.AddModelError("", "Error submitting query to the server.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Failed to connect to backend: {ex.Message}");
                }
            }

            return View(query);
        }
    }
}
