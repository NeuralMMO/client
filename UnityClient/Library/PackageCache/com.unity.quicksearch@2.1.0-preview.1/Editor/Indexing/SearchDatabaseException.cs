using System;

namespace Unity.QuickSearch
{
    public class SearchDatabaseException : Exception
    {
        public readonly string guid;

        public SearchDatabaseException(string message, string guid)
            : base(message)
        {
            this.guid = guid;
        }

        public SearchDatabaseException(string message, string guid, Exception inner)
            : base(message, inner)
        {
            this.guid = guid;
        }
    }
}
