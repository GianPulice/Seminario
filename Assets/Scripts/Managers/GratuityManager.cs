using UnityEngine;

public class GratuityManager : Singleton<GratuityManager>
{
    //[SerializeField] private GratuityManagerData gratuityManagerData;


    void Awake()
    {
        CreateSingleton(false);
    }


    public void TryGiveGratuity(int paymentAmount, ClientData clientData)
    {
        int roll = Random.Range(0, 100);
        if (roll >= clientData.ProbabilityToGiveGratuity) return;

        int randomGratuityRoll = Random.Range(0, 100);
        int cumulative = 0;

        foreach (var option in clientData.GratuityOptionsPercentage)
        {
            cumulative += option.Probability;

            if (randomGratuityRoll < cumulative)
            {
                int gratuity = Mathf.RoundToInt(paymentAmount * option.GratuityPercentage / 100f);
                MoneyManager.Instance.AddMoney(gratuity);
                break;
            }
        }

        /*int roll = Random.Range(0, 100);
        if (roll >= gratuityManagerData.ProbabilityToGiveGratuity) return; 

        int randomGratuityRoll = Random.Range(0, 100);
        int cumulative = 0;

        foreach (var option in gratuityManagerData.GratuityOptionsPercentage)
        {
            cumulative += option.Probability;
            if (randomGratuityRoll < cumulative)
            {
                int gratuity = Mathf.RoundToInt(paymentAmount * option.GratuityPercentage / 100f);
                MoneyManager.Instance.AddMoney(gratuity);
                break;
            }
        }*/
    }
}
