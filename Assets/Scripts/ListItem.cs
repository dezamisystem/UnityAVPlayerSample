using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ListItem : MonoBehaviour
{
    [SerializeField] private Text Text_Description = null;
    [SerializeField] private Button Button_Select = null;

    public int index = 0;

    private const string PREFAB_NAME = "Panel_ListItem";
    private static GameObject prefab;

    public static ListItem CreateItem(int index, string description, Action<ListItem> action)
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
        var item = instance.GetComponent<ListItem>();
        if (item == null)
        {
            return null;
        }
        item.index = index;
        item.Text_Description.text = description;
        item.Button_Select.onClick.AddListener(()=>{ action(item); });

        return item;
    }
}
