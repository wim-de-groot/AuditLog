using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AuditLog
{
    [Serializable]
    [ExcludeFromCodeCoverage]
    public class InvalidEnvironmentException : Exception
    {
        public InvalidEnvironmentException(string message) : base(message)
        {
        }

        protected InvalidEnvironmentException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}