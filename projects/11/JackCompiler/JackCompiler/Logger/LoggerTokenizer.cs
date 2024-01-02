using JackCompiler.SyntaxAnalyzer;
using System.IO;

namespace JackCompiler.Logger
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
            Token token = _tokenizer.GetCurrentToken();
            Log($"{_tokenizer.Filename} " +
                $"({_tokenizer.CurrentLine}, {_tokenizer.CurrentSymbol}): " +
                $"Unexpected token {token.Value} ");
        }
    }
}
