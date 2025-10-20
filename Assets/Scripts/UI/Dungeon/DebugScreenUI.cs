using UnityEngine;
using UnityEngine.UI;

public class TeleportUIManager : MonoBehaviour
{
    [System.Serializable]
    public class TeleportButton
    {
        public Button button;   // El botón en la UI
        public string spawnID;  // El ID que coincide con el SpawnPointManager
    }

    [Header("Player Reference")]
    [SerializeField] private Transform player;

    [Header("Teleport Buttons")]
    [SerializeField] private TeleportButton[] teleportButtons;

    [SerializeField] private GameObject fatherObject;

    private void Awake()
    {
        fatherObject.SetActive(false);
    }
    private void Start()
    {

        foreach (var tb in teleportButtons)
        {
            if (tb.button != null && !string.IsNullOrEmpty(tb.spawnID))
            {
                string id = tb.spawnID; // necesario para evitar closure bug
                tb.button.onClick.AddListener(() => TeleportTo(id));
               
            }
            else
            {
                Debug.LogWarning("Button o SpawnID vacío en TeleportUIManager.");
            }
        }
    }

    private void Update()
    {
        if (PlayerInputs.Instance._Debug())
        {
            fatherObject.SetActive(!fatherObject.activeSelf);
            DeviceManager.Instance.IsUIModeActive = fatherObject.activeSelf;
        }
    }

    private void TeleportTo(string id)
    {
        Debug.Log($"Intentando teletransportar a {id}");

        if (DebugScreenManager.Instance == null)
        {
            Debug.LogWarning("SpawnPointManager no encontrado en la escena.");
            return;
        }

        Transform spawn = DebugScreenManager.Instance.GetSpawn(id);
        if (spawn != null && player != null)
        {
            Debug.Log($"Teletransportando player a {id}");
            player.position = spawn.position;
            player.rotation = spawn.rotation;
        }
        else
        {
            Debug.LogWarning($"No se encontró spawn con ID: {id}");
        }
    }
}
