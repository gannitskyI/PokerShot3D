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

    public void Init(Health health, RunStateController state)
    {
        playerHealth = health;
        runState = state;
        maxHp = playerHealth.maxHealth;

        playerHealth.OnDamageTaken.AddListener(OnDamage);
        playerHealth.OnHealed.AddListener(OnHeal);
        runState.OnScoreChanged.AddListener(OnScoreChanged);
        runState.OnWaveChanged.AddListener(OnWaveChanged);

        UpdateHUD();
    }

    private void OnEnable()
    {
        if (playerHealth != null && runState != null)
        {
            playerHealth.OnDamageTaken.AddListener(OnDamage);
            playerHealth.OnHealed.AddListener(OnHeal);
            runState.OnScoreChanged.AddListener(OnScoreChanged);
            runState.OnWaveChanged.AddListener(OnWaveChanged);
        }
    }

    private void OnDisable()
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
        if (playerHealth == null || runState == null) return;

        hpBarFill.fillAmount = playerHealth.NormalizedHealth;
        hpText.text = $"{Mathf.RoundToInt(playerHealth.CurrentHealth)} / {Mathf.RoundToInt(maxHp)}";
        scoreText.text = $"{runState.CurrentScore}";
        waveText.text = $"Wave {runState.CurrentWave}";
    }

    private void OnDamage(float dmg)
    {
        UpdateHUD();
    }

    private void OnHeal(float heal)
    {
        UpdateHUD();
    }

    private void OnScoreChanged(int score)
    {
        scoreText.text = $"{score}";
    }

    private void OnWaveChanged(int wave)
    {
        waveText.text = $"Wave {wave}";
    }

    private void UpdateWaveTimer()
    {
        waveTimerFill.fillAmount = 0.7f;
    }
}