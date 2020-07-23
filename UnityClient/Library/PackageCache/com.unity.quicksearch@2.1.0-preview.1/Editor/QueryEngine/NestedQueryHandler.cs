using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace Unity.QuickSearch
{
    interface INestedQueryHandlerTransformer
    {
        Type enumerableType { get; }
        Type rightHandSideType { get; }
    }

    class NestedQueryHandlerTransformer<TQueryData, TRhs> : INestedQueryHandlerTransformer
    {
        public Func<TQueryData, TRhs> handler { get; }

        public Type enumerableType => typeof(TQueryData);
        public Type rightHandSideType => typeof(TRhs);

        public NestedQueryHandlerTransformer(Func<TQueryData, TRhs> handler)
        {
            this.handler = handler;
        }
    }

    interface INestedQueryHandler
    {
        Type enumerableType { get; }
    }

    class NestedQueryHandler<TQueryData> : INestedQueryHandler
    {
        public Func<string, string, IEnumerable<TQueryData>> handler { get; }
        public Type enumerableType => typeof(TQueryData);

        public NestedQueryHandler(Func<string, string, IEnumerable<TQueryData>> handler)
        {
            this.handler = handler;
        }
    }

    interface INestedQueryAggregator
    {
        Type enumerableType { get; }
    }

    class NestedQueryAggregator<TQueryData> : INestedQueryAggregator
    {
        public Type enumerableType => typeof(TQueryData);
        public Func<IEnumerable<TQueryData>, IEnumerable<TQueryData>> aggregator { get; }

        public NestedQueryAggregator(Func<IEnumerable<TQueryData>, IEnumerable<TQueryData>> aggregator)
        {
            this.aggregator = aggregator;
        }
    }
}
