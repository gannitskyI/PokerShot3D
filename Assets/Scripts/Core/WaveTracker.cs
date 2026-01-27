using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class WaveTracker : MonoBehaviour
{
    public static WaveTracker Instance { get; private set; }

    [Header("Events")]
    public UnityEvent OnWaveStarted = new UnityEvent();
    public UnityEvent OnWaveCompleted = new UnityEvent();

    public List<GameObject> activeEnemies = new List<GameObject>();

    private int liveEnemies = 0;  
    private bool isWaveActive;

    public int LiveEnemies => liveEnemies;   

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
        liveEnemies++;  

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
        liveEnemies--;  
        activeEnemies.RemoveAll(e => e == null || !e.activeSelf);

        if (liveEnemies == 0 && isWaveActive)
        {
            isWaveActive = false;
            OnWaveCompleted.Invoke();
            Debug.Log("[WaveTracker] End wave");
        }
    }
}