using Moq;
using PhoenixApi.Controllers;
using PhoenixApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationTests.Service
{
    public partial class AuthenticationServiceTests
    {
        private string ClientSecretIsValidHashCheck(string stringToHash)
        {
            var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(stringToHash));
            var hashedSecret = Convert.ToBase64String(hashedBytes);

            return hashedSecret;
        }
        #region ClientSecretIsValid Tests
        [Fact]
        public async Task ClientSecretIsValid_ValidSecret_ReturnsTrue()
        {
            HubLoginDto loginDto = new HubLoginDto { ClientId = Guid.NewGuid(), ClientSecret = "validSecret" };

            Hub hub = new Hub { ClientSecret = ClientSecretIsValidHashCheck(loginDto.ClientSecret)};

            _mockHubRepository.Setup(repo => repo.GetHubByClientIdAsync(loginDto.ClientId)).ReturnsAsync(hub);

            var result = await _authService.ClientSecretIsValid(loginDto);

            Assert.True(result);
            _mockHubRepository.Verify(repo => repo.GetHubByClientIdAsync(loginDto.ClientId), Times.Once);
        }

        [Fact]
        public async Task ClientSecretIsValid_InvalidSecret_ReturnsFalse()
        {
            Assert.Fail();
        }

        [Fact]
        public async Task ClientSecretIsValid_NonExistentClientId_ReturnsFalse()
        {
            Assert.Fail();
        }

        [Fact]
        public async Task ClientSecretIsValid_RepositoryThrowsException_LogsAndReturnsFalse()
        {
            Assert.Fail();
        }
        #endregion
    }
}
