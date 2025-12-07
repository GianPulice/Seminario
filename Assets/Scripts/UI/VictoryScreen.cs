using UnityEngine;
using System;
public class VictoryScreen : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup rootGroup;
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private RectTransform panelRoot;

    [Header("Buttons")]
    [SerializeField] private GenericTweenButton continueButton;
    [SerializeField] private GenericTweenButton menuButton;
    [SerializeField] private CanvasGroup continueGroup;
    [SerializeField] private CanvasGroup menuGroup;

    [Header("Animation Times")]
    [SerializeField] private float fadeTime = 0.3f;
    [SerializeField] private float scaleTime = 0.25f;
    [SerializeField] private float buttonsDelay = 0.08f;

    public static event Action OnVictory;
    public static event Action OnVictoryClosed;
    public static event Action OnContinuePressed;
    public static event Action OnMenuPressed;

    void Awake()
    {
        HideImmediate();
        continueButton.OnClick.AddListener(() =>
        {
            RaiseContinue();
            Hide();
        });
        menuButton.OnClick.AddListener(() => 
        {
            RaiseMenu();
            HideImmediate();
        });
    }

    public void Show()
    {
        PlayerInputs.Instance.HasWon(true);
        gameObject.SetActive(true);
        // INITIAL VALUES
        rootGroup.alpha = 0f;
        panelGroup.alpha = 0f;
        panelRoot.localScale = Vector3.one * 0.8f;

        continueGroup.alpha = 0f;
        menuGroup.alpha = 0f;

        Vector3 continueStart = continueGroup.transform.localPosition + new Vector3(0, -25, 0);
        Vector3 menuStart = menuGroup.transform.localPosition + new Vector3(0, -25, 0);

        continueGroup.transform.localPosition = continueStart;
        menuGroup.transform.localPosition = menuStart;

        // BACKGROUND FADE
        LeanTween.alphaCanvas(rootGroup, 1f, fadeTime)
            .setIgnoreTimeScale(true);

        // PANEL
        LeanTween.alphaCanvas(panelGroup, 1f, fadeTime)
            .setIgnoreTimeScale(true);

        LeanTween.scale(panelRoot, Vector3.one * 1.05f, scaleTime)
            .setEaseOutBack()
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                LeanTween.scale(panelRoot, Vector3.one, 0.12f)
                    .setEaseOutQuad()
                    .setIgnoreTimeScale(true);
            });

        // BUTTONS (delayed stagger)
        AnimateButtonGroup(continueGroup, 0.15f);
        AnimateButtonGroup(menuGroup, 0.15f + buttonsDelay);

        OnVictory?.Invoke();
        // AudioManager.Instance.PlayOneShotSFX("UI_PanelOpen");
    }
    private void AnimateButtonGroup(CanvasGroup group, float delay)
    {
        LeanTween.alphaCanvas(group, 1f, 0.25f)
            .setDelay(delay)
            .setIgnoreTimeScale(true);

        LeanTween.moveLocalY(group.gameObject,
            group.transform.localPosition.y + 25,
            0.25f)
            .setDelay(delay)
            .setEaseOutQuad()
            .setIgnoreTimeScale(true);
    }
    public void Hide()
    {
        LeanTween.alphaCanvas(rootGroup, 0f, fadeTime)
              .setIgnoreTimeScale(true);

        LeanTween.alphaCanvas(panelGroup, 0f, fadeTime)
            .setIgnoreTimeScale(true);

        LeanTween.scale(panelRoot, Vector3.one * 0.9f, scaleTime)
        .setEaseInBack()
        .setIgnoreTimeScale(true)
        .setOnComplete(() =>
        {
            HideImmediate();
            OnVictoryClosed?.Invoke();
        });


        //  AudioManager.Instance.PlayOneShotSFX("UI_PanelClose");
    }

    public void HideImmediate()
    {
        rootGroup.alpha = 0f;
        panelGroup.alpha = 0f;
        gameObject.SetActive(false);
        PlayerInputs.Instance.HasWon(false);
    }

    public static void RaiseContinue() => OnContinuePressed?.Invoke();
    public static void RaiseMenu() => OnMenuPressed?.Invoke();
}
