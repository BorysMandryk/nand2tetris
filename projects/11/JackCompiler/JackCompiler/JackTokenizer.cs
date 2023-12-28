using System;
using System.IO;

namespace JackCompiler
{
    internal class JackTokenizer : IDisposable
    {
        private static readonly char[] _symbols =
        {
            '{',
            '}',
            '(',
            ')',
            '[',
            ']',
            '.',
            ',',
            ';',
            '+',
            '-',
            '*',
            '/',
            '&',
            '|',
            '<',
            '>',
            '=',
            '~'
        };

        private readonly StreamReader _streamReader;
        private Token? _currentToken;

        public string Filename { get; private set; }
        public int CurrentLine { get; private set; }
        public int CurrentSymbol { get; private set; }

        private bool _disposed = false;

        public JackTokenizer(string path)
        {
            Filename = path;
            _streamReader = new StreamReader(path);
        }

        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _streamReader.Dispose();

            _disposed = true;
        }

        public bool Advance()
        {
            _currentToken = GetNextToken();
            return _currentToken != null;
        }

        public Token GetCurrentToken()
        {
            return _currentToken;
        }

        private Token? GetNextToken()
        {
            if (_streamReader.EndOfStream)
            {
                return null;
            }

            char nextChar = Read();

            // Skip comments and whitespaces
            while (char.IsWhiteSpace(nextChar)
                || (nextChar == '/' && "/*".Contains((char)_streamReader.Peek())))
            {
                if (char.IsWhiteSpace(nextChar))
                {
                    SkipWhitespaces();
                }
                else
                {
                    SkipComment();
                }

                if (_streamReader.EndOfStream)
                {
                    return null;
                }
                nextChar = Read();
            }

            // Read a string constant
            if (nextChar == '"')
            {
                return new Token(TokenType.STRING_CONST, ReadString());
            }

            // Read a symbol
            if (Array.IndexOf(_symbols, nextChar) != -1)
            {
                return new Token(TokenType.SYMBOL, nextChar);
            }

            // Read an identifier
            if (char.IsLetter(nextChar) || nextChar == '_')
            {
                string token = nextChar + ReadIdentifier();
                if (Enum.TryParse(typeof(Keyword), token, true, out object result))
                {
                    return new Token(TokenType.KEYWORD, result);
                }
                else
                {
                    return new Token(TokenType.IDENTIFIER, token);
                }
            }

            // Read an integer constant
            if (char.IsDigit(nextChar))
            {
                return new Token(TokenType.INT_CONST, int.Parse(nextChar + ReadInt()));
            }

            throw new Exception($"Unsupported character {nextChar}");
        }

        private void SkipWhitespaces()
        {
            while (!_streamReader.EndOfStream && char.IsWhiteSpace((char)_streamReader.Peek()))
            {
                Read();
            }
        }

        private void SkipComment()
        {
            char ch = Read();
            if (ch == '/')
            {
                SkipOneLineComment();
            }
            else if (ch == '*')
            {
                SkipMultiLineComment();
            }
        }

        private void SkipOneLineComment()
        {
            ReadLine();
        }

        private void SkipMultiLineComment()
        {
            char ch = Read();
            while (ch != '*' || (char)_streamReader.Peek() != '/')
            {
                ch = Read();
            }
            Read();
        }

        private string ReadString()
        {
            string token = "";

            while (!_streamReader.EndOfStream && (char)_streamReader.Peek() != '"')
            {
                token += Read();
            }
            Read();
            return token;
        }

        private string ReadIdentifier()
        {
            char peek;
            string token = "";
            while (!_streamReader.EndOfStream
                && (char.IsLetterOrDigit(peek = (char)_streamReader.Peek()) || peek == '_'))
            {
                token += Read();
            }

            return token;
        }

        private string ReadInt()
        {
            string token = "";

            while (!_streamReader.EndOfStream && char.IsDigit((char)_streamReader.Peek()))
            {
                token += Read();
            }

            return token;
        }

        private char Read()
        {
            char ch = (char)_streamReader.Read();

            CurrentSymbol++;
            if (ch == '\n')
            {
                CurrentLine++;
                CurrentSymbol = 0;
            }
            return ch;
        }

        private string? ReadLine()
        {
            string? str = _streamReader.ReadLine();

            CurrentLine++;
            CurrentSymbol = 0;

            return str;
        }
    }
}
