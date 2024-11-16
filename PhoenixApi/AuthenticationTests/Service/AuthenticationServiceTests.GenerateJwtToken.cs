using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationTests.Service
{
    public partial class AuthenticationServiceTests
    {
        #region GenerateJwtToken Tests
        [Fact]
        public async Task GenerateJwtToken_ValidHubId_ReturnsJwtToken()
        {
            Assert.Fail();
        }
        [Fact]
        public async Task GenerateJwtToken_ValidHubId_LogsTokenInfo()
        {
            Assert.Fail();
        }
        [Fact]
        public async Task GenerateJwtToken_SigningKeyIsNull_ThrowsArgumentNullException()
        {
            Assert.Fail();
        }

        #endregion
    }
}
