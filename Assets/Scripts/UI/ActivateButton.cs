using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ActivateButton : MonoBehaviour
{
    [SerializeField] private Button button;
    private ActivationAnimator activationAnimator;
    [SerializeField] private float baseCooldown = 10f;
    [Header("Hand Visualization")]
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
            Debug.LogError("[ActivateButton] Init: null!");
            return;
        }

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
                ? $"ACTIVATION ({Mathf.Ceil(currentCooldown)}s)"
                : "ACTIVATION";
        }

        if (currentCooldown > 0)
            currentCooldown -= Time.deltaTime;
    }

    private void OnActivatePressed()
    {
        if (chipMagnet == null || chipMagnet.hand.Count < 2) return;

        var result = PokerEvaluator.EvaluateHand(chipMagnet.hand);
        Debug.Log($"[Activate] Activation: {result.description} (x{result.multiplier})");

        activationAnimator.PlayActivation(result);

        var handCopy = chipMagnet.hand.ToList();
        foreach (var chip in handCopy)
            ChipPool.Instance.ReturnChip(chip.gameObject);

        chipMagnet.hand.Clear();

        currentCooldown = baseCooldown - (result.multiplier * 0.5f);
        button.interactable = false;
    }

    private void ApplyComboEffect(PokerEvaluator.HandResult result)
    {
        var comboEffects = FindObjectOfType<ComboEffects>();
        if (comboEffects != null)
            comboEffects.ApplyCombo(result);
        else
            Debug.LogWarning("[ActivateButton] ComboEffects not found");
    }
}