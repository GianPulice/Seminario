using UnityEngine;

public class TutorialTabButton : MonoBehaviour
{
    [Header("Configuración del Tutorial")]
    [Tooltip("El tipo de tutorial que este botón debe activar.")]
    [SerializeField] private TutorialType tutorialType;

    [Tooltip("Arrastra el objeto 'TutorialScreensManager' de tu escena aquí.")]
    [SerializeField] private TutorialScreensManager tutorialManager;

    private TabTweenButton tabButton;

    private void Awake()
    {
        tabButton = GetComponent<TabTweenButton>();

        if (tutorialManager == null)
        {
            tutorialManager = FindAnyObjectByType<TutorialScreensManager>();
        }

        if (tabButton != null && tutorialManager != null)
        {
            tabButton.OnClick.AddListener(OnThisTabClicked);
        }
        else if (tutorialManager == null)
        {
            Debug.LogError("TutorialManager no asignado en " + gameObject.name, this);
        }
    }

    private void OnDestroy()
    {
        if (tabButton != null)
        {
            tabButton.OnClick.RemoveListener(OnThisTabClicked);
        }
    }

    /// <summary>
    /// Esta función es llamada por el evento 'OnClick' del TabTweenButton.
    /// </summary>
    private void OnThisTabClicked()
    {
        // Le decimos al manager qué tutorial poner.
        tutorialManager.SetTutorialType(tutorialType);
    }
}
