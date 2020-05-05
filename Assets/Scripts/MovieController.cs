using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using AVPlayer;

public class MovieController : MonoBehaviour
{
    public RawImage videoImage;
    public Button prepareButton;
    public Button playButton;
    public Slider seekSlider;
    public Text currentTimeText;
    public Text debugText;
    private const string TEST_CONTENT_PATH = "https://dezamisystem.com/movie/vtuber/index.m3u8";
    private IntPtr avPlayer;
    private int renderEventId;
    private IntPtr renderEventFunc;
    private bool isSeekSliderDoing;
    private bool isSeekWaiting;

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
        }
        StartCoroutine(OnRender());

        // Seek settings
        if (seekSlider != null)
        {
            seekSlider.interactable = true;
            seekSlider.maxValue = AVPlayerConnect.AVPlayerGetDuration(avPlayer);
            seekSlider.minValue = 0f;
            seekSlider.value = 0f;
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
        if (debugText != null)
        {
            debugText.text = message;
        }
    }

    public void SeekSliderPointerDown()
    {
        isSeekSliderDoing = true;
        OnSeek();
        if (debugText != null)
        {
            debugText.text = "SeekSliderBeginDrag";
        }
    }

    public void SeekSliderDrag()
    {
        OnSeek();
    }

    public void SeekSliderEndDrag()
    {
        OnSeek();
        isSeekSliderDoing = false;
        if (debugText != null)
        {
            debugText.text = "SeekSliderEndDrag";
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
            yield return new WaitForEndOfFrame();
            Assert.IsFalse(renderEventFunc.Equals(IntPtr.Zero),"renderEventFunc is Zero");
            Assert.IsTrue(renderEventId>0, "renderEventId <= 0");
            GL.IssuePluginEvent(renderEventFunc,renderEventId);
        }
    }

    IEnumerator OnUpdateText()
    {
        for(;;)
        {
            yield return new WaitForSeconds(0.5f);
            float current = AVPlayerConnect.AVPlayerGetCurrentPosition(avPlayer);
            float duration = AVPlayerConnect.AVPlayerGetDuration(avPlayer);
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
}
