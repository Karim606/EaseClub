using EaseClub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace EaseClub.Api.IntegrationTests.Common
{
    public abstract class BaseIntegrationTest : IClassFixture<ApiWebApplicationFactory<Program>>, IAsyncLifetime
    {

        protected readonly HttpClient Client;
        protected readonly AppDbContext DbContext;
        private readonly IServiceScope _scope;
        protected readonly Helpers _helpers;
        protected readonly JsonSerializerOptions JsonOptions;

        protected BaseIntegrationTest(ApiWebApplicationFactory<Program> factory)
        {
            // 1. Initialize the HTTP Client to hit endpoints
            Client = factory.CreateClient();

            JsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            JsonOptions.Converters.Add(new JsonStringEnumConverter());

            // 2. Create a scope to access the Database for 'Assert' checks
            _scope = factory.Services.CreateScope();
            DbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
            _helpers = new Helpers(DbContext); 
        }

        // Runs BEFORE every [Fact]
        public async Task InitializeAsync()
        {
            // Wipe the data so previous tests don't interfere
           // await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();

            // Seed fresh data for THIS test
            var initializer = _scope.ServiceProvider.GetRequiredService<DbInitializer>();
            await initializer.SeedAsync();
            ClearTracker();
        }

        public Task DisposeAsync()
        {
            DbContext.Database.EnsureDeleted();
            _scope.Dispose();
            return Task.CompletedTask;
        }

        #region Authentication Helpers

        /// <summary>
        /// Authenticates as the Admin seeded in your DbInitializer
        /// </summary>
        protected async Task LoginAsAdminAsync()
        {
            await AuthenticateClientAsync("admin@example.com", "Admin123456");
        }

        /// <summary>
        /// Authenticates as the Member seeded in your DbInitializer
        /// </summary>
        protected async Task LoginAsMemberAsync()
        {
            await AuthenticateClientAsync("user@example.com", "User123456");
        }
        protected void ClearTracker()
        {
            DbContext.ChangeTracker.Clear();
        }

        private async Task AuthenticateClientAsync(string email, string password)
        {
            var loginRequest = new { Email = email, Password = password };

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/login")
            {
                Content = JsonContent.Create(loginRequest)
            };

            // ADD THE MISSING HEADERS HERE
            request.Headers.Add("X-Client-Type", "Web");

            var response = await Client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Auth failed. Status: {response.StatusCode}. Error: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            // Attach the token to all future requests from this Client instance
            Client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result!.AccessToken);
        }

        #endregion

        

    }

    public class LoginResponse { public string AccessToken { get; set; } }
}
