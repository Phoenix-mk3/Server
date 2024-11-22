using Moq;
using PhoenixApi.Models;
using PhoenixApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationTests.Service
{
    public partial class AuthenticationServiceTests
    {
        [Fact]
        public async Task UpdateUserWithCredentials_ValidCredentials_UpdatesUserSuccessfully()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }));
            var clientId = Guid.NewGuid();
            var clientSecret = "validSecret";

            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                ClientId = null,
                ClientSecret = null
            };

            _mockClaimsRetrievalService.Setup(service => service.GetSubjectIdFromClaims(claimsPrincipal))
                .Returns(userId);

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _mockUnitOfWork.Setup(unit => unit.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            // Act
            await _authService.UpdateUserWithCredentials(claimsPrincipal, clientId, clientSecret);

            // Assert
            Assert.Equal(clientId, user.ClientId);
            Assert.NotNull(user.ClientSecret); // Ensure the secret is hashed and updated
            _mockUserRepository.Verify(repo => repo.UpdateAsync(userId, user), Times.Once);
            _mockUnitOfWork.Verify(unit => unit.SaveChangesAsync(), Times.Once);
        }


        [Fact]
        public async Task UpdateUserWithCredentials_ClientInfoExists_ThrowsDuplicateClientInfoException()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }));
            var clientId = Guid.NewGuid();
            var clientSecret = "validSecret";

            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                ClientId = Guid.NewGuid(), // Credentials already exist
                ClientSecret = "existingSecret"
            };

            _mockClaimsRetrievalService.Setup(service => service.GetSubjectIdFromClaims(claimsPrincipal))
                .Returns(userId);

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DuplicateClientInfoException>(() => _authService.UpdateUserWithCredentials(claimsPrincipal, clientId, clientSecret));

            Assert.Equal($"ClientId or Secret already exists for user {userId}.", exception.Message);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Guid>(), It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(unit => unit.SaveChangesAsync(), Times.Never);
        }


        [Fact]
        public async Task UpdateUserWithCredentials_InvalidInputs_ThrowsArgumentException()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }));
            var invalidClientId = Guid.Empty;
            var invalidClientSecret = string.Empty;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _authService.UpdateUserWithCredentials(claimsPrincipal, invalidClientId, invalidClientSecret));

            Assert.Equal("ClientId or ClientSecret cannot be empty or null.", exception.Message);
            _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Guid>(), It.IsAny<User>()), Times.Never);
            _mockUnitOfWork.Verify(unit => unit.SaveChangesAsync(), Times.Never);
        }

    }
}
