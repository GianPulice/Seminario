using UnityEngine;

public class InteractionManager : Singleton<InteractionManager>
{
    [SerializeField] private InteractionManagerData interactionManagerData;

    private IInteractable currentTarget;
    private IInteractable previousTarget;


    void Awake()
    {
        CreateSingleton(true);
        SuscribeToUpdateManagerEvent();
        SuscribeToScenesManagerEvent();
        SuscribeToBookManagerUI();
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

    private void SuscribeToBookManagerUI()
    {
        BookManagerUI.OnHideOutlinesAndTextsFromInteractableElements += HideAllOutlinesAndTexts;
    }

    private void OnCleanReferencesWhenChangeScene()
    {
        previousTarget = null;
        currentTarget = null;
    }

    private void HideAllOutlinesAndTexts()
    {
        previousTarget?.HideOutline();
        previousTarget?.HideMessage(InteractionManagerUI.Instance.InteractionMessageText);
        currentTarget?.HideOutline();
        currentTarget?.HideMessage(InteractionManagerUI.Instance.InteractionMessageText);

        OnCleanReferencesWhenChangeScene();
    }

    private void DetectTarget()
    {
        if (InteractionManagerUI.Instance == null) return;
        if (!InteractionManagerUI.Instance.CenterPointUI.gameObject.activeSelf) return;
        if (ScenesManager.instance.CurrentSceneName == "Tabern")
        {
            if (BookManagerUI.Instance == null) return;
            if (BookManagerUI.Instance.IsBookOpen) return;
        }

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f));

        // Si no hay target y antes había uno, limpiamos
        if (previousTarget != null)
        {
            previousTarget?.HideOutline();
            previousTarget?.HideMessage(InteractionManagerUI.Instance.InteractionMessageText);
            currentTarget?.HideOutline();
            currentTarget?.HideMessage(InteractionManagerUI.Instance.InteractionMessageText);
           
            previousTarget = null;
            currentTarget = null;
        }

        if (Physics.Raycast(ray, out RaycastHit hit, interactionManagerData.InteractionDistance, LayerMask.GetMask("Interactable")))
        {
            IInteractable hitTarget = hit.collider.GetComponent<IInteractable>()?? hit.collider.GetComponentInChildren<IInteractable>()?? hit.collider.GetComponentInParent<IInteractable>();

            if (hitTarget != null)
            {

                if (hitTarget != previousTarget && previousTarget != null)
                {
                    previousTarget.HideOutline();
                    previousTarget.HideMessage(InteractionManagerUI.Instance.InteractionMessageText);
                   
                }

                currentTarget = hitTarget;
                previousTarget = hitTarget;

                // Mostrar outline siempre, aunque sea el mismo objeto
                currentTarget.ShowOutline();
                currentTarget.ShowMessage(InteractionManagerUI.Instance.InteractionMessageText);
               
                return;
            }
        }
    }

    private void InteractWithTarget()
    {
        if (InteractionManagerUI.Instance == null) return;
        if (!InteractionManagerUI.Instance.CenterPointUI.gameObject.activeSelf) return;
        if (ScenesManager.instance.CurrentSceneName == "Tabern")
        {
            if (BookManagerUI.Instance == null) return;
            if (BookManagerUI.Instance.IsBookOpen) return;
        }
        if (currentTarget != null && !PauseManager.Instance.IsGamePaused)
        {
            switch (currentTarget.InteractionMode)
            {
                case InteractionMode.Press:
                    if (PlayerInputs.Instance.InteractPress())
                    {
                        currentTarget.Interact(true);
                        currentTarget.HideOutline();
                        currentTarget.HideMessage(InteractionManagerUI.Instance.InteractionMessageText);
                        
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
