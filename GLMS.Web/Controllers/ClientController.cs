using System.Net.Http.Headers;
using System.Net.Http.Json;
using GLMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GLMS.Web.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly HttpClient _httpClient;

        public ClientsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
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
            var response = await _httpClient.GetAsync("api/clients");
            if (response.IsSuccessStatusCode)
            {
                var clients = await response.Content.ReadFromJsonAsync<List<Client>>();
                return View(clients ?? new List<Client>());
            }
            return View(new List<Client>());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                AddAuthHeader();
                var response = await _httpClient.PostAsJsonAsync("api/clients", client);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error creating client on the server.");
            }
            return View(client);
        }

        public async Task<IActionResult> Details(int id)
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync($"api/clients/{id}");
            if (response.IsSuccessStatusCode)
            {
                var client = await response.Content.ReadFromJsonAsync<Client>();
                if (client != null)
                {
                    return View(client);
                }
            }
            return NotFound();
        }

        public async Task<IActionResult> Edit(int id)
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync($"api/clients/{id}");
            if (response.IsSuccessStatusCode)
            {
                var client = await response.Content.ReadFromJsonAsync<Client>();
                if (client != null)
                {
                    return View(client);
                }
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Client client)
        {
            if (id != client.ClientId)
                return NotFound();

            if (ModelState.IsValid)
            {
                AddAuthHeader();
                var response = await _httpClient.PutAsJsonAsync($"api/clients/{id}", client);
                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error updating client on the server.");
            }
            return View(client);
        }

        public async Task<IActionResult> Delete(int id)
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync($"api/clients/{id}");
            if (response.IsSuccessStatusCode)
            {
                var client = await response.Content.ReadFromJsonAsync<Client>();
                if (client != null)
                {
                    return View(client);
                }
            }
            return NotFound();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            AddAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/clients/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Client and all linked contracts/service requests were deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Error deleting client on the server.";
            return RedirectToAction(nameof(Index));
        }
    }
}
