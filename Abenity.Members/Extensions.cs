using System;
using System.Net;
using System.Text;

namespace Abenity.Members
{
    internal static class Extensions
    {
        public static byte[] ToByteArray(this string s) => Encoding.UTF8.GetBytes(s);

        public static string ToBase64(this byte[] s) => Convert.ToBase64String(s);

        public static string ToUrlEncoded(this string s) => WebUtility.UrlEncode(s);
    }
}
