using TMPro;
using UnityEngine;
using System.Collections;

public class Teleport : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [SerializeField] private InteractionMode interactionMode = InteractionMode.Press;
    [SerializeField] private string interactionMessage = "Teleport to Lobby";

    [Header("UI References")]
    [SerializeField] private Color interactionColor;

    public InteractionMode InteractionMode => interactionMode;

    private void Awake()
    {
        StartCoroutine(RegisterOutline());
    }

    private void OnDestroy()
    {
        if (OutlineManager.Instance != null)
        {
            OutlineManager.Instance.Unregister(gameObject);
        }
    }
    public void Interact(bool isPressed)
    {
        PlayerDungeonHUD.OnShowTeleportConfirm?.Invoke(interactionMessage);
    }
    public void ShowOutline()
    {
        if (OutlineManager.Instance != null)
        {
            OutlineManager.Instance.ShowWithCustomColor(gameObject,interactionColor);
        }
    }

    public void HideOutline()
    {
        if (OutlineManager.Instance != null)
        {
            OutlineManager.Instance.Hide(gameObject);
        }
    }

    public void ShowMessage(TextMeshProUGUI interactionManagerUIText)
    {
       
    }

    public void HideMessage(TextMeshProUGUI interactionManagerUIText)
    {
       
    }

    private IEnumerator RegisterOutline()
    {
        yield return new WaitUntil(()=>OutlineManager.Instance != null);
        OutlineManager.Instance.Register(this.gameObject);  
    }
}