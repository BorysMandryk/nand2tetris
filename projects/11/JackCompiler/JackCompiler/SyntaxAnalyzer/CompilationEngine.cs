using System;
using System.Collections.Generic;
using System.Linq;
using JackCompiler.CodeWriter;
using JackCompiler.Logger;
using JackCompiler.SymbolTable;

namespace JackCompiler.SyntaxAnalyzer
{
    internal class CompilationEngine : IDisposable
    {
        private JackTokenizer _tokenizer;
        private VMWriter _vmWriter;
        private SymbolTableHandler _symbolTable;
        private LoggerTokenizer _logger;

        private Token? _currentToken;
        private bool _disposed = false;
        private static readonly char[] _binaryOperators =
            { '+', '-', '*', '/', '&', '|', '<', '>', '=' };
        private static readonly char[] _unaryOperators =
            { '-', '~' };

        private static readonly Dictionary<VariableKind, MemorySegment> _kindToSegment
            = new Dictionary<VariableKind, MemorySegment>()
            {
                {VariableKind.VAR, MemorySegment.LOCAL },
                {VariableKind.STATIC, MemorySegment.STATIC },
                {VariableKind.FIELD, MemorySegment.THIS },
                {VariableKind.ARG, MemorySegment.ARG }
            };

        private string _className;
        private int _ifCnt = 0;
        private int _whileCnt = 0;

        public CompilationEngine(string input, string output)
        {
            _tokenizer = new JackTokenizer(input);
            _logger = new LoggerTokenizer(Console.Out, _tokenizer);
            _vmWriter = new VMWriter(output);
            Advance();
            CompileClass();
        }

        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _tokenizer.Dispose();
            _logger.Dispose();
            _vmWriter.Dispose();
            _disposed = true;
        }

        private void CompileClass()
        {
            ExpectKeyword(Keyword.CLASS);
            _className = ExpectIdentifier();
            ExpectSymbol('{');
            _symbolTable = new SymbolTableHandler();

            while (CheckKeyword(Keyword.STATIC, Keyword.FIELD))
            {
                CompileClassVarDec();
            }

            while (CheckKeyword(Keyword.CONSTRUCTOR, Keyword.FUNCTION, Keyword.METHOD))
            {
                CompileSubroutineDec();
            }

            ExpectSymbol('}');
        }

        private void CompileClassVarDec()
        {
            VariableKind kind;
            if (MatchKeyword(out _, Keyword.STATIC))
            {
                kind = VariableKind.STATIC;
            }
            else
            {
                ExpectKeyword(Keyword.FIELD);
                kind = VariableKind.FIELD;
            }

            string? type = CompileType(Keyword.INT, Keyword.CHAR, Keyword.BOOLEAN);

            do
            {
                string? name = ExpectIdentifier();
                
                _symbolTable.Define(name, kind, type);

            }
            while (MatchSymbol(out _, ','));

            ExpectSymbol(';');
        }

        private void CompileSubroutineDec()
        {
            _symbolTable.StartSubroutine();

            Keyword subroutineType = ExpectKeyword(Keyword.CONSTRUCTOR, Keyword.FUNCTION, Keyword.METHOD);
            if(subroutineType == Keyword.METHOD)
            {
                _symbolTable.Define("this", VariableKind.ARG, _className);
            }

            CompileType(Keyword.VOID, Keyword.INT, Keyword.CHAR, Keyword.BOOLEAN);

            string subroutineName = ExpectIdentifier();
            ExpectSymbol('(');
            CompileParameterList();
            ExpectSymbol(')');

            CompileSubroutineBody($"{_className}.{subroutineName}", subroutineType);
        }

        private void CompileParameterList()
        {
            if (!CheckSymbol(')'))
            {
                do
                {
                    string? type = CompileType(Keyword.INT, Keyword.CHAR, Keyword.BOOLEAN);
                    string paramName = ExpectIdentifier();
                    _symbolTable.Define(paramName, VariableKind.ARG, type);
                }
                while (MatchSymbol(out _, ','));
            }
        }

