using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DialogHandler : MonoBehaviour
{
    [SerializeField] private Image Image_Background = null;
    [SerializeField] private Image Image_Dialog = null;

    [SerializeField] private Text Text_Title = null;
    [SerializeField] private Text Text_Description = null;

    [SerializeField] private Button Button_Yes;
    [SerializeField] private Button Button_No;

    private UnityAction destroyedAction;

    private const string PREFAB_NAME = "GameObject_Dialog";
    private static GameObject prefab;

    public static DialogHandler ShowDialog(string title, string description, string no = null, string yes = null)
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
        var handler = instance.GetComponent<DialogHandler>();
        if (handler == null)
        {
            return null;
        }

        handler.Text_Title.text = title;
        handler.Text_Description.text = description;

        if (string.IsNullOrEmpty(no))
        {
            Destroy(handler.Button_No.gameObject);
            handler.Button_No = null;
        }
        else
        {
            handler.Button_No.GetComponentInChildren<Text>().text = no;
            handler.Button_No.onClick.AddListener(()=>Destroy(handler.gameObject));
        }

        if (string.IsNullOrEmpty(yes))
        {
            Destroy(handler.Button_Yes.gameObject);
            handler.Button_Yes = null;
        }
        else
        {
            handler.Button_Yes.GetComponentInChildren<Text>().text = yes;
            handler.Button_Yes.onClick.AddListener(()=>Destroy(handler.gameObject));
        }

        if (handler.Image_Background != null)
        {
            var trigger = handler.Image_Background.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener(evntData => { Destroy(handler.gameObject); });
            trigger.triggers.Add(entry);
        }
        if (handler.Image_Dialog != null)
        {
            var trigger = handler.Image_Dialog.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener(evntData => { });
            trigger.triggers.Add(entry);
        }

        return handler;
    }

    public static void SetYesAction(DialogHandler handler, UnityAction action)
    {
        if (handler == null)
        {
            return;
        }
        if (handler.Button_Yes == null)
        {
            return;
        }

        handler.Button_Yes.onClick.AddListener(()=>{ action(); });
    }

    public static void SetNoAction(DialogHandler handler, UnityAction action)
    {
        if (handler == null)
        {
            return;
        }
        if (handler.Button_No == null)
        {
            return;
        }

        handler.Button_No.onClick.AddListener(()=>{ action(); });
    }

    public static void AddDestroyAction(DialogHandler handler, UnityAction action)
    {
        if (handler == null)
        {
            return;
        }

        handler.destroyedAction += () =>
        {
            action();
        };
    }

    void OnEnable()
    {
        Debug.Log("OnEnable");
    }

    void OnDisable()
    {
        Debug.Log("OnDisable");
    }

    void OnDestroy()
    {
        Debug.Log("OnDestroy");

        if (destroyedAction != null)
        {
            destroyedAction.Invoke();
        }
    }
}
