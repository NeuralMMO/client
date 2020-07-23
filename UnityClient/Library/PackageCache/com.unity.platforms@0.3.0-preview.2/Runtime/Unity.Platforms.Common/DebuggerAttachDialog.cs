#if !UNITY_EDITOR
    using System.Runtime.InteropServices;
namespace Unity.Platforms
{
    public static class DebuggerAttachDialog
    {
        [DllImport("lib_unity_platforms_common")]
        private static extern void ShowDebuggerAttachDialog(string message, BroadcastFunction broadcast);

        public delegate void BroadcastFunction();

        public static void Show(BroadcastFunction broadcast)
        {
            ShowDebuggerAttachDialog("You can attach a managed debugger now if you want", broadcast);
        }
    }
}
#endif
