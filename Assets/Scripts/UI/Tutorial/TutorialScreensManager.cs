using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public enum TutorialType
{
    Admin,
    Cooking,
    Clients
}

public class TutorialScreensManager : Singleton<TutorialScreensManager>
{
    [SerializeField] private TutorialData tutorialData;
    [SerializeField] private AppearTutorialScreen appearAnim;
    [SerializeField] private Image imageToChange;
    [Header("Datos de Tutorial")]
    [SerializeField] private List<TutorialImageData> tutorialImages;

    private TutorialType currentTutorialType;
    public TutorialType CurrentTutorialType => currentTutorialType;

    private Dictionary<TutorialType, Sprite> tutorialImageDictionary;
    private void Awake()
    {
        CreateSingleton(false);
        BuildDictionary();

    }
    public void SetTutorialType(TutorialType tutorialType)
    {
        if (tutorialData.ActivateTutorial == false) return;
        DeviceManager.instance.IsUIModeActive = true;
        currentTutorialType = tutorialType;
        UpdateTutorialImage();
    }
    public void Close()
    {
        DeviceManager.instance.IsUIModeActive = false;
        appearAnim?.HidePanel();
    }

    public void SetTutorialType(int tutorialTypeIndex)
    {
        if (tutorialData.ActivateTutorial == false) return;
        if (System.Enum.IsDefined(typeof(TutorialType), tutorialTypeIndex))
            SetTutorialType((TutorialType)tutorialTypeIndex);
        else
            Debug.LogError($"Índice de tutorial '{tutorialTypeIndex}' no es válido.", this);
    }

    private void UpdateTutorialImage()
    {
        if (tutorialData.ActivateTutorial == false) return;
        if (imageToChange == null)
        {
            Debug.LogError("No hay una 'Image' asignada para cambiar.", this);
            return;
        }

        if (tutorialImageDictionary.TryGetValue(currentTutorialType, out Sprite spriteToShow))
            imageToChange.sprite = spriteToShow;
        else
        {
            Debug.LogWarning($"No se encontró un Sprite para el tipo '{currentTutorialType}'.", this);
            imageToChange.sprite = null;
        }
        appearAnim?.ShowPannel();
    }
    private void BuildDictionary()
    {
        tutorialImageDictionary = tutorialImages
            .GroupBy(data => data.type)
            .ToDictionary(g => g.Key, g => g.First().sprite);
    }

}

[System.Serializable]
public class TutorialImageData
{
    public TutorialType type;
    public Sprite sprite;
}