        private void CompileVarDec()
        {
            if (MatchKeyword(out _, Keyword.VAR))
            {
                string? type = CompileType(Keyword.INT, Keyword.CHAR, Keyword.BOOLEAN);

                do
                {
                    string? varName = ExpectIdentifier();
                    _symbolTable.Define(varName, VariableKind.VAR, type);
                } while (MatchSymbol(out _, ','));
                ExpectSymbol(';');
            }
            return;
        }

        private void CompileSubroutineBody(string subroutineName, Keyword subroutineType)
        {
            ExpectSymbol('{');

            while (CheckKeyword(Keyword.VAR))
            {
                CompileVarDec();
            }

            int nLocals = _symbolTable.VarCount(VariableKind.VAR);

            if (subroutineType == Keyword.METHOD)
            {
                _vmWriter.WriteFunction(subroutineName, nLocals); 
                _vmWriter.WritePush(MemorySegment.ARG, 0);        
                _vmWriter.WritePop(MemorySegment.POINTER, 0);     
            }
            else if (subroutineType == Keyword.CONSTRUCTOR)
            {
                _vmWriter.WriteFunction(subroutineName, nLocals);
                _vmWriter.WritePush(MemorySegment.CONST, _symbolTable.VarCount(VariableKind.FIELD)); 
                _vmWriter.WriteCall("Memory.alloc", 1);          
                _vmWriter.WritePop(MemorySegment.POINTER, 0);
            }
            else
            {
                _vmWriter.WriteFunction(subroutineName, nLocals);  
            }

            CompileStatements();
            ExpectSymbol('}');
        }

        private void CompileStatements()
        {
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

        }

        private void CompileLet()
        {
            ExpectKeyword(Keyword.LET);
            string? varName = ExpectIdentifier();
            VariableKind kind = _symbolTable.KindOf(varName);
            int index = _symbolTable.IndexOf(varName);

            bool isArray = false;
            if (MatchSymbol(out _, '['))
            {
                isArray = true;
                _vmWriter.WritePush(_kindToSegment[kind], index);
                CompileExpression();
                _vmWriter.WriteArithmetic(ArithmeticCommand.ADD);
                ExpectSymbol(']');
            }

            ExpectSymbol('=');
            CompileExpression();

            if (isArray)
            {
                _vmWriter.WritePop(MemorySegment.TEMP, 0);
                _vmWriter.WritePop(MemorySegment.POINTER, 1);
                _vmWriter.WritePush(MemorySegment.TEMP, 0);
                _vmWriter.WritePop(MemorySegment.THAT, 0);
            }
            else
            {
                _vmWriter.WritePop(_kindToSegment[kind], index);
            }
            ExpectSymbol(';');
        }

        private void CompileIf()
        {
            string ifTrue = $"IF_TRUE{_ifCnt}";
            string ifFalse = $"IF_FALSE{_ifCnt}";
            _ifCnt++;
            ExpectKeyword(Keyword.IF);
            ExpectSymbol('(');
            CompileExpression();
            ExpectSymbol(')');
            _vmWriter.WriteArithmetic(ArithmeticCommand.NOT);
            _vmWriter.WriteIf(ifFalse);

            ExpectSymbol('{');
            CompileStatements();
            ExpectSymbol('}');
            _vmWriter.WriteGoto(ifTrue);

            _vmWriter.WriteLabel(ifFalse);
            if (MatchKeyword(out _, Keyword.ELSE))
            {
                ExpectSymbol('{');
                CompileStatements();
                ExpectSymbol('}');
            }
            _vmWriter.WriteLabel(ifTrue);
        }

        private void CompileWhile()
        {
            string whileBegin = $"WHILE_BEGIN{_whileCnt}";
            string whileEnd = $"WHILE_END{_whileCnt}";
            _whileCnt++;
            ExpectKeyword(Keyword.WHILE);
            _vmWriter.WriteLabel(whileBegin);
            ExpectSymbol('(');
            CompileExpression();
            ExpectSymbol(')');
            _vmWriter.WriteArithmetic(ArithmeticCommand.NOT);
            _vmWriter.WriteIf(whileEnd);

            ExpectSymbol('{');
            CompileStatements();
            ExpectSymbol('}');
            _vmWriter.WriteGoto(whileBegin);

            _vmWriter.WriteLabel(whileEnd);
        }

