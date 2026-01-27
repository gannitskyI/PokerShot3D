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

    protected float currentHealth;
    private float lastDamageTime;
    protected bool isDead;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public float NormalizedHealth => maxHealth > 0 ? currentHealth / maxHealth : 0f;
    public bool IsDead => isDead;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    protected virtual void Update()
    {
        if (isDead || regenPerSecond <= 0) return;

        if (Time.time - lastDamageTime > regenDelayAfterDamage)
        {
            float oldHealth = currentHealth;
            currentHealth = Mathf.Min(currentHealth + regenPerSecond * Time.deltaTime, maxHealth);

            // Fire OnHealed event if health actually increased
            if (currentHealth > oldHealth)
            {
                float healAmount = currentHealth - oldHealth;
                OnHealed.Invoke(healAmount);
            }
        }
    }

    /// <summary>
    /// Applies damage to this entity. Triggers OnDamageTaken event and potentially OnDeath.
    /// </summary>
    /// <param name="amount">Amount of damage to apply</param>
    public virtual void TakeDamage(float amount)
    {
        if (isDead || amount <= 0) return;

        float actualDamage = Mathf.Min(amount, currentHealth);
        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        lastDamageTime = Time.time;

        OnDamageTaken.Invoke(actualDamage);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    /// <summary>
    /// Heals this entity. Cannot exceed max health. Triggers OnHealed event.
    /// </summary>
    /// <param name="amount">Amount of healing to apply</param>
    public virtual void Heal(float amount)
    {
        if (isDead || amount <= 0) return;

        float oldHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        float actualHeal = currentHealth - oldHealth;
        if (actualHeal > 0)
        {
            OnHealed.Invoke(actualHeal);
        }
    }

    /// <summary>
    /// Sets the maximum health. Current health is clamped to new max if necessary.
    /// </summary>
    /// <param name="newMax">New maximum health value</param>
    public virtual void SetMaxHealth(float newMax)
    {
        if (newMax <= 0)
        {
            Debug.LogWarning($"[Health] Attempted to set maxHealth to {newMax}. Must be > 0.");
            return;
        }

        maxHealth = newMax;

        // Clamp current health to new max
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    /// <summary>
    /// Fully restores health to maximum.
    /// </summary>
    public void FullHeal()
    {
        if (isDead) return;

        float healAmount = maxHealth - currentHealth;
        if (healAmount > 0)
        {
            currentHealth = maxHealth;
            OnHealed.Invoke(healAmount);
        }
    }

    /// <summary>
    /// Instantly kills this entity, triggering death events.
    /// </summary>
    public void Kill()
    {
        if (isDead) return;

        currentHealth = 0f;
        Die();
    }

    /// <summary>
    /// Called when health reaches 0. Override to add custom death behavior.
    /// Always call base.Die() when overriding!
    /// </summary>
    protected virtual void Die()
    {
        if (isDead) return; // Prevent double-death

        isDead = true;
        currentHealth = 0f;
        OnDeath.Invoke();

        Debug.Log($"[Health] {gameObject.name} died");
    }

    /// <summary>
    /// Revives this entity with specified health amount.
    /// </summary>
    /// <param name="reviveHealth">Health to revive with (clamped to maxHealth)</param>
    public virtual void Revive(float reviveHealth)
    {
        if (!isDead) return;

        isDead = false;
        currentHealth = Mathf.Clamp(reviveHealth, 1f, maxHealth);
        lastDamageTime = Time.time;

        Debug.Log($"[Health] {gameObject.name} revived with {currentHealth} HP");
    }

    /// <summary>
    /// Resets health to max and clears dead state. Useful for object pooling.
    /// </summary>
    public virtual void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        lastDamageTime = 0f;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (maxHealth <= 0)
        {
            Debug.LogWarning($"[Health] {gameObject.name}: maxHealth must be > 0");
            maxHealth = 1f;
        }

        if (regenPerSecond < 0)
        {
            regenPerSecond = 0f;
        }

        if (regenDelayAfterDamage < 0)
        {
            regenDelayAfterDamage = 0f;
        }
    }
#endif
}