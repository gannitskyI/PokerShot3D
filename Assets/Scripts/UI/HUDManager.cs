using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("HP")]
    [SerializeField] private Image hpBarFill;
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Score")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Wave")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private Image waveTimerFill;

    private Health playerHealth;
    private RunStateController runState;
    private float maxHp;
    private bool isInitialized;

    public void Init(Health health, RunStateController state)
    {
        if (health == null || state == null)
        {
            Debug.LogError("[HUDManager] Init called with null parameters!");
            return;
        }

        UnsubscribeFromEvents();

        playerHealth = health;
        runState = state;
        maxHp = playerHealth.maxHealth;

        SubscribeToEvents();
        isInitialized = true;

        UpdateHUD();

        Debug.Log("[HUDManager] Initialized successfully");
    }

    private void OnEnable()
    {
        if (isInitialized && playerHealth != null && runState != null)
        {
            SubscribeToEvents();
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamageTaken.AddListener(OnDamage);
            playerHealth.OnHealed.AddListener(OnHeal);
        }

        if (runState != null)
        {
            runState.OnScoreChanged.AddListener(OnScoreChanged);
            runState.OnWaveChanged.AddListener(OnWaveChanged);
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamageTaken.RemoveListener(OnDamage);
            playerHealth.OnHealed.RemoveListener(OnHeal);
        }

        if (runState != null)
        {
            runState.OnScoreChanged.RemoveListener(OnScoreChanged);
            runState.OnWaveChanged.RemoveListener(OnWaveChanged);
        }
    }

    private void Update()
    {
        UpdateWaveTimer();
    }

    private void UpdateHUD()
    {
        if (!isInitialized || playerHealth == null || runState == null)
        {
            return;
        }

        UpdateHealthDisplay();
        UpdateScoreDisplay();
        UpdateWaveDisplay();
    }

    private void UpdateHealthDisplay()
    {
        if (hpBarFill != null)
        {
            hpBarFill.fillAmount = playerHealth.NormalizedHealth;
        }

        if (hpText != null)
        {
            hpText.text = $"{Mathf.RoundToInt(playerHealth.CurrentHealth)} / {Mathf.RoundToInt(maxHp)}";
        }
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"{runState.CurrentScore}";
        }
    }

    private void UpdateWaveDisplay()
    {
        if (waveText != null)
        {
            waveText.text = $"Wave {runState.CurrentWave}";
        }
    }

    private void OnDamage(float dmg)
    {
        UpdateHealthDisplay();
    }

    private void OnHeal(float heal)
    {
        UpdateHealthDisplay();
    }

    private void OnScoreChanged(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"{score}";
        }
    }

    private void OnWaveChanged(int wave)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave {wave}";
        }
    }

    private void UpdateWaveTimer()
    {
        if (waveTimerFill != null)
        {
            waveTimerFill.fillAmount = 0.7f;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (hpBarFill == null)
            Debug.LogWarning("[HUDManager] HP Bar Fill not assigned");
        if (hpText == null)
            Debug.LogWarning("[HUDManager] HP Text not assigned");
        if (scoreText == null)
            Debug.LogWarning("[HUDManager] Score Text not assigned");
        if (waveText == null)
            Debug.LogWarning("[HUDManager] Wave Text not assigned");
    }
#endif
}