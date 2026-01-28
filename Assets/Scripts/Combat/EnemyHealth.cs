using UnityEngine;

public class EnemyHealth : Health
{
    [Header("Enemy Rewards")]
    [SerializeField] private int scoreReward = 50;
    [SerializeField] private float chipDropChance = 1f;
    [SerializeField] private Vector3 chipDropOffset = new Vector3(0, 0.5f, 0);

    [Header("Dependencies (Optional - will use singletons if null)")]
    [SerializeField] private RunStateController scoreSystem;
    [SerializeField] private WaveTracker waveTracker;
    [SerializeField] private ChipPool chipPool;
    [SerializeField] private EnemyPool enemyPool;

    protected override void Awake()
    {
        base.Awake();

        if (scoreSystem == null)
            scoreSystem = RunStateController.Instance;

        if (waveTracker == null)
            waveTracker = WaveTracker.Instance;

        if (chipPool == null)
            chipPool = ChipPool.Instance;

        if (enemyPool == null)
            enemyPool = EnemyPool.Instance;
    }

    public void Initialize(float maxHp, RunStateController scoreCtrl = null,
                          WaveTracker tracker = null, ChipPool chips = null, EnemyPool pool = null)
    {
        SetMaxHealth(maxHp);
        ResetHealth();

        if (scoreCtrl != null) scoreSystem = scoreCtrl;
        if (tracker != null) waveTracker = tracker;
        if (chips != null) chipPool = chips;
        if (pool != null) enemyPool = pool;
    }

    protected override void Die()
    {
        base.Die();

        if (scoreSystem != null)
        {
            scoreSystem.AddScore(scoreReward);
        }
        else
        {
            Debug.LogWarning($"[EnemyHealth] {gameObject.name}: No score system available");
        }

        if (chipPool != null && Random.value <= chipDropChance)
        {
            DropChip();
        }

        ReturnToPool();
    }

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

    private void ReturnToPool()
    {
        if (enemyPool != null)
        {
            enemyPool.ReturnEnemy(gameObject);
        }
        else
        {
            Debug.LogWarning($"[EnemyHealth] {gameObject.name}: No enemy pool, destroying instead");
            Destroy(gameObject, 0.1f);
        }
    }

    public override void ResetHealth()
    {
        base.ResetHealth();
    }

    public void SetScoreReward(int reward)
    {
        scoreReward = Mathf.Max(0, reward);
    }

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