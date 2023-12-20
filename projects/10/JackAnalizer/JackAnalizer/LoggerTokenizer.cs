namespace JackAnalizer
{
    internal class LoggerTokenizer : Logger
    {
        private JackTokenizer _tokenizer;

        public LoggerTokenizer(TextWriter output, JackTokenizer tokenizer)
            : base(output)
        {
            _tokenizer = tokenizer;
        }

        public void Log()
        {
            Log($"{_tokenizer.Filename} " +
                $"({_tokenizer.CurrentLine}, {_tokenizer.CurrentSymbol}): " +
                $"Unexpected token type {_tokenizer.GetTokenType()} ");
        }
    }
}
