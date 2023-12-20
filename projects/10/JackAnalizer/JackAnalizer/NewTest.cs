using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackAnalizer
{
    class BaseToken
    {
        protected BaseToken prev;
        protected BaseToken next;
        public virtual void Execute()
        {

        }

        public BaseToken AddNext(BaseToken nextToken)
        {
            next = nextToken;
            return next;
        }
    }

    class TerminalToken : BaseToken
    {

    }

    class NonTerminalToken : BaseToken
    {

    }

    class KeywordToken : TerminalToken
    {
        private Keyword[] _keywords;
        public KeywordToken()
        {
            
        }

        public static bool Match(KeywordToken token, params Keyword[] keyword)
        {

        }
    }

    class SymbolToken : TerminalToken
    {
        private char[] _symbols;
        public SymbolToken(params char[] symbols)
        {
            _symbols = symbols;
        }
    }

    class IdentifierToken : TerminalToken
    {
        private Keyword keyword;
        public IdentifierToken()
        {

        }
    }

    class ClassRule : NonTerminalToken
    {
        void Execute()
        {
            KeywordToken(Keyword.VAR)
        }
    }

    internal class NewProgram
    {
        void main()
        {
            // 'class' className '{' classvardec* soubrutineDec* '}'
            NonTerminalToken classVarDec = new NonTerminalToken();
            classVarDec
                .AddNext(new KeywordToken(Keyword.VAR))
                .AddNext(new KeywordToken(Keyword.BOOLEAN, Keyword.INT, Keyword.CHAR));
            NonTerminalToken classRule = new NonTerminalToken();
            classRule
                .AddNext(new KeywordToken(Keyword.CLASS))
                .AddNext(new IdentifierToken())
                .AddNext(new SymbolToken('{'))
                .AddNext(classVarDec);
            classRule.Execute();
        }
    }
}
