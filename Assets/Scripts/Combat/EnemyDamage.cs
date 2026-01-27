using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private float damage = 10f;            
    [SerializeField] private float attackCooldown = 1f;    

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
            }
        }
    }
}