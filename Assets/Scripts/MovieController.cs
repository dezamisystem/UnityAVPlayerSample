/*
 * MovieController.cs
 * Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;
using AVPlayer;

public class MovieController : MonoBehaviour
{
    [SerializeField] private RawImage videoImage = null;
    [SerializeField] private Button prepareButton = null;
    [SerializeField] private Button playButton = null;
    [SerializeField] private Slider seekSlider = null;
    [SerializeField] private Text currentTimeText = null;
    [SerializeField] private Text debugText = null;
    // private const string TEST_CONTENT_PATH = "https://dezamisystem.com/movie/vtuber/index.m3u8";
    private const string TEST_CONTENT_PATH = "https://devstreaming-cdn.apple.com/videos/streaming/examples/bipbop_16x9/bipbop_16x9_variant.m3u8";

    private IntPtr avPlayer;
    private int renderEventId;
    private IntPtr renderEventFunc;
    private bool isSeekSliderDoing;
    private bool isSeekWaiting;
    private float videoDuration;
    private float videoSizeWidth = 0;
    private float videoSizeHeight = 0;

    // Start is called before the first frame update
    void Start()
    {
        avPlayer = IntPtr.Zero;
        isSeekSliderDoing = false;
        isSeekWaiting = false;

        if (prepareButton != null)
        {
            prepareButton.interactable = true;
        }
        if (playButton != null)
        {
            playButton.interactable = false;
        }
        if (seekSlider != null)
        {
            seekSlider.interactable = false;
        }
    }

    public void OnPrepareMovie()
    {
        if (avPlayer == IntPtr.Zero)
        {
            avPlayer = AVPlayerConnect.AVPlayerCreate();
        }
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
        videoDuration = AVPlayerConnect.AVPlayerGetDuration(avPlayer);
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
        if (videoImage != null)
        {
            videoImage.texture = texture;
            Vector2 scale = new Vector2(1f,-1f);
            videoImage.material.SetTextureScale("_MainTex", scale);
        }
        StartCoroutine(OnRender());

        // Seek settings
        if (seekSlider != null)
        {
            seekSlider.interactable = true;
            seekSlider.maxValue = videoDuration;
            seekSlider.minValue = 0f;
            seekSlider.value = 0f;
            SliderEventTrigger trigger = seekSlider.GetComponentInChildren<SliderEventTrigger>();
            if (trigger != null)
            {
                trigger.BeginAction = SeekSliderInitializePointerDrag;
                trigger.MovingAction = SeekSliderDrag;
                trigger.EndAction = SeekSliderPointerUp;
            }
        }
        StartCoroutine(OnUpdateText());
        StartCoroutine(OnUpdateSeekSlider());

        // Callback settings
        AVPlayerConnect.AVPlayerSetOnEndTime(
            avPlayer,
            transform.root.gameObject.name,
            ((Action<string>)CallbackEndTime).Method.Name);
        AVPlayerConnect.AVPlayerSetOnSeek(
            avPlayer,
            transform.root.gameObject.name,
            ((Action<string>)CallbackSeek).Method.Name);
        VideoSizeEvent videoSizeEvent = new VideoSizeEvent();
        videoSizeEvent.SetCallback(avPlayer, (sender,width,height )=>
        {
            StartCoroutine(OnUpdateVideoSize(width, height));
        });

        // UI settings
        if (prepareButton != null)
        {
            prepareButton.interactable = false;
        }
        if (playButton != null)
        {
            playButton.interactable = true;
        }
    }

    public void OnPlayMovie()
    {
        if (AVPlayerConnect.AVPlayerIsPlaying(avPlayer))
        {
            AVPlayerConnect.AVPlayerPause(avPlayer);
        }
        else
        {
            AVPlayerConnect.AVPlayerPlay(avPlayer);
        }
    }

    private void CallbackEndTime(string message)
    {
        if (debugText != null)
        {
            debugText.text = message;
        }
    }

    private void OnSeek()
    {
        if (seekSlider != null && !isSeekWaiting)
        {
            float current = seekSlider.value;
            AVPlayerConnect.AVPlayerSeek(avPlayer, current);
            isSeekWaiting = true;
            seekSlider.value = current;
        }
    }

    private void CallbackSeek(string message)
    {
        isSeekWaiting = false;
    }

    public void SeekSliderInitializePointerDrag()
    {
        isSeekSliderDoing = true;
        OnSeek();
        if (debugText != null)
        {
            debugText.text = "SeekSliderInitializePointerDrag";
        }
    }

    public void SeekSliderDrag()
    {
        OnSeek();
    }

    public void SeekSliderPointerUp()
    {
        OnSeek();
        isSeekSliderDoing = false;
        if (debugText != null)
        {
            debugText.text = "SeekSliderPointerUp";
        }
    }

    // Update is called once per frame
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
            if (videoImage != null)
            {
                videoImage.material.SetTextureScale("_MainTex", scale);
            }
        }
    }

    IEnumerator OnUpdateText()
    {
        for(;;)
        {
            yield return null;
            float current = AVPlayerConnect.AVPlayerGetCurrentPosition(avPlayer);
            float duration = videoDuration;
            if (currentTimeText != null)
            {
                currentTimeText.text = "["
                + current
                + " | "
                + duration
                + "]";
            }
        }
    }

    IEnumerator OnUpdateSeekSlider()
    {
        for(;;)
        {
            yield return null;
            if (seekSlider != null && AVPlayerConnect.AVPlayerIsPlaying(avPlayer))
            {
                if (!isSeekSliderDoing && !isSeekWaiting)
                {
                    float current = AVPlayerConnect.AVPlayerGetCurrentPosition(avPlayer);
                    seekSlider.value = current;
                }
            }
        }
    }

    public void MoveTestScene()
    {
        SceneManager.LoadScene("UITestScene");
    }
    
    void OnDestroy()
    {
        AVPlayerConnect.AVPlayerClose(avPlayer);
    }
}
