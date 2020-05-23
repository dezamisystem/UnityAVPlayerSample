using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AVPlayer {

    public class VideoSizeEvent : MonoBehaviour
    {
        public delegate void Callback(IntPtr sender, int width, int height);

        public delegate void CallbackCaller(IntPtr sender, int width, int height, IntPtr methodHandle);

        #if UNITY_IOS
            [DllImport ("__Internal")]
            public static extern void AVPlayerCallbackOnVideoSize(IntPtr sender, IntPtr methodHandle, CallbackCaller caller);
        #else
            public static void AVPlayerCallbackOnVideoSize(IntPtr sender, IntPtr methodHandle, CallbackCaller caller) {}
        #endif

        private IntPtr gcHandle = IntPtr.Zero;

        public void SetCallback(IntPtr sender, Callback callback)
        {
            gcHandle = (IntPtr)GCHandle.Alloc(callback, GCHandleType.Normal);
            AVPlayerCallbackOnVideoSize(sender, gcHandle, CallCallback);
        }

        void OnDestroy()
        {
            if (!gcHandle.Equals(IntPtr.Zero))
            {
                GCHandle handle = (GCHandle)gcHandle;
                handle.Free();
            }
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(CallbackCaller))]
        static void CallCallback(IntPtr sender, int width, int height, IntPtr methodhandle)
        {
            GCHandle handle = (GCHandle)methodhandle;
            Callback callback = handle.Target as Callback;
            callback(sender, width, height);
        }
    }

}