        private void CompileDo()
        {
            ExpectKeyword(Keyword.DO);

            CompileSubroutineCall();
            _vmWriter.WritePop(MemorySegment.TEMP, 0);

            ExpectSymbol(';');
        }

        private void CompileSubroutineCall(string? subroutineName = null)
        {
            if (subroutineName == null)
            {
                subroutineName = ExpectIdentifier();
            }
            string className = _className;
            int nArgs = 0;
            if (MatchSymbol(out _, '.'))
            {
                className = subroutineName;
                subroutineName = ExpectIdentifier();

                VariableKind kind = _symbolTable.KindOf(className);
                if (kind != VariableKind.NONE)
                {
                    nArgs++;
                    string type = _symbolTable.TypeOf(className);
                    int index = _symbolTable.IndexOf(className);

                    _vmWriter.WritePush(_kindToSegment[kind], index);
                    className = type;
                }
            }
            else
            {
                nArgs++;
                _vmWriter.WritePush(MemorySegment.POINTER, 0);
            }
            ExpectSymbol('(');
            nArgs += CompileExpressionList();
            _vmWriter.WriteCall($"{className}.{subroutineName}", nArgs);
            ExpectSymbol(')');
        }

        private void CompileReturn()
        {
            ExpectKeyword(Keyword.RETURN);

            if (!MatchSymbol(out _, ';'))
            {
                CompileExpression();
                ExpectSymbol(';');
            }
            else
            {
                _vmWriter.WritePush(MemorySegment.CONST, 0);
            }
            _vmWriter.WriteReturn();
        }

        private void CompileExpression()
        {

            CompileTerm();
            while (MatchSymbol(out char op, _binaryOperators))
            {
                CompileTerm();
                switch (op)
                {
                    case '+':
                        _vmWriter.WriteArithmetic(ArithmeticCommand.ADD);
                        break;
                    case '-':
                        _vmWriter.WriteArithmetic(ArithmeticCommand.SUB);
                        break;
                    case '*':
                        _vmWriter.WriteCall("Math.multiply", 2);
                        break;
                    case '/':
                        _vmWriter.WriteCall("Math.divide", 2);
                        break;
                    case '&':
                        _vmWriter.WriteArithmetic(ArithmeticCommand.AND);
                        break;
                    case '|':
                        _vmWriter.WriteArithmetic(ArithmeticCommand.OR);
                        break;
                    case '<':
                        _vmWriter.WriteArithmetic(ArithmeticCommand.LT);
                        break;
                    case '>':
                        _vmWriter.WriteArithmetic(ArithmeticCommand.GT);
                        break;
                    case '=':
                        _vmWriter.WriteArithmetic(ArithmeticCommand.EQ);
                        break;
                    default:
                        break;
                }
            }
        }

