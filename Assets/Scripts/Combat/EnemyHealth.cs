using UnityEngine;

/// <summary>
/// Enemy-specific health component. Handles score rewards, chip drops, and pooling on death.
/// Extends base Health class with enemy-specific death behavior.
/// </summary>
public class EnemyHealth : Health
{
    [Header("Enemy Rewards")]
    [SerializeField] private int scoreReward = 50;
    [SerializeField] private float chipDropChance = 1f; // 0-1 probability
    [SerializeField] private Vector3 chipDropOffset = new Vector3(0, 0.5f, 0);

    [Header("Dependencies (Optional - will use singletons if null)")]
    [SerializeField] private RunStateController scoreSystem;
    [SerializeField] private WaveTracker waveTracker;
    [SerializeField] private ChipPool chipPool;
    [SerializeField] private EnemyPool enemyPool;

    protected override void Awake()
    {
        base.Awake();

        // Auto-find dependencies if not assigned
        if (scoreSystem == null)
            scoreSystem = RunStateController.Instance;

        if (waveTracker == null)
            waveTracker = WaveTracker.Instance;

        if (chipPool == null)
            chipPool = ChipPool.Instance;

        if (enemyPool == null)
            enemyPool = EnemyPool.Instance;
    }

    /// <summary>
    /// Initializes enemy health with dependencies. Use this when spawning from pool.
    /// </summary>
    public void Initialize(float maxHp, RunStateController scoreCtrl = null,
                          WaveTracker tracker = null, ChipPool chips = null, EnemyPool pool = null)
    {
        SetMaxHealth(maxHp);
        ResetHealth();

        // Override dependencies if provided
        if (scoreCtrl != null) scoreSystem = scoreCtrl;
        if (tracker != null) waveTracker = tracker;
        if (chips != null) chipPool = chips;
        if (pool != null) enemyPool = pool;
    }

    /// <summary>
    /// Called when enemy dies. Handles rewards and returns enemy to pool.
    /// </summary>
    protected override void Die()
    {
        // Call base class Die() first to trigger events and set isDead
        base.Die();

        // Award score
        if (scoreSystem != null)
        {
            scoreSystem.AddScore(scoreReward);
        }
        else
        {
            Debug.LogWarning($"[EnemyHealth] {gameObject.name}: No score system available");
        }

        // Notify wave tracker
        if (waveTracker != null)
        {
            waveTracker.EnemyDied();
        }
        else
        {
            Debug.LogWarning($"[EnemyHealth] {gameObject.name}: No wave tracker available");
        }

        // Drop chip with probability
        if (chipPool != null && Random.value <= chipDropChance)
        {
            DropChip();
        }

        // Return to pool (or destroy if no pool)
        ReturnToPool();
    }

    /// <summary>
    /// Spawns a chip at this enemy's position.
    /// </summary>
    private void DropChip()
    {
        GameObject chip = chipPool.GetChip();
        if (chip != null)
        {
            chip.transform.position = transform.position + chipDropOffset;

            var chipComponent = chip.GetComponent<Chip>();
            if (chipComponent != null)
            {
                chipComponent.SetRandomCard();
            }
            else
            {
                Debug.LogWarning("[EnemyHealth] Chip prefab missing Chip component!");
            }
        }
        else
        {
            Debug.LogWarning("[EnemyHealth] Failed to get chip from pool");
        }
    }

    /// <summary>
    /// Returns this enemy to the pool or destroys it.
    /// </summary>
    private void ReturnToPool()
    {
        if (enemyPool != null)
        {
            enemyPool.ReturnEnemy(gameObject);
        }
        else
        {
            Debug.LogWarning($"[EnemyHealth] {gameObject.name}: No enemy pool, destroying instead");
            Destroy(gameObject, 0.1f); // Small delay to allow events to finish
        }
    }

    /// <summary>
    /// Resets enemy health and state. Call when retrieving from pool.
    /// </summary>
    public override void ResetHealth()
    {
        base.ResetHealth();

        // Reset any enemy-specific state here if needed
        // e.g., clear status effects, reset animations, etc.
    }

    /// <summary>
    /// Sets the score reward for killing this enemy.
    /// </summary>
    public void SetScoreReward(int reward)
    {
        scoreReward = Mathf.Max(0, reward);
    }

    /// <summary>
    /// Sets the chip drop probability (0-1).
    /// </summary>
    public void SetChipDropChance(float chance)
    {
        chipDropChance = Mathf.Clamp01(chance);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (scoreReward < 0)
        {
            scoreReward = 0;
        }

        chipDropChance = Mathf.Clamp01(chipDropChance);
    }
#endif
}