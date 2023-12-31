﻿using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace JackAnalyzer
{
    internal class CompilationEngine : IDisposable
    {
        private JackTokenizer _tokenizer;
        private XmlTextWriter _xmlWriter;
        //private XmlWriter _xmlWriter;
        private LoggerTokenizer _logger;

        private bool _disposed = false;
        private static readonly char[] _binaryOperators =
            { '+', '-', '*', '/', '&', '|', '<', '>', '=' };
        private static readonly char[] _unaryOperators =
            { '-', '~' };

        public CompilationEngine(string input, string output)
        {
            _tokenizer = new JackTokenizer(input);
            _logger = new LoggerTokenizer(Console.Out, _tokenizer);
            //XmlWriterSettings settings = new XmlWriterSettings();
            //settings.Encoding = new UTF8Encoding(false);
            //settings.Indent = true;
            //settings.OmitXmlDeclaration = true;
            //settings.NewLineChars = Environment.NewLine;
            //settings.NewLineHandling = NewLineHandling.Entitize;
            //_xmlWriter = XmlWriter.Create(output, settings);
            _xmlWriter = new XmlTextWriter(output, new UTF8Encoding(false));
            _xmlWriter.Formatting = Formatting.Indented;
            Advance();
            CompileClass();
        }

        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _xmlWriter.Dispose();
            _tokenizer.Dispose();
            _logger.Dispose();
            _disposed = true;
        }

        public void CompileClass()
        {
            _xmlWriter.WriteStartElement("class");
            ExpectKeyword(Keyword.CLASS);

            ExpectIdentifier();
            ExpectSymbol('{');

            while (CheckKeyword(Keyword.STATIC, Keyword.FIELD))
            {
                CompileClassVarDec();
            }

            while (CheckKeyword(Keyword.CONSTRUCTOR, Keyword.FUNCTION, Keyword.METHOD))
            {
                CompileSubroutineDec();
            }

            ExpectSymbol('}');
            _xmlWriter.WriteFullEndElement();
        }

        public void CompileClassVarDec()
        {
            _xmlWriter.WriteStartElement("classVarDec");
            ExpectKeyword(Keyword.STATIC, Keyword.FIELD);

            // type
            if (!MatchIdentifier())
            {
                ExpectKeyword(Keyword.INT, Keyword.CHAR, Keyword.BOOLEAN);
            }

            // varName
            ExpectIdentifier();
            while (MatchSymbol(','))
            {
                ExpectIdentifier();
            }

            ExpectSymbol(';');
            _xmlWriter.WriteFullEndElement();
        }

        public void CompileSubroutineDec()
        {
            _xmlWriter.WriteStartElement("subroutineDec");
            ExpectKeyword(Keyword.CONSTRUCTOR, Keyword.FUNCTION, Keyword.METHOD);

            // type
            if (!MatchIdentifier())
            {
                ExpectKeyword(Keyword.VOID, Keyword.INT, Keyword.CHAR, Keyword.BOOLEAN);
            }

            ExpectIdentifier();
            ExpectSymbol('(');
            CompileParameterList();
            ExpectSymbol(')');

            CompileSubroutineBody();
            _xmlWriter.WriteFullEndElement();
        }

        public void CompileParameterList()
        {
            _xmlWriter.WriteStartElement("parameterList");
            if (MatchIdentifier() || MatchKeyword(Keyword.INT, Keyword.CHAR, Keyword.BOOLEAN))
            {
                ExpectIdentifier();
                while (MatchSymbol(','))
                {
                    if (!MatchIdentifier())
                    {
                        ExpectKeyword(Keyword.INT, Keyword.CHAR, Keyword.BOOLEAN);
                    }
                    ExpectIdentifier();
                }
            }
            _xmlWriter.WriteFullEndElement();
        }

        public void CompileVarDec()
        {
            _xmlWriter.WriteStartElement("varDec");
            if (MatchKeyword(Keyword.VAR))
            {
                if (!MatchIdentifier())
                {
                    ExpectKeyword(Keyword.INT, Keyword.CHAR, Keyword.BOOLEAN);
                }

                ExpectIdentifier();
                while (MatchSymbol(','))
                {
                    ExpectIdentifier();
                }
                ExpectSymbol(';');
            }
            _xmlWriter.WriteFullEndElement();
        }

        public void CompileSubroutineBody()
        {
            _xmlWriter.WriteStartElement("subroutineBody");
            ExpectSymbol('{');
            while (CheckKeyword(Keyword.VAR))
            {
                CompileVarDec();
            }
            CompileStatements();
            ExpectSymbol('}');
            _xmlWriter.WriteFullEndElement();
        }

        public void CompileStatements()
        {
            _xmlWriter.WriteStartElement("statements");
            while (true)
            {
                if (CheckKeyword(Keyword.LET))
                {
                    CompileLet();
                }
                else if (CheckKeyword(Keyword.IF))
                {
                    CompileIf();
                }
                else if (CheckKeyword(Keyword.WHILE))
                {
                    CompileWhile();
                }
                else if (CheckKeyword(Keyword.DO))
                {
                    CompileDo();
                }
                else if (CheckKeyword(Keyword.RETURN))
                {
                    CompileReturn();
                }
                else
                {
                    break;
                }
            }

            _xmlWriter.WriteFullEndElement();
        }

        public void CompileLet()
        {
            _xmlWriter.WriteStartElement("letStatement");
            ExpectKeyword(Keyword.LET);
            ExpectIdentifier();

            if (MatchSymbol('['))
            {
                CompileExpression();
                ExpectSymbol(']');
            }

            ExpectSymbol('=');
            CompileExpression();
            ExpectSymbol(';');
            _xmlWriter.WriteFullEndElement();
        }

        public void CompileIf()
        {
            _xmlWriter.WriteStartElement("ifStatement");
            ExpectKeyword(Keyword.IF);
            ExpectSymbol('(');
            CompileExpression();
            ExpectSymbol(')');
            ExpectSymbol('{');
            CompileStatements();
            ExpectSymbol('}');

            if (MatchKeyword(Keyword.ELSE))
            {
                ExpectSymbol('{');
                CompileStatements();
                ExpectSymbol('}');
            }
            _xmlWriter.WriteFullEndElement();
        }

        public void CompileWhile()
        {
            _xmlWriter.WriteStartElement("whileStatement");
            ExpectKeyword(Keyword.WHILE);
            ExpectSymbol('(');
            CompileExpression();
            ExpectSymbol(')');
            ExpectSymbol('{');
            CompileStatements();
            ExpectSymbol('}');
            _xmlWriter.WriteFullEndElement();
        }

        public void CompileDo()
        {
            _xmlWriter.WriteStartElement("doStatement");
            ExpectKeyword(Keyword.DO);

            ExpectIdentifier();
            if (MatchSymbol('('))
            {
                CompileExpressionList();
                ExpectSymbol(')');
            }
            else if (MatchSymbol('.'))
            {
                ExpectIdentifier();
                ExpectSymbol('(');
                CompileExpressionList();
                ExpectSymbol(')');
            }

            ExpectSymbol(';');
            _xmlWriter.WriteFullEndElement();
        }

        public void CompileReturn()
        {
            _xmlWriter.WriteStartElement("returnStatement");
            ExpectKeyword(Keyword.RETURN);

            if (!MatchSymbol(';'))
            {
                CompileExpression();
                ExpectSymbol(';');
            }

            _xmlWriter.WriteFullEndElement();
        }

        public void CompileExpression()
        {
            _xmlWriter.WriteStartElement("expression");

            CompileTerm();
            while (MatchSymbol(_binaryOperators))
            {
                CompileTerm();
            }
            _xmlWriter.WriteFullEndElement();
        }

        public void CompileTerm()
        {
            _xmlWriter.WriteStartElement("term");
            if (MatchInt()) { }
            else if (MatchString()) { }
            else if (MatchKeyword(Keyword.TRUE, Keyword.FALSE, Keyword.NULL, Keyword.THIS)) { }
            else if (MatchIdentifier())
            {
                if (MatchSymbol('['))
                {
                    CompileExpression();
                    ExpectSymbol(']');
                }
                else if (MatchSymbol('('))
                {
                    CompileExpressionList();
                    ExpectSymbol(')');
                }
                else if (MatchSymbol('.'))
                {
                    ExpectIdentifier();
                    ExpectSymbol('(');
                    CompileExpressionList();
                    ExpectSymbol(')');
                }
            }
            else if (MatchSymbol('('))
            {
                CompileExpression();
                ExpectSymbol(')');
            }
            else if (MatchSymbol(_unaryOperators))
            {
                CompileTerm();
            }
            else
            {
                _logger.Log();
            }
            _xmlWriter.WriteFullEndElement();
        }

        private void CompileExpressionList()
        {
            _xmlWriter.WriteStartElement("expressionList");
            if (!CheckSymbol(')'))
            {
                CompileExpression();
                while (MatchSymbol(','))
                {
                    CompileExpression();
                }
            }
            _xmlWriter.WriteFullEndElement();
        }

        private void ProcessKeyword()
        {
            XElement element = new XElement("keyword",
                $" {_tokenizer.GetKeywordString(_tokenizer.GetKeyword())} ");
            element.WriteTo(_xmlWriter);
        }

        private void ProcessSymbol(params char[] expectedSymbols)
        {
            XElement element = new XElement("symbol",
                $" {_tokenizer.GetSymbol()} ");
            element.WriteTo(_xmlWriter);
        }

        private void ProcessInt()
        {
            XElement element = new XElement("integerConstant",
                $" {_tokenizer.GetIntValue()} ");
            element.WriteTo(_xmlWriter);
        }

        private void ProcessString()
        {
            XElement element = new XElement("stringConstant",
                $" {_tokenizer.GetStringValue()} ");
            element.WriteTo(_xmlWriter);
        }

        private void ProcessIdentifier()
        {
            XElement element = new XElement("identifier",
                $" {_tokenizer.GetIdentifier()} ");
            element.WriteTo(_xmlWriter);
        }

        private bool MatchKeyword(params Keyword[] keywords)
        {
            if (TokenType.KEYWORD == _tokenizer.GetTokenType()
                && keywords.Contains(_tokenizer.GetKeyword()))
            {
                ProcessKeyword();
                Advance();
                return true;
            }
            return false;
        }

        private bool MatchSymbol(params char[] symbols)
        {
            if (TokenType.SYMBOL == _tokenizer.GetTokenType()
                && symbols.Contains(_tokenizer.GetSymbol()))
            {
                ProcessSymbol();
                Advance();
                return true;
            }
            return false;
        }

        private bool MatchIdentifier()
        {
            if (TokenType.IDENTIFIER == _tokenizer.GetTokenType())
            {
                ProcessIdentifier();
                Advance();
                return true;
            }
            return false;
        }

        private bool MatchString()
        {
            if (TokenType.STRING_CONST == _tokenizer.GetTokenType())
            {
                ProcessString();
                Advance();
                return true;
            }
            return false;
        }

        private bool MatchInt()
        {
            if (TokenType.INT_CONST == _tokenizer.GetTokenType())
            {
                ProcessInt();
                Advance();
                return true;
            }
            return false;
        }

        private void ExpectKeyword(params Keyword[] keywords) 
        {
            if (!MatchKeyword(keywords))
            {
                _logger.Log();
            }
        }

        private void ExpectSymbol(params char[] symbols)
        {
            if (!MatchSymbol(symbols))
            {
                _logger.Log();
            }
        }

        private void ExpectIdentifier()
        {
            if (!MatchIdentifier())
            {
                _logger.Log();
            }
        }

        private bool CheckKeyword(params Keyword[] keywords)
        {
            return TokenType.KEYWORD == _tokenizer.GetTokenType()
                && keywords.Contains(_tokenizer.GetKeyword());
        }

        private bool CheckSymbol(params char[] symbols)
        {
            return TokenType.SYMBOL == _tokenizer.GetTokenType()
                && symbols.Contains(_tokenizer.GetSymbol());
        }

        private void Advance()
        {
            _tokenizer.Advance();
        }
    }
}
