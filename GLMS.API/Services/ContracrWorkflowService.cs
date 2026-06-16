using GLMS.Shared.Models;

namespace GLMS.API.Services
{
    public class ContractWorkflowService : IContractWorkflowService
    {
        public bool CanCreateServiceRequest(Contract contract)
        {
            if (contract == null)
                return false;

            return contract.Status == ContractStatus.Active || contract.Status == ContractStatus.Draft;
        }

        public bool IsValidContractDateRange(DateTime startDate, DateTime endDate)
        {
            return endDate > startDate;
        }
    }
}

