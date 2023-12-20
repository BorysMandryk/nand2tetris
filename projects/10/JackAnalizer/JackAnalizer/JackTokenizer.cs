using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JackAnalizer
{
    internal class JackTokenizer : IDisposable
    {
        //private static readonly string[] _keywords = new string[]
        //{
        //    "class", "constructor", "function", "method", "field", "static",
        //    "var", "int", "char", "boolean", "void", "true", "false",
        //    "null", "this", "let", "do", "if", "else", "while", "return"
        //};
        private static readonly char[] _symbols =
        [
            '{', '}', '(', ')', '[', ']', '.', ',', ';', '+',
            '-', '*', '/','&', '|', '<', '>', '=', '~'
        ];
        //private static readonly Regex _identifierRegex = new Regex(@"^(\D)([a-zA-Z0-9_]*)$");
        private static readonly Dictionary<string, Keyword> _stringKeywordDict = new Dictionary<string, Keyword>()
        {
            { "class", Keyword.CLASS},
            { "constructor", Keyword.CONSTRUCTOR},
            { "function", Keyword.FUNCTION},
            { "method", Keyword.METHOD},
            { "field", Keyword.FIELD},
            { "static", Keyword.STATIC},
            { "var", Keyword.VAR},
            { "int", Keyword.INT},
            { "char", Keyword.CHAR},
            { "boolean", Keyword.BOOLEAN},
            { "void", Keyword.VOID},
            { "true", Keyword.TRUE},
            { "false", Keyword.FALSE},
            { "null", Keyword.NULL},
            { "this", Keyword.THIS},
            { "let", Keyword.LET},
            { "do", Keyword.DO},
            { "if", Keyword.IF},
            { "else", Keyword.ELSE},
            { "while", Keyword.WHILE},
            { "return", Keyword.RETURN}
        };

        private readonly StreamReader _streamReader;
        private string? _currentToken, _nextToken;
        private TokenType _currentTokenType, _nextTokenType;

        public string Filename { get; private set; }
        //private int _currentLine, _currentSymbol;
        public int CurrentLine { get; private set; }
        public int CurrentSymbol { get; private set; }

        private bool _disposed = false;

        public JackTokenizer(string path)
        {
            Filename = path;
            _streamReader = new StreamReader(path);
            _nextToken = GetNextToken();
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

        // TODO Delete???? Architecture
        public string GetKeywordString(Keyword keyword)
        {
            return _stringKeywordDict.First(x => x.Value == keyword).Key;
        }

        public bool HasMoreTokens()
        {
            return _nextToken != null;
        }

        public void Advance()
        {
            _currentToken = _nextToken;
            _currentTokenType = _nextTokenType;
            _nextToken = GetNextToken();
        }

        public TokenType GetTokenType()
        {
            return _currentTokenType;
        }

        public Keyword GetKeyword()
        {
            return _stringKeywordDict[_currentToken];
        }

        public char GetSymbol()
        {
            return _currentToken[0];    // TODO char.Parse() ???
        }

        public string GetIdentifier()
        {
            return _currentToken;
        }

        public int GetIntValue()
        {
            return int.Parse(_currentToken);    // TODO TryParse??
        }

        public string GetStringValue()
        {
            return _currentToken;
        }

        private string? GetNextToken()
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
                if(char.IsWhiteSpace(nextChar))
                {
                    SkipWhitespaces();
                }
                else
                {
                    SkipComment();
                }

                if(_streamReader.EndOfStream)
                {
                    return null;
                }
                nextChar = Read();
            }


            // Read a string constant
            if (nextChar == '"')
            {
                _nextTokenType = TokenType.STRING_CONST;
                return ReadString();
            }

            // Read a symbol
            if (Array.IndexOf(_symbols, nextChar) != -1)
            {
                _nextTokenType = TokenType.SYMBOL;
                return nextChar.ToString();
            }

            // Read an identifier
            if (char.IsLetter(nextChar) || nextChar == '_')
            {
                string token = nextChar + ReadIdentifier();
                if(_stringKeywordDict.Keys.Contains(token))
                {
                    _nextTokenType = TokenType.KEYWORD;
                }
                else
                {
                    _nextTokenType = TokenType.IDENTIFIER;
                }

                return token;
            }
            
            // Read an integer constant
            if(char.IsDigit(nextChar))
            {
                // _nextTokenType = INT
                _nextTokenType = TokenType.INT_CONST;
                return nextChar + ReadInt();
            }

            throw new Exception($"Unsupported character {nextChar}");   // TODO Exception type 
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
            else if(ch == '*')
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

            while(!_streamReader.EndOfStream && (char)_streamReader.Peek() != '"')
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

        private char Read()     // Not current line and symbol but next
        {
            char ch = (char)_streamReader.Read();

            CurrentSymbol++;
            if (ch == '\n')  // TODO not Environment.NewLine() ?????
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
