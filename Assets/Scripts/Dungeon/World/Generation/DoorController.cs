using UnityEngine;
using TMPro;
using System.Collections;
public class DoorController : MonoBehaviour,IInteractable
{
    [Header("Room Connection")]
    [SerializeField] private bool isExitDoor = true;
    [SerializeField] private bool isFirstDoor = false;

    [Header("Outline")]
    [SerializeField] private Outline doorOutline;
    [SerializeField] private Color openColor = Color.blue;
    [SerializeField] private Color closedColor= Color.red;

   [SerializeField] private bool isLocked = true;

    public InteractionMode InteractionMode => InteractionMode.Press;
    private void Awake()
    {
        StartCoroutine(RegisterOutline());
    }
    private void OnDestroy()
    {
        OutlineManager.Instance.Unregister(gameObject);
    }
    public void Unlock()
    {
        if (!isLocked) return;

        isLocked = false;

        Debug.Log("[DoorController] Puerta desbloqueada.");
    }

    public void Lock()
    {
        if (isLocked) return;

        isLocked = true;


        Debug.Log("[DoorController] Puerta bloqueada.");
    }

    public void Interact(bool isPressed)
    {
        Debug.Log("Interactuando con la puerta");

        if (isLocked)
        {

            Debug.Log("[DoorController] Intento de interactuar, pero está bloqueada.");
            return;
        }

        if (isFirstDoor && !DungeonManager.Instance.RunStarted)
        {
            DungeonManager.Instance.StartDungeonRun();
            return;
        }
       
        if (isExitDoor)
        {
            DungeonManager.Instance.MoveToNext();
        }
    }

    public void ShowOutline()
    {
        if (doorOutline != null)
        {
            doorOutline.OutlineWidth = 2.5f;
            doorOutline.OutlineColor = isLocked ? closedColor : openColor;

            InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Interactive);
        }
    }

    public void HideOutline()
    {
        if (doorOutline != null)
        {
            doorOutline.OutlineWidth = 0f;
            InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Normal);
        }
    }

    public void ShowMessage(TextMeshProUGUI interactionManagerUIText)
    {
        if (interactionManagerUIText == null) return;

        string keyText = $"<color=yellow>{PlayerInputs.Instance.GetInteractInput()}</color>";

        interactionManagerUIText.text = isLocked
            ? "Quedan enemigos por eliminar"
            : $"Presiona {keyText} para pasar a la siguiente sala";
    }

    public void HideMessage(TextMeshProUGUI interactionManagerUIText)
    {
        if (interactionManagerUIText == null) return;
        interactionManagerUIText.text = "";
    }
    private IEnumerator RegisterOutline()
    {
        yield return new WaitUntil(() => OutlineManager.Instance != null);
        OutlineManager.Instance.Register(gameObject);
    }
}