namespace Abenity.Members
{
    /// <summary>
    /// Configuration values for the Abenity Members API Client
    /// </summary>
    public class AbenityConfiguration
    {
        /// <summary>
        /// Indicates if the client should target production or the sandbox
        /// </summary>
        public bool UseProduction { get; set; }

        /// <summary>
        /// Paths to encryption key files
        /// </summary>
        public ClientKeys KeyPaths { get; set; }

        /// <summary>
        /// API consumer credentials
        /// </summary>
        public ApiCredential Credentials { get; set; }

        /// <summary>
        /// API consumer credentials
        /// </summary>
        public class ApiCredential
        {
            /// <summary>
            /// Client username
            /// </summary>
            public string Username { get; set; }

            /// <summary>
            /// Client password
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// Client API key
            /// </summary>
            public string Key { get; set; }
        }

        /// <summary>
        /// Paths to encryption key files
        /// </summary>
        public class ClientKeys
        {
            /// <summary>
            /// Path to location of private key file
            /// </summary>
            /// <remarks>.pem file</remarks>
            public string PrivateKeyFilePath { get; set; }

            /// <summary>
            /// Path to location of public key file
            /// </summary>
            /// <remarks>.pem file</remarks>
            public string PublicKeyFilePath { get; set; }
        }
    }
}
