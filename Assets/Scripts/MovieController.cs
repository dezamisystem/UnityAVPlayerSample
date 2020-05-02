using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AVPlayer;

public class MovieController : MonoBehaviour
{
    public Button prepareButton;
    public Button playButton;
    public Text currentTimeText;
    private const string TEST_CONTENT_PATH = "https://dezamisystem.com/movie/vtuber/index.m3u8";
    private IntPtr avPlayer;
    private int frameCount;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(OnRender());

        frameCount = 0;
        avPlayer = AVPlayerConnect.AVPlayerCreate();

        if (prepareButton != null)
        {
            prepareButton.interactable = true;
        }
        if (playButton != null)
        {
            playButton.interactable = false;
        }
    }

    public void OnPrepareMovie()
    {
        AVPlayerConnect.AVPlayerSetOnReady(avPlayer, transform.root.gameObject.name, ((Action<string>)CallbackReadyPlayer).Method.Name);
        AVPlayerConnect.AVPlayerSetContent(avPlayer, TEST_CONTENT_PATH);
    }

    private void CallbackReadyPlayer(string message)
    {
        AVPlayerConnect.AVPlayerSetOnEndTime(avPlayer, transform.root.gameObject.name, ((Action<string>)CallbackEndTime).Method.Name);

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
    }

    // Update is called once per frame
    void Update()
    {
        frameCount ++;
        if (frameCount % 15 == 0) 
        {
            if (currentTimeText != null)
            {
                currentTimeText.text = "["
                + AVPlayerConnect.AVPlayerGetCurrentPosition(avPlayer)
                + " | "
                + AVPlayerConnect.AVPlayerGetDuration(avPlayer)
                + "]";
            }
        }
    }

    IEnumerator OnRender()
    {
        for (;;) {
            yield return new WaitForEndOfFrame();
            GL.IssuePluginEvent(AVPlayerConnect.AVPlayerGetRenderEventFunc(), 0);
        }
    }
}
