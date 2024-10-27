using Moq;
using PhoenixApi.Services;
using PhoenixApi.Controllers;
using Microsoft.Extensions.Logging;
using PhoenixApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace AuthenticationTests
{
    public class AuthenticationControllerTests
    {
        private readonly Mock<IAuthenticationService> _mockAuthService;
        private readonly AuthenticationController _authController;
        private readonly Mock<ILogger<AuthenticationController>> _mockLogger;

        public AuthenticationControllerTests()
        {
            _mockAuthService = new Mock<IAuthenticationService>();
            _mockLogger = new Mock<ILogger<AuthenticationController>>();
            _authController = new AuthenticationController( _mockAuthService.Object, _mockLogger.Object);
        }
        #region AuthenticateHub Tests
        [Fact]
        public async Task AuthenticateHub_ValidLogin_ReturnsAccessToken()
        {
            //Arrange
            var loginDto = new HubLoginDto { ClientId = Guid.NewGuid(), ClientSecret = "validSecret" };
            var hub = new Hub { HubId = Guid.NewGuid(), ClientId = loginDto.ClientId, ClientSecret = "validSecret" };
            var token = "generatedToken";

            _mockAuthService.Setup(service => service.GetHubByClientId(loginDto.ClientId))
                .ReturnsAsync(hub);
            _mockAuthService.Setup(service => service.ClientSecretIsValid(loginDto))
                .ReturnsAsync(true);
            _mockAuthService.Setup(service => service.GenerateJwtTokenWithHubId(hub.HubId))
                .ReturnsAsync(token);


            //Act
            var result = await _authController.AuthenticateHub(loginDto);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.Equal(token, response.AccessToken);
        }
        [Fact]
        public async Task AuthenticateHub_InvalidClientId_ReturnsUnauthorized()
        {
            //Arrange
            var loginDto = new HubLoginDto { ClientId = Guid.NewGuid(), ClientSecret = "somSecret" };

            _mockAuthService.Setup(serivce => serivce.GetHubByClientId(loginDto.ClientId))
                .ReturnsAsync((Hub)null);

            //Act
            var result = await _authController.AuthenticateHub(loginDto);

            //Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);

        }
        [Fact]
        public async Task AuthenticateHub_InvalidClientSecret_ReturnsUnauthorized()
        {
            //Arrange
            var loginDto = new HubLoginDto { ClientId = Guid.NewGuid(), ClientSecret = "invalidSecret" };
            var hub = new Hub { HubId = Guid.NewGuid(), ClientId = loginDto.ClientId, ClientSecret = "wrongSecret" };

            _mockAuthService.Setup(service => service.GetHubByClientId(loginDto.ClientId)).ReturnsAsync(hub);
            _mockAuthService.Setup(service => service.ClientSecretIsValid(loginDto)).ReturnsAsync(false);

            //Act
            var result = await _authController.AuthenticateHub(loginDto);

            //Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);
        }
        [Fact]
        public async Task AuthenticateHub_ExceptionDuringTokenGeneration_ReturnsInternalServerError()
        {
            var loginDto = new HubLoginDto { ClientId = Guid.NewGuid(), ClientSecret = "validSecret" };
            var hub = new Hub { HubId = Guid.NewGuid(), ClientId = loginDto.ClientId, ClientSecret = "validSecret" };

            _mockAuthService.Setup(service => service.GetHubByClientId(loginDto.ClientId))
                 .ReturnsAsync(hub);
            _mockAuthService.Setup(service => service.ClientSecretIsValid(loginDto))
                .ReturnsAsync(true);
            _mockAuthService.Setup(service => service.GenerateJwtTokenWithHubId(hub.HubId))
                .ThrowsAsync(new Exception("Some error occurred"));

            var result = await _authController.AuthenticateHub(loginDto);

            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);

        }
        [Fact]
        public async Task AuthenticateHub_NullLoginDto_ReturnsBadRequest()
        {
            var result = await _authController.AuthenticateHub(null);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }
        #endregion
        #region GetHubCredentials
        [Fact]
        public async Task GetHubCredentials_ValidHubId_ReturnsClientCredentials()
        {
            var hubId = Guid.NewGuid();
            var expectedLoginDto = new HubLoginDto
            {
                ClientId = Guid.NewGuid(),
                ClientSecret = "testSecret"
            };

            _mockAuthService.Setup(s => s.GenerateHubCredentials()).Returns(expectedLoginDto);
            _mockAuthService.Setup(s => s.UpdateHubWithCredentials(hubId, expectedLoginDto)).Returns(Task.CompletedTask);

            var result = await _authController.GetHubCredentials(hubId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var loginDto = Assert.IsType<HubLoginDto>(okResult.Value);
            Assert.Equal(expectedLoginDto.ClientId, loginDto.ClientId);
            Assert.Equal(expectedLoginDto.ClientSecret, loginDto.ClientSecret);
        }
        [Fact]
        public async Task GetHubCredentials_HubNotFound_ReturnsNotFound()
        {
            var hubId = Guid.NewGuid();
            _mockAuthService.Setup(s => s.GenerateHubCredentials()).Throws(new KeyNotFoundException());

            var result = await _authController.GetHubCredentials(hubId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }
        [Fact]
        public async Task GetHubCredentials_DuplicateClientInfo_ReturnsConflict()
        {
            var hubId = Guid.NewGuid();
            _mockAuthService.Setup(s => s.GenerateHubCredentials()).Throws(new DuplicateClientInfoException());

            var result = await _authController.GetHubCredentials(hubId);

            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(StatusCodes.Status409Conflict, conflictResult.StatusCode);
        }
        [Fact]
        public async Task GetHubCredentials_UnhandledException_ReturnsInternalServerError()
        {
            var hubId = Guid.NewGuid();
            _mockAuthService.Setup(s => s.GenerateHubCredentials()).Throws(new Exception("Unexpected Error."));

            var result = await _authController.GetHubCredentials(hubId);

            var internalServerErrrorResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status500InternalServerError, internalServerErrrorResult.StatusCode);
        }
        #endregion
    }
}
