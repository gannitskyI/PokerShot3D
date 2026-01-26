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
            WaveTracker.Instance.EnemyDied(gameObject);  // ? теперь public

        gameObject.SetActive(false);
    }
}