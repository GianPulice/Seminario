using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using UnityEngine.UI;

public class ScenesManager : Singleton<ScenesManager>
{
    /// <sumary>
    /// Agregar metodo futuro para cerrar el juego con pantalla de carga y guardar los datos en ese tiempo
    /// </sumary>

    [SerializeField] private ScenesManagerData scenesManagerData;

    [SerializeField] private GameObject loadingScenePanel;
    [SerializeField] private GameObject exitGamePanel;

    [SerializeField] private TextMeshProUGUI loadingScenePanelText;
    [SerializeField] private Slider loadingBar;

    private event Action onSceneLoadedEvent;

    private Scene currentScene;

    private bool isInLoadingScenePanel = false; 
    private bool isInExitGamePanel = false;

    public Action OnSceneLoadedEvent { get => onSceneLoadedEvent; set => onSceneLoadedEvent = value; }

    public string CurrentSceneName { get => currentScene.name; }

    public bool IsInLoadingScenePanel { get => isInLoadingScenePanel; }
    public bool IsInExitGamePanel { get => isInExitGamePanel; }


    void Awake()
    {
        CreateSingleton(true);
        DontDestroyOnLoadPanels();
        SuscribeToUpdateManagerEvent();
        SetInitializedScene();
    }

    // Simulacion de Update
    void UpdateScenesManager()
    {
        UpdateCurrentSceneName();
    }


    // Para pasar de una escena a otra con pantalla de carga
    public IEnumerator LoadScene(string sceneName, string[] additiveScenes)
    {
        isInLoadingScenePanel = true;
        loadingScenePanel.SetActive(true);

        onSceneLoadedEvent?.Invoke();

        int randomNumber = UnityEngine.Random.Range(0, scenesManagerData.PanelTips.Count);
        loadingScenePanelText.text = scenesManagerData.PanelTips[randomNumber];

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        loadingBar.value = 0.15f;
        StartCoroutine(UpdateLoadingSlider(asyncLoad));

        bool additiveScenesLoaded = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f && !additiveScenesLoaded)
            {
                if (additiveScenes != null)
                {
                    foreach (var additive in additiveScenes)
                    {
                        LoadSceneAdditive(additive);
                    }
                }

                additiveScenesLoaded = true;   
                StartCoroutine(DisableLoadingScenePanelAfterSeconds());
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    // Para cerrar el juego con pantalla de carga
    public IEnumerator ExitGame()
    {
        isInExitGamePanel = true;
        exitGamePanel.SetActive(true);

        yield return new WaitForSecondsRealtime(scenesManagerData.DuringTimeExitGamePanel);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }


    private void DontDestroyOnLoadPanels()
    {
        if (loadingScenePanel != null)
        {
            DontDestroyOnLoad(loadingScenePanel.transform.root.gameObject);
        }

        if (exitGamePanel != null)
        {
            DontDestroyOnLoad(exitGamePanel.transform.root.gameObject);
        }
    }

    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdateAllTime += UpdateScenesManager;
    }

    // Esto sirve para que una vez cargada la nueva escena, espere 3 segundos para desactivar el panel, para que permita cargar Awake y Start de la nueva escena cargada
    private IEnumerator DisableLoadingScenePanelAfterSeconds()
    {
        yield return new WaitForSeconds(scenesManagerData.DuringTimeLoadingScenePanel);
        yield return new WaitUntil(() => loadingBar.value == 1f);
        yield return new WaitForSeconds(0.3f); // Esperar un tiempito mas extra

        isInLoadingScenePanel = false;
        loadingBar.value = 0f;
        loadingScenePanel.SetActive(false);
    }

    // Este metodo solamente funciona para cuando se inicia el programa, es decir solamente una vez en toda la ejecucion
    private void SetInitializedScene()
    {
        UpdateCurrentSceneName();

        switch (currentScene.name)
        {
            case "MainMenu":
                DeviceManager.Instance.IsUIModeActive = true;
                LoadSceneAdditive("MainMenuUI");
                break;

            case "Tabern":
                DeviceManager.Instance.IsUIModeActive = false;
                LoadSceneAdditive("TabernUI");
                LoadSceneAdditive("CompartidoUI");
                break;

            case "Dungeon":
                DeviceManager.Instance.IsUIModeActive = false;
                LoadSceneAdditive("DungeonUI");
                LoadSceneAdditive("CompartidoUI");
                break;
        }
    }

    // Para agregar escenas aditivas a la escena actual
    private AsyncOperation LoadSceneAdditive(string sceneName)
    {
        return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
    }

    private void UpdateCurrentSceneName()
    {
        currentScene = SceneManager.GetActiveScene();
    }

    private IEnumerator UpdateLoadingSlider(AsyncOperation asyncLoad)
    {
        float totalTime = 0.9f + scenesManagerData.DuringTimeLoadingScenePanel;
        float timer = 0f;
        const float startValue = 0.15f;

        while (timer < totalTime)
        {
            timer += Time.deltaTime;

            float sceneProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f); // normaliza a 0-1
            float timeProgress = Mathf.Clamp01(timer / totalTime);

            loadingBar.value = startValue + (Mathf.Min(sceneProgress, timeProgress) * (1f - startValue));

            yield return null;
        }

        loadingBar.value = 1f;
    }
}
