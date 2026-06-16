using GLMS.API.Data;
using GLMS.API.Services;
using GLMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;
        private readonly IContractWorkflowService _workflowService;

        public ServiceRequestsController(
            ApplicationDbContext context,
            ICurrencyService currencyService,
            IContractWorkflowService workflowService)
        {
            _context = context;
            _currencyService = currencyService;
            _workflowService = workflowService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceRequest>>> GetServiceRequests()
        {
            return await _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c.Client)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceRequest>> GetServiceRequest(int id)
        {
            var request = await _context.ServiceRequests
                .Include(s => s.Contract)
                .ThenInclude(c => c.Client)
                .FirstOrDefaultAsync(s => s.ServiceRequestId == id);

            if (request == null)
            {
                return NotFound();
            }

            return request;
        }

        [HttpPost]
        public async Task<ActionResult<ServiceRequest>> CreateServiceRequest(ServiceRequest serviceRequest)
        {
            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.ContractId == serviceRequest.ContractId);

            if (contract == null)
            {
                return BadRequest("Please select a valid contract.");
            }

            if (!_workflowService.CanCreateServiceRequest(contract))
            {
                return BadRequest("Service Request cannot be created because the selected contract is Expired or On Hold.");
            }

            try
            {
                serviceRequest.ExchangeRate = await _currencyService.GetUsdToZarRateAsync();
                serviceRequest.Cost = _currencyService.ConvertUsdToZar(
                    serviceRequest.UsdAmount,
                    serviceRequest.ExchangeRate
                );
                serviceRequest.Status = ServiceRequestStatus.Pending;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            _context.ServiceRequests.Add(serviceRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetServiceRequest), new { id = serviceRequest.ServiceRequestId }, serviceRequest);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceRequest(int id)
        {
            var serviceRequest = await _context.ServiceRequests.FindAsync(id);
            if (serviceRequest == null)
            {
                return NotFound();
            }

            _context.ServiceRequests.Remove(serviceRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
