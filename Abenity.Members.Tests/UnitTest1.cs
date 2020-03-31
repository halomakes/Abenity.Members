using Abenity.Members.Schema;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Abenity.Members.Tests
{
    public class UnitTest1
    {
        private readonly IAbenityMembersApiClient apiClient = new AbenityMembersApiClient(new AbenityConfiguration
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
        private readonly ITestOutputHelper output;

        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
        }

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
            output.WriteLine($"Successfully authenticated user: {result.Token}");
        }
    }
}
