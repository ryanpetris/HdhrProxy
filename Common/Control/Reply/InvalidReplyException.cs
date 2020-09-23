using System;

namespace HdhrProxy.Common.Control.Reply
{
    public class InvalidReplyException : Exception
    {
        public InvalidReplyException(string message) : base(message)
        {
            
        }

        public InvalidReplyException(string message, Exception innerException) : base(message, innerException)
        {
            
        }
    }
}