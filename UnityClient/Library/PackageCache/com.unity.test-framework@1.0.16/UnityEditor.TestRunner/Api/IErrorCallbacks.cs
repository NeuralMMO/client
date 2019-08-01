namespace UnityEditor.TestTools.TestRunner.Api
{
    internal interface IErrorCallbacks : ICallbacks
    {
        void OnError(string message);
    }
}
