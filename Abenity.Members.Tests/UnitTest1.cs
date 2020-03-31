using Abenity.Members.Schema;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Abenity.Members.Tests
{
    public class UnitTest1
    {
        private IAbenityMembersApiClient apiClient = new AbenityMembersApiClient(new AbenityConfiguration
        {
            UseProduction = false,
            Credentials = new AbenityConfiguration.ApiCredential
            {
                Key = "",
                Password = "",
                Username = ""
            },
            KeyPaths = new AbenityConfiguration.ClientKeys
            {
                PrivateKeyFilePath = "",
                PublicKeyFilePath = ""
            }
        }, new HttpClient());


        [Fact]
        public async Task Should_Login()
        {
            var result = await apiClient.AuthenticateUserAsync(new SsoRequest
            {
                Address = "1 Main St.",
                City = "Nashville",
                ClientUserId = "1",
                Country = "US",
                Email = "jane.doe@maildomain.com",
                Username = "jane.doe@maildomain.com",
                FirstName = "Jane",
                LastName = "Doe",
                SendWelcomeEmail = false,
                State = "TN",
                Zip = "37201"
            });
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.NotEmpty(result.Token);
        }
    }
}
