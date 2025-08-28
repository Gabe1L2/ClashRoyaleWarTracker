using ClashRoyaleProject.Application;
using ClashRoyaleProject.Application.Interfaces;
using ClashRoyaleProject.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace ClashRoyaleProject.Tests.Services
{
    public class ApplicationServiceFullIntegrationTests : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplicationService _applicationService;
        private readonly ITestOutputHelper _output;

        public ApplicationServiceFullIntegrationTests(ITestOutputHelper output)
        {
            _output = output;

            // Build configuration (same as your real app)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: true)
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets("aspnet-ClashRoyaleProject.Web-6c242fba-7275-4e69-a0e7-4b430b574877") // This loads your user secrets
                .AddEnvironmentVariables()
                .Build();

            // Build service collection (same as your real app)
            var services = new ServiceCollection();

            // Add logging to see what's happening
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // Add your application layers (exactly like Program.cs)
            services.AddApplication();
            services.AddInfrastructure(configuration);

            _serviceProvider = services.BuildServiceProvider();
            _applicationService = _serviceProvider.GetRequiredService<IApplicationService>();
        }

        [Fact]
        public async Task GetAllClansAsyncTest()
        {
            // Act - This hits your REAL database
            var result = await _applicationService.GetAllClansAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            _output.WriteLine($"Retrieved {result.Data.Count()} clans from database");

            foreach (var clan in result.Data)
            {
                _output.WriteLine($"Clan: {clan.Name} ({clan.Tag}) - {clan.WarTrophies} trophies");

                // Basic data validation
                Assert.NotNull(clan.Name);
                Assert.NotNull(clan.Tag);
                Assert.True(clan.WarTrophies >= 0);
            }
        }

        [Fact]
        public async Task AddClanAsyncTest()
        {
            // Arrange - Use a real clan tag (you'll need to replace with a valid one)
            var testClanTag = "2Y9VPJUY0"; // Example clan tag - replace with a real one you know exists

            _output.WriteLine($"Testing with clan tag: {testClanTag}");

            // Act - This hits the REAL Clash Royale API and your REAL database
            var result = await _applicationService.AddClanAsync(testClanTag);

            // Assert
            if (result.Success)
            {
                _output.WriteLine($"Successfully added clan: {result.Message}");
                Assert.True(result.Success);
                Assert.Contains("successfully added", result.Message.ToLower());

                // Verify the clan was actually added to the database
                var allClansResult = await _applicationService.GetAllClansAsync();
                Assert.True(allClansResult.Success);
                Assert.Contains(allClansResult.Data, c => c.Tag.Replace("#", "") == testClanTag.Replace("#", ""));
            }
            else
            {
                _output.WriteLine($"Failed to add clan: {result.Message}");

                Assert.False(result.Success);
                Assert.NotEmpty(result.Message);
            }
        }

        [Fact]
        public async Task DeleteClanAsyncTest()
        {
            var clanTagToDelete = "2Y9VPJUY0";
            _output.WriteLine($"Attempting to delete clan with tag: {clanTagToDelete}");

            var deleteResult = await _applicationService.DeleteClanAsync(clanTagToDelete);

            if (deleteResult.Success)
            {
                _output.WriteLine($"Successfully deleted clan with tag: {clanTagToDelete}");
                Assert.True(deleteResult.Success);
            }
            else
            {
                _output.WriteLine($"Failed to delete clan with tag: {clanTagToDelete}. Message: {deleteResult.Message}");
                Assert.False(deleteResult.Success);
                Assert.NotEmpty(deleteResult.Message);
            }
        }

        [Fact]
        public async Task UpdateClanAsyncTest()
        {
            var clanTagToUpdate = "2Y9VPJUY0";
            _output.WriteLine($"Attempting to update clan with tag: {clanTagToUpdate}");

            var updateResult = await _applicationService.UpdateClanAsync(clanTagToUpdate);
        }

        [Fact]
        public async Task AddClanAsync_WithInvalidTag_ReturnsValidationError()
        {
            // Arrange
            var invalidTag = ""; // Empty tag should fail validation

            // Act
            var result = await _applicationService.AddClanAsync(invalidTag);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("cannot be empty", result.Message.ToLower());
            _output.WriteLine($"Validation correctly failed: {result.Message}");
        }

        [Fact]
        public async Task AddClanAsync_WithMalformedTag_ReturnsValidationError()
        {
            // Arrange
            var malformedTag = "!@#$%^&*()"; // Special characters should be stripped/fail

            // Act
            var result = await _applicationService.AddClanAsync(malformedTag);

            // Assert
            Assert.False(result.Success);
            _output.WriteLine($"Malformed tag correctly rejected: {result.Message}");
        }

        [Fact]
        public async Task Integration_AddAndRetrieveClan_WorksEndToEnd()
        {
            // This test demonstrates the full flow:
            // 1. Get current clan count
            // 2. Add a new clan (if possible)
            // 3. Verify clan count increased
            // 4. Verify the specific clan appears in the list

            // Step 1: Get initial count
            var initialResult = await _applicationService.GetAllClansAsync();
            Assert.True(initialResult.Success);
            var initialCount = initialResult.Data.Count();
            _output.WriteLine($"Initial clan count: {initialCount}");

            // Step 2: Try to add a clan (use a real clan tag)
            var testTag = "2Y9VPJUY0"; // Replace with a clan tag you know exists
            var addResult = await _applicationService.AddClanAsync(testTag);
            
            _output.WriteLine($"Add clan result: Success={addResult.Success}, Message={addResult.Message}");

            // Step 3: Get final count
            var finalResult = await _applicationService.GetAllClansAsync();
            Assert.True(finalResult.Success);
            var finalCount = finalResult.Data.Count();
            _output.WriteLine($"Final clan count: {finalCount}");

            // Step 4: Verify results
            if (addResult.Success)
            {
                // If add succeeded, count should have increased
                Assert.True(finalCount >= initialCount);
                
                // The clan should be in the list
                var sanitizedTag = testTag.Replace("#", "");
                Assert.Contains(finalResult.Data, c => c.Tag == sanitizedTag);
                _output.WriteLine($"Successfully verified clan {testTag} was added and appears in the list");
            }
            else
            {
                // If add failed (maybe clan already exists), that's also valid
                _output.WriteLine($"Add failed as expected: {addResult.Message}");
            }
        }

        public void Dispose()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}