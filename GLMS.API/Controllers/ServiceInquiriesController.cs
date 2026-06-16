using GLMS.API.Data;
using GLMS.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceInquiriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServiceInquiriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceInquiry>>> GetServiceInquiries()
        {
            return await _context.ServiceInquiries
                .OrderByDescending(q => q.SubmittedAt)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<ServiceInquiry>> CreateServiceInquiry(ServiceInquiry inquiry)
        {
            inquiry.SubmittedAt = DateTime.Now;
            _context.ServiceInquiries.Add(inquiry);
            await _context.SaveChangesAsync();

            return Ok(inquiry);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServiceInquiry(int id)
        {
            var inquiry = await _context.ServiceInquiries.FindAsync(id);
            if (inquiry == null)
            {
                return NotFound();
            }

            _context.ServiceInquiries.Remove(inquiry);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
