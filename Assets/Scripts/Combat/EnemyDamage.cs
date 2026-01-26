using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private float damage = 10f;           // урон за касание
    [SerializeField] private float attackCooldown = 1f;    // чтобы не били каждую миллисекунду

    private float lastAttackTime;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Time.time - lastAttackTime > attackCooldown)
        {
            var health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                lastAttackTime = Time.time;
                Debug.Log($"[EnemyDamage] Игрок получил {damage} урона от {gameObject.name}");
            }
        }
    }
}