using Moq;
using PhoenixApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationTests.Service
{
    public partial class AuthenticationServiceTests
    {
        [Fact]
        public async Task GetHubByClientId_ValidClientId_ReturnsHub()
        {
            var validClientId = Guid.NewGuid();
            var expectedHub = new Hub
            {
                HubId = Guid.NewGuid(),
                ClientId = validClientId,
                Name = "TestHub"
            };

            _mockHubRepository.Setup(repo => repo.GetHubByClientIdAsync(validClientId)).ReturnsAsync(expectedHub);

            var result = await _authService.GetHubByClientId(validClientId);

            Assert.NotNull(result);
            Assert.Equal(expectedHub.ClientId, result.ClientId);
            Assert.Equal(expectedHub.HubId, result.HubId);
            Assert.Equal(expectedHub.Name, result.Name);

            _mockHubRepository.Verify(repo => repo.GetHubByClientIdAsync(validClientId), Times.Once());
        }
        [Fact]
        public async Task GetHubByClientId_InvalidClientId_ThrowsKeyNotFoundException()
        {
            var invalidClientId = Guid.NewGuid();
            _mockHubRepository.Setup(repo => repo.GetHubByClientIdAsync(invalidClientId)).ReturnsAsync((Hub)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _authService.GetHubByClientId(invalidClientId));
            _mockHubRepository.Verify(repo => repo.GetHubByClientIdAsync(invalidClientId), Times.Once());
        }
        [Fact]
        public async Task GetHubByClientId_RepositoryThrowsException_ThrowsException()
        {
            var clientId = Guid.NewGuid();
            _mockHubRepository.Setup(repo => repo.GetHubByClientIdAsync(clientId)).ThrowsAsync(new Exception("Database Failure"));

            await Assert.ThrowsAsync<Exception>(() => _authService.GetHubByClientId(clientId));
            _mockHubRepository.Verify(repo => repo.GetHubByClientIdAsync(clientId), Times.Once());
        }
    }
}
