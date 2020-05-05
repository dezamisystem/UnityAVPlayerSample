using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SliderEventTrigger : EventTrigger
{
    public override void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("SliderEventTrigger: OnPointerEnter");
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("SliderEventTrigger: OnPointerDown");
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("SliderEventTrigger: OnPointerUp");
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("SliderEventTrigger: OnPointerExit");
    }

    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        Debug.Log("SliderEventTrigger: OnInitializePotentialDrag");
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("SliderEventTrigger: OnBeginDrag");
    }

    public override void OnDrag(PointerEventData eventData)
    {
        Debug.Log("SliderEventTrigger: OnDrag");
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("SliderEventTrigger: OnEndDrag");
    }

    public override void OnDrop(PointerEventData eventData)
    {
        Debug.Log("SliderEventTrigger: OnDrop");
    }
}
