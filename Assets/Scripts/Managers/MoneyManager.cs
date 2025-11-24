using System;
using UnityEngine;

public class MoneyManager : Singleton<MoneyManager>
{
    [SerializeField] private MoneyManagerData moneyManagerData;
       
    private float currentMoney;
    public float CurrentMoney { get => currentMoney; }
   
    public event Action<float> OnMoneyUpdated;
    public event Action<float, bool> OnMoneyTransaction;
    void Awake()
    {
        CreateSingleton(true);
        SuscribeToGameManagerEvent();
    }
    public void AddMoney(float amount, bool isFromGratuity = false)
    {
        AudioManager.Instance.PlayOneShotSFX(isFromGratuity ? "Gratuity" : "AddMoney");

        currentMoney += amount;

        SaveMoney();
        NotifyChanges(amount, true);
    }

    public void SubMoney(float amount)
    {
        currentMoney -= amount;
        if (currentMoney < 0) currentMoney = 0;

        SaveMoney();
        NotifyChanges(amount, false);
    }

    private void NotifyChanges(float amountChanged, bool isPositive)
    {
        OnMoneyUpdated?.Invoke(currentMoney);
        OnMoneyTransaction?.Invoke(amountChanged, isPositive);
    }
    private void SuscribeToGameManagerEvent()
    {
        GameManager.Instance.OnGameSessionStarted += OnInitializeCurrentMoney;
    }

    private void OnInitializeCurrentMoney()
    {
        if (GameManager.Instance.GameSessionType == GameSessionType.Load && SaveSystemManager.SaveExists())
        {
            SaveData data = SaveSystemManager.LoadGame();
            currentMoney = data.money;
        }
        else
        {
            currentMoney = moneyManagerData.InitializeCurrentMoneyValue;
        }

        SaveMoney();
        OnMoneyUpdated?.Invoke(currentMoney);
    }
    private void SaveMoney()
    {
        SaveData data = SaveSystemManager.LoadGame();
        data.money = currentMoney;
        SaveSystemManager.SaveGame(data);
    }
}
