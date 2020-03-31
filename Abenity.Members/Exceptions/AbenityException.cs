using Abenity.Members.Schema;
using System;

namespace Abenity.Members.Exceptions
{
    /// <summary>
    /// An exception spawned from the Abenity API
    /// </summary>
    public class AbenityException : Exception
    {
        /// <summary>
        /// Error content receieved from Abenity API
        /// </summary>
        public AbenityError ApiError { get; set; }

        public override string Message => $"Recieved error response from Abenity API: {ApiError.Error?.Signature}";
    }
}
