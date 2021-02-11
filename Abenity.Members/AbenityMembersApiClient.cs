using Abenity.Members.Encryption;
using Abenity.Members.Exceptions;
using Abenity.Members.Schema;
using Abenity.Members.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Abenity.Members
{
    /// <summary>
    /// A client for interacting with the Abenity Members API
    /// </summary>
    public class AbenityMembersApiClient : IAbenityMembersApiClient
    {
        protected static JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Include,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        private readonly AbenityConfiguration config;
        private readonly HttpClient httpClient;

        /// <summary>
        /// Create an instance of the Abenity Members API Client
        /// </summary>
        /// <param name="config">Configuration for the Abenity API</param>
        /// <param name="httpClient">HTTP Client to use for API invokations</param>
        /// <remarks>Recommended to use with Dependency Injection and an IHttpClientFactory</remarks>
        public AbenityMembersApiClient(AbenityConfiguration config, HttpClient httpClient)
        {
            this.config = config;
            this.httpClient = httpClient;
            this.httpClient.BaseAddress = config.UseProduction
                ? new Uri("https://api.abenity.com")
                : new Uri("https://sandbox.abenity.com");
        }

        /// <summary>
        /// Authenticate an SSO user to Abenity
        /// </summary>
        /// <param name="request">Information about user to log in</param>
        /// <returns>SSO authentication result</returns>
        /// <exception cref="AbenityException">Error recieved from Abenity API</exception>
        public async Task<SsoResponse> AuthenticateUserAsync(SsoRequest request) =>
            await PostAsync<SsoResponse>("/v2/client/sso_member.json", CreateSsoBody(request));

        /// <summary>
        /// Deactivate a user in Abenity
        /// </summary>
        /// <param name="userId">User ID of the client to deactivate</param>
        /// <param name="sendNotification">Indicates if the user should be notified of the deactivation</param>
        /// <exception cref="AbenityException">Error recieved from Abenity API</exception>
        public async Task DeactivateUserAsync(string userId, bool sendNotification = false) =>
            await PostAsync("/v2/client/deactivate_member.json", CreateHttpContent(new AbenityChangeActivationRequest(config.Credentials)
            {
                UserId = userId,
                SendNotification = sendNotification
            }));

        /// <summary>
        /// Reactivate a user in Abenity
        /// </summary>
        /// <param name="userId">User ID of the client to reactivate</param>
        /// <param name="sendNotification">Indicates if the user should be notified of the reactivation</param>
        /// <exception cref="AbenityException">Error recieved from Abenity API</exception>
        public async Task ReactivateUserAsync(string userId, bool sendNotification = false) =>
            await PostAsync("/v2/client/reactivate_member.json", CreateHttpContent(new AbenityChangeActivationRequest(config.Credentials)
            {
                UserId = userId,
                SendNotification = sendNotification
            }));


        private async Task<TResponse> PostAsync<TResponse>(string endpoint, HttpContent content)
        {
            SetSecurityProtocol();
            using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
            {
                request.Headers.UserAgent.ParseAdd("abenity/abenity-members-netstandard");
                request.Content = content;
                var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                var stream = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = DeserializeJsonStream<AbenityResponse<TResponse>>(stream);
                    if (result.Status == AbenityStatus.Ok)
                    {
                        return result.Data;
                    }
                }

                var responseContent = await StreamToStringAsync(stream);
                throw new AbenityException
                {
                    RawResponse = responseContent,
                    ApiError = JsonConvert.DeserializeObject<AbenityError>(responseContent)
                };
            }
        }

        private async Task PostAsync(string endpoint, HttpContent content)
        {
            SetSecurityProtocol();
            using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
            {
                request.Headers.UserAgent.ParseAdd("abenity/abenity-members-netstandard");
                request.Content = content;
                var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                var stream = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = DeserializeJsonStream<AbenityResponse>(stream);
                    if (result.Status == AbenityStatus.Ok)
                    {
                        return;
                    }
                }

                var responseContent = await StreamToStringAsync(stream);
                throw new AbenityException
                {
                    RawResponse = responseContent,
                    ApiError = JsonConvert.DeserializeObject<AbenityError>(responseContent)
                };
            }
        }

        private HttpContent CreateSsoBody<TPayload>(TPayload payload)
        {
            var encryptedPayload = EncryptPayloadWithDes(payload);
            var request = new AbenitySsoRequest(config.Credentials)
            {
                EncryptedPayload = BaseEncode(encryptedPayload.EncodedMessage),
                Signature = GetSignature(encryptedPayload),
                Cipher = GetCipher(encryptedPayload),
                Iv = BaseEncode(encryptedPayload.Iv)
            };
            return CreateHttpContent(request);
        }

        private EncryptedPayload EncryptPayloadWithDes<TPayload>(TPayload payload)
        {
            // Uses CBC by default.
            // https://msdn.microsoft.com/en-us/library/system.security.cryptography.symmetricalgorithm.mode(v=vs.110).aspx
            using (var des = new TripleDESCryptoServiceProvider())
            {
                var key = des.Key;
                var iv = des.IV;
                var encryptor = des.CreateEncryptor();

                var data = FormSerializer<TPayload>.AsString(payload).ToByteArray();
                var resultArray = encryptor.TransformFinalBlock(data, 0, data.Length);
                des.Clear();
                return new EncryptedPayload
                {
                    EncodedMessage = resultArray,
                    Iv = iv,
                    Key = key
                };
            }
        }

        private string GetSignature(EncryptedPayload payload)
        {
            byte[] data = payload.EncodedMessage.ToBase64().ToByteArray();
            using (var fileReader = File.OpenText(config.KeyPaths.PrivateKeyFilePath))
            {
                var pemReader = new PemReader(fileReader);
                var key = (AsymmetricCipherKeyPair)pemReader.ReadObject();
                ISigner signer = SignerUtilities.GetSigner("MD5WithRSA");
                signer.Init(true, key.Private);
                signer.BlockUpdate(data, 0, data.Length);

                return BaseEncode(signer.GenerateSignature());
            }
        }

        private string GetCipher(EncryptedPayload payload)
        {
            using (var fileReader = File.OpenText(config.KeyPaths.PublicKeyFilePath))
            {
                var pemReader = new PemReader(fileReader);
                var key = (AsymmetricKeyParameter)pemReader.ReadObject();

                var engine = new Pkcs1Encoding(new RsaEngine());
                engine.Init(true, key);

                var data = engine.ProcessBlock(payload.Key, 0, payload.Key.Length);

                return BaseEncode(data);
            }
        }

        private static HttpContent CreateHttpContent<TPayload>(TPayload content)
        {
            var data = FormSerializer<TPayload>.AsString(content);
            return new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");
        }

        private static TModel DeserializeJsonStream<TModel>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
            {
                return default;
            }

            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                var js = new JsonSerializer();
                var model = js.Deserialize<TModel>(jtr);
                return model;
            }
        }

        private static async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
            {
                using (var sr = new StreamReader(stream))
                {
                    content = await sr.ReadToEndAsync();
                }
            }

            return content;
        }

        private string BaseEncode(string input) => $"{input.ToByteArray().ToBase64().ToUrlEncoded()}decode";

        private string BaseEncode(byte[] input) => $"{input.ToBase64().ToUrlEncoded()}decode";

        private static void SetSecurityProtocol() =>
           ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
    }
}
