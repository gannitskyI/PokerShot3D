using UnityEngine;
using System;
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
        Preparing,      // Перед запуском первой волны / в меню
        Playing,        // Активная волна
        Shopping,       // Магазин после волны
        BossFight,      // Босс-фаза
        GameOver,       // Проигрыш
        Victory         // Победа после босса
    }
    [Header("Events")]
    public UnityEvent<int> OnScoreChanged = new UnityEvent<int>();

    [Header("Wave Progression")]
    [Tooltip("Последовательность обычных волн (8–10 штук)")]
    [SerializeField] private WaveConfig[] normalWaveSequence;

    [Tooltip("Конфиг босс-волны (отдельный SO)")]
    [SerializeField] private WaveConfig bossWaveConfig;

    private WaveConfig currentWaveConfig;

    // Свойства для доступа из других систем
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
        // Для воспроизводимости на собесе можно закомментировать и использовать фиксированный seed
        // Random.InitState(42);
    }

    public void StartNewRun()
    {
        currentWave = 0;
        currentScore = 0;
        currentPhase = RunPhase.Preparing;

        AdvanceToNextWave(); // сразу переходим к первой волне
        Debug.Log("[RunState] Новый забег запущен");
    }

    public void AdvanceToNextWave()
    {
        currentWave++;

        if (currentWave - 1 < normalWaveSequence.Length)
        {
            currentWaveConfig = normalWaveSequence[currentWave - 1];
            currentPhase = RunPhase.Playing;
            Debug.Log($"[RunState] Волна {currentWave} начата: {currentWaveConfig.name} ({currentWaveConfig.totalEnemies} врагов)");
        }
        else
        {
            currentWaveConfig = bossWaveConfig;
            currentPhase = RunPhase.BossFight;
            Debug.Log("[RunState] Переход к босс-фазе!");
        }

        // ? Вот это добавляем — запуск спавна!
        if (waveSpawner != null && currentWaveConfig != null)
        {
            waveSpawner.StartWave(currentWaveConfig);
            Debug.Log("[RunState] Вызван StartWave на WaveSpawner");
        }
        else
        {
            Debug.LogError("[RunState] WaveSpawner или currentWaveConfig = null!");
        }
        if (waveSpawner != null && currentWaveConfig != null)
            waveSpawner.StartWave(currentWaveConfig);
    }

    public void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged.Invoke(currentScore);  // ? теперь вызываем событие
        Debug.Log($"[RunState] Очки обновлены: {currentScore}");
    }

    public void EndWave()
    {
        if (currentPhase == RunPhase.Playing)
        {
            currentPhase = RunPhase.Shopping;
            Debug.Log("[RunState] Волна завершена ? магазин");
            // В будущем: OnWaveEnded?.Invoke();
        }
        else if (currentPhase == RunPhase.BossFight)
        {
            currentPhase = RunPhase.Victory;
            Debug.Log("[RunState] Босс побеждён ? победа!");
            // В будущем: OnRunCompleted?.Invoke(true);
        }
    }

    public void GameOver()
    {
        currentPhase = RunPhase.GameOver;
        Debug.Log("[RunState] Игра окончена — поражение");
        // В будущем: OnRunCompleted?.Invoke(false);
    }

    // Для отладки в инспекторе / собесе
    private void OnValidate()
    {
        if (normalWaveSequence == null || normalWaveSequence.Length == 0)
            Debug.LogWarning("[RunStateController] Не назначены normalWaveSequence!");
        if (bossWaveConfig == null)
            Debug.LogWarning("[RunStateController] Не назначен bossWaveConfig!");
    }
}