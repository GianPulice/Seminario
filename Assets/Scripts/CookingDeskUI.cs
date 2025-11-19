using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CookingDeskUI : MonoBehaviour, IInteractable
{
    private PlayerController playerController;

    [SerializeField] private Camera cookingCamera;
    
    [SerializeField] private List<Transform> stovePositionsThisDesk;
    private Queue<Transform> availableStoves = new Queue<Transform>();
    private HashSet<Transform> occupiedStoves = new HashSet<Transform>();

    public Camera CookingCamera => cookingCamera;

    public Queue<Transform> AvailableStoves { get => availableStoves; }

    public InteractionMode InteractionMode { get => InteractionMode.Press; }


    void Awake()
    {
        GetComponents();
        EnqueueCurrentStovesPositions();
        InitializeCurrentCamer();
        StartCoroutine(RegisterOutline());
    }

    void OnDestroy()
    {
        OutlineManager.Instance.Unregister(gameObject);
    }


    public void Interact(bool isPressed)
    {
        playerController.PlayerModel.IsCooking = true;
        CookingManager.Instance.SetCurrentDesk(this);
        CookingManagerUI.Instance.OpenKitchen(this);
    }

    public void ShowOutline()
    {
        OutlineManager.Instance.ShowWithDefaultColor(gameObject);
        InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Interactive);
    }

    public void HideOutline()
    {
        OutlineManager.Instance.Hide(gameObject);
        InteractionManagerUI.Instance.ModifyCenterPointUI(InteractionType.Normal);
    }

    public bool TryGetInteractionMessage(out string message)
    {
        string keyText = $"<color=yellow> {PlayerInputs.Instance.GetInteractInput()} </color>";
        message = $"Press {keyText} to start cooking";

        return true;
    }

    public Transform GetAvailableStove()
    {
        if (availableStoves.Count == 0)
            return null;

        Transform stove = null;

        while (availableStoves.Count > 0)
        {
            stove = availableStoves.Dequeue();

            if (occupiedStoves.Contains(stove))
            {
                availableStoves.Enqueue(stove);
                continue;
            }

            occupiedStoves.Add(stove);
            break;
        }

        return stove;
    }

    public void ReleaseStove(Transform stove)
    {
        if (occupiedStoves.Contains(stove))
        {
            occupiedStoves.Remove(stove);
            availableStoves.Enqueue(stove);
        }
    }

    public int StoveIndexOf(Transform stove)
    {
        return stovePositionsThisDesk.IndexOf(stove);
    }


    private void GetComponents()
    {
        playerController = FindFirstObjectByType<PlayerController>();
    }

    private void EnqueueCurrentStovesPositions()
    {
        foreach (var pos in stovePositionsThisDesk)
        {
            availableStoves.Enqueue(pos);
        }
    }

    private void InitializeCurrentCamer()
    {
        cookingCamera.targetTexture = null;
        cookingCamera.enabled = false;
    }

    private IEnumerator RegisterOutline()
    {
        yield return new WaitUntil(() => OutlineManager.Instance != null);

        OutlineManager.Instance.Register(gameObject);
    }
}
