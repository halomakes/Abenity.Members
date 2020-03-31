namespace Abenity.Members
{
    public class AbenityConfiguration
    {
        public bool UseProduction { get; set; }
        public ClientKeys KeyPaths { get; set; }
        public ApiCredential Credentials { get; set; }


        public class ApiCredential
        {
            public string Username { get; private set; }
            public string Password { get; private set; }
            public string Key { get; private set; }

        }

        public class ClientKeys
        {
            public string PrivateKeyFilePath { get; set; }
            internal string PublicKeyFilePath { get; set; }
        }
    }
}
