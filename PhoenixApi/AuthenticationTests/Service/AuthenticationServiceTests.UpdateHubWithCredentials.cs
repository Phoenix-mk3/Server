using Moq;
using PhoenixApi.Models;
using PhoenixApi.Services;
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
        public async Task UpdateHubWithCredentials_ValidCredentials_UpdatesHubSuccessfully()
        {
            // Arrange
            var hubId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var clientSecret = "validSecret";

            var hub = new Hub
            {
                HubId = hubId,
                ClientId = null,
                ClientSecret = null
            };

            _mockHubRepository.Setup(repo => repo.GetByIdAsync(hubId))
                .ReturnsAsync(hub);

            _mockUnitOfWork.Setup(unit => unit.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _authService.UpdateHubWithCredentials(hubId, clientId, clientSecret);

            // Assert
            Assert.Equal(clientId, hub.ClientId);
            Assert.NotNull(hub.ClientSecret); // Ensure secret is hashed and updated
            _mockHubRepository.Verify(repo => repo.UpdateAsync(hubId, hub), Times.Once);
            _mockUnitOfWork.Verify(unit => unit.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateHubWithCredentials_ClientInfoExists_ThrowsDuplicateClientInfoException()
        {
            // Arrange
            var hubId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var clientSecret = "validSecret";

            var hub = new Hub
            {
                HubId = hubId,
                ClientId = Guid.NewGuid(),
                ClientSecret = "existingSecret"
            };

            _mockHubRepository.Setup(repo => repo.GetByIdAsync(hubId))
                .ReturnsAsync(hub);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DuplicateClientInfoException>(
                () => _authService.UpdateHubWithCredentials(hubId, clientId, clientSecret)
            );

            Assert.Equal($"ClientId or Secret already exists for Hub {hubId}", exception.Message);
            _mockHubRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Hub>()), Times.Never);
            _mockUnitOfWork.Verify(unit => unit.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateHubWithCredentials_HubNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var hubId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var clientSecret = "validSecret";

            _mockHubRepository.Setup(repo => repo.GetByIdAsync(hubId))
                .ReturnsAsync((Hub)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _authService.UpdateHubWithCredentials(hubId, clientId, clientSecret)
            );

            Assert.Contains($"Could not find Hub with id {hubId}", exception.Message);
            _mockHubRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Hub>()), Times.Never);
            _mockUnitOfWork.Verify(unit => unit.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateHubWithCredentials_RepositoryThrowsException_ThrowsException()
        {
            // Arrange
            var hubId = Guid.NewGuid();
            var clientId = Guid.NewGuid();
            var clientSecret = "validSecret";

            var hub = new Hub
            {
                HubId = hubId,
                ClientId = null,
                ClientSecret = null
            };

            _mockHubRepository.Setup(repo => repo.GetByIdAsync(hubId))
                .ReturnsAsync(hub);

            _mockHubRepository.Setup(repo => repo.UpdateAsync(hubId, hub))
                .ThrowsAsync(new Exception("Database failure"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.UpdateHubWithCredentials(hubId, clientId, clientSecret)
            );

            Assert.Contains($"An error occurred while updating Hub {hubId}.", exception.Message);
            _mockUnitOfWork.Verify(unit => unit.SaveChangesAsync(), Times.Never);
        }

    }
}
