using System;

namespace DNSimplezilla
{
    public sealed class FetchExternalIpException : Exception
    {
        public FetchExternalIpException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}