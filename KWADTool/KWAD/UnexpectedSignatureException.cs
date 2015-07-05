using System;

namespace KWADTool.Kwad
{
    public class UnexpectedSignatureException : Exception
    {
        public UnexpectedSignatureException()
        {
        }

        public UnexpectedSignatureException(string message) : base(message)
        {
        }

        public UnexpectedSignatureException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}