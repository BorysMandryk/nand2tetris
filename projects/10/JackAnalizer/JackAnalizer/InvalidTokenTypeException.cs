using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace JackAnalizer
{
    [Serializable]
    internal class InvalidTokenTypeException : Exception
    {
        public TokenType TokenType { get; set; }

        public InvalidTokenTypeException()
        {
        }

        public InvalidTokenTypeException(string? message) : base(message)
        {
        }

        public InvalidTokenTypeException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected InvalidTokenTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidTokenTypeException(TokenType tokenType)
        {
            TokenType = tokenType;
        }

        public override string Message => $"{base.Message} Unexpected token type {TokenType}";
    }
}