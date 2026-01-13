using UnityEngine;

public class EnemyHealth : MonoBehaviour
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
        if (ChipPool.Instance != null)
        {
            GameObject chip = ChipPool.Instance.GetChip();
            if (chip != null)
            {
                chip.transform.position = transform.position + Vector3.up * 0.5f;
                var chipComp = chip.GetComponent<Chip>();
                if (chipComp != null)
                {
                    chipComp.SetRandomCard(); // ? добавим метод позже
                }
                Debug.Log("[EnemyHealth] Дропнул чип!");
            }
        }

        EnemyPool.Instance.ReturnEnemy(gameObject, gameObject); // пока заглушка
    }
}