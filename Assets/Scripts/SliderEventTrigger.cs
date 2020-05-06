using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SliderEventTrigger : EventTrigger
{
    public Action BeginAction;
    public Action MovingAction;
    public Action EndAction;

    public override void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("SliderEventTrigger: OnPointerUp");
        if (EndAction != null)
        {
            EndAction();
        }
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        Debug.Log("SliderEventTrigger: OnInitializePotentialDrag");
        if (BeginAction != null)
        {
            BeginAction();
        }
    }

    public override void OnDrag(PointerEventData eventData)
    {
        Debug.Log("SliderEventTrigger: OnDrag");
        if (MovingAction != null)
        {
            MovingAction();
        }
    }

}
