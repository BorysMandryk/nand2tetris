namespace JackCompiler
{
    internal class Token
    {
        public TokenType TokenType { get; init; }
        public object Value { get; init; }

        public Token(TokenType tokenType, object value)
        {
            TokenType = tokenType;
            Value = value;
        }
    }
}
