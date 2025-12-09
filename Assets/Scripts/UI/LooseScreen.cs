using UnityEngine;
using System;
using TMPro;

public class LooseScreen : Singleton<LooseScreen>
{
    [Header("References")]
    [SerializeField] private CanvasGroup rootGroup;
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private RectTransform panelRoot;
    [SerializeField] private TextMeshProUGUI looseText;

    [Header("Buttons")]
    [SerializeField] private GenericTweenButton restartButton;
    [SerializeField] private GenericTweenButton menuButton;
    [SerializeField] private CanvasGroup restartGroup;
    [SerializeField] private CanvasGroup menuGroup;

    [Header("Animation Times")]
    [SerializeField] private float fadeTime = 0.3f;
    [SerializeField] private float scaleTime = 0.25f;
    [SerializeField] private float buttonsDelay = 0.08f;

    private static event Action onDefeat;
    private static event Action onDefeatClosed;
    private static event Action onRestartPressed;
    private static event Action onMenuPressed;

    public TextMeshProUGUI LooseText { get => looseText; }

    public static Action OnDefeat { get => onDefeat; set => onDefeat = value; }
    public static Action OnDefeatClosed { get => onDefeatClosed; set => onDefeatClosed = value; }
    public static Action OnRestartPressed { get => onRestartPressed; set => onRestartPressed = value; }
    public static Action OnMenuPressed { get => onMenuPressed; set => onMenuPressed = value; }


    void Awake()
    {
        CreateSingleton(false);
        HideImmediate();

        restartButton.OnClick.AddListener(() =>
        {
            DeviceManager.Instance.IsUIModeActive = false;
            SaveSystemManager.DeleteAllData();
            AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
            string[] additiveScenes = new string[] { "TabernUI", "CompartidoUI" };
            StartCoroutine(ScenesManager.Instance.LoadScene("Tabern", additiveScenes));
            Hide();
        });

        menuButton.OnClick.AddListener(() =>
        {
            DeviceManager.Instance.IsUIModeActive = true;
            SaveSystemManager.DeleteAllData();
            AudioManager.Instance.PlayOneShotSFX("ButtonClickWell");
            RaiseMenu();
            HideImmediate();
        });
    }


    public void PlayAudioButtonSelectedHover()
    {
        AudioManager.Instance.PlayOneShotSFX("ButtonSelected");        
    }

    public void Show()
    {
        PlayerInputs.Instance.HasWon(true);
        gameObject.SetActive(true);

        // INITIAL VALUES
        rootGroup.alpha = 0f;
        panelGroup.alpha = 0f;
        panelRoot.localScale = Vector3.one * 0.8f;

        restartGroup.alpha = 0f;
        menuGroup.alpha = 0f;

        Vector3 continueStart = restartGroup.transform.localPosition + new Vector3(0, -25, 0);
        Vector3 menuStart = menuGroup.transform.localPosition + new Vector3(0, -25, 0);

        restartGroup.transform.localPosition = continueStart;
        menuGroup.transform.localPosition = menuStart;

        // BACKGROUND FADE
        LeanTween.alphaCanvas(rootGroup, 1f, fadeTime);

        // PANEL
        LeanTween.alphaCanvas(panelGroup, 1f, fadeTime);

        LeanTween.scale(panelRoot, Vector3.one * 1.05f, scaleTime)
            .setEaseOutBack()
            .setOnComplete(() =>
            {
                LeanTween.scale(panelRoot, Vector3.one, 0.12f)
                    .setEaseOutQuad();
            });

        // BUTTONS
        AnimateButtonGroup(restartGroup, 0.15f);
        AnimateButtonGroup(menuGroup, 0.15f + buttonsDelay);

        onDefeat?.Invoke();
    }

    private void AnimateButtonGroup(CanvasGroup group, float delay)
    {
        LeanTween.alphaCanvas(group, 1f, 0.25f)
            .setDelay(delay);

        LeanTween.moveLocalY(group.gameObject,
            group.transform.localPosition.y + 25,
            0.25f)
            .setDelay(delay)
            .setEaseOutQuad();
    }

    public void Hide()
    {
        LeanTween.alphaCanvas(rootGroup, 0f, fadeTime);
        LeanTween.alphaCanvas(panelGroup, 0f, fadeTime);

        LeanTween.scale(panelRoot, Vector3.one * 0.9f, scaleTime)
        .setEaseInBack()
        .setOnComplete(() =>
        {
            HideImmediate();
            onDefeatClosed?.Invoke();
        });
    }

    public void HideImmediate()
    {
        rootGroup.alpha = 0f;
        panelGroup.alpha = 0f;
        gameObject.SetActive(false);
        PlayerInputs.Instance.HasWon(false);
    }

    public static void RaiseContinue() => onRestartPressed?.Invoke();
    public static void RaiseMenu() => onMenuPressed?.Invoke();
}
