using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SliderEventTrigger : EventTrigger
{
    public Action BeginAction;
    public Action MovingAction;
    public Action EndAction;

    /// <summary>
    /// ドラッグ開始
    /// </summary>
    /// <param name="eventData">イベントデータ</param>
    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        Debug.Log("SliderEventTrigger: OnInitializePotentialDrag");
        if (BeginAction != null)
        {
            BeginAction();
        }
    }

    /// <summary>
    /// ドラッグ中
    /// </summary>
    /// <param name="eventData">イベントデータ</param>
    public override void OnDrag(PointerEventData eventData)
    {
        Debug.Log("SliderEventTrigger: OnDrag");
        if (MovingAction != null)
        {
            MovingAction();
        }
    }

    /// <summary>
    /// ドラッグ解除
    /// </summary>
    /// <param name="eventData">イベントデータ</param>
    public override void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("SliderEventTrigger: OnPointerUp");
        if (EndAction != null)
        {
            EndAction();
        }
    }
}
