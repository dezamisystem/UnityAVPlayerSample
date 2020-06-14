﻿/*
 * SliderEventTrigger.cs
 * Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
 */
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SliderEventTrigger : EventTrigger
{
    public UnityAction BeginAction;
    public UnityAction MovingAction;
    public UnityAction EndAction;

    private bool isMoved = false;

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
        isMoved = false;
    }

    /// <summary>
    /// ドラッグ中
    /// </summary>
    /// <param name="eventData">イベントデータ</param>
    public override void OnDrag(PointerEventData eventData)
    {
        if (!isMoved)
        {
            Debug.Log("SliderEventTrigger: OnDrag");
            isMoved = true;
        }
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
        isMoved = false;
    }
}
