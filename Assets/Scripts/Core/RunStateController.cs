using UnityEngine;
using UnityEngine.Events;

public class RunStateController : MonoBehaviour
{
    public static RunStateController Instance { get; private set; }

    [Header("Run State")]
    [SerializeField] private int currentWave = 0;
    [SerializeField] private int currentScore = 0;
    [SerializeField] private RunPhase currentPhase = RunPhase.Preparing;

    [Header("Systems")]
    [SerializeField] private WaveSpawner waveSpawner;

    public enum RunPhase
    {
        Preparing,
        Playing,
        Shopping,
        BossFight,
        GameOver,
        Victory
    }

    [Header("Events")]
    public UnityEvent<int> OnScoreChanged = new UnityEvent<int>();
    public UnityEvent<int> OnWaveChanged = new UnityEvent<int>();

    [Header("Wave Progression")]
    [SerializeField] private WaveConfig[] normalWaveSequence;
    [SerializeField] private WaveConfig bossWaveConfig;

    private WaveConfig currentWaveConfig;

    public int CurrentWave => currentWave;
    public int CurrentScore => currentScore;
    public RunPhase CurrentPhase => currentPhase;
    public WaveConfig CurrentWaveConfig => currentWaveConfig;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (WaveTracker.Instance != null)
            WaveTracker.Instance.OnWaveCompleted.AddListener(EndWave);
    }

    public void StartNewRun()
    {
        currentWave = 1;
        currentScore = 0;
        currentPhase = RunPhase.Playing;

        if (currentWave - 1 < normalWaveSequence.Length)
        {
            currentWaveConfig = normalWaveSequence[currentWave - 1];
        }
        else
        {
            currentWaveConfig = bossWaveConfig;
            currentPhase = RunPhase.BossFight;
        }

        if (waveSpawner != null && currentWaveConfig != null)
        {
            waveSpawner.StartWave(currentWaveConfig);
        }

        OnWaveChanged.Invoke(currentWave);

        Debug.Log($"[RunState] Новый забег — сразу Волна {currentWave}");
    }

    public void AdvanceToNextWave()
    {
        if (currentPhase != RunPhase.Shopping) return;

        currentWave++;

        if (currentWave - 1 < normalWaveSequence.Length)
        {
            currentWaveConfig = normalWaveSequence[currentWave - 1];
            currentPhase = RunPhase.Playing;
        }
        else
        {
            currentWaveConfig = bossWaveConfig;
            currentPhase = RunPhase.BossFight;
        }

        if (waveSpawner != null && currentWaveConfig != null)
        {
            waveSpawner.StartWave(currentWaveConfig);
        }

        OnWaveChanged.Invoke(currentWave);
    }

    public void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged.Invoke(currentScore);
    }

    public void EndWave()
    {
        if (currentPhase == RunPhase.Playing)
        {
            currentPhase = RunPhase.Shopping;
        }
        else if (currentPhase == RunPhase.BossFight)
        {
            currentPhase = RunPhase.Victory;
        }
    }

    public void GameOver()
    {
        currentPhase = RunPhase.GameOver;
    }

    private void OnValidate()
    {
        if (normalWaveSequence == null || normalWaveSequence.Length == 0)
            Debug.LogWarning("[RunStateController] Не назначены normalWaveSequence!");
        if (bossWaveConfig == null)
            Debug.LogWarning("[RunStateController] Не назначен bossWaveConfig!");
    }
}