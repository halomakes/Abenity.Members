using Abenity.Members.Serialization;

namespace Abenity.Members.Schema
{
    internal class AbenityChangeActivationRequest : AbenityRequest
    {
        public AbenityChangeActivationRequest(AbenityConfiguration.ApiCredential credentials) : base(credentials) { }

        [FormName("client_user_id")]
        public string UserId { get; set; }

        [FormName("send_notification")]
        public bool SendNotification { get; set; }
    }
}
