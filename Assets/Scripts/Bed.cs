using System.Collections;
using UnityEngine;

public class Bed : MonoBehaviour, IInteractable
{
    public InteractionMode InteractionMode { get => InteractionMode.Press; }


    void Awake()
    {
        StartCoroutine(RegisterOutline());
    }

    void OnDestroy()
    {
        OutlineManager.Instance.Unregister(gameObject);
    }


    public void Interact(bool isPressed)
    {
        if (TabernManager.Instance.TabernCurrentTimeText.text == "24 : 00")
        {
            TabernManager.Instance.SkipCurrentDay();
        }
    }

    public void ShowOutline()
    {
        if (TabernManager.Instance.TabernCurrentTimeText.text == "24 : 00")
        {
            OutlineManager.Instance.ShowWithDefaultColor(gameObject);
        }
    }

    public void HideOutline()
    {
        OutlineManager.Instance.Hide(gameObject);
    }

    public bool TryGetInteractionMessage(out string message)
    {
        if (TabernManager.Instance.TabernCurrentTimeText.text == "24 : 00")
        {
            string keyText = $"<color=yellow> {PlayerInputs.Instance.GetInteractInput()} </color>";
            message = $"Press {keyText} to skip the day";
            return true;
        }

        message = null;
        return false;
    }


    private IEnumerator RegisterOutline()
    {
        yield return new WaitUntil(() => OutlineManager.Instance != null);

        OutlineManager.Instance.Register(gameObject);
    }
}
