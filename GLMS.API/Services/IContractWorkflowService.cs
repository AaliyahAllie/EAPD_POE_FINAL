using GLMS.Shared.Models;

namespace GLMS.API.Services
{
    public interface IContractWorkflowService
    {
        bool CanCreateServiceRequest(Contract contract);
        bool IsValidContractDateRange(DateTime startDate, DateTime endDate);
    }
}

