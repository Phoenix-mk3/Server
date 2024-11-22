using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PhoenixApi.Models;
using PhoenixApi.Repositories;
using PhoenixApi.Services;
using PhoenixApi.UnitofWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationTests.Service
{
    public partial class AuthenticationServiceTests
    {
        private readonly Mock<IHubRepository> _mockHubRepository;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<ILogger<AuthenticationService>> _mockLogger;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly AuthenticationService _authService;
        private readonly Mock<IClaimsRetrievalService> _mockClaimsRetrievalService;
        private readonly Mock<IUserRepository> _mockUserRepository;

        public AuthenticationServiceTests()
        {
            _mockHubRepository = new Mock<IHubRepository>();
            _mockConfig = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AuthenticationService>>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockClaimsRetrievalService = new Mock<IClaimsRetrievalService>();
            _mockUserRepository = new Mock<IUserRepository>();

            _authService = new AuthenticationService(
                _mockHubRepository.Object,
                _mockConfig.Object,
                _mockLogger.Object,
                _mockUnitOfWork.Object,
                _mockClaimsRetrievalService.Object,
                _mockUserRepository.Object

            );
        }
    }
}
