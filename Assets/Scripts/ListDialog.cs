/*
 * ListDialog.cs
 * Copyright (c) 2020 東亜プリン秘密研究所. All rights reserved.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ListDialog : MonoBehaviour
{
    [SerializeField] private Image Image_Background = null;
    [SerializeField] private GameObject content = null;

    private UnityAction destroyedAction;
    private UnityAction selectedAction;

    private const string PREFAB_NAME = "GameObject_ListDialog";
    private static GameObject prefab;

    public static ListDialog ShowDialog(List<ListItem> items)
    {
        if (prefab == null)
        {
            prefab = Resources.Load(PREFAB_NAME) as GameObject;
        }
        if (prefab == null)
        {
            Debug.LogError("Cannot find prefab : " + PREFAB_NAME);
            return null;
        }

        var instance = Instantiate(prefab);
        var dialog = instance.GetComponent<ListDialog>();
        if (dialog == null)
        {
            return null;
        }

        if (dialog.Image_Background != null)
        {
            var trigger = dialog.Image_Background.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener(evntData => { Destroy(dialog.gameObject); });
            trigger.triggers.Add(entry);
        }

        if (dialog.content != null && items.Count > 0)
        {
            var rectTransform = dialog.content.GetComponent<RectTransform>();
            foreach (ListItem item in items)
            {
                var itemRectTransform = item.GetComponentInChildren<RectTransform>();
                itemRectTransform.SetParent(rectTransform, false);
            }
        }

        return dialog;
    }

    public static void SetSelectedAction(ListDialog dialog, UnityAction action)
    {
        if (dialog == null)
        {
            return;
        }
        dialog.selectedAction = action;
    }

    public static void AddDestroyAction(ListDialog dialog, UnityAction action)
    {
        if (dialog == null)
        {
            return;
        }
        dialog.destroyedAction += action;
    }

    public void CloseDialog()
    {
        Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        if (destroyedAction != null)
        {
            destroyedAction.Invoke();
        }
    }

}
