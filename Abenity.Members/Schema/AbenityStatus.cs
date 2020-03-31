using System.Runtime.Serialization;

namespace Abenity.Members.Schema
{
    internal enum AbenityStatus
    {
        [EnumMember(Value = "fail")]
        Failure,
        [EnumMember(Value = "ok")]
        Ok
    }
}
