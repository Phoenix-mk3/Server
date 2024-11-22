using Microsoft.Extensions.Logging;
using Moq;
using PhoenixApi.Models;
using PhoenixApi.Models.Dto;
using PhoenixApi.Models.Security;
using PhoenixApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationTests.Service
{
    public partial class AuthenticationServiceTests
    {
        [Fact]
        public async Task AuthorizeSubject_ValidInputs_ReturnsAuthResponse()
        {
            //Arrange
            var clientId = Guid.NewGuid();
            var clientSecret = "validSecret";
            var isHub = true;

            var hashedClientSecret = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(clientSecret)));

            var subjectDto = new SubjectDto
            {
                Id = Guid.NewGuid(),
                ClientSecret = hashedClientSecret,
                ClientId = clientId,
                Role = AuthRole.Hub
            };

            var expectedAccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9";

            //Mock CreateSubject
            _mockHubRepository.Setup(repo => repo.GetHubByClientIdAsync(clientId))
                .ReturnsAsync(new Hub
                {
                    HubId = subjectDto.Id,
                    ClientId = clientId,
                    ClientSecret = subjectDto.ClientSecret
                });

            _mockConfig.Setup(config => config["Jwt:SecretKey"]).Returns("SecretKeyOfAtLeastThirtyTwoCharacters");
            _mockConfig.Setup(config => config["Jwt:Issuer"]).Returns("Issuer");
            _mockConfig.Setup(config => config["Jwt:Audience"]).Returns("Audience");

            _mockClaimsRetrievalService.Setup(service => service.GetSubjectIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(clientId);


            _mockUnitOfWork.Setup(unit => unit.SaveChangesAsync()).Returns(Task.CompletedTask);



            //Act
            var result = await _authService.AuthorizeSubject(clientId, clientSecret, isHub);

            //Issues finding methods to avoid unpredictable tokens.
            var accessTokenResult = result.AccessToken.ToString().Split('.').First();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedAccessToken, accessTokenResult);
        }

        [Fact]
        public async Task AuthorizeSubject_InvalidClientSecret_ThrowsUnauthorizedAccessException()
        {
            //Arrange
            var clientId = Guid.NewGuid();
            var clientSecret = "invalidSecret";
            var isHub = true;

            var subjectDto = new SubjectDto
            {
                Id = Guid.NewGuid(),
                ClientSecret = "hashedSecret",
                ClientId = clientId,
                Role = AuthRole.Hub
            };

            _mockHubRepository.Setup(repo => repo.GetHubByClientIdAsync(clientId))
                .ReturnsAsync(new Hub
                {
                    HubId = subjectDto.Id,
                    ClientId = clientId,
                    ClientSecret = subjectDto.ClientSecret
                });

            _mockConfig.Setup(config => config["Jwt:SecretKey"]).Returns("SuperSecretKey");
            _mockConfig.Setup(config => config["Jwt:Issuer"]).Returns("Issuer");
            _mockConfig.Setup(config => config["Jwt:Audience"]).Returns("Audience");

            _mockClaimsRetrievalService.Setup(service => service.GetSubjectIdFromClaims(It.IsAny<ClaimsPrincipal>()))
                .Returns(clientId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.AuthorizeSubject(clientId, clientSecret, isHub)
            );

            Assert.Equal("Client secret is invalid.", exception.Message);
        }

        [Fact]
        public async Task AuthorizeSubject_CreateSubjectThrowsException_ThrowsInvalidOperationException()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var clientSecret = "validSecret";
            var isHub = true;

            _mockHubRepository.Setup(repo => repo.GetHubByClientIdAsync(clientId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.AuthorizeSubject(clientId, clientSecret, isHub)
            );

            Assert.Contains($"An error occurred while creating the subject for ClientId: {clientId}", exception.Message);
            Assert.NotNull(exception.InnerException);
            Assert.Equal("Database error", exception.InnerException.Message);
        }

    }

}
