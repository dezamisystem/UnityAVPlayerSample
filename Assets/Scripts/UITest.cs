using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UITest : MonoBehaviour
{
    private ListDialog listDialog;

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
