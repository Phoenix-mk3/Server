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
        public async Task BuildUserAuthResponse_ValidClaims_ReturnsAuthResponse()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }));

            var hubId = Guid.NewGuid();
            var hub = new Hub { HubId = hubId };
            var userId = Guid.NewGuid();
            var expectedAccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9";

            _mockClaimsRetrievalService.Setup(service => service.GetSubjectIdFromClaims(claimsPrincipal))
                .Returns(hubId);

            _mockHubRepository.Setup(repo => repo.GetByIdAsync(hubId))
                .ReturnsAsync(hub);

            _mockUserRepository.Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(unit => unit.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _mockConfig.Setup(config => config["Jwt:SecretKey"]).Returns("SecretKeyOfAtLeastThirtyTwoCharacters");
            _mockConfig.Setup(config => config["Jwt:Issuer"]).Returns("Issuer");
            _mockConfig.Setup(config => config["Jwt:Audience"]).Returns("Audience");

            

            // Act
            var result = await _authService.BuildUserAuthResponse(claimsPrincipal);

            //Issues finding methods to avoid unpredictable tokens.
            var accessTokenResult = result.TemporaryAuthToken.ToString().Split('.').First();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<Guid>(result.UserId);
            Assert.Equal(expectedAccessToken, accessTokenResult);
        }


        [Fact]
        public async Task BuildUserAuthResponse_HubNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }));

            var hubId = Guid.NewGuid();

            _mockClaimsRetrievalService.Setup(service => service.GetSubjectIdFromClaims(claimsPrincipal))
                .Returns(hubId);

            _mockHubRepository.Setup(repo => repo.GetByIdAsync(hubId))
                .ReturnsAsync((Hub)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.BuildUserAuthResponse(claimsPrincipal)
            );

            Assert.Contains($"Failed to retrieve hub with ID: {hubId}", exception.Message);
        }
        [Fact]
        public async Task BuildUserAuthResponse_InvalidClaims_ThrowsInvalidOperationException()
        {
            // Arrange
            var invalidClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity()); // Empty claims

            _mockClaimsRetrievalService.Setup(service => service.GetSubjectIdFromClaims(invalidClaimsPrincipal))
                .Throws<ArgumentNullException>();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.BuildUserAuthResponse(invalidClaimsPrincipal)
            );

            Assert.Contains("Invalid claims data provided.", exception.Message);
        }
        [Fact]
        public async Task BuildUserAuthResponse_SaveUserFails_ThrowsInvalidOperationException()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            }));

            var hubId = Guid.NewGuid();
            var hub = new Hub { HubId = hubId };

            _mockClaimsRetrievalService.Setup(service => service.GetSubjectIdFromClaims(claimsPrincipal))
                .Returns(hubId);

            _mockHubRepository.Setup(repo => repo.GetByIdAsync(hubId))
                .ReturnsAsync(hub);

            _mockUserRepository.Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .ThrowsAsync(new Exception("Database failure"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.BuildUserAuthResponse(claimsPrincipal)
            );

            Assert.Contains("Failed to save user data to the database.", exception.Message);
        }

    }
}
