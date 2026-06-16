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
    public class ContractsController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IFileValidationService _fileValidationService;
        private readonly IContractWorkflowService _workflowService;

        public ContractsController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IFileValidationService fileValidationService,
            IContractWorkflowService workflowService)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001");
            _fileValidationService = fileValidationService;
            _workflowService = workflowService;
        }

        private void AddAuthHeader()
        {
            var token = User.FindFirst("Token")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, ContractStatus? status)
        {
            AddAuthHeader();

            var url = "api/contracts?";
            if (startDate.HasValue) url += $"startDate={startDate.Value:yyyy-MM-dd}&";
            if (endDate.HasValue) url += $"endDate={endDate.Value:yyyy-MM-dd}&";
            if (status.HasValue) url += $"status={status.Value}&";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>();
                return View(contracts ?? new List<Contract>());
            }
            return View(new List<Contract>());
        }

        public async Task<IActionResult> Details(int id)
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync($"api/contracts/{id}");
            if (response.IsSuccessStatusCode)
            {
                var contract = await response.Content.ReadFromJsonAsync<Contract>();
                if (contract != null)
                {
                    return View(contract);
                }
            }
            return NotFound();
        }

        public async Task<IActionResult> Create()
        {
            await LoadClientDropdownAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract)
        {
            if (!_workflowService.IsValidContractDateRange(contract.StartDate, contract.EndDate))
            {
                ModelState.AddModelError("", "End date must be after start date.");
            }

            if (contract.SignedAgreementUpload != null)
            {
                try
                {
                    _fileValidationService.ValidatePdf(contract.SignedAgreementUpload);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            if (ModelState.IsValid)
            {
                AddAuthHeader();
                
                // 1. Create the contract
                var response = await _httpClient.PostAsJsonAsync("api/contracts", contract);
                if (response.IsSuccessStatusCode)
                {
                    var createdContract = await response.Content.ReadFromJsonAsync<Contract>();
                    
                    // 2. Upload file if selected
                    if (contract.SignedAgreementUpload != null && createdContract != null)
                    {
                        var filePath = await UploadFile(createdContract.ContractId, contract.SignedAgreementUpload);
                        if (filePath == null)
                        {
                            ModelState.AddModelError("", "Contract created, but file upload failed.");
                            await LoadClientDropdownAsync(contract.ClientId);
                            return View(contract);
                        }
                    }

                    TempData["SuccessMessage"] = "Contract created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", $"Failed to create contract on server: {errorMsg}");
                }
            }

            await LoadClientDropdownAsync(contract.ClientId);
            return View(contract);
        }

        public async Task<IActionResult> Edit(int id)
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync($"api/contracts/{id}");
            if (response.IsSuccessStatusCode)
            {
                var contract = await response.Content.ReadFromJsonAsync<Contract>();
                if (contract != null)
                {
                    await LoadClientDropdownAsync(contract.ClientId);
                    return View(contract);
                }
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contract contract)
        {
            if (id != contract.ContractId)
                return NotFound();

            if (!_workflowService.IsValidContractDateRange(contract.StartDate, contract.EndDate))
            {
                ModelState.AddModelError("", "End date must be after start date.");
            }

            if (contract.SignedAgreementUpload != null)
            {
                try
                {
                    _fileValidationService.ValidatePdf(contract.SignedAgreementUpload);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            if (ModelState.IsValid)
            {
                AddAuthHeader();
                
                // 1. Update file first if uploaded
                if (contract.SignedAgreementUpload != null)
                {
                    var path = await UploadFile(id, contract.SignedAgreementUpload);
                    if (path != null)
                    {
                        contract.SignedAgreementPath = path;
                    }
                }

                // 2. Update contract details
                var response = await _httpClient.PutAsJsonAsync($"api/contracts/{id}", contract);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Contract updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError("", "Error updating contract on server.");
            }

            await LoadClientDropdownAsync(contract.ClientId);
            return View(contract);
        }

        public async Task<IActionResult> Delete(int id)
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync($"api/contracts/{id}");
            if (response.IsSuccessStatusCode)
            {
                var contract = await response.Content.ReadFromJsonAsync<Contract>();
                if (contract != null)
                {
                    return View(contract);
                }
            }
            return NotFound();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            AddAuthHeader();
            var response = await _httpClient.DeleteAsync($"api/contracts/{id}");
            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Contract and linked service requests deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = "Error deleting contract on server.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DownloadAgreement(int id)
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync($"api/contracts/{id}/download");
            if (response.IsSuccessStatusCode)
            {
                var fileStream = await response.Content.ReadAsStreamAsync();
                var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/pdf";
                return File(fileStream, contentType, $"Contract_{id}_Agreement.pdf");
            }
            return NotFound();
        }

        private async Task<string?> UploadFile(int contractId, IFormFile file)
        {
            using var content = new MultipartFormDataContent();
            var fileStream = file.OpenReadStream();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.FileName);

            AddAuthHeader();
            var response = await _httpClient.PostAsync($"api/contracts/{contractId}/upload", content);
            if (response.IsSuccessStatusCode)
            {
                var uploadResult = await response.Content.ReadFromJsonAsync<UploadResult>();
                return uploadResult?.Path;
            }
            return null;
        }

        private async Task LoadClientDropdownAsync(int? selectedClientId = null)
        {
            AddAuthHeader();
            var response = await _httpClient.GetAsync("api/clients");
            var clients = new List<Client>();
            if (response.IsSuccessStatusCode)
            {
                clients = await response.Content.ReadFromJsonAsync<List<Client>>() ?? new List<Client>();
            }

            ViewBag.ClientId = new SelectList(clients, "ClientId", "Name", selectedClientId);
        }

        private class UploadResult
        {
            public string Path { get; set; } = string.Empty;
        }
    }
}
