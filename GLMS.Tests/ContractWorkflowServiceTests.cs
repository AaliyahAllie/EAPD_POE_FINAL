using GLMS.Shared.Models;
using GLMS.Web.Services;
using Xunit;

namespace GLMS.Tests
{
    public class ContractWorkflowServiceTests
    {
        [Fact]
        public void CanCreateServiceRequest_ShouldReturnTrue_WhenContractIsActive()
        {
            var service = new ContractWorkflowService();

            var contract = new Contract
            {
                Status = ContractStatus.Active
            };

            var result = service.CanCreateServiceRequest(contract);

            Assert.True(result);
        }

        [Fact]
        public void CanCreateServiceRequest_ShouldReturnTrue_WhenContractIsDraft()
        {
            var service = new ContractWorkflowService();

            var contract = new Contract
            {
                Status = ContractStatus.Draft
            };

            var result = service.CanCreateServiceRequest(contract);

            Assert.True(result);
        }

        [Fact]
        public void CanCreateServiceRequest_ShouldReturnFalse_WhenContractIsExpired()
        {
            var service = new ContractWorkflowService();

            var contract = new Contract
            {
                Status = ContractStatus.Expired
            };

            var result = service.CanCreateServiceRequest(contract);

            Assert.False(result);
        }

        [Fact]
        public void CanCreateServiceRequest_ShouldReturnFalse_WhenContractIsOnHold()
        {
            var service = new ContractWorkflowService();

            var contract = new Contract
            {
                Status = ContractStatus.OnHold
            };

            var result = service.CanCreateServiceRequest(contract);

            Assert.False(result);
        }

        [Fact]
        public void IsValidContractDateRange_ShouldReturnTrue_WhenEndDateIsAfterStartDate()
        {
            var service = new ContractWorkflowService();

            var result = service.IsValidContractDateRange(
                new DateTime(2026, 1, 1),
                new DateTime(2026, 12, 31));

            Assert.True(result);
        }

        [Fact]
        public void IsValidContractDateRange_ShouldReturnFalse_WhenEndDateIsBeforeStartDate()
        {
            var service = new ContractWorkflowService();

            var result = service.IsValidContractDateRange(
                new DateTime(2026, 12, 31),
                new DateTime(2026, 1, 1));

            Assert.False(result);
        }
    }
}
