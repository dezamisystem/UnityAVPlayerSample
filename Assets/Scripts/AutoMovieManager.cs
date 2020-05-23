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
    private float videoSizeWidth = 0;
    private float videoSizeHeight = 0;

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
        videoSizeWidth = AVPlayerConnect.AVPlayerGetVideoWidth(avPlayer);
        videoSizeHeight = AVPlayerConnect.AVPlayerGetVideoHeight(avPlayer);
        if (videoObject != null)
        {
            Renderer renderer = videoObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material material = renderer.material;
                if (material != null)
                {
                    Vector2 scale = new Vector2(1f,-1f);
                    material.SetTextureScale("_MainTex", scale);
                    material.SetTexture("_MainTex", texture);
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
            yield return null;
            Assert.IsFalse(renderEventFunc.Equals(IntPtr.Zero),"renderEventFunc is Zero");
            Assert.IsTrue(renderEventId>0, "renderEventId <= 0");
            GL.IssuePluginEvent(renderEventFunc,renderEventId);
        }
    }

    IEnumerator OnUpdateVideoSize(float width, float height)
    {
        yield return null;
        if (width != 0 && height != 0)
        {
            Vector2 scale = new Vector2(videoSizeWidth/width,-videoSizeHeight/height);
            if (videoObject != null)
            {
                Renderer renderer = videoObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material material = renderer.material;
                    if (material != null)
                    {
                        material.SetTextureScale("_MainTex", scale);
                    }
                }
            }
        }
    }

}
