using System;

namespace Unity.Platforms
{
    public struct SuspendResumeEvent
    {
        public bool Suspend { get; }
        public SuspendResumeEvent(bool suspend)
        {
            Suspend = suspend;
        }
    }

    public struct QuitEvent
    {
    }

    public struct ScreenOrientationEvent
    {
        public int Orientation { get; }
        public ScreenOrientationEvent(int orientation)
        {
            Orientation = orientation;
        }
    }

    public struct DeviceOrientationEvent
    {
        public int Orientation { get; }
        public DeviceOrientationEvent(int orientation)
        {
            Orientation = orientation;
        }
    }

    public static class PlatformEvents
    {
        public delegate void SuspendResumeEventHandler(object sender, SuspendResumeEvent evt);
        public delegate void QuitEventHandler(object sender, QuitEvent evt);
        public delegate void ScreenOrientationEventHandler(object sender, ScreenOrientationEvent evt);
        public delegate void DeviceOrientationEventHandler(object sender, DeviceOrientationEvent evt);

        public static void SendSuspendResumeEvent(object sender, SuspendResumeEvent evt)
        {
            var handler = OnSuspendResume;
            handler?.Invoke(sender, evt);
        }

        public static void SendQuitEvent(object sender, QuitEvent evt)
        {
            var handler = OnQuit;
            handler?.Invoke(sender, evt);
        }

        public static void SendScreenOrientationEvent(object sender, ScreenOrientationEvent evt)
        {
            var handler = OnScreenOrientation;
            handler?.Invoke(sender, evt);
        }

        public static void SendDeviceOrientationEvent(object sender, DeviceOrientationEvent evt)
        {
            var handler = OnDeviceOrientation;
            handler?.Invoke(sender, evt);
        }

        public static event SuspendResumeEventHandler OnSuspendResume;
        public static event QuitEventHandler OnQuit;
        public static event ScreenOrientationEventHandler OnScreenOrientation;
        public static event DeviceOrientationEventHandler OnDeviceOrientation;
    }
}
