using UnityEngine;

public class InteractionManager : Singleton<InteractionManager>
{
    [SerializeField] private InteractionManagerData interactionManagerData;

    private IInteractable currentTarget;
    // private IInteractable previousTarget;


    void Awake()
    {
        CreateSingleton(true);
        SuscribeToUpdateManagerEvent();
        SuscribeToScenesManagerEvent();
    }

    // Simulacion de Update
    void UpdateInteractionManager()
    {
        DetectTarget();
        InteractWithTarget();
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
            // Ocultamos el UI del objeto que estábamos mirando
            HideCurrentTargetUI();
        }
        currentTarget = null;
    }
    private void ShowCurrentTargetUI()
    {
        // No hacer nada si no hay objetivo o UI
        if (currentTarget == null || InteractionManagerUI.Instance == null) return;

        currentTarget.ShowOutline();

        if (currentTarget.TryGetInteractionMessage(out string message))
        {
            InteractionManagerUI.Instance.MessageAnimator.Show(message);
        }
        else
        {
            InteractionManagerUI.Instance.MessageAnimator.Hide();
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

    //private void DetectTarget()
    //{
    //    if (InteractionManagerUI.Instance == null) return;
    //    if (!InteractionManagerUI.Instance.CenterPointUI.gameObject.activeSelf) return;

    //    Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));

    //    // Si no hay target y antes había uno, limpiamos
    //    if (previousTarget != null)
    //    {
    //        HideAllOutlinesAndTexts();
    //    }

    //    if (Physics.Raycast(ray, out RaycastHit hit, interactionManagerData.InteractionDistance, LayerMask.GetMask("Interactable")))
    //    {
    //        IInteractable hitTarget = hit.collider.GetComponent<IInteractable>()?? hit.collider.GetComponentInChildren<IInteractable>()?? hit.collider.GetComponentInParent<IInteractable>();

    //        if (hitTarget != null)
    //        {

    //            if (hitTarget != previousTarget && previousTarget != null)
    //            {
    //                previousTarget.HideOutline();
    //                previousTarget.HideMessage(InteractionManagerUI.Instance.InteractionMessageText);

    //            }

    //            currentTarget = hitTarget;
    //            previousTarget = hitTarget;

    //            // Mostrar outline siempre, aunque sea el mismo objeto
    //            currentTarget.ShowOutline();
    //            currentTarget.ShowMessage(InteractionManagerUI.Instance.InteractionMessageText);

    //            return;
    //        }
    //    }
    //}

    //private void InteractWithTarget()
    //{
    //    if (InteractionManagerUI.Instance == null) return;
    //    if (!InteractionManagerUI.Instance.CenterPointUI.gameObject.activeSelf) return;

    //    if (currentTarget != null && !PauseManager.Instance.IsGamePaused)
    //    {
    //        switch (currentTarget.InteractionMode)
    //        {
    //            case InteractionMode.Press:
    //                if (PlayerInputs.Instance.InteractPress())
    //                {
    //                    currentTarget.Interact(true);
    //                    currentTarget.HideOutline();
    //                    currentTarget.HideMessage(InteractionManagerUI.Instance.InteractionMessageText);

    //                }
    //                break;

    //            case InteractionMode.Hold:
    //                if (PlayerInputs.Instance.InteractHold())
    //                {
    //                    currentTarget.Interact(true);

    //                }

    //                else
    //                {
    //                    currentTarget.Interact(false);

    //                }
    //                break;
    //        }
    //    }
    //}
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

            if (currentTarget != null)
            {
                ShowCurrentTargetUI();
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
}
