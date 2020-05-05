using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using AVPlayer;

public class AutoMovieManager : MonoBehaviour
{
    public GameObject videoObject;
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
                renderer.material.mainTexture = texture;
            }
        }
        // Render settings
        StartCoroutine(OnRender());

        // Play settings
        AVPlayerConnect.AVPlayerSetLoop(avPlayer, true);
        AVPlayerConnect.AVPlayerPlay(avPlayer);
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

}
