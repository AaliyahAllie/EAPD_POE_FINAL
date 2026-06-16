using System.Net.Http.Headers;
using System.Net.Http.Json;
using GLMS.Shared.Models;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GLMS.Web.Controllers
{
    [Authorize]
    public class ServiceRequestsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly ICurrencyService _currencyService;

        public ServiceRequestsController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ICurrencyService currencyService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001");
            _currencyService = currencyService;
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
            var response = await _httpClient.GetAsync("api/servicerequests");
            if (response.IsSuccessStatusCode)
            {
                var requests = await response.Content.ReadFromJsonAsync<List<ServiceRequest>>();
                return View(requests ?? new List<ServiceRequest>());
            }
            return View(new List<ServiceRequest>());
        }

        public async Task<IActionResult> Details(int id)
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync($"api/servicerequests/{id}");
            if (response.IsSuccessStatusCode)
            {
                var serviceRequest = await response.Content.ReadFromJsonAsync<ServiceRequest>();
                if (serviceRequest != null)
                {
                    return View(serviceRequest);
                }
            }
            return NotFound();
        }

        public async Task<IActionResult> Create()
        {
            await LoadContractDropdownAsync();
            ViewBag.ExchangeRate = await _currencyService.GetUsdToZarRateAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequest serviceRequest)
        {
            AddAuthHeader();
            
            // Post serviceRequest to the Web API
            var response = await _httpClient.PostAsJsonAsync("api/servicerequests", serviceRequest);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Service request created successfully.";
                return RedirectToAction(nameof(Index));
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", string.IsNullOrEmpty(errorContent) ? "Error creating service request on API." : errorContent);

            await LoadContractDropdownAsync(serviceRequest.ContractId);
            ViewBag.ExchangeRate = await _currencyService.GetUsdToZarRateAsync();

            return View(serviceRequest);
        }

        public async Task<IActionResult> Delete(int id)
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync($"api/servicerequests/{id}");
            if (response.IsSuccessStatusCode)
            {
                var serviceRequest = await response.Content.ReadFromJsonAsync<ServiceRequest>();
                if (serviceRequest != null)
                {
                    return View(serviceRequest);
                }
            }
            return NotFound();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            AddAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/servicerequests/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Service request deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Error deleting service request on server.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadContractDropdownAsync(int? selectedContractId = null)
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync("api/contracts");
            var contracts = new List<Contract>();

            if (response.IsSuccessStatusCode)
            {
                contracts = await response.Content.ReadFromJsonAsync<List<Contract>>() ?? new List<Contract>();
            }

            var contractList = contracts
                .OrderBy(c => c.Client?.Name)
                .ThenBy(c => c.ServiceLevel)
                .Select(c => new
                {
                    c.ContractId,
                    DisplayName = $"{c.Client?.Name ?? "Client " + c.ClientId} - {c.ServiceLevel} ({c.Status})"
                });

            ViewBag.ContractId = new SelectList(
                contractList,
                "ContractId",
                "DisplayName",
                selectedContractId
            );
        }
    }
}
