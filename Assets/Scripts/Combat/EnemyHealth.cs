using UnityEngine;

public class EnemyHealth : Health
{
    [SerializeField] private float maxHp = 15f;
    private float currentHp;

    public void SetMaxHp(float newMaxHp)
    {
        maxHp = newMaxHp;
        currentHp = maxHp;
    }

    private void Awake()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(float dmg)
    {
        currentHp -= dmg;
        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        RunStateController.Instance.AddScore(50);

        if (WaveTracker.Instance != null)
            WaveTracker.Instance.EnemyDied();

        // ÂÎÑÑÒÀÍÎÂËÅÍÍÛÉ ÄĞÎÏ ×ÈÏÎÂ
        if (ChipPool.Instance != null)
        {
            GameObject chip = ChipPool.Instance.GetChip();
            if (chip != null)
            {
                chip.transform.position = transform.position + Vector3.up * 0.5f;
                var chipComp = chip.GetComponent<Chip>();
                if (chipComp != null)
                {
                    chipComp.SetRandomCard();
                }
                Debug.Log("[EnemyHealth] Äğîïíóë ÷èï!");
            }
        }

        // Âîçâğàò â ïóë
        EnemyPool.Instance.ReturnEnemy(gameObject);
    }
}