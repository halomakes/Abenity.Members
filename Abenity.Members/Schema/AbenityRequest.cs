using Abenity.Members.Serialization;

namespace Abenity.Members.Schema
{
    internal abstract class AbenityRequest
    {
        public AbenityRequest(AbenityConfiguration.ApiCredential credentials)
        {
            Username = credentials.Username;
            Password = credentials.Password;
            Key = credentials.Key;
        }

        [FormName("api_username")]
        public string Username { get; }

        [FormName("api_password")]
        public string Password { get; }

        [FormName("api_key")]
        public string Key { get; }
    }
}
