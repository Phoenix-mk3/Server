using Moq;
using PhoenixApi.Models;
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
        public async Task CheckUserInHub_ValidUserAndHub_VerifiesSuccessfully()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }));

            var hubId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var hub = new Hub { HubId = hubId };
            var user = new User
            {
                Id = userId
            };
            user.Hubs.Add(hub);

            _mockClaimsRetrievalService.Setup(service => service.GetSubjectIdFromClaims(claimsPrincipal))
                .Returns(userId);

            _mockHubRepository.Setup(repo => repo.GetByIdAsync(hubId))
                .ReturnsAsync(hub);

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act
            var exception = await Record.ExceptionAsync(() => _authService.CheckUserInHub(claimsPrincipal, hubId));

            // Assert
            Assert.Null(exception); // No exception should be thrown for valid input
        }



        [Fact]
        public async Task CheckUserInHub_UserNotInHub_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }));

            var hubId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var hub = new Hub { HubId = hubId };
            var user = new User
            {
                Id = userId
            };

            _mockClaimsRetrievalService.Setup(service => service.GetSubjectIdFromClaims(claimsPrincipal))
                .Returns(userId);

            _mockHubRepository.Setup(repo => repo.GetByIdAsync(hubId))
                .ReturnsAsync(hub);

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.CheckUserInHub(claimsPrincipal, hubId)
            );

            Assert.Contains($"User {userId} is not associated with the specified Hub {hubId}.", exception.Message);
        }

    }
}
