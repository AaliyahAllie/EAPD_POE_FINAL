using GLMS.Shared.Models;

namespace GLMS.Web.Data
{
    public static class DbSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (context.Clients.Any())
                return;

            var client1 = new Client
            {
                Name = "Cape Freight Holdings",
                ContactDetails = "info@capefreight.co.za | 021 555 1200",
                Region = "South Africa"
            };

            var client2 = new Client
            {
                Name = "EuroMove Logistics",
                ContactDetails = "admin@euromove.com | +49 30 123456",
                Region = "Europe"
            };

            context.Clients.AddRange(client1, client2);
            context.SaveChanges();

            context.Contracts.AddRange(
                new Contract
                {
                    ClientId = client1.ClientId,
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddYears(1),
                    Status = ContractStatus.Active,
                    ServiceLevel = "Premium"
                },
                new Contract
                {
                    ClientId = client2.ClientId,
                    StartDate = DateTime.Today.AddYears(-1),
                    EndDate = DateTime.Today.AddDays(-1),
                    Status = ContractStatus.Expired,
                    ServiceLevel = "Standard"
                }
            );

            context.SaveChanges();
        }
    }
}
