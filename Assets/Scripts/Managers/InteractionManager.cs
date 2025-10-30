using UnityEngine;

public class InteractionManager : Singleton<InteractionManager>
{
    [SerializeField] private InteractionManagerData interactionManagerData;

    private IInteractable currentTarget;

    private bool isPlayerInUI = false;
    

    void Awake()
    {
        CreateSingleton(true);
        SuscribeToPlayerViewEvents();
        SuscribeToUpdateManagerEvent();
        SuscribeToScenesManagerEvent();
    }

    // Simulacion de Update
    void UpdateInteractionManager()
    {
        if (isPlayerInUI) return;
        DetectTarget();
        InteractWithTarget();
    }


    private void SuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode += HandlePlayerEnterUI;
        PlayerView.OnEnterInCookMode += HandlePlayerEnterUI;

        PlayerView.OnExitInAdministrationMode += HandlePlayerExitUI;
        PlayerView.OnExitInCookMode += HandlePlayerExitUI;
    }

    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdate += UpdateInteractionManager;
    }

    private void SuscribeToScenesManagerEvent()
    {
        ScenesManager.Instance.OnSceneLoadedEvent += OnCleanReferencesWhenChangeScene;
    }

    private void OnCleanReferencesWhenChangeScene()
    {
        if (currentTarget != null)
        {
            HideCurrentTargetUI();
        }

        currentTarget = null;
    }

    private bool ShowCurrentTargetUI()
    {
        if (currentTarget == null || InteractionManagerUI.Instance == null) return false;
       
        if (currentTarget.TryGetInteractionMessage(out string message))
        {
            currentTarget.ShowOutline();
            InteractionManagerUI.Instance.MessageAnimator.Show(message);
            return true;
        }

        else
        {
            HideCurrentTargetUI();
            return false;
        }
    }

    private void HideCurrentTargetUI()
    {
        // No hacer nada si no hay objetivo
        if (currentTarget == null) return;

        currentTarget.HideOutline();

        if (InteractionManagerUI.Instance != null)
        {
            InteractionManagerUI.Instance.MessageAnimator.Hide();
        }
    }

    private void DetectTarget()
    {
        if (InteractionManagerUI.Instance == null) return;
        if (!InteractionManagerUI.Instance.CenterPointUI.gameObject.activeSelf) return;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        IInteractable newTarget = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionManagerData.InteractionDistance, LayerMask.GetMask("Interactable")))
        {
            newTarget = hit.collider.GetComponent<IInteractable>() ??
                        hit.collider.GetComponentInChildren<IInteractable>() ??
                        hit.collider.GetComponentInParent<IInteractable>();
        }

        if (newTarget != currentTarget)
        {
            if (currentTarget != null)
            {
                HideCurrentTargetUI();
            }

            currentTarget = newTarget;
        }

        if (currentTarget != null)
        {
            if (!ShowCurrentTargetUI())
            {
                currentTarget = null;
            }
        }
    }
    private void InteractWithTarget()
    {
        if (InteractionManagerUI.Instance == null) return;
        if (!InteractionManagerUI.Instance.CenterPointUI.gameObject.activeSelf) return;

        if (currentTarget != null && !PauseManager.Instance.IsGamePaused)
        {
            switch (currentTarget.InteractionMode)
            {
                case InteractionMode.Press:
                    if (PlayerInputs.Instance.InteractPress())
                    {
                        currentTarget.Interact(true);

                        HideCurrentTargetUI();

                        currentTarget = null;
                    }
                    break;

                case InteractionMode.Hold:
                    if (PlayerInputs.Instance.InteractHold())
                    {
                        currentTarget.Interact(true);
                    }
                    else
                    {
                        currentTarget.Interact(false);
                    }
                    break;
            }
        }
    }

    private void HandlePlayerEnterUI()
    {
        isPlayerInUI = true;

        if (currentTarget != null)
        {
            currentTarget.HideOutline();
            if (InteractionManagerUI.Instance != null)
            {
                InteractionManagerUI.Instance.MessageAnimator.Hide();
            }
            currentTarget = null;
        }
    }

    private void HandlePlayerExitUI()
    {
        isPlayerInUI = false;
    }
}
