using Abenity.Members.Serialization;
using System;
using System.ComponentModel.DataAnnotations;

namespace Abenity.Members.Schema
{
    /// <summary>
    /// Information to authenticate a user for Single Sign-On
    /// </summary>
    public class SsoRequest
    {
        [FormName("creation_time", Encode = true)]
        public string _creationTime => DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");

        /// <summary>
        /// A random integer used to increase message entropy
        /// </summary>
        [FormName("salt")]
        public int _salt => new Random().Next();

        /// <summary>
        /// Indicates if a welcome email should be sent to the user upon registration
        /// </summary>
        [FormName("send_welcome_email")]
        public bool SendWelcomeEmail { get; set; }

        /// <summary>
        /// An immutable, unique ID for the member within your system
        /// </summary>
        /// <remarks>A hash of this value is stored by Abenity to determine if the member is new or returning. This is how your member is uniquely identified.</remarks>
        [FormName("client_user_id"), Required]
        public string ClientUserId { get; set; }

        /// <summary>
        /// The member's email address
        /// </summary>
        /// <remarks>This will also be used for the member's username if possible.</remarks>
        [FormName("email", Encode = true), Required, EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Unique username for user
        /// </summary>
        /// <remarks>This may be set to override the default setting of using the member's email address as their username. The username must be unique among all Abenity members (not just within your program).</remarks>
        [FormName("username", Encode = true)]
        public string Username { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        [FormName("firstname", Encode = true)]
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        [FormName("lastname", Encode = true)]
        public string LastName { get; set; }

        /// <summary>
        /// User's street address
        /// </summary>
        [FormName("address", Encode = true)]
        public string Address { get; set; }

        /// <summary>
        /// User's city/locality
        /// </summary>
        [FormName("city", Encode = true)]
        public string City { get; set; }

        /// <summary>
        /// User's state/region
        /// </summary>
        [FormName("state", Encode = true)]
        public string State { get; set; }

        /// <summary>
        /// User's ZIP/postal code
        /// </summary>
        /// <remarks>The format of this value will be validated against the Country value.</remarks>
        [FormName("zip", Encode = true)]
        public string Zip { get; set; }

        /// <summary>
        /// User's country code
        /// </summary>
        /// <remarks>Formatted as ISO 3166-1 alpha-2 (e.g. US).</remarks>
        [FormName("country", Encode = true)]
        public string Country { get; set; }

        /// <summary>
        /// Whether to enroll member in the monthly Spotlight email
        /// </summary>
        [FormName("spotlight")]
        public bool Spotlight { get; set; }

        /// <summary>
        /// A custom password for the user
        /// </summary>
        /// <remarks>By default a randomized password will be set. For security, passwords are never communicated to users. Password requirements are subject to change</remarks>
        [FormName("password", Encode = true), Obsolete]
        public string Password { get; set; }

        [FormName("registration_code", Encode = true)]
        public string RegistrationCode { get; set; }
    }
}
