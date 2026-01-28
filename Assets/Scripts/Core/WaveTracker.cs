using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class WaveTracker : MonoBehaviour
{
    public static WaveTracker Instance { get; private set; }

    [Header("Events")]
    public UnityEvent OnWaveStarted = new UnityEvent();
    public UnityEvent OnWaveCompleted = new UnityEvent();
    public UnityEvent OnEnemyDied = new UnityEvent();

    private Dictionary<GameObject, Health> activeEnemiesHealth = new Dictionary<GameObject, Health>();
    private int liveEnemies = 0;
    private bool isWaveActive;

    public int LiveEnemies => liveEnemies;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[WaveTracker] Duplicate instance destroyed");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        CleanupAllEnemies();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void StartWave()
    {
        CleanupAllEnemies();

        liveEnemies = 0;
        isWaveActive = true;
        OnWaveStarted.Invoke();

        Debug.Log("[WaveTracker] Wave started");
    }

    public void EnemySpawned(GameObject enemy)
    {
        if (!isWaveActive || enemy == null)
        {
            Debug.LogWarning("[WaveTracker] Cannot spawn enemy: wave inactive or enemy null");
            return;
        }

        if (activeEnemiesHealth.ContainsKey(enemy))
        {
            Debug.LogWarning($"[WaveTracker] Enemy {enemy.name} already registered");
            return;
        }

        var health = enemy.GetComponent<Health>();
        if (health == null)
        {
            Debug.LogError($"[WaveTracker] Enemy {enemy.name} missing Health component!");
            return;
        }

        activeEnemiesHealth.Add(enemy, health);
        liveEnemies++;

        health.OnDeath.AddListener(OnEnemyDeath);

        Debug.Log($"[WaveTracker] Enemy spawned. Live count: {liveEnemies}");
    }

    public void EnemyReturned(GameObject enemy)
    {
        if (enemy == null) return;

        UnregisterEnemy(enemy);

        Debug.Log($"[WaveTracker] Enemy returned. Live count: {liveEnemies}");
    }

    private void OnEnemyDeath()
    {
        List<GameObject> deadEnemies = new List<GameObject>();

        foreach (var kvp in activeEnemiesHealth)
        {
            if (kvp.Value != null && kvp.Value.IsDead)
            {
                deadEnemies.Add(kvp.Key);
            }
        }

        foreach (var deadEnemy in deadEnemies)
        {
            UnregisterEnemy(deadEnemy);
        }

        liveEnemies -= deadEnemies.Count;

        OnEnemyDied.Invoke();

        Debug.Log($"[WaveTracker] Enemy died. Live count: {liveEnemies}");

        if (liveEnemies <= 0 && isWaveActive)
        {
            CompleteWave();
        }
    }

    private void CompleteWave()
    {
        isWaveActive = false;
        OnWaveCompleted.Invoke();

        Debug.Log("[WaveTracker] Wave completed!");

        CleanupAllEnemies();
    }

    private void UnregisterEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        if (activeEnemiesHealth.TryGetValue(enemy, out Health health))
        {
            if (health != null)
            {
                health.OnDeath.RemoveListener(OnEnemyDeath);
            }
            activeEnemiesHealth.Remove(enemy);
        }
    }

    private void CleanupAllEnemies()
    {
        foreach (var kvp in activeEnemiesHealth)
        {
            if (kvp.Value != null)
            {
                kvp.Value.OnDeath.RemoveListener(OnEnemyDeath);
            }
        }

        activeEnemiesHealth.Clear();
    }

    private void LateUpdate()
    {
        if (!isWaveActive) return;

        int actualActive = 0;
        foreach (var kvp in activeEnemiesHealth)
        {
            if (kvp.Key != null && kvp.Key.activeSelf && !kvp.Value.IsDead)
            {
                actualActive++;
            }
        }

        if (actualActive != liveEnemies)
        {
            Debug.LogWarning($"[WaveTracker] Count mismatch detected. Tracked: {liveEnemies}, Actual: {actualActive}. Syncing...");
            liveEnemies = actualActive;

            if (liveEnemies <= 0)
            {
                CompleteWave();
            }
        }
    }
}