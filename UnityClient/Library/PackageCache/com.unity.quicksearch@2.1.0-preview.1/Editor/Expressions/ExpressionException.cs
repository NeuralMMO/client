using System;

namespace Unity.QuickSearch
{
    class ExpressionException : Exception
    {
        public SearchExpressionNode node { get; private set; }

        public ExpressionException(string message)
            : base(message)
        {
        }

        public ExpressionException(SearchExpressionNode node, string message)
            : base(message)
        {
            this.node = node;
        }
    }
}
