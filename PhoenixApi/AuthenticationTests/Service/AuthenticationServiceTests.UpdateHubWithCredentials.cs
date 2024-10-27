using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationTests.Service
{
    public partial class AuthenticationServiceTests
    {
        #region UpdateHubWithCredentials Tests
        [Fact]
        public async Task UpdateHubWithCredentials_ValidCredentials_UpdatesHubSuccessfully()
        {
            Assert.Fail();
        }
        [Fact]
        public async Task UpdateHubWithCredentials_ClientInfoExists_ThrowsDublicateClientInfoException()
        {
            Assert.Fail();
        }
        [Fact]
        public async Task UpdateHubWithCredentials_HubNotFound_ThrowsKeyNotFoundException()
        {
            Assert.Fail();
        }
        [Fact]
        public async Task UpdateHubWithCredentials_RepositoryThrowsException_ThrowsException()
        {
            Assert.Fail();
        }
        #endregion
    }
}
