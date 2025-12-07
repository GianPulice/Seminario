using UnityEngine;

public class VictoryOnUpgradesCompleted : MonoBehaviour
{
    [SerializeField] private VictoryScreen victoryScreen;

    private void OnEnable()
    {
        UpgradesManager.Instance.OnAllUpgradesCompleted += ShowVictory;
    }

    private void OnDisable()
    {
        if (UpgradesManager.Exists)
            UpgradesManager.Instance.OnAllUpgradesCompleted -= ShowVictory;
    }

    private void ShowVictory()
    {
        victoryScreen.Show(); 
    }
}
