﻿using Abenity.Members.Encryption;
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
            await PostAsync<SsoResponse>("/v2/client/sso_member.json", await CreateSsoBody(request));

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
            using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
            {
                request.Headers.UserAgent.ParseAdd("abenity/abenity-members-netstandard");
                request.Content = content;
                var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                var stream = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = DeserializeJsonResponse<AbenityResponse<TResponse>>(stream);
                    if (result.Status == AbenityStatus.Ok)
                    {
                        return result.Data;
                    }
                }

                throw new AbenityException
                {
                    ApiError = DeserializeJsonResponse<AbenityError>(stream)
                };
            }
        }

        private async Task PostAsync(string endpoint, HttpContent content)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, endpoint))
            {
                request.Headers.UserAgent.ParseAdd("abenity/abenity-members-netstandard");
                request.Content = content;
                var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                var stream = await response.Content.ReadAsStreamAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = DeserializeJsonResponse<AbenityResponse>(stream);
                    if (result.Status == AbenityStatus.Ok)
                    {
                        return;
                    }
                }

                throw new AbenityException
                {
                    ApiError = DeserializeJsonResponse<AbenityError>(stream)
                };
            }
        }

        private async Task<HttpContent> CreateSsoBody<TPayload>(TPayload payload)
        {
            var encryptedPayload = await EncryptPayloadWithDes(payload);
            var request = new AbenitySsoRequest(config.Credentials)
            {
                EncryptedPayload = encryptedPayload.EncodedMessage,
                Signature = GetSignature(encryptedPayload),
                Cipher = GetCiper(encryptedPayload),
                Iv = BaseEncode(encryptedPayload.Iv)
            };
            return CreateHttpContent(request);
        }


        private async Task<EncryptedPayload> EncryptPayloadWithDes<TPayload>(TPayload payload)
        {
            // Uses CBC by default.
            // https://msdn.microsoft.com/en-us/library/system.security.cryptography.symmetricalgorithm.mode(v=vs.110).aspx
            using (var des = new TripleDESCryptoServiceProvider())
            {
                var key = des.Key;
                var iv = des.IV;
                var encryptor = des.CreateEncryptor();

                var data = await CreateHttpContent(payload).ReadAsByteArrayAsync();
                var resultArray = encryptor.TransformFinalBlock(data, 0, data.Length);
                des.Clear();
                return new EncryptedPayload
                {
                    EncodedMessage = BaseEncode(resultArray),
                    Iv = iv,
                    Key = key
                };
            }
        }

        private string GetSignature(EncryptedPayload payload)
        {
            byte[] data = Encoding.UTF8.GetBytes(payload.EncodedMessage);
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

        private string GetCiper(EncryptedPayload payload)
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

        private static FormUrlEncodedContent CreateHttpContent<TPayload>(TPayload content)
        {
            var data = FormSerializer<TPayload>.Serialize(content);
            return new FormUrlEncodedContent(data);
        }

        private static TModel DeserializeJsonResponse<TModel>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
            {
                return default;
            }

            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                var js = new JsonSerializer();
                var searchResult = js.Deserialize<TModel>(jtr);
                return searchResult;
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

        private string BaseEncode(string input) => $"{WebUtility.UrlEncode(Convert.ToBase64String(Encoding.UTF8.GetBytes(input)))}decode";

        private string BaseEncode(byte[] input) => $"{WebUtility.UrlEncode(Convert.ToBase64String(input))}decode";

        private static void SetSecurityProtocol() =>
           ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
    }
}
