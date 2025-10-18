using System;
using TMPro;
using UnityEngine;
using System.Collections;

public enum DoorAnimationType
{
    Open, Close
}

public class TeleportDungeonUI : MonoBehaviour, IInteractable
{
    private PlayerModel playerModel;

    private Transform leftDoor;
    private Transform rightDoor;

    [SerializeField] private float openDistance = 2f;
    [SerializeField] private float openSpeed = 2f;

    private Vector3 leftClosedLocal;
    private Vector3 rightClosedLocal;
    private Vector3 leftOpenLocal;
    private Vector3 rightOpenLocal;

    private int clientsInside = 0;

    private bool isOpenByPlayer = false;
    private bool isDoorOpen = false;

    private static event Action onShowTeleportPanel;

    public static Action OnShowTeleportPanel { get => onShowTeleportPanel; set => onShowTeleportPanel = value; }

    public InteractionMode InteractionMode { get => InteractionMode.Press; }

    public bool IsOpenByPlayer { get => isOpenByPlayer; set => isOpenByPlayer = value; }


    void Awake()
    {
        GetComponents();
        StartCoroutine(RegisterOutline());
        InitializeDoorLocalPositions();
        RecalculateOpenPositions();
    }

    void OnDestroy()
    {
        OutlineManager.Instance.Unregister(gameObject);
    }

    void OnTriggerEnter(Collider collider)
    {
        OnTriggerEnterWithClients(collider);
    }

    void OnTriggerExit(Collider collider)
    {
        OnTriggerExitWithClients(collider);
    }


    public void Interact(bool isPressed)
    {
        if (!isDoorOpen || (isDoorOpen && !isOpenByPlayer))
        {
            onShowTeleportPanel?.Invoke();
            playerModel.StopVelocity();
            playerModel.IsInTeleportPanel = true;
            isOpenByPlayer = true;
            StartCoroutine(MoveDoorsCoroutine(DoorAnimationType.Open));
        }
    }

    public void ShowOutline()
    {
        if (!isDoorOpen || (isDoorOpen && !isOpenByPlayer))
        {
            OutlineManager.Instance.ShowWithDefaultColor(gameObject);
            InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Interactive);
        }
    }

    public void HideOutline()
    {
        OutlineManager.Instance.Hide(gameObject);
        InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Normal);
    }

    public void ShowMessage(TextMeshProUGUI interactionManagerUIText)
    {
        if (!isDoorOpen || (isDoorOpen && !isOpenByPlayer))
        {
            string keyText = $"<color=yellow> {PlayerInputs.Instance.GetInteractInput()} </color>";
            interactionManagerUIText.text = $"Press" + keyText + "to Teleport";
        }
    }

    public void HideMessage(TextMeshProUGUI interactionManagerUIText)
    {
        interactionManagerUIText.text = string.Empty;
    }

    public IEnumerator MoveDoorsCoroutine(DoorAnimationType animationType)
    {
        if ((animationType == DoorAnimationType.Open && isDoorOpen) || (animationType == DoorAnimationType.Close && !isDoorOpen)) yield break;

        RecalculateOpenPositions();

        Vector3 leftStart = leftDoor.localPosition;
        Vector3 rightStart = rightDoor.localPosition;

        Vector3 leftTarget = animationType == DoorAnimationType.Open ? leftOpenLocal : leftClosedLocal;
        Vector3 rightTarget = animationType == DoorAnimationType.Open ? rightOpenLocal : rightClosedLocal;

        float elapsed = 0f;
        float duration = 1f / openSpeed;

        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            leftDoor.localPosition = Vector3.Lerp(leftStart, leftTarget, t);
            rightDoor.localPosition = Vector3.Lerp(rightStart, rightTarget, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        leftDoor.localPosition = leftTarget;
        rightDoor.localPosition = rightTarget;

        isDoorOpen = animationType == DoorAnimationType.Open;
    }


    private void GetComponents()
    {
        playerModel = FindFirstObjectByType<PlayerModel>();
        leftDoor = transform.Find("LeftDoor");
        rightDoor = transform.Find("RightDoor");
    }

    private IEnumerator RegisterOutline()
    {
        yield return new WaitUntil(() => OutlineManager.Instance != null);

        OutlineManager.Instance.Register(gameObject);
    }

    private void InitializeDoorLocalPositions()
    {
        leftClosedLocal = leftDoor.localPosition;
        rightClosedLocal = rightDoor.localPosition;
    }

    private void RecalculateOpenPositions()
    {
        Vector3 leftTargetWorld = leftDoor.position - transform.forward * openDistance;
        Vector3 rightTargetWorld = rightDoor.position + transform.forward * openDistance;

        leftOpenLocal = leftDoor.parent.InverseTransformPoint(leftTargetWorld);
        rightOpenLocal = rightDoor.parent.InverseTransformPoint(rightTargetWorld);
    }

    private void OnTriggerEnterWithClients(Collider collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Clients"))
        {
            clientsInside++;
            if (clientsInside == 1 && !isOpenByPlayer)
            {
                StartCoroutine(MoveDoorsCoroutine(DoorAnimationType.Open));
            }
        }
    }

    private void OnTriggerExitWithClients(Collider collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Clients"))
        {
            clientsInside--;
            clientsInside = Mathf.Max(clientsInside, 0);
            if (clientsInside == 0 && !isOpenByPlayer)
            {
                StartCoroutine(MoveDoorsCoroutine(DoorAnimationType.Close));
            }
        }
    }
}