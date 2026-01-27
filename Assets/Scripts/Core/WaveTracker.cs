using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class WaveTracker : MonoBehaviour
{
    public static WaveTracker Instance { get; private set; }

    [Header("События")]
    public UnityEvent OnWaveStarted = new UnityEvent();
    public UnityEvent OnWaveCompleted = new UnityEvent();

    public List<GameObject> activeEnemies = new List<GameObject>();

    private int liveEnemies = 0;  // ? concurrentEnemies переименован в liveEnemies
    private bool isWaveActive;

    public int LiveEnemies => liveEnemies;  // ? для HUD

    private void Awake()
    {
        Instance = this;
    }

    public void StartWave()
    {
        activeEnemies.Clear();
        liveEnemies = 0;
        isWaveActive = true;
        OnWaveStarted.Invoke();
    }

    public void EnemySpawned(GameObject enemy)
    {
        if (!isWaveActive || enemy == null) return;

        activeEnemies.Add(enemy);
        liveEnemies++;  // ? +1 живой враг

        var health = enemy.GetComponent<Health>();
        if (health != null)
        {
            health.OnDeath.AddListener(EnemyDied);
        }
    }

    public void EnemyReturned(GameObject enemy)
    {
        if (enemy == null) return;

        activeEnemies.Remove(enemy);

        var health = enemy.GetComponent<Health>();
        if (health != null)
        {
            health.OnDeath.RemoveListener(EnemyDied);
        }
    }

    public void EnemyDied()
    {
        liveEnemies--;  // ? -1 живой враг (только при смерти)
        activeEnemies.RemoveAll(e => e == null || !e.activeSelf);

        if (liveEnemies == 0 && isWaveActive)
        {
            isWaveActive = false;
            OnWaveCompleted.Invoke();
            Debug.Log("[WaveTracker] Все враги мертвы — волна завершена");
        }
    }
}