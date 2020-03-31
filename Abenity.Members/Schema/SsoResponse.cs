using Newtonsoft.Json;
using System;

namespace Abenity.Members.Schema
{
    /// <summary>
    /// Successful response for SSO Authentication
    /// </summary>
    public class SsoResponse
    {
        /// <summary>
        /// Authentication token for user
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; set; }

        /// <summary>
        /// Time when the token will expire
        /// </summary>
        [JsonProperty("token_expiration")]
        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// Url to direct the user to on the frontend
        /// </summary>
        [JsonProperty("token_url")]
        public Uri Url { get; set; }
    }
}
