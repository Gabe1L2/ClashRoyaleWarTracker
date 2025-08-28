using ClashRoyaleProject.Application.Interfaces;
using ClashRoyaleProject.Application.Models;
using ClashRoyaleProject.Application.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ClashRoyaleProject.Tests.Services
{
    public class ApplicationServiceTests
    {
        private readonly Mock<IClashRoyaleService> _mockClashRoyaleService;
        private readonly Mock<IClanRepository> _mockClanRepository;
        private readonly Mock<ILogger<ApplicationService>> _mockLogger;
        private readonly ApplicationService _applicationService;

        public ApplicationServiceTests()
        {
            _mockClashRoyaleService = new Mock<IClashRoyaleService>();
            _mockClanRepository = new Mock<IClanRepository>();
            _mockLogger = new Mock<ILogger<ApplicationService>>();
            _applicationService = new ApplicationService(_mockClashRoyaleService.Object, _mockClanRepository.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllClansAsync_WhenClansExist_ReturnsSuccessWithClans()
        {
            // Arrange
            var expectedClans = new List<Clan>
            {
                new Clan { ID = 1, Tag = "ABC123", Name = "Test Clan 1", WarTrophies = 1000, LastUpdated = DateTime.Now },
                new Clan { ID = 2, Tag = "DEF456", Name = "Test Clan 2", WarTrophies = 1500, LastUpdated = DateTime.Now }
            };

            _mockClanRepository.Setup(x => x.GetAllClansAsync())
                .ReturnsAsync(expectedClans);

            // Act
            var result = await _applicationService.GetAllClansAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal("Test Clan 1", result.Data.First().Name);
        }

        [Fact]
        public async Task GetAllClansAsync_WhenNoClansExist_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var emptyClans = new List<Clan>();
            _mockClanRepository.Setup(x => x.GetAllClansAsync())
                .ReturnsAsync(emptyClans);

            // Act
            var result = await _applicationService.GetAllClansAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllClansAsync_WhenRepositoryThrowsException_ReturnsFailure()
        {
            // Arrange
            _mockClanRepository.Setup(x => x.GetAllClansAsync())
                .ThrowsAsync(new Exception("Database connection failed"));

            // Act
            var result = await _applicationService.GetAllClansAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("An unexpected error occurred while retrieving all clans", result.Message);
            Assert.Null(result.Data);
        }
    }
}