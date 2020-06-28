/*
 * MovieController.cs
 * Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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
    [SerializeField] private Slider speedSlider = null;

    [SerializeField] private Slider rectWidthSlider = null;
    [SerializeField] private Slider rectHeightSlider = null;

    // private const string TEST_CONTENT_PATH = "https://dezamisystem.com/movie/vtuber/index.m3u8";
    private const string TEST_CONTENT_PATH = "https://devstreaming-cdn.apple.com/videos/streaming/examples/bipbop_16x9/bipbop_16x9_variant.m3u8";

    private const int s_VideoDefaultWidth = 3840;
    private const int s_VideoDefaultHeight = 2160;

    private IntPtr avPlayer;
    private int renderEventId;
    private IntPtr renderEventFunc;
    private bool isSeekSliderDoing;
    private bool isSeekWaiting;
    private float videoDuration;
    private float movedSeekValue = 0;
    private float prevSeekValue = 0;
    private float enableSeekRange = 10;
    private SynchronizationContext currentContext;

    // Start is called before the first frame update
    void Start()
    {
        currentContext = SynchronizationContext.Current;

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
        if (videoImage != null)
        {
            videoImage.texture = texture;
            float width = AVPlayerConnect.AVPlayerGetVideoWidth(avPlayer);
            float height = AVPlayerConnect.AVPlayerGetVideoHeight(avPlayer);
            UpdateVideoImageSize(width, height);
        }
        StartCoroutine(OnRender());

        // Seek settings
        enableSeekRange = AVPlayerConnect.AVPlayerGetDuration(avPlayer) / 100;
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
        StartCoroutine(OnUpdatePositionText());
        StartCoroutine(OnUpdateSeekSlider());
        AVPlayerConnect.AVPlayerSetOnSeek(
            avPlayer,
            transform.root.gameObject.name,
            ((Action<string>)CallbackSeek).Method.Name);

        // Speed Settings
        if (speedSlider != null)
        {
            speedSlider.onValueChanged.AddListener((value)=>
            {
                // speed
                AVPlayerConnect.AVPlayerSetRate(avPlayer, value);
            });
        }

        // Other Callback settings
        AVPlayerConnect.AVPlayerSetOnEndTime(
            avPlayer,
            transform.root.gameObject.name,
            ((Action<string>)CallbackEndTime).Method.Name);
        VideoSizeEvent videoSizeEvent = new VideoSizeEvent();
        videoSizeEvent.SetCallback(avPlayer, (sender,width,height)=>
        {
            UpdateVideoImageSize(width, height);
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

        // Test Settings
        if (rectWidthSlider != null)
        {
            rectWidthSlider.onValueChanged.AddListener((value)=>
            {
                var rectU = value;
                var rectV = -1f;
                if (rectHeightSlider != null)
                {
                    rectV = rectHeightSlider.value;
                }
                UpdateVideoImageRect(rectU,rectV);
            });
        }
        if (rectHeightSlider != null)
        {
            rectHeightSlider.onValueChanged.AddListener((value)=>
            {
                var rectU = 1f;
                var rectV = value;
                if (rectWidthSlider != null)
                {
                    rectU = rectWidthSlider.value;
                }
                UpdateVideoImageRect(rectU,rectV);
            });
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
            if (debugText != null)
            {
                debugText.text = "Seek: " + current;
            }
        }
    }

    private void CallbackSeek(string message)
    {
        isSeekWaiting = false;
    }

    public void SeekSliderInitializePointerDrag()
    {
        isSeekSliderDoing = true;
        movedSeekValue = 0;
        if (seekSlider != null)
        {
            prevSeekValue = seekSlider.value;
        }
        OnSeek();
    }

    public void SeekSliderDrag()
    {
        // OnSeek();
        if (seekSlider != null && !isSeekWaiting)
        {
            var value = seekSlider.value;
            movedSeekValue += (value - prevSeekValue);
            if (Mathf.Abs(movedSeekValue) >= enableSeekRange)
            {
                movedSeekValue = 0;
                OnSeek();
            }
            prevSeekValue = value;
        }
    }

    public void SeekSliderPointerUp()
    {
        OnSeek();
        isSeekSliderDoing = false;
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

    public void UpdateVideoImageSize(float width, float height)
    {
        var u = width / s_VideoDefaultWidth;
        var v = height / s_VideoDefaultHeight;
        UpdateVideoImageRect(u,v);
    }

    public void UpdateVideoImageRect(float u, float v)
    {
        var w = u;
        var h = -v;
        var y = 1f - h;
        if (videoImage != null)
        {
            videoImage.uvRect = new Rect(0, y, w, h);
        }
        if (debugText != null)
        {
            var text = "Y: " + y + ",U: " + w + ", V: " + h;
            debugText.text = text;
        }
    }

    IEnumerator OnUpdatePositionText()
    {
        for(;;)
        {
            yield return new WaitForEndOfFrame();
            float current = Mathf.Floor(AVPlayerConnect.AVPlayerGetCurrentPosition(avPlayer));
            float duration = Mathf.Floor(videoDuration);
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
            yield return new WaitForEndOfFrame();
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
        AVPlayerConnect.AVPlayerClose(avPlayer);
        avPlayer = IntPtr.Zero;
        SceneManager.LoadScene("UITestScene");
    }
    
}
