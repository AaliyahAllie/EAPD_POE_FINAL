using GLMS.API.Data;
using GLMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var clientCount = await _context.Clients.CountAsync();
            var contractCount = await _context.Contracts.CountAsync();
            var serviceRequestCount = await _context.ServiceRequests.CountAsync();
            var activeContracts = await _context.Contracts.CountAsync(c => c.Status == ContractStatus.Active);

            return Ok(new
            {
                clientCount,
                contractCount,
                serviceRequestCount,
                activeContracts
            });
        }
    }
}
