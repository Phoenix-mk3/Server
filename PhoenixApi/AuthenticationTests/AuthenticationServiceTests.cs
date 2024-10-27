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

        public AuthenticationServiceTests()
        {
            _mockHubRepository = new Mock<IHubRepository>();
            _mockConfig = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<AuthenticationService>>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _authService = new AuthenticationService(
                _mockHubRepository.Object,
                _mockConfig.Object,
                _mockLogger.Object,
                _mockUnitOfWork.Object
            );
        }
        
        

    }
}
