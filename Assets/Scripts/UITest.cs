/*
 * UITest.cs
 * Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using AVPlayer;

public class UITest : MonoBehaviour
{
    [SerializeField] private Slider testSlider = null;
    [SerializeField] private Text debugText = null;

    private ListDialog listDialog;
    private float movedValue;
    private float prevValue;

    private string settingKey;
    private string settingValue;

    void Awake()
    {
        settingKey = "";
        settingValue = "";
        const string KeyContent = "CONTENT";
        const string ValueContent = "AllYourBaseAreBelongToUs!";
        var settingPath = Application.persistentDataPath + "/unityavplayersample.txt";
        if (!File.Exists(settingPath))
        {
            var settingText = KeyContent + "=" + ValueContent;
            File.WriteAllText(settingPath, settingText);
        }
        var settingLines = File.ReadAllLines(settingPath);
        foreach (var line in settingLines)
        {
            var trueLine = line.Replace(" ","");
            string[] paramArray = trueLine.Split('=');
            if (paramArray.Length >= 2)
            {
                settingKey = paramArray[0];
                if (paramArray[0].Equals(KeyContent)){
                    settingValue = paramArray[1];
                }
            }
        }
    }

    void Start()
    {
        if (testSlider != null)
        {
            SliderEventTrigger trigger = testSlider.GetComponentInChildren<SliderEventTrigger>();
            if (trigger != null)
            {
                trigger.BeginAction = ()=>
                {
                    movedValue = 0;
                    prevValue = testSlider.value;
                };
                trigger.MovingAction = ()=>
                {
                    var value = testSlider.value;
                    movedValue += (value - prevValue);
                    if (Mathf.Abs(movedValue) >= 5)
                    {
                        movedValue = 0;
                        Debug.Log("!!!!!!!!Move Reset!!!!!!!!");
                    }
                    prevValue = value;
                };
            }
        }
        if (debugText != null)
        {
            debugText.text = settingKey + " = " + settingValue;
        }
    }

    public void ShowTestDialog()
    {
        var handler = DialogHandler.ShowDialog(
            "タイトル",
            "恥の多い生涯を送って来ました。自分には、人間の生活というものが、見当つかないのです。自分は東北の田舎に生れましたので、汽車をはじめて見たのは、よほど大きくなってからでした。自分は停車場のブリッジを、上って、降りて、そうしてそれが線路をまたぎ越えるために造られたものだという事には全然気づかず、ただそれは停車場の構内を外国の遊戯場みたいに、複雑に楽しく、ハイカラにするためにのみ、設備せられてあるものだ",
            "失格",
            "合格");
        DialogHandler.SetNoAction(handler, ()=>{ Debug.Log("失格"); });
        DialogHandler.SetYesAction(handler, ()=>{ Debug.Log("合格"); });
        DialogHandler.AddDestroyAction(handler, ()=>{ Debug.Log("人類滅亡"); });
    }

    public void ShowListDialog()
    {
        List<ListItem> items = new List<ListItem>();
        for (int i = 0; i < 10; i++)
        {
            var item = ListItem.CreateItem(i, "index = " + i, OnSelectedItem);
            items.Add(item);
        }
        listDialog = ListDialog.ShowDialog(items);
        ListDialog.AddDestroyAction(listDialog, ()=>{ Debug.Log("ListDialog destroyed!"); });
    }

    private void OnSelectedItem(ListItem sender)
    {
        Debug.Log("index = "+sender.index);
        listDialog.CloseDialog();
    }

    public void TakeScreenshot()
    {
#if UNITY_IOS
        string fileName = "screenshot.png";
        StartCoroutine(CaptureScreenshot(fileName));
#else
#endif
    }

    IEnumerator CaptureScreenshot(string fileName)
    {
        ScreenCapture.CaptureScreenshot(fileName);

        var fullPath = Path.Combine(Application.persistentDataPath, fileName);
        while (!File.Exists(fullPath))
        {
            yield return null;
        }
        Debug.Log("CaptureScreenshot was completed.");
        yield return new WaitForEndOfFrame();
        DialogHandler.ShowDialog("Screenshot","Completed");
    }
}
