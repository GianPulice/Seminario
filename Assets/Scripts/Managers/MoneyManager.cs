using TMPro;
using UnityEngine;

public class MoneyManager : Singleton<MoneyManager>
{
    [SerializeField] private MoneyManagerData moneyManagerData;

    [SerializeField] private ObjectPooler floatingMoneyTextPool;
    [SerializeField] private FloatingMoneyText floatingMoneyText;

    [SerializeField] private Color tipColor = Color.green;
    [SerializeField] private Color deductionColor = Color.red;

    private TextMeshProUGUI moneyText;

    private bool isInAdminMode = false;
    private float currentMoney;

    public float CurrentMoney { get => currentMoney; }


    void Awake()
    {
        CreateSingleton(true);
        SuscribeToMoneyTextEvent();
        SuscribeToGameManagerEvent();
        SuscribeToPlayerViewEvents();
        InitializeMoneyDefault();
    }


    public void AddMoney(float amount, bool isFromGratuity = false)
    {
        if (isFromGratuity)
        {
            AudioManager.Instance.PlayOneShotSFX("Gratuity");
            TabernManager.Instance.TipsEarnedAmount += amount;
        }

        else
        {
            AudioManager.Instance.PlayOneShotSFX("AddMoney");
            TabernManager.Instance.OrderPaymentsAmount += amount;
        }

        currentMoney += amount;
        UpdateMoneyText();
        ShowFloatingMoneyText(amount, true);

        if(UpgradesManager.Exists)
            UpgradesManager.Instance.RefreshAvailabilityState();
    }

    public void SubMoney(float amount)
    {
        currentMoney -= amount;
        if (currentMoney < 0)
        {
            currentMoney = 0;
        }

        UpdateMoneyText();
        ShowFloatingMoneyText(amount, false);
        if (UpgradesManager.Exists)
            UpgradesManager.Instance.RefreshAvailabilityState();
    }


    private void SuscribeToMoneyTextEvent()
    {
        MoneyManagerUI.OnTextGetComponent += GetComponentFromEvent;
    }

    private void SuscribeToGameManagerEvent()
    {
        //GameManager.Instance.OnGameSessionStarted += OnInitializeCurrentMoney;
        SaveSystemManager.OnSavOrLoadAllGame += OnInitializeCurrentMoneyWithSaveSystem;
    }

    private void SuscribeToPlayerViewEvents()
    {
        PlayerView.OnEnterInAdministrationMode += HandleEnterAdminMode;
        PlayerView.OnExitInAdministrationMode += HandleExitAdminMode;
    }

    private void HandleEnterAdminMode()
    {
        isInAdminMode = true;
    }

    private void HandleExitAdminMode()
    {
        isInAdminMode = false;
    }

    private void GetComponentFromEvent(TextMeshProUGUI moneyText)
    {
        this.moneyText = moneyText;

        UpdateMoneyText();
    }

    private void OnInitializeCurrentMoneyWithSaveSystem()
    {
        if (!SaveSystemManager.SaveExists())
        {
            SaveMoney();
            return;
        }

        if (SaveSystemManager.SaveExists())
        {
            SaveData data = SaveSystemManager.LoadGame();
            currentMoney = data.money;
            SaveMoney();
            return;
        }

        if (UpgradesManager.Exists)
        {
            UpgradesManager.Instance.RefreshAvailabilityState();
        }
    }

    private void InitializeMoneyDefault()
    {
        if (!SaveSystemManager.SaveExists())
        {
            currentMoney = moneyManagerData.InitializeCurrentMoneyValue;
        }
    }

    private void SaveMoney()
    {
        SaveData data = SaveSystemManager.LoadGame();
        data.money = currentMoney;
        SaveSystemManager.SaveGame(data);
    }

    private void UpdateMoneyText()
    {
        moneyText.text = currentMoney.ToString();
    }

    private void ShowFloatingMoneyText(float amount, bool positive)
    {   
        FloatingMoneyText go = Instantiate(floatingMoneyText, moneyText.transform.position, Quaternion.identity);

        if (positive)
        {
            go.TextAmount.text = "+" + amount.ToString();
            go.TextAmount.color = tipColor;
        }

        else
        {
            go.TextAmount.text = "-" + amount.ToString();
            go.TextAmount.color = deductionColor;
        }
        
        go.ActivateAdminAnimation(isInAdminMode);
        
        Destroy(go.gameObject, go.MaxTimeToReturnObjectToPool);
    }
}
