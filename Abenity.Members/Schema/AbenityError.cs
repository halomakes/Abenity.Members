namespace Abenity.Members.Schema
{
    /// <summary>
    /// Error message returned from Abenity API
    /// </summary>
    public class AbenityError
    {
        public string Class { get; set; }
        public string Method { get; set; }

        public string Status { get; set; }
        public InternalError Error { get; set; }

        public class InternalError
        {
            public string Signature { get; set; }
        }
    }
}
