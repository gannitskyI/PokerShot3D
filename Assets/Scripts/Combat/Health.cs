using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] public float maxHealth = 100f;
    [SerializeField] private float regenPerSecond = 1f;
    [SerializeField] private float regenDelayAfterDamage = 3f;

    [Header("Events")]
    public UnityEvent<float> OnDamageTaken = new UnityEvent<float>();
    public UnityEvent<float> OnHealed = new UnityEvent<float>();
    public UnityEvent OnDeath = new UnityEvent();

    private float currentHealth;
    private float lastDamageTime;
    private bool isDead;

    public float CurrentHealth => currentHealth;
    public float NormalizedHealth => currentHealth / maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    private void Update()
    {
        if (isDead) return;

        if (Time.time - lastDamageTime > regenDelayAfterDamage)
        {
            currentHealth = Mathf.Min(currentHealth + regenPerSecond * Time.deltaTime, maxHealth);
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead || amount <= 0) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        lastDamageTime = Time.time;

        OnDamageTaken.Invoke(amount);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealed.Invoke(amount);
    }

    // Новый метод — теперь можно менять maxHealth runtime
    public void SetMaxHealth(float newMax)
    {
        maxHealth = newMax;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    private void Die()
    {
        isDead = true;
        currentHealth = 0f;
        OnDeath.Invoke();
        Debug.Log($"[Health] {gameObject.name} погиб");
    }
}