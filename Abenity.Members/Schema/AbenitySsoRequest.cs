using Abenity.Members.Serialization;

namespace Abenity.Members.Schema
{
    internal class AbenitySsoRequest : AbenityRequest
    {
        public AbenitySsoRequest(AbenityConfiguration.ApiCredential credentials) : base(credentials) { }

        [FormName("Payload")]
        public string EncryptedPayload { get; set; }

        [FormName("Signature")]
        public string Signature { get; set; }

        [FormName("Cipher")]
        public string Cipher { get; set; }

        [FormName("Iv")]
        public string Iv { get; set; }
    }
}
