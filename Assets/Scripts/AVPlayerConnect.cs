using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AVPlayer {
    public class AVPlayerConnect : MonoBehaviour
    {
        #if UNITY_IOS
            [DllImport ("__Internal")]
            public static extern IntPtr AVPlayerGetRenderEventFunc();
        #else
            public static IntPtr AVPlayerGetRenderEventFunc() { return IntPtr.Zero; }
        #endif

        #if UNITY_IOS
            [DllImport ("__Internal")]
            public static extern IntPtr AVPlayerCreate();
        #else
            public static IntPtr AVPlayerCreate() { return IntPtr.Zero; }
        #endif

        #if UNITY_IOS
            [DllImport ("__Internal")]
            public static extern void AVPlayerSetContent(IntPtr op, string contentPath);
        #else
            public static void AVPlayerSetContent(IntPtr op, string contentPath) {}
        #endif

        #if UNITY_IOS
            [DllImport ("__Internal")]
            public static extern void AVPlayerPlay(IntPtr op);
        #else
            public static void AVPlayerPlay(IntPtr op) {}
        #endif

        #if UNITY_IOS
            [DllImport ("__Internal")]
            public static extern void AVPlayerPause(IntPtr op);
        #else
            public static void AVPlayerPause(IntPtr op) {}
        #endif

        #if UNITY_IOS
            [DllImport ("__Internal")]
            public static extern void AVPlayerSeek(IntPtr op, float seconds);
        #else
            public static void AVPlayerSeek(IntPtr op, float seconds) {}
        #endif

        #if UNITY_IOS
            [DllImport ("__Internal")]
            public static extern void AVPlayerSetRate(IntPtr op, float rate);
        #else
            public static void AVPlayerSetRate(IntPtr op, float rate) {}
        #endif

        #if UNITY_IOS
            [DllImport ("__Internal")]
            public static extern void AVPlayerSetVolume(IntPtr op, float volume);
        #else
            public static void AVPlayerSetVolume(IntPtr op, float volume) {}
        #endif

        #if UNITY_IOS
            [DllImport ("__Internal")]
            public static extern void AVPlayerClose(IntPtr op);
        #else
            public static void AVPlayerClose(IntPtr op) {}
        #endif

        #if UNITY_IOS
            [DllImport ("__Internal")]
            public static extern float AVPlayerGetCurrentPosition(IntPtr op);
        #else
            public static float AVPlayerGetCurrentPosition(IntPtr op) { return -1.0f; }
        #endif

        #if UNITY_IOS
            [DllImport ("__Internal")]
            public static extern float AVPlayerGetDuration(IntPtr op);
        #else
            public static float AVPlayerGetDuration(IntPtr op) { return 0.0f; }
        #endif

        #if UNITY_IOS
            [DllImport ("__Internal")]
            public static extern bool AVPlayerIsPlaying(IntPtr op);
        #else
            public static bool AVPlayerIsPlaying(IntPtr op) { retrun false; }
        #endif

        #if UNITY_IOS
            [DllImport ("__Internal")]
            public static extern void AVPlayerSetOnReady(IntPtr op, string objectName, string methodName);
        #else
            public static void AVPlayerSetOnReady(IntPtr op, string objectName, string methodName) {}
        #endif

        #if UNITY_IOS
            [DllImport ("__Internal")]
            public static extern void AVPlayerSetOnSeek(IntPtr op, string objectName, string methodName);
        #else
            public static void AVPlayerSetOnSeek(IntPtr op, string objectName, string methodName) {}
        #endif

        #if UNITY_IOS
            [DllImport ("__Internal")]
            public static extern void AVPlayerSetOnEndTime(IntPtr op, string objectName, string methodName);
        #else
            public static void AVPlayerSetOnEndTime(IntPtr op, string objectName, string methodName) {}
        #endif
    }
}
