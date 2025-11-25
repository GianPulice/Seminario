using TMPro;
using UnityEngine;

public class FloatingMoneyText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textAmount;
    [SerializeField] private Animator animator;

    private float maxTimeToReturnObjectToPool = 0.45f;

    public TextMeshProUGUI TextAmount { get => textAmount; set => textAmount = value; }
    public float MaxTimeToReturnObjectToPool { get => maxTimeToReturnObjectToPool; }


    void Awake()
    {
        GetComponent();
    }

    private void GetComponent()
    {
        textAmount = GetComponentInChildren<TextMeshProUGUI>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }
    public void ActivateAdminAnimation(bool value)
    {
        if (animator != null)
        {
            animator.SetBool("isInAdmin",value);
        }
    }
}