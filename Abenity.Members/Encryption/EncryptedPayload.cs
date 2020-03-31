namespace Abenity.Members.Encryption
{
    internal class EncryptedPayload
    {
        public string EncodedMessage { get; set; }
        public byte[] Key { get; set; }
        public byte[] Iv { get; set; }
    }
}