        private void CompileTerm()
        {
            if (MatchInt(out int n))
            {
                _vmWriter.WritePush(MemorySegment.CONST, n);
            }
            else if (MatchString(out string str))
            {
                _vmWriter.WritePush(MemorySegment.CONST, str.Length);
                _vmWriter.WriteCall("String.new", 1);
                foreach (char ch in str)
                {
                    _vmWriter.WritePush(MemorySegment.CONST, ch);
                    _vmWriter.WriteCall("String.appendChar", 2);
                }
            }
            else if (MatchKeyword(out Keyword keyword, Keyword.TRUE, Keyword.FALSE, Keyword.NULL, Keyword.THIS))
            {
                switch (keyword)
                {
                    case Keyword.TRUE:
                        _vmWriter.WritePush(MemorySegment.CONST, 0);
                        _vmWriter.WriteArithmetic(ArithmeticCommand.NOT);
                        break;
                    case Keyword.FALSE:
                        _vmWriter.WritePush(MemorySegment.CONST, 0);
                        break;
                    case Keyword.NULL:
                        _vmWriter.WritePush(MemorySegment.CONST, 0);
                        break;
                    case Keyword.THIS:
                        _vmWriter.WritePush(MemorySegment.POINTER, 0);
                        break;
                    default:
                        break;
                }
            }
            else if (MatchIdentifier(out string identifier))
            {
                if (MatchSymbol(out _, '['))
                {
                    VariableKind kind = _symbolTable.KindOf(identifier);
                    int index = _symbolTable.IndexOf(identifier);
                    _vmWriter.WritePush(_kindToSegment[kind], index);
                    CompileExpression();
                    _vmWriter.WriteArithmetic(ArithmeticCommand.ADD);
                    _vmWriter.WritePop(MemorySegment.POINTER, 1);
                    _vmWriter.WritePush(MemorySegment.THAT, 0);
                    ExpectSymbol(']');
                }
                else if (CheckSymbol('(', '.'))
                {
                    CompileSubroutineCall(identifier);
                }
                else
                {
                    VariableKind kind = _symbolTable.KindOf(identifier);    // TODO check if not None
                    int index = _symbolTable.IndexOf(identifier);
                    _vmWriter.WritePush(_kindToSegment[kind], index);
                }
            }
            else if (MatchSymbol(out _, '('))
            {
                CompileExpression();
                ExpectSymbol(')');
            }
            else if (MatchSymbol(out char op, _unaryOperators))
            {
                CompileTerm();
                switch (op)
                {
                    case '~':
                        _vmWriter.WriteArithmetic(ArithmeticCommand.NOT);
                        break;
                    case '-':
                        _vmWriter.WriteArithmetic(ArithmeticCommand.NEG);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                _logger.Log();
            }
        }

        private int CompileExpressionList()
        {
            int nArgs = 0;
            if (!CheckSymbol(')'))
            {
                do
                {
                    CompileExpression();
                    nArgs++;
                }
                while (MatchSymbol(out _, ','));
            }
            return nArgs;
        }

        private string CompileType(params Keyword[] keywords)
        {
            string type;
            if (!MatchIdentifier(out type))
            {
                type = ExpectKeyword(keywords).ToLowerString();
            }
            return type;
        }

        private bool MatchKeyword(out Keyword value, params Keyword[] keywords)
        {
            if (TokenType.KEYWORD == _currentToken.TokenType
                && keywords.Contains((Keyword)_currentToken.Value))
            {
                value = (Keyword)_currentToken.Value;
                Advance();
                return true;
            }
            value = default;
            return false;
        }

        private bool MatchSymbol(out char value, params char[] symbols)
        {
            if (TokenType.SYMBOL == _currentToken.TokenType
                && symbols.Contains((char)_currentToken.Value))
            {
                value = (char)_currentToken.Value;
                Advance();
                return true;
            }
            value = default;
            return false;
        }

        private bool MatchIdentifier(out string value)
        {
            if (TokenType.IDENTIFIER == _currentToken.TokenType)
            {
                value = (string)_currentToken.Value;
                Advance();
                return true;
            }
            value = default;
            return false;
        }

        private bool MatchString(out string value)
        {
            if (TokenType.STRING_CONST == _currentToken.TokenType)
            {
                value = (string)_currentToken.Value;
                Advance();
                return true;
            }
            value = default;
            return false;
        }

        private bool MatchInt(out int value)
        {
            if (TokenType.INT_CONST == _currentToken.TokenType)
            {
                value = (int)_currentToken.Value;
                Advance();
                return true;
            }
            value = default;
            return false;
        }

        private Keyword ExpectKeyword(params Keyword[] keywords)
        {
            Keyword value;
            if (!MatchKeyword(out value, keywords))
            {
                _logger.Log();
            }
            return value;
        }

        private char ExpectSymbol(params char[] symbols)
        {
            char value;
            if (!MatchSymbol(out value, symbols))
            {
                _logger.Log();
            }
            return value;
        }

        private string ExpectIdentifier()
        {
            string value;
            if (!MatchIdentifier(out value))
            {
                _logger.Log();
            }
            return value;
        }

        private bool CheckKeyword(params Keyword[] keywords)
        {
            return TokenType.KEYWORD == _currentToken.TokenType
                && keywords.Contains((Keyword)_currentToken.Value);
        }

        private bool CheckSymbol(params char[] symbols)
        {
            return TokenType.SYMBOL == _currentToken.TokenType
                && symbols.Contains((char)_currentToken.Value);
        }

        private void Advance()
        {
            _tokenizer.Advance();
            _currentToken = _tokenizer.GetCurrentToken();
        }
    }
}
