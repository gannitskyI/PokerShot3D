using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class WaveTracker : MonoBehaviour
{
    public static WaveTracker Instance { get; private set; }

    [Header("События")]
    public UnityEvent OnWaveStarted = new UnityEvent();
    public UnityEvent OnWaveCompleted = new UnityEvent();

    // Открываем для редактора (можно [HideInInspector] если не хочешь видеть в инспекторе)
    public List<GameObject> activeEnemies = new List<GameObject>();

    private int concurrentEnemies = 0;
    private bool isWaveActive;

    public int ConcurrentEnemies => concurrentEnemies;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartWave()
    {
        activeEnemies.Clear();
        concurrentEnemies = 0;
        isWaveActive = true;
        OnWaveStarted.Invoke();
        Debug.Log("[WaveTracker] Волна стартовала");
    }

    public void EnemySpawned(GameObject enemy)
    {
        if (!isWaveActive || enemy == null) return;

        activeEnemies.Add(enemy);
        concurrentEnemies++;

        var health = enemy.GetComponent<Health>();
        if (health != null)
        {
            health.OnDeath.AddListener(() => EnemyDied(enemy));
        }
    }

    public void EnemyReturned(GameObject enemy)
    {
        if (enemy == null) return;

        activeEnemies.Remove(enemy);
        concurrentEnemies = Mathf.Max(0, concurrentEnemies - 1);

        var health = enemy.GetComponent<Health>();
        if (health != null)
        {
            health.OnDeath.RemoveListener(() => EnemyDied(enemy));
        }
    }

    public void EnemyDied(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        concurrentEnemies = Mathf.Max(0, concurrentEnemies - 1);

        if (concurrentEnemies == 0 && isWaveActive)
        {
            isWaveActive = false;
            OnWaveCompleted.Invoke();
            Debug.Log("[WaveTracker] Все враги мертвы — волна завершена");
        }
    }
}