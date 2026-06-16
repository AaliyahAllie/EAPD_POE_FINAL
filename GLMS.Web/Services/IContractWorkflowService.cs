using GLMS.Shared.Models;

namespace GLMS.Web.Services
{
    public interface IContractWorkflowService
    {
        bool CanCreateServiceRequest(Contract contract);
        bool IsValidContractDateRange(DateTime startDate, DateTime endDate);
    }
}
