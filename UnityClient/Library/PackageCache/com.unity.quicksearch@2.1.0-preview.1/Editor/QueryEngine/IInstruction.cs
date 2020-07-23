using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace Unity.QuickSearch
{
    internal interface IInstruction<T>
    {
        long estimatedCost { get; }
        IQueryNode node { get; }
    }
    internal interface IOperandInstruction<T> : IInstruction<T>
    {
        IInstruction<T> LeftInstruction { get; }
        IInstruction<T> RightInstruction { get; }
    }
    internal interface IAndInstruction<T> : IOperandInstruction<T>
    {
    }
    internal interface IOrInstruction<T> : IOperandInstruction<T>
    {

    }
    internal interface IResultInstruction<T> : IInstruction<T>
    {
    }
}
