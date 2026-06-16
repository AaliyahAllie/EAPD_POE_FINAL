using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GLMS.API.Controllers;
using GLMS.Shared.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GLMS.Tests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        private async Task<string> GetAdminTokenAsync(HttpClient client)
        {
            var loginModel = new GLMS.API.Controllers.LoginViewModel
            {
                Username = "admin",
                Password = "Admin@123"
            };

            var response = await client.PostAsJsonAsync("api/auth/login", loginModel);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.Token));

            return result.Token;
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var loginModel = new GLMS.API.Controllers.LoginViewModel
            {
                Username = "wrongadmin",
                Password = "wrongpassword"
            };

            var response = await client.PostAsJsonAsync("api/auth/login", loginModel);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetContracts_WithoutToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("api/contracts");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetContracts_WithToken_ReturnsOkWithData()
        {
            var client = _factory.CreateClient();
            var token = await GetAdminTokenAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/contracts");
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.Fail($"Request failed with status {response.StatusCode}. Content: {content}");
            }
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var contracts = await response.Content.ReadFromJsonAsync<List<Contract>>();
            Assert.NotNull(contracts);
            Assert.NotEmpty(contracts); // Since seeder adds contracts on startup
        }

        [Fact]
        public async Task CreateAndPatchContract_WithToken_Succeeds()
        {
            var client = _factory.CreateClient();
            var token = await GetAdminTokenAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 1. Create client first to link contract to
            var newClient = new Client
            {
                Name = "Integration Test Client",
                ContactDetails = "test@test.com",
                Region = "Test Region"
            };
            var clientResponse = await client.PostAsJsonAsync("api/clients", newClient);
            if (clientResponse.StatusCode != HttpStatusCode.Created)
            {
                var content = await clientResponse.Content.ReadAsStringAsync();
                Assert.Fail($"Request to create client failed with status {clientResponse.StatusCode}. Content: {content}");
            }
            Assert.Equal(HttpStatusCode.Created, clientResponse.StatusCode);
            var createdClient = await clientResponse.Content.ReadFromJsonAsync<Client>();
            Assert.NotNull(createdClient);

            // 2. Create contract
            var jsonStr = $"{{\"clientId\":{createdClient.ClientId},\"startDate\":\"2026-06-15\",\"endDate\":\"2026-12-15\",\"status\":0,\"serviceLevel\":\"Premium\"}}";
            var jsonPayload = new StringContent(jsonStr, System.Text.Encoding.UTF8, "application/json");
            var contractResponse = await client.PostAsync("api/contracts", jsonPayload);
            if (contractResponse.StatusCode != HttpStatusCode.Created)
            {
                var responseContent = await contractResponse.Content.ReadAsStringAsync();
                Assert.Fail($"Request to create contract failed with status {contractResponse.StatusCode}. Sent JSON: '{jsonStr}'. Response Content: {responseContent}");
            }
            Assert.Equal(HttpStatusCode.Created, contractResponse.StatusCode);

            var createdContract = await contractResponse.Content.ReadFromJsonAsync<Contract>();
            Assert.NotNull(createdContract);
            Assert.Equal(ContractStatus.Draft, createdContract.Status);

            // 3. Patch contract status
            var patchResponse = await client.PatchAsJsonAsync($"api/contracts/{createdContract.ContractId}/status", "Active");
            Assert.Equal(HttpStatusCode.NoContent, patchResponse.StatusCode);

            // 4. Retrieve and verify updated status
            var getContractResponse = await client.GetAsync($"api/contracts/{createdContract.ContractId}");
            Assert.Equal(HttpStatusCode.OK, getContractResponse.StatusCode);
            var retrievedContract = await getContractResponse.Content.ReadFromJsonAsync<Contract>();
            Assert.NotNull(retrievedContract);
            Assert.Equal(ContractStatus.Active, retrievedContract.Status);
        }

        private class TokenResponse
        {
            public string Token { get; set; } = string.Empty;
        }
    }
}
