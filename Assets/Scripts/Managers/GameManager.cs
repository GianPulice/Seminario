using System;

public enum GameSessionType
{
    None, New, Load
}

public class GameManager : Singleton<GameManager>
{
    private event Action onGameSessionStarted;

    private GameSessionType gameSessionType = GameSessionType.None;

    public Action OnGameSessionStarted { get => onGameSessionStarted; set => onGameSessionStarted = value; }
    public GameSessionType GameSessionType { get => gameSessionType; set => gameSessionType = value; }


    void Awake()
    {
        CreateSingleton(true);
    }

    void Start()
    {
        InitializeGameSessionByDefault();
    }

    // Esto sirve para inicializar el sistema de guardado correctamente si corremos el programa desde cualquier escena menos "MainMenu"
    private void InitializeGameSessionByDefault()
    {
        if (SaveSystemManager.Instance.SaveSystemData.UseSaveSystem)
        {
            if (ScenesManager.Instance.CurrentSceneName != "MainMenu")
            {
                gameSessionType = GameSessionType.Load;
                onGameSessionStarted?.Invoke();
            }
        }

        else
        {
            if (ScenesManager.Instance.CurrentSceneName != "MainMenu")
            {
                gameSessionType = GameSessionType.New;
                onGameSessionStarted?.Invoke();
            }
        }
    }
}
