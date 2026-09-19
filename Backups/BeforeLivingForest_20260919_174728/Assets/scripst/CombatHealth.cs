namespace DOAN.LegacyCombat {
using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Min(1)] public int maxHealth = 100;
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;
    public event Action<int, int> OnHealthChanged;
    public event Action OnPlayerDeath;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;
        
        CurrentHealth = Mathf.Clamp(CurrentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        // Tìm GameHUD và hiện Damage Indicator
        var hud = FindAnyObjectByType<GameHUD>();
        if (hud != null)
        {
            hud.ShowDamageIndicator();
        }

        if (IsDead)
        {
            OnPlayerDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}

public class EnemyHealth : MonoBehaviour
{
    [Min(1)] public int maxHealth = 80;
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;
    public event Action OnEnemyDeath;
    private Animator animator;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        CurrentHealth -= amount;
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            OnEnemyDeath?.Invoke();
            if (animator != null)
            {
                animator.SetTrigger("Die");
            }
        }
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;
    }
}

}