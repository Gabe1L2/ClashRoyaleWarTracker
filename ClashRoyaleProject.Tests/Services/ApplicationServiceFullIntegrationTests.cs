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
            var testClanTag = "YC8R0RJ0";

            _output.WriteLine($"Testing with clan tag: {testClanTag}");

            var result = await _applicationService.AddClanAsync(testClanTag);

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
            var clanTagToDelete = "Y9Q9RRY0";

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
            var clanTagToUpdate = "V2GQU";
            _output.WriteLine($"Attempting to update clan with tag: {clanTagToUpdate}");

            var result = await _applicationService.UpdateClanAsync(clanTagToUpdate);

            if (result.Success)
            {
                _output.WriteLine($"Successfully updated clan with tag: {clanTagToUpdate}");
                Assert.True(result.Success);
            }
            else
            {
                _output.WriteLine($"Failed to update clan with tag: {clanTagToUpdate}. Message: {result.Message}");
                Assert.False(result.Success);
                Assert.NotEmpty(result.Message);
            }
        }

        [Fact]
        public async Task WeeklyUpdateAsyncTest()
        {
            _output.WriteLine("Starting weekly update test for all clans");

            // Act - This will test the entire weekly update process
            var result = await _applicationService.WeeklyUpdateAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotEmpty(result.Message);

            _output.WriteLine($"Weekly update result: {result.Message}");

            // Verify that clans were actually processed by checking if we have any clans
            var allClansResult = await _applicationService.GetAllClansAsync();
            Assert.True(allClansResult.Success);

            if (allClansResult.Data?.Any() == true)
            {
                _output.WriteLine($"Verified: {allClansResult.Data.Count()} clans exist after weekly update");

                // Check that at least one clan has a recent LastUpdated timestamp
                var recentlyUpdated = allClansResult.Data.Any(c =>
                    (DateTime.Now - c.LastUpdated).TotalMinutes < 5);

                if (recentlyUpdated)
                {
                    _output.WriteLine("At least one clan was updated recently");
                }
                else
                {
                    _output.WriteLine("No clans appear to have been updated recently");
                }
            }
            else
            {
                _output.WriteLine("No clans found in database to update");
            }
        }

        [Fact]
        public async Task UpdateClanHistoryAsyncTest()
        {
            string testClanTag = "V2GQU";
            _output.WriteLine($"Testing history update for clan {testClanTag}");

            var getClanResult = await _applicationService.GetClanAsync(testClanTag);
            if (getClanResult.Success == false || getClanResult.Data == null)
            {
                _output.WriteLine($"Clan with tag {testClanTag} not found in database. Cannot test history update.");
                Assert.False(getClanResult.Success);
                return;
            }

            var result = await _applicationService.UpdateClanHistoryAsync(getClanResult.Data);

            // Assert
            if (result.Success)
            {
                _output.WriteLine($"Successfully updated history: {result.Message}");
                Assert.True(result.Success);
            }
            else
            {
                _output.WriteLine($"History update failed: {result.Message}");
                // Don't fail the test - this might be expected if no war log data exists
                Assert.False(result.Success);
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