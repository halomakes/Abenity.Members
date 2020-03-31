namespace Abenity.Members.Schema
{
    internal class AbenityResponse<TPayload>
    {
        public string Method { get; set; }
        public AbenityStatus Status { get; set; }
        public TPayload Data { get; set; }
    }

    internal class AbenityResponse
    {
        public string Method { get; set; }
        public AbenityStatus Status { get; set; }
        public string Class { get; set; }
    }
}
