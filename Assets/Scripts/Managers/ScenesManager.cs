using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class ScenesManager : Singleton<ScenesManager>
{
    /// <sumary>
    /// Agregar metodo futuro para cerrar el juego con pantalla de carga y guardar los datos en ese tiempo
    /// </sumary>

    [SerializeField] private ScenesManagerData scenesManagerData;

    [SerializeField] private GameObject loadingScenePanel;
    [SerializeField] private GameObject exitGamePanel;

    [SerializeField] private TextMeshProUGUI loadingScenePanelText;

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
        SuscribeToSceneLoadedEvent();
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
        loadingScenePanel.SetActive(true);
        isInLoadingScenePanel = true;

        int randomNumber = UnityEngine.Random.Range(0, scenesManagerData.PanelTips.Count);
        loadingScenePanelText.text = scenesManagerData.PanelTips[randomNumber];

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        float elapsedTime = 0f;

        while (!asyncLoad.isDone)
        {
            elapsedTime += Time.deltaTime;

            if (asyncLoad.progress >= 0.9f && elapsedTime >= scenesManagerData.DuringTimeLoadingScenePanel)
            {
                if (additiveScenes != null)
                {
                    for (int i = 0; i < additiveScenes.Length; i++)
                    {
                        AsyncOperation additiveLoad = LoadSceneAdditive(additiveScenes[i]);
                    }
                }

                StartCoroutine(DisableLoadingScenePanelAfterSeconds());

                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    // Para cerrar el juego con pantalla de carga
    public IEnumerator ExitGame()
    {
        exitGamePanel.SetActive(true);
        isInExitGamePanel = true;

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

    private void SuscribeToSceneLoadedEvent()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void SuscribeToUpdateManagerEvent()
    {
        UpdateManager.OnUpdateAllTime += UpdateScenesManager;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        onSceneLoadedEvent?.Invoke();
    }

    // Esto sirve para que una vez cargada la nueva escena, espere 3 segundos para desactivar el panel, para que permita cargar Awake y Start de la nueva escena cargada
    private IEnumerator DisableLoadingScenePanelAfterSeconds()
    {
        yield return new WaitForSeconds(3);

        isInLoadingScenePanel = false;
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
}
