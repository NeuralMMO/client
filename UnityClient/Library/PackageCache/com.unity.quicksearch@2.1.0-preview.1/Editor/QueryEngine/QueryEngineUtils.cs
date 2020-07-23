
using System.Collections.Generic;

namespace Unity.QuickSearch
{
    static class QueryEngineUtils
    {
        static readonly HashSet<char> k_WhiteSpaceChars = new HashSet<char>(" \f\n\r\t\v");

        public static bool IsPhraseToken(string token)
        {
            if (token.Length < 2)
                return false;
            var startIndex = token[0] == '!' ? 1 : 0;
            var endIndex = token.Length - 1;
            return token[startIndex] == '"' && token[endIndex] == '"';
        }

        public static bool IsNestedQueryToken(string token)
        {
            if (token.Length < 2)
                return false;
            var startIndex = token.IndexOf('{');
            var endIndex = token.LastIndexOf('}');
            return startIndex != -1 && endIndex == token.Length - 1 && startIndex < endIndex;
        }

        public static bool IsWhiteSpaceChar(char c)
        {
            return k_WhiteSpaceChars.Contains(c);
        }
    }
}
