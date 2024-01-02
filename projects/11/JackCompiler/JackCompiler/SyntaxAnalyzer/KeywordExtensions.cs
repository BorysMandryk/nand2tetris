namespace JackCompiler.SyntaxAnalyzer
{
    public static class KeywordExtensions
    {
        public static string ToLowerString(this Keyword keyword)
        {
            return keyword.ToString().ToLowerInvariant();
        }
    }
}