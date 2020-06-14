/*
 * AutoMovieManager.cs
 * Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using AVPlayer;

public class AutoMovieManager : MonoBehaviour
{
    [SerializeField] private GameObject videoObject = null;
    private const string TEST_CONTENT_PATH = "https://devstreaming-cdn.apple.com/videos/streaming/examples/bipbop_4x3/bipbop_4x3_variant.m3u8";
    private IntPtr avPlayer;
    private int renderEventId;
    private IntPtr renderEventFunc;

    // Start is called before the first frame update
    void Start()
    {
        avPlayer = AVPlayerConnect.AVPlayerCreate();
        renderEventId = AVPlayerConnect.AVPlayerGetEventID(avPlayer);
        renderEventFunc = AVPlayerConnect.AVPlayerGetRenderEventFunc();

        AVPlayerConnect.AVPlayerSetOnReady(
            avPlayer,
            transform.root.gameObject.name,
            ((Action<string>)CallbackReadyPlayer).Method.Name);
        AVPlayerConnect.AVPlayerSetContent(avPlayer, TEST_CONTENT_PATH);
    }

    private void CallbackReadyPlayer(string message)
    {
        // Texture settings
        IntPtr texPtr = AVPlayerConnect.AVPlayerGetTexturePtr(avPlayer);
        Texture2D texture = Texture2D.CreateExternalTexture(
            512,
            512,
            TextureFormat.BGRA32,
            false,
            false,
            texPtr);
        texture.UpdateExternalTexture(texPtr);
        if (videoObject != null)
        {
            Renderer renderer = videoObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = renderer.materials[0];
                if (material != null)
                {
                    material.SetTexture("_MainTex", texture);
                    float width = AVPlayerConnect.AVPlayerGetVideoWidth(avPlayer);
                    float height = AVPlayerConnect.AVPlayerGetVideoHeight(avPlayer);
                    StartCoroutine(OnUpdateVideoSize(width,height));
                }
            }
        }
        // Render settings
        StartCoroutine(OnRender());

        // Play settings
        AVPlayerConnect.AVPlayerSetLoop(avPlayer, true);
        AVPlayerConnect.AVPlayerPlay(avPlayer);

        // Callbacks
        var videoSizeEvent = new VideoSizeEvent();
        videoSizeEvent.SetCallback(avPlayer, (sender,width,height )=>
        {
            StartCoroutine(OnUpdateVideoSize(width, height));
        });
    }

    void Update()
    {
    }

    IEnumerator OnRender()
    {
        for (;;)
        {
            yield return new WaitForEndOfFrame();
            Assert.IsFalse(renderEventFunc.Equals(IntPtr.Zero),"renderEventFunc is Zero");
            Assert.IsTrue(renderEventId>0, "renderEventId <= 0");
            GL.IssuePluginEvent(renderEventFunc,renderEventId);
        }
    }

    IEnumerator OnUpdateVideoSize(float width, float height)
    {
        yield return null;
        var videoSizeWidth = AVPlayerConnect.AVPlayerGetVideoWidth(avPlayer);
        var videoSizeHeight = AVPlayerConnect.AVPlayerGetVideoHeight(avPlayer);
        if (videoSizeWidth != 0 && videoSizeHeight != 0)
        {
            var w = width / videoSizeWidth;
            var h = -(height / videoSizeHeight);
            var scale = new Vector2(w,h);
            if (videoObject != null)
            {
                var renderer = videoObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var material = renderer.materials[0];
                    if (material != null)
                    {
                        material.SetTextureScale("_MainTex", scale);
                    }
                }
            }
        }
    }

}
