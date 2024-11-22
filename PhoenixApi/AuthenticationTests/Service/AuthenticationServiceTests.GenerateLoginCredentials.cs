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
        public void GenerateLoginCredentials_ShouldReturnValidLoginDto()
        {
            // Arrange
            // No dependencies to mock for this method since it doesn't depend on external services.

            // Act
            var result = _authService.GenerateLoginCredentials();

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.ClientId); 
            Assert.False(string.IsNullOrEmpty(result.ClientSecret)); 
        }

    }
}
