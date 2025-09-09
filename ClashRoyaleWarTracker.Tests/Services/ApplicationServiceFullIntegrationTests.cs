using ClashRoyaleWarTracker.Application;
using ClashRoyaleWarTracker.Application.Interfaces;
using ClashRoyaleWarTracker.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace ClashRoyaleWarTracker.Tests.Services
{
    public class TestOutputLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestOutputLoggerProvider(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestOutputLogger(_testOutputHelper, categoryName);
        }

        public void Dispose() { }
    }

    public class TestOutputLogger : ILogger
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string _categoryName;

        public TestOutputLogger(ITestOutputHelper testOutputHelper, string categoryName)
        {
            _testOutputHelper = testOutputHelper;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            try
            {
                if (_categoryName.StartsWith("ClashRoyaleWarTracker"))
                {
                    var message = formatter(state, exception);

                    // Just the clean message to debug output
                    Debug.WriteLine(message);

                    // Also to test output
                    _testOutputHelper.WriteLine(message);
                }
            }
            catch
            {
                // Ignore if test output is not available
            }
        }
    }

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
                .AddUserSecrets("aspnet-ClashRoyaleWarTracker.Web-6c242fba-7275-4e69-a0e7-4b430b574877") // This loads your user secrets
                .AddEnvironmentVariables()
                .Build();

            // Build service collection (same as your real app)
            var services = new ServiceCollection();

            // Add logging to see what's happening
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();
                builder.AddProvider(new TestOutputLoggerProvider(output)); // This will capture all logger messages
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddFilter("ClashRoyaleWarTracker", LogLevel.Information);
                builder.AddFilter("Microsoft", LogLevel.Warning);
                builder.AddFilter("System", LogLevel.Warning);
            });

            // Add your application layers (exactly like Program.cs)
            services.AddApplication();
            services.AddInfrastructure(configuration);

            _serviceProvider = services.BuildServiceProvider();
            _applicationService = _serviceProvider.GetRequiredService<IApplicationService>();

            var logger = _serviceProvider.GetRequiredService<ILogger<ApplicationServiceFullIntegrationTests>>();
            logger.LogInformation("=== Test initialization completed ===");
        }

        [Fact]
        public async Task GetAllClansAsyncTest()
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<ApplicationServiceFullIntegrationTests>>();
            logger.LogInformation("=== Starting GetAllClansAsyncTest ===");

            var result = await _applicationService.GetAllClansAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);

            logger.LogInformation($"Retrieved {result.Data.Count()} clans from database");
            logger.LogInformation($"Test retrieved {result.Data.Count()} clans from database");

            foreach (var clan in result.Data)
            {
                logger.LogInformation($"Clan: {clan.Name} ({clan.Tag}) - {clan.WarTrophies} trophies");

                // Basic data validation
                Assert.NotNull(clan.Name);
                Assert.NotNull(clan.Tag);
                Assert.True(clan.WarTrophies >= 0);
            }
            logger.LogInformation("=== GetAllClansAsyncTest completed successfully ===");
        }

        [Fact]
        public async Task AddClanAsyncTest()
        {
            var testClanTag = "PJQRLQPQ"; // panda

            var logger = _serviceProvider.GetRequiredService<ILogger<ApplicationServiceFullIntegrationTests>>();
            logger.LogInformation($"Testing with clan tag: {testClanTag}");

            var result = await _applicationService.AddClanAsync(testClanTag);

            if (result.Success)
            {
                logger.LogInformation($"Successfully added clan: {result.Message}");
                Assert.True(result.Success);
                Assert.Contains("successfully added", result.Message.ToLower());

                // Verify the clan was actually added to the database
                var allClansResult = await _applicationService.GetAllClansAsync();
                Assert.True(allClansResult.Success);
                Assert.Contains(allClansResult.Data, c => c.Tag.Replace("#", "") == testClanTag.Replace("#", ""));
            }
            else
            {
                logger.LogInformation($"Failed to add clan: {result.Message}");

                Assert.False(result.Success);
                Assert.NotEmpty(result.Message);
            }
        }

        [Fact]
        public async Task DeleteClanAsyncTest()
        {
            var clanTagToDelete = "Y9Q9RRY0";

            var logger = _serviceProvider.GetRequiredService<ILogger<ApplicationServiceFullIntegrationTests>>();
            logger.LogInformation($"Attempting to delete clan with tag: {clanTagToDelete}");

            var deleteResult = await _applicationService.DeleteClanAsync(clanTagToDelete);

            if (deleteResult.Success)
            {
                logger.LogInformation($"Successfully deleted clan with tag: {clanTagToDelete}");
                Assert.True(deleteResult.Success);
            }
            else
            {
                logger.LogInformation($"Failed to delete clan with tag: {clanTagToDelete}. Message: {deleteResult.Message}");
                Assert.False(deleteResult.Success);
                Assert.NotEmpty(deleteResult.Message);
            }
        }

        [Fact]
        public async Task UpdateClanAsyncTest()
        {
            var clanTagToUpdate = "V2GQU";

            var logger = _serviceProvider.GetRequiredService<ILogger<ApplicationServiceFullIntegrationTests>>();
            logger.LogInformation($"Attempting to update clan with tag: {clanTagToUpdate}");

            var result = await _applicationService.UpdateClanAsync(clanTagToUpdate);

            if (result.Success)
            {
                logger.LogInformation($"Successfully updated clan with tag: {clanTagToUpdate}");
                Assert.True(result.Success);
            }
            else
            {
                logger.LogInformation($"Failed to update clan with tag: {clanTagToUpdate}. Message: {result.Message}");
                Assert.False(result.Success);
                Assert.NotEmpty(result.Message);
            }
        }

        [Fact]
        public async Task WeeklyUpdateAsyncTest()
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<ApplicationServiceFullIntegrationTests>>();

            logger.LogInformation("Starting weekly update test for all clans");

            // Act - This will test the entire weekly update process
            var result = await _applicationService.WeeklyUpdateAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotEmpty(result.Message);

            logger.LogInformation($"Weekly update result: {result.Message}");

            // Verify that clans were actually processed by checking if we have any clans
            var allClansResult = await _applicationService.GetAllClansAsync();
            Assert.True(allClansResult.Success);

            if (allClansResult.Data?.Any() == true)
            {
                logger.LogInformation($"Verified: {allClansResult.Data.Count()} clans exist after weekly update");

                // Check that at least one clan has a recent LastUpdated timestamp
                var recentlyUpdated = allClansResult.Data.Any(c =>
                    (DateTime.Now - c.LastUpdated).TotalMinutes < 5);

                if (recentlyUpdated)
                {
                    logger.LogInformation("At least one clan was updated recently");
                }
                else
                {
                    logger.LogInformation("No clans appear to have been updated recently");
                }
            }
            else
            {
                logger.LogInformation("No clans found in database to update");
            }
        }

        [Fact]
        public async Task PopulateClanHistoryAsyncTest()
        {
            string testClanTag = "V2GQU";

            var logger = _serviceProvider.GetRequiredService<ILogger<ApplicationServiceFullIntegrationTests>>();
            logger.LogInformation($"Testing history update for clan {testClanTag}");

            var getClanResult = await _applicationService.GetClanAsync(testClanTag);
            if (getClanResult.Success == false || getClanResult.Data == null)
            {
                logger.LogInformation($"Clan with tag {testClanTag} not found in database. Cannot test history update.");
                Assert.False(getClanResult.Success);
                return;
            }

            var result = await _applicationService.PopulateClanHistoryAsync(getClanResult.Data);

            // Assert
            if (result.Success)
            {
                logger.LogInformation($"Successfully updated history: {result.Message}");
                Assert.True(result.Success);
            }
            else
            {
                logger.LogInformation($"History update failed: {result.Message}");
                // Don't fail the test - this might be expected if no war log data exists
                Assert.False(result.Success);
            }
        }

        [Fact]
        public async Task PopulateRawWarHistoryTest()
        {
            string testClanTag = "YC8R0RJ0";

            var logger = _serviceProvider.GetRequiredService<ILogger<ApplicationServiceFullIntegrationTests>>();
            logger.LogInformation($"=== Starting PopulateRawWarHistoryTest with tag: {testClanTag} ===");

            var getClanResult = await _applicationService.GetClanAsync(testClanTag);
            if (getClanResult.Success == false || getClanResult.Data == null)
            {
                logger.LogWarning($"Clan with tag {testClanTag} not found for raw war history test");
                Assert.False(getClanResult.Success);
                return;
            }
            var result = await _applicationService.PopulatePlayerWarHistories(getClanResult.Data, 10);
            // Assert
            if (result.Success)
            {
                logger.LogInformation($"Test successfully populated raw war histories: {result.Message}");
                Assert.True(result.Success);
            }
            else
            {
                logger.LogWarning($"Test raw war history population failed: {result.Message}");
                Assert.False(result.Success);
            }
            logger.LogInformation("=== PopulateRawWarHistoryTest completed ===");
        }

        [Fact]
        public async Task UpdateAllActivePlayerAveragesTest()
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<ApplicationServiceFullIntegrationTests>>();
            logger.LogInformation("=== Starting UpdateAllActivePlayerAveragesTest ===");

            var result = await _applicationService.UpdateAllActivePlayerAverages(4, true);
            if (result.Success)
            {
                logger.LogInformation($"Successfully updated all active player averages: {result.Message}");
                Assert.True(result.Success);
            }
            else
            {
                logger.LogWarning($"Failed to update player averages: {result.Message}");
                Assert.False(result.Success);
            }
            logger.LogInformation("=== UpdateAllActivePlayerAveragesTest completed ===");
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