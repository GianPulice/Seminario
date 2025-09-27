using UnityEngine;
using TMPro;
public class DoorController : MonoBehaviour,IInteractable
{
    //[Header("Animation")]
    //[SerializeField] private Animator doorAnimator;
    //[SerializeField] private string openTrigger = "Open";
    //[SerializeField] private string closeTrigger = "Close";

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
        GetComponents();
    }

    public void Unlock()
    {
        if (!isLocked) return;

        isLocked = false;

        // Play opening animation
        //if (doorAnimator != null)
        //    doorAnimator.SetTrigger(openTrigger);

        // Play VFX
        //if (openVFX != null)
        //    openVFX.Play();

        Debug.Log("[DoorController] Puerta desbloqueada.");
    }

    public void Lock()
    {
        if (isLocked) return;

        isLocked = true;

        //// Play closing animation
        //if (doorAnimator != null)
        //    doorAnimator.SetTrigger(closeTrigger);

        //// Play VFX
        //if (closeVFX != null)
        //    closeVFX.Play();

        Debug.Log("[DoorController] Puerta bloqueada.");
    }

    public Vector3 GetSpawnPoint() => transform.position;
    public bool IsLocked => isLocked;

    public void Interact(bool isPressed)
    {
        Debug.Log("Interactuando con la puerta");

        if (isLocked)
        {
            Debug.Log("[DoorController] Intento de interactuar, pero está bloqueada.");
            return;
        }

        // Si es la primera puerta y la run no ha comenzado, iniciar la run.
        if (isFirstDoor && !DungeonManager.Instance.RunStarted)
        {
            DungeonManager.Instance.StartDungeonRun();
            return;
        }
        // Si la puerta es una salida y no está bloqueada (porque la sala ya se despejó),
        // el jugador puede pasar a la siguiente sala.
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
    private void GetComponents()
    {
        doorOutline = GetComponent<Outline>();
       //doorAnimator = GetComponent<Animator>();
    }
}