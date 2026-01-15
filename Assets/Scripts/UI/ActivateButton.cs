using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ActivateButton : MonoBehaviour
{
    [SerializeField] private Button button;
    private ActivationAnimator activationAnimator;
    [SerializeField] private float baseCooldown = 10f;
    [Header("Визуализация руки")]
    [SerializeField] private HandVisualManager handVisualManager;
    private ChipMagnet chipMagnet;
    private float currentCooldown;
    private HandVisualManager handVisual;

    public void Init(ChipMagnet magnet, HandVisualManager visual, ActivationAnimator animator)
    {
        chipMagnet = magnet;
        handVisual = visual;
        activationAnimator = animator;

        if (chipMagnet == null || handVisual == null || activationAnimator == null)
        {
            Debug.LogError("[ActivateButton] Init: один из компонентов null!");
            return;
        }

        Debug.Log("[ActivateButton] Init успешен: всё получено");
        button.interactable = chipMagnet.hand.Count >= 2 && currentCooldown <= 0;
    }

    private void Awake()
    {
        button.interactable = false;
    }

    private void OnEnable()
    {
        button.onClick.AddListener(OnActivatePressed);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnActivatePressed);
    }

    private void Update()
    {
        if (chipMagnet == null) return;

        button.interactable = chipMagnet.hand.Count >= 2 && currentCooldown <= 0;

        var text = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null)
        {
            text.text = currentCooldown > 0
                ? $"АКТИВАЦИЯ ({Mathf.Ceil(currentCooldown)}с)"
                : "АКТИВАЦИЯ";
        }

        if (currentCooldown > 0)
            currentCooldown -= Time.deltaTime;
    }

    private void OnActivatePressed()
    {
        if (chipMagnet == null || chipMagnet.hand.Count < 2) return;

        var result = PokerEvaluator.EvaluateHand(chipMagnet.hand);
        Debug.Log($"[Activate] Активация: {result.description} (x{result.multiplier})");

        activationAnimator.PlayActivation(result);  // через ссылку, без Find

        var handCopy = chipMagnet.hand.ToList();
        foreach (var chip in handCopy)
            ChipPool.Instance.ReturnChip(chip.gameObject);

        chipMagnet.hand.Clear();

        currentCooldown = baseCooldown - (result.multiplier * 0.5f);
        button.interactable = false;
    }

    private void ApplyComboEffect(PokerEvaluator.HandResult result)
    {
        var animator = FindObjectOfType<ActivationAnimator>(); 
        if (animator != null)
            animator.PlayActivation(result);
        switch (result.type)
        {
            case PokerEvaluator.HandType.Pair:
                Debug.Log("[Combo] ?2 урон на 6 сек");
                break;
            case PokerEvaluator.HandType.RoyalFlush:
                Debug.Log("[Combo] РОЯЛ-ФЛЕШ — НЮК АРЕНЫ!");
                var enemies = FindObjectsOfType<EnemyHealth>();
                foreach (var e in enemies) e.TakeDamage(9999f);
                break;
            default:
                Debug.Log("[Combo] Базовый эффект");
                break;
        }
    }
}