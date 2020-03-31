using Abenity.Members.Exceptions;
using Abenity.Members.Schema;
using System.Threading.Tasks;

namespace Abenity.Members
{
    /// <summary>
    /// A client for interacting with the Abenity Members API
    /// </summary>
    public interface IAbenityMembersApiClient
    {
        /// <summary>
        /// Authenticate an SSO user to Abenity
        /// </summary>
        /// <param name="request">Information about user to log in</param>
        /// <returns>SSO authentication result</returns>
        /// <exception cref="AbenityException">Error recieved from Abenity API</exception>
        Task<SsoResponse> AuthenticateUserAsync(SsoRequest request);

        /// <summary>
        /// Deactivate a user in Abenity
        /// </summary>
        /// <param name="userId">User ID of the client to deactivate</param>
        /// <param name="sendNotification">Indicates if the user should be notified of the deactivation</param>
        /// <exception cref="AbenityException">Error recieved from Abenity API</exception>
        Task DeactivateUserAsync(string userId, bool sendNotification = false);

        /// <summary>
        /// Reactivate a user in Abenity
        /// </summary>
        /// <param name="userId">User ID of the client to reactivate</param>
        /// <param name="sendNotification">Indicates if the user should be notified of the reactivation</param>
        /// <exception cref="AbenityException">Error recieved from Abenity API</exception>
        Task ReactivateUserAsync(string userId, bool sendNotification = false);
    }
